using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LEDDisplay
{
    class ScrollingFrameBuffer
    {
        private int viewOffset = 0;
        public int ViewOffset
        {
            get
            {
                return viewOffset;
            }
            set
            {
                viewOffset = value;
                if (viewOffset > width)
                {
                    viewOffset = -21;
                }
            }
        }
        private int width = 21;
        private bool[] scrollingBuffer;
        private bool[] buffer = new bool[21 * 7];
        private byte brightness = 15;

        public ScrollingFrameBuffer(int width)
        {
            this.width = Math.Max(21, width);
            scrollingBuffer = new bool[width * 7];
        }

        public bool Get(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= 7)
                return false;
            return scrollingBuffer[x + (y * width)];
        }

        private bool GetRaw(int x, int y)
        {
            if (x < 0 || x >= 21 || y < 0 || y >= 7)
                return false;
            return buffer[x + (y * 21)];
        }

        public bool Set(int x, int y, bool on)
        {
            if (x < 0 || x >= width || y < 0 || y >= 7)
                return false;
            return (scrollingBuffer[x + (y * width)] = on);
        }

        private bool SetRaw(int x, int y, bool on)
        {
            if (x < 0 || x >= 21 || y < 0 || y >= 7)
                return false;
            return (buffer[x + (y * 21)] = on);
        }

        private void setViewport()
        {
            for (int xi = 0; xi < 21; xi++)
            {
                for (int y = 0; y < 7; y++)
                {
                    SetRaw(xi, y, Get(xi + viewOffset, y));
                }
            }
        }

        private byte LED8(bool[] x)
        {
            byte ret = 0xff;
            for (int i = 0; i < 8; i++)
            {
                if (x[i])
                    ret &= (byte)(~(1 << i));
            }

            return ret;
        }

        public void FillPacket(int row, ref byte[] packet)
        {
            // set our viewport
            setViewport();

            // our report ID must be 0
            packet[0] = 0;

            // set our brightness
            packet[1] = brightness;

            // set our row
            packet[2] = (byte)row;

            // do our first row
            packet[5] = LED8(new bool[] { GetRaw(0, row), GetRaw(1, row), GetRaw(2, row), GetRaw(3, row), GetRaw(4, row), GetRaw(5, row), GetRaw(6, row), GetRaw(7, row) });
            packet[4] = LED8(new bool[] { GetRaw(8, row), GetRaw(9, row), GetRaw(10, row), GetRaw(11, row), GetRaw(12, row), GetRaw(13, row), GetRaw(14, row), GetRaw(15, row) });
            packet[3] = LED8(new bool[] { GetRaw(16, row), GetRaw(17, row), GetRaw(18, row), GetRaw(19, row), GetRaw(20, row), false, false, false });

            // our second row
            row++;
            packet[8] = LED8(new bool[] { GetRaw(0, row), GetRaw(1, row), GetRaw(2, row), GetRaw(3, row), GetRaw(4, row), GetRaw(5, row), GetRaw(6, row), GetRaw(7, row) });
            packet[7] = LED8(new bool[] { GetRaw(8, row), GetRaw(9, row), GetRaw(10, row), GetRaw(11, row), GetRaw(12, row), GetRaw(13, row), GetRaw(14, row), GetRaw(15, row) });
            packet[6] = LED8(new bool[] { GetRaw(16, row), GetRaw(17, row), GetRaw(18, row), GetRaw(19, row), GetRaw(20, row), false, false, false });
        }
    }
}
