using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LEDDisplay
{
    class DisplayProvider_Time : DisplayProvider
    {
        public static new string Title
        {
            get
            {
                return "Time";
            }
        }

        public override void Draw(ref ScrollingFrameBuffer buffer)
        {
            DateTime time = DateTime.Now;
            string timeString = time.ToString(Properties.Settings.Default.TimeFormat);
            buffer = new ScrollingFrameBuffer(LEDFont.stringWidth(timeString));
            buffer.ViewOffset = -21;
            LEDFont.DrawString(ref buffer, 0, timeString);
        }

        public override void OnClick(object sender, EventArgs e)
        {
            Draw(ref LEDDisplayApp.frameBuffer);
            LEDDisplayApp.HandleClick(Title);
        }
    }
}
