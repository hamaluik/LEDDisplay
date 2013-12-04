using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LEDDisplay
{
    class DisplayProvider
    {
        public static string Title
        {
            get
            {
                return "KDH";
            }
        }
        public MenuItem menuItem;

        public virtual void Draw(ref ScrollingFrameBuffer buffer)
        {

        }

        public virtual void OnClick(object sender, EventArgs e)
        {

        }
    }
}
