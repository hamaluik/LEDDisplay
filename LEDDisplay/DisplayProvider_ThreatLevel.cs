using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LEDDisplay
{
    class DisplayProvider_ThreatLevel : DisplayProvider
    {
        public static new string Title
        {
            get
            {
                return "Threat Level";
            }
        }

        public override void Draw(ref ScrollingFrameBuffer buffer)
        {
            //DateTime time = DateTime.Now;
            string threatString = "";
            System.Net.WebClient wc = new System.Net.WebClient();
            string rawData = wc.DownloadString("https://msisac.cisecurity.org/alert-level/");
            if (rawData.Contains(@"<img src=""https://msisac.cisecurity.org/alert-level/images/low.gif"""))
            {
                threatString = "Threat level: Low";
            }else
            {
                if (rawData.Contains(@"<img src=""https://msisac.cisecurity.org/alert-level/images/guarded.gif"""))
                {
                    threatString = "Threat level: Guarded";
                }else
                {
                    if (rawData.Contains(@"<img src=""https://msisac.cisecurity.org/alert-level/images/elevated.gif"""))
                    {
                        threatString = "Threat level: Elevated";
                    }else
                    {
                        if (rawData.Contains(@"<img src=""https://msisac.cisecurity.org/alert-level/images/high.gif"""))
                        {
                            threatString = "Threat level: High";
                        }else
                        {
                            if (rawData.Contains(@"<img src=""https://msisac.cisecurity.org/alert-level/images/severe.gif"""))
                            {
                                threatString = "Threat level: Severe";
                            }else
                            {
                                threatString = "Threat level: Unknown";
                            }
                        }
                    }
                }
            }
            buffer = new ScrollingFrameBuffer(LEDFont.stringWidth(threatString));
            buffer.ViewOffset = -21;
            LEDFont.DrawString(ref buffer, 0, threatString);
        }

        public override void OnClick(object sender, EventArgs e)
        {
            Draw(ref LEDDisplayApp.frameBuffer);
            LEDDisplayApp.HandleClick(Title);
        }
    }
}
