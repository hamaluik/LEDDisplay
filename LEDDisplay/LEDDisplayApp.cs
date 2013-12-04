using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using HidLibrary;
using System.Timers;

namespace LEDDisplay
{
    class LEDDisplayApp : Form
    {
        private static int VendorID = 0x1D34;
        private static int ProductID = 0x0013;
        private static HidDevice device;
        private static bool attached = false;
        private static System.Timers.Timer scrollTimer;
        public static ScrollingFrameBuffer frameBuffer = new ScrollingFrameBuffer(48);
        private static NotifyIcon trayIcon;
        private static ContextMenu trayMenu;
        private static bool cycleDisplay = true;
        private static Dictionary<string, DisplayProvider> displayProviders = new Dictionary<string, DisplayProvider>();
        private static Dictionary<string, DisplayProvider>.Enumerator displayProviderEnumerator;

        [STAThread]
        public static void Main()
        {
            Application.Run(new LEDDisplayApp());
        }

        public LEDDisplayApp()
        {
            // load up our list of display providers
            if(Properties.Settings.Default.EnableTime)
                displayProviders.Add(DisplayProvider_Time.Title, new DisplayProvider_Time());
            if (Properties.Settings.Default.EnableWeather)
                displayProviders.Add(DisplayProvider_Weather.Title, new DisplayProvider_Weather());
            if (Properties.Settings.Default.EnableGMail)
                displayProviders.Add(DisplayProvider_GMail.Title, new DisplayProvider_GMail());

            // create the tray menu
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Connect", OnConnect);
            trayMenu.MenuItems.Add("-");
            foreach (string displayProviderTitle in displayProviders.Keys)
            {
                displayProviders[displayProviderTitle].menuItem = trayMenu.MenuItems.Add(displayProviderTitle, displayProviders[displayProviderTitle].OnClick);
            }
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Cycle Display", OnCycle);
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Exit", OnExit);
            trayMenu.MenuItems[trayMenu.MenuItems.Count - 3].Checked = cycleDisplay;

            // create a tray icon
            trayIcon = new NotifyIcon();
            trayIcon.Text = "LED Display";
            trayIcon.Icon = LEDDisplay.Properties.Resources.led;

            // add a menu to the icon and show it
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            // start our scroll timer
            scrollTimer = new System.Timers.Timer(100);
            scrollTimer.Elapsed += scrollTimer_Elapsed;
            scrollTimer.Enabled = true;

            // display our first provider
            displayProviderEnumerator = displayProviders.GetEnumerator();
            displayProviderEnumerator.MoveNext();
            displayProviderEnumerator.Current.Value.Draw(ref frameBuffer);
            displayProviderEnumerator.Current.Value.menuItem.Checked = true;
        }

        public static void HandleClick(string clickedTitle)
        {
            // uncheck all of our providers
            foreach (string displayProviderTitle in displayProviders.Keys)
            {
                displayProviders[displayProviderTitle].menuItem.Checked = false;
            }
            // re-check our selected one
            displayProviders[clickedTitle].menuItem.Checked = true;
            
            // reset our enumerator
            displayProviderEnumerator = displayProviders.GetEnumerator();
            displayProviderEnumerator.MoveNext();
            while (displayProviderEnumerator.Current.Key.CompareTo(clickedTitle) != 0)
            {
                if (!displayProviderEnumerator.MoveNext())
                {
                    displayProviderEnumerator = displayProviders.GetEnumerator();
                    displayProviderEnumerator.MoveNext();
                    break;
                }
            }
        }

        private void OnCycle(object sender, EventArgs e)
        {
            // toggle cycling
            cycleDisplay = !cycleDisplay;
            trayMenu.MenuItems[trayMenu.MenuItems.Count - 3].Checked = cycleDisplay;
        }

        private void CycleDisplay()
        {
            // if we actually want to move on to the next display, do so
            if (cycleDisplay)
            {
                if (!displayProviderEnumerator.MoveNext())
                {
                    displayProviderEnumerator = displayProviders.GetEnumerator();
                    displayProviderEnumerator.MoveNext();
                }
            }
            // simulate a click so everything gets done
            displayProviderEnumerator.Current.Value.OnClick(null, null);
        }

        void scrollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            // only do anything if we're attached
            if (!attached)
            {
                return;
            }

            // lock the frame buffer so we don't get visual artefacts
            lock (frameBuffer)
            {
                frameBuffer.ViewOffset++;
                if (frameBuffer.ViewOffset == -21)
                {
                    CycleDisplay();
                }
            }

            // use a lock so we don't shear
            lock (frameBuffer)
            {
                // our packets are 9 bytes long
                // [report id] [brightness] [row specifier] [data] [data] [data] [data] [data] [data]
                byte[] data = new byte[] { 0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff };

                // build our packet
                for (int row = 0; row < 7; row += 2)
                {
                    // get our frame buffer to fill it in
                    frameBuffer.FillPacket(row, ref data);

                    // write it out!
                    device.Write(data);
                }
            }
        }

        private void OnConnect(object sender, EventArgs e)
        {
            // enumerate our devices
            device = HidDevices.Enumerate(VendorID, ProductID).FirstOrDefault();

            if (device != null)
            {
                // open the device
                device.OpenDevice();

                // and attach events
                device.Inserted += DeviceAttachedHandler;
                device.Removed += DeviceRemovedHandler;
                device.MonitorDeviceEvents = true;

                // change our menu items
                trayMenu.MenuItems[0].Text = "Connected!";
                trayMenu.MenuItems[0].Click -= OnConnect;
                trayMenu.MenuItems[0].Enabled = false;
                trayMenu.MenuItems[0].Checked = true;
            }
        }

        private void DeviceAttachedHandler()
        {
            attached = true;
            //MessageBox.Show("Dream Cheeky LED Display Connected!");
        }

        private void DeviceRemovedHandler()
        {
            attached = false;

            // change our menu items
            trayMenu.MenuItems[0].Text = "Connect";
            trayMenu.MenuItems[0].Click += OnConnect;
            trayMenu.MenuItems[0].Enabled = true;
            trayMenu.MenuItems[0].Checked = false;

            //MessageBox.Show("Dream Cheeky LED Display Removed!");
        }

        private void OnExit(object sender, EventArgs e)
        {
            device.CloseDevice();
            Application.Exit();
        }

        protected override void OnLoad(EventArgs e)
        {
            // hide the window and remove it from the taskbar
            Visible = false;
            ShowInTaskbar = false;

            // try to connect to the display
            OnConnect(null, null);

            // propogate
            base.OnLoad(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // release the icon resource
                trayIcon.Dispose();
            }

            // propogate
            base.Dispose(isDisposing);
        }
    }
}
