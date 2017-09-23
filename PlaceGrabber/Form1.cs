using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlaceGrabber
{
    public partial class Form1 : Form
    {
        public static readonly HttpClient client = new HttpClient();
        public String apiKey = ItineraryData.getApiKey();

        List<RadarSearch> arrRadarSearch = new List<RadarSearch>();

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            string nama = textBox1.Text;
            string result = await doTaskAsync(RadarSearch.getRadarSearchQuery(apiKey, "-7.2756141", "112.6416444", "food"));
           // Console.WriteLine(result);

            JObject radarsearchResult = JObject.Parse(result);
            String radarsearchStatus = (String)radarsearchResult["status"];

            if(radarsearchStatus == "OK")
            {
                //radarsearch
                JArray radarsearchArray = (JArray)radarsearchResult["results"];
                //progressBar1.Maximum = radarsearchArray.Count;

                foreach (var item in radarsearchArray)
                {
                    String radarsearchLatitude = (String)item["geometry"]["location"]["lat"];
                    String radarsearchLongitude = (String)item["geometry"]["location"]["lng"];
                    String radarsearchPlaceID = (String)item["place_id"];

                    RadarSearch radarsearchObj = new RadarSearch(radarsearchLatitude, radarsearchLongitude, radarsearchPlaceID);
                    arrRadarSearch.Add(radarsearchObj);

                    Console.WriteLine(radarsearchObj);

                    //progressBar1.Value++;
                }

                //placedetails
                byte idx = 0;
                String placedetailsStatus = "";
                do
                {

                } while (idx<10);
            }

            listBox1.Items.Add(arrRadarSearch.Count.ToString());
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        public async Task<string> doTaskAsync(string url)
        {
            string responseString = await client.GetStringAsync(url);
            return responseString;
        }
    }
}
