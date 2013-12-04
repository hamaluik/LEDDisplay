using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace LEDDisplay
{
    class DisplayProvider_GMail : DisplayProvider
    {
        public static new string Title
        {
            get
            {
                return "GMail";
            }
        }

        public override void Draw(ref ScrollingFrameBuffer buffer)
        {
            try
            {
                // get our url
                string url = "https://mail.google.com/mail/feed/atom";
                WebRequest request = WebRequest.Create(url);
                NetworkCredential cred = new NetworkCredential();
                cred.UserName = Properties.Settings.Default.GMailUserName;
                cred.Password = Properties.Settings.Default.GMailApplicationPassword;
                request.Credentials = cred;
                Stream urlStream = request.GetResponse().GetResponseStream();
                StreamReader streamReader = new StreamReader(urlStream);
                string xml = streamReader.ReadToEnd();
                urlStream.Close();
                streamReader.Close();

                // now parse our xml
                using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
                {
                    // get the unread count
                    reader.ReadToFollowing("fullcount");
                    int unread = reader.ReadElementContentAsInt();

                    string mailString = Properties.Settings.Default.GMailFormat;
                    mailString = mailString.Replace("$unread$", unread.ToString());

                    buffer = new ScrollingFrameBuffer(LEDFont.stringWidth(mailString));
                    buffer.ViewOffset = -21;
                    LEDFont.DrawString(ref buffer, 0, mailString);
                }
            }
            catch (System.Exception exc)
            {
                //MessageBox.Show("Error: " + exc.Message);
                string errorString = "GMail error: " + exc.Message;
                buffer = new ScrollingFrameBuffer(LEDFont.stringWidth(errorString));
                buffer.ViewOffset = -21;
                LEDFont.DrawString(ref buffer, 0, errorString);
            }
        }

        public override void OnClick(object sender, EventArgs e)
        {
            Draw(ref LEDDisplayApp.frameBuffer);
            LEDDisplayApp.HandleClick(Title);
        }
    }
}
