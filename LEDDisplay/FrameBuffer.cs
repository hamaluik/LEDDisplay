using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEDDisplay
{
    class FrameBuffer
    {
        protected bool[] buffer = new bool[21 * 7];
        protected byte brightness = 15;

        public virtual bool Get(int x, int y)
        {
            if (x < 0 || x >= 21 || y < 0 || y >= 7)
                return false;
            return buffer[x + (y * 21)];
        }

        public virtual bool Set(int x, int y, bool on)
        {
            if (x < 0 || x >= 21 || y < 0 || y >= 7)
                return false;
            return (buffer[x + (y * 21)] = on);
        }

        protected byte LED8(bool[] x)
        {
            byte ret = 0xff;
            for (int i = 0; i < 8; i++)
            {
                if (x[i])
                    ret &= (byte)(~(1 << i));
            }

            return ret;
        }

        public virtual void FillPacket(int row, ref byte[] packet)
        {
            // our report ID must be 0
            packet[0] = 0;

            // set our brightness
            packet[1] = brightness;

            // set our row
            packet[2] = (byte)row;

            // do our first row
            packet[5] = LED8(new bool[] { Get(0, row), Get(1, row), Get(2, row), Get(3, row), Get(4, row), Get(5, row), Get(6, row), Get(7, row) });
            packet[4] = LED8(new bool[] { Get(8, row), Get(9, row), Get(10, row), Get(11, row), Get(12, row), Get(13, row), Get(14, row), Get(15, row) });
            packet[3] = LED8(new bool[] { Get(16, row), Get(17, row), Get(18, row), Get(19, row), Get(20, row), false, false, false });

            // our second row
            row++;
            packet[8] = LED8(new bool[] { Get(0, row), Get(1, row), Get(2, row), Get(3, row), Get(4, row), Get(5, row), Get(6, row), Get(7, row) });
            packet[7] = LED8(new bool[] { Get(8, row), Get(9, row), Get(10, row), Get(11, row), Get(12, row), Get(13, row), Get(14, row), Get(15, row) });
            packet[6] = LED8(new bool[] { Get(16, row), Get(17, row), Get(18, row), Get(19, row), Get(20, row), false, false, false });
        }
    }
}
