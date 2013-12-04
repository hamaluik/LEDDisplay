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
    class DisplayProvider_Weather : DisplayProvider
    {
        public static new string Title
        {
            get
            {
                return "Weather";
            }
        }

        public override void Draw(ref ScrollingFrameBuffer buffer)
        {
            try
            {
                // get our url
                string url = "http://api.openweathermap.org/data/2.5/weather?q=" + Properties.Settings.Default.WeatherLocation + "&units=" + Properties.Settings.Default.WeatherUnits + "&mode=xml";
                WebRequest request = WebRequest.Create(url);
                Stream urlStream = request.GetResponse().GetResponseStream();
                StreamReader streamReader = new StreamReader(urlStream);
                string xml = streamReader.ReadToEnd();
                urlStream.Close();
                streamReader.Close();

                // now parse our XML
                using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
                {
                    // get the current temperature
                    reader.ReadToFollowing("temperature");
                    reader.MoveToFirstAttribute();
                    string tempString = reader.Value;
                    float temperature = float.Parse(tempString);

                    // get the humidity
                    reader.ReadToFollowing("humidity");
                    reader.MoveToFirstAttribute();
                    string humidityString = reader.Value;
                    float humidity = float.Parse(humidityString);

                    // wind speed
                    reader.ReadToFollowing("speed");
                    reader.MoveToFirstAttribute();
                    string speedString = reader.Value;
                    float windSpeed = float.Parse(speedString);

                    // wind direction
                    reader.ReadToFollowing("direction");
                    reader.MoveToAttribute("code");
                    string direction = reader.Value;

                    // clouds
                    reader.ReadToFollowing("clouds");
                    reader.MoveToAttribute("name");
                    string clouds = reader.Value;

                    string weatherString = Properties.Settings.Default.WeatherFormat;//"Temp: " + Math.Round(temperature, 1) + "`C Hum: " + humidity + "% Wind: " + Math.Round(windSpeed, 1) + "km/h " + direction + " Clouds: " + clouds;
                    weatherString = weatherString.Replace("$temperature$", Math.Round(temperature, 1).ToString());
                    weatherString = weatherString.Replace("$humidity$", humidity.ToString());
                    weatherString = weatherString.Replace("$windSpeed$", Math.Round(windSpeed, 1).ToString());
                    weatherString = weatherString.Replace("$windDirection$", direction);
                    weatherString = weatherString.Replace("$clouds$", clouds);

                    buffer = new ScrollingFrameBuffer(LEDFont.stringWidth(weatherString));
                    buffer.ViewOffset = -21;
                    LEDFont.DrawString(ref buffer, 0, weatherString);
                }
            }
            catch (System.Exception exc)
            {
                string errorString = "Weather error: " + exc.Message;
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
