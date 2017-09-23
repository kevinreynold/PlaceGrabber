using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlaceGrabber
{
    public partial class FormDetails : Form
    {
        String apiKey;
        public static readonly HttpClient client = new HttpClient();

        public FormDetails()
        {
            InitializeComponent();
            apiKey = ItineraryData.getApiKey()[0];
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            button1.Enabled = false;
            String placeid = textBox1.Text;
            textBox1.Clear();
            richTextBox1.Clear();

            //lookingForConnectionAsync();
            String placedetailsResult = await doTaskAsync(PlaceDetails.getPlaceDetailsQuery(apiKey, placeid));
            JObject placedetailsJSON = JObject.Parse(placedetailsResult);
            String placedetailsStatus = (String)placedetailsJSON["status"];

            if (placedetailsStatus == "OK")
            {
                try
                {
                    String placedetailsName = (String)placedetailsJSON["result"]["name"];
                    String placedetailsAddress = (String)placedetailsJSON["result"]["formatted_address"];
                    String placedetailsPhoneNumber = (String)placedetailsJSON["result"]["international_phone_number"];
                    String placedetailsURL = (String)placedetailsJSON["result"]["url"];
                    float placedetailsRating = (float)placedetailsJSON["result"]["rating"];

                    JArray placedetailsOpeningHoursArray;
                    ListOpeningHours placedetailsOpeningHours = new ListOpeningHours();
                    try
                    {
                        placedetailsOpeningHoursArray = (JArray)placedetailsJSON["result"]["opening_hours"]["periods"];
                        foreach (var openinghours in placedetailsOpeningHoursArray)
                        {
                            int day_index = (int)openinghours["close"]["day"];
                            String close = (String)openinghours["close"]["time"];
                            String open = (String)openinghours["open"]["time"];
                            placedetailsOpeningHours[day_index].close = close;
                            placedetailsOpeningHours[day_index].open = open;
                        }
                    }
                    catch (Exception)
                    {
                        placedetailsOpeningHoursArray = null;
                        for (int i = 0; i < placedetailsOpeningHours.Count; i++)
                        {
                            placedetailsOpeningHours[i].open = "0000";
                            placedetailsOpeningHours[i].close = "2400";
                        }
                    }

                    JArray placedetailsArray = (JArray)placedetailsJSON["result"]["types"];
                    List<String> placedetailsTypes = new List<String>();
                    foreach (var type in placedetailsArray)
                    {
                        placedetailsTypes.Add((String)type);
                    }

                    PlaceDetails placedetailsOBJ = new PlaceDetails(placedetailsName, placeid, placedetailsAddress, placedetailsPhoneNumber, placedetailsRating, placedetailsOpeningHours, placedetailsTypes, placedetailsURL);
                    richTextBox1.AppendText(placedetailsOBJ.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            button1.Enabled = true;
        }

        public async Task<string> doTaskAsync(string url)
        {
            string responseString = await client.GetStringAsync(url);
            return responseString;
        }

        private void FormDetails_Load(object sender, EventArgs e)
        {
        }
    }
}
