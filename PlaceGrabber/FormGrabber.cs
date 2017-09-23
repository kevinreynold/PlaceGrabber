using CsvHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlaceGrabber
{
    public partial class FormGrabber : Form
    {
        public static readonly HttpClient client = new HttpClient();
        public String apiKey = ItineraryData.getApiKey()[0];

        List<String> listcountry = Country.getAllCountry();
        ListCity listcity = new ListCity();
        List<RadarSearch> arrRadarSearch = new List<RadarSearch>();
        List<PlaceDetails> arrPlaceDetails = new List<PlaceDetails>();

        List<ItineraryData> arrData = new List<ItineraryData>();

        String city, country;
        String latitude, longitude;
        String types;

        public int offset;
        public int limit;

        public FormGrabber()
        {
            InitializeComponent();
        }

        private void FormGrabber_Load(object sender, EventArgs e)
        {
            setListBoxTypes();
            cbCountry.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbCountry.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbCity.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbCity.AutoCompleteMode = AutoCompleteMode.Suggest;

            cbCountry.Items.AddRange(listcountry.OrderBy(c => c).ToArray());
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are You Sure to Proceed ?", "Confirmation Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (checkInput())
                {
                    progressBar1.Value = 0;
                    richTextBox1.Clear();
                    arrRadarSearch.Clear();
                    arrPlaceDetails.Clear();
                    arrData.Clear();

                    country = cbCountry.Text;
                    city = cbCity.Text;
                    latitude = cbCity.SelectedValue.ToString().Split(';')[0];
                    longitude = cbCity.SelectedValue.ToString().Split(';')[1];
                    types = getAllTypes();

                    offset = (int)numericUpDownOffset.Value;
                    limit = (int)numericUpDownLimit.Value;
                    try
                    {
                        groupBox1.Enabled = false;
                        //radarsearch               
                        //lookingForConnectionAsync();
                        String radarsearchResult = await doTaskAsync(RadarSearch.getRadarSearchQuery(apiKey, latitude, longitude, types));

                        String dir_log = getDateTimeDirectory(); 
                        String dir = dir_log + "_" + city + "_" + types;
                        Directory.CreateDirectory("Images/" + dir);

                        JObject radarsearchJSON = JObject.Parse(radarsearchResult);
                        String radarsearchStatus = (String)radarsearchJSON["status"];

                        if (radarsearchStatus == "OK")
                        {
                            JArray radarsearchArray = (JArray)radarsearchJSON["results"];
                            if (checkBox1.Checked && offset == 0) progressBar1.Maximum = radarsearchArray.Count * 3;
                            else progressBar1.Maximum = radarsearchArray.Count + (limit * 2);

                            foreach (var item in radarsearchArray)
                            {
                                String radarsearchLatitude = (String)item["geometry"]["location"]["lat"];
                                String radarsearchLongitude = (String)item["geometry"]["location"]["lng"];
                                String radarsearchPlaceID = (String)item["place_id"];

                                RadarSearch radarsearchObj = new RadarSearch(radarsearchLatitude, radarsearchLongitude, radarsearchPlaceID);
                                arrRadarSearch.Add(radarsearchObj);

                                //Console.WriteLine(radarsearchObj);

                                progressBar1.Value++;
                            }

                            richTextBox1.AppendText("Starting fetch " + arrRadarSearch.Count.ToString() + " items...\n");

                            //placedetails
                            byte idx = 0;
                            if (checkBox1.Checked) limit = radarsearchArray.Count - offset;
                            String placedetailsStatus = "";
                            do
                            {
                                if (offset + limit > radarsearchArray.Count)
                                {
                                    richTextBox1.AppendText("Invalid Input...\n");
                                    break;
                                }

                                String placeid = arrRadarSearch[idx + offset].place_id;
                                //lookingForConnectionAsync();
                                String placedetailsResult = await doTaskAsync(PlaceDetails.getPlaceDetailsQuery(apiKey, placeid));
                                JObject placedetailsJSON = JObject.Parse(placedetailsResult);
                                placedetailsStatus = (String)placedetailsJSON["status"];

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

                                        //photo
                                        String photoreference;
                                        PlacePhoto photo;

                                        try
                                        {
                                            photoreference = (String)placedetailsJSON["result"]["photos"][0]["photo_reference"];
                                            //Console.WriteLine(PlacePhoto.getPlacePhotoQuery(apiKey, photoreference));
                                            photo = new PlacePhoto(placeid, dir);
                                            String save_directory = "Images/" + dir + "/" + placeid + ".jpg";
                                            //lookingForConnectionAsync();
                                            PlacePhoto.SaveImage(apiKey, photoreference, save_directory);
                                            progressBar1.Value++;
                                        }
                                        catch (Exception)
                                        {
                                            Console.WriteLine("No Photos");
                                            photo = new PlacePhoto();
                                            progressBar1.Value++;
                                        }

                                        PlaceDetails placedetailsOBJ = new PlaceDetails(placedetailsName, placeid, placedetailsAddress, placedetailsPhoneNumber, placedetailsRating, placedetailsOpeningHours, placedetailsTypes, placedetailsURL);
                                        arrPlaceDetails.Add(placedetailsOBJ);
                                        //Console.WriteLine(placedetailsOBJ);

                                        ItineraryData data = new ItineraryData(arrRadarSearch[idx + offset], placedetailsOBJ, photo, city, country);
                                        arrData.Add(data);
                                        richTextBox1.AppendText("Success fetch item - " + (idx + offset).ToString() + "...\n");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                        //progressBar1.Value++;
                                        richTextBox1.AppendText("Fail fetch item - " + (idx + offset).ToString() + "...\n");
                                    }
                                }
                                else
                                {
                                    break;
                                }

                                progressBar1.Value++;
                                ++idx;
                            } while (idx < limit && placedetailsStatus == "OK");
                        }

                        //foreach (ItineraryData item in arrData)
                        //{
                        //    Console.WriteLine(item);
                        //}

                        exportCSV("Result/" + dir + ".csv", arrData);
                        savetoLog(dir_log, city, country, arrRadarSearch.Count.ToString(), offset, limit, types);

                        progressBar1.Value = progressBar1.Maximum;
                        richTextBox1.AppendText("Done...\n");

                        System.Media.SoundPlayer player = new System.Media.SoundPlayer("alert.wav");
                        player.Play();
                    }
                    catch (Exception)
                    {
                        richTextBox1.AppendText("No Connection...\n");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Input!!");
                }

            }
            groupBox1.Enabled = true;
            setListBoxTypes();
        }

        private void btnType_Click(object sender, EventArgs e)
        {
            if (lbType.Items.Contains(tbType.Text))
                MessageBox.Show("Same Value Detected!");
            else
                lbType.Items.Add(tbType.Text);

            tbType.Clear();
            tbType.Focus();
        }

        public async Task<string> doTaskAsync(string url)
        {
            string responseString = await client.GetStringAsync(url);
            return responseString;
        }

        public String getDateTimeDirectory()
        {
            return DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Hour.ToString().PadLeft(2, '0') + "." + DateTime.Now.Minute.ToString().PadLeft(2, '0') + "." + DateTime.Now.Second.ToString().PadLeft(2, '0');
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownLimit.Enabled = (checkBox1.Checked) ? false : true;
        }

        public async void lookingForConnectionAsync()
        {
            while (!checkConnection())
            {
                richTextBox1.AppendText("No Connection...\n");
                await Task.Delay(500);
            }
        }

        public bool checkConnection()
        {
            Ping myPing = new Ping();
            String host = "8.8.8.8";
            byte[] buffer = new byte[32];
            int timeout = 1000;
            PingOptions pingOptions = new PingOptions();
            PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            return false;
        }

        private void savetoLog(String time, String city, String country, String fetch, int offset, int limit, String types)
        {
            StreamWriter writer = new StreamWriter("log.txt", true);
            writer.WriteLine(time + ";" + city + ";" + country + ";" + fetch + "(" + offset.ToString() + "-" + (offset + limit).ToString() + ");" + types);
            writer.Close();
        }

        private void exportCSV(String filename, List<ItineraryData> data)
        {
            using (var sw = new StreamWriter(filename,false,System.Text.Encoding.UTF8))
            {
                var writer = new CsvWriter(sw);
                writer.WriteHeader(typeof(ItineraryData));
                foreach (ItineraryData item in data)
                {
                    writer.WriteRecord(item);
                }
            }
            richTextBox1.AppendText("Successfully Saved Data...\n");
        }

        private void cbCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            String country = cbCountry.SelectedItem.ToString();
            var query = (from city in listcity
                         where city.country == country
                         orderby city.name
                         select new { city.name, location = city.latitude + ";" + city.longitude }).ToList();

            cbCity.DataSource = query;
            cbCity.DisplayMember = "name";
            cbCity.ValueMember = "location";
        }

        private void lbType_DoubleClick(object sender, EventArgs e)
        {
            int idx = lbType.SelectedIndex;
            lbType.Items.RemoveAt(idx);
        }

        public String getAllTypes()
        {
            String res = "";
            int idx = 0;
            foreach (String item in lbType.Items)
            {
                if (idx++ > 0) res += ",";
                res += item;
            }
            return res;
        }

        private void imagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFolder("Images");
        }

        private void resultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFolder("Result");
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile("log.txt");
        }

        public bool checkInput()
        {
            if (lbType.Items.Count > 0 && !string.IsNullOrEmpty(numericUpDownOffset.Text) && !string.IsNullOrEmpty(numericUpDownLimit.Text) && cbCity.SelectedItem != null && cbCountry.SelectedItem != null)
                return true;
            return false;
        }

        private void aPIKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(apiKey, "API Key", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenFile(string folderPath)
        {
            folderPath = Application.StartupPath + "\\" + folderPath;
            ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", folderPath);
            Process.Start(startInfo);
        }

        private void placeDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormDetails f = new FormDetails();
            f.Show();
        }
        
        private void cityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormDistanceMatrix f = new FormDistanceMatrix();
            f.Show();
        }

        private void oneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormDistanceMatrixOne f = new FormDistanceMatrixOne();
            f.Show();
        }

        private void OpenFolder(string folderPath)
        {
            folderPath = Application.StartupPath + "\\" + folderPath;
            //MessageBox.Show(folderPath);
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", folderPath);
                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show(string.Format("{0} Directory does not exist!", folderPath));
            }
        }

        private void setListBoxTypes()
        {
            lbType.Items.Clear();
            lbType.Items.Add("food");
            lbType.Items.Add("restaurant");
            lbType.Items.Add("tourist+attraction");
            lbType.Items.Add("point+of+interest");
            lbType.Items.Add("hotels");
            lbType.Items.Add("lodging");
        }
    }
}
