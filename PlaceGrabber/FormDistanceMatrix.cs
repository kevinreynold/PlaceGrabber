using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlaceGrabber
{
    public partial class FormDistanceMatrix : Form
    {
        public static readonly HttpClient client = new HttpClient();
        List<DistanceMatrix> place_data = new List<DistanceMatrix>();
        List<MatrixData> matrix_data = new List<MatrixData>();
        int totalRequest = 0;

        public FormDistanceMatrix()
        {
            InitializeComponent();
        }

        private void FormDistanceMatrix_Load(object sender, EventArgs e)
        {
            cbKey.DataSource = ItineraryData.getApiKey();
            cbKey.SelectedIndex = 1;
            cbMode.SelectedIndex = 1;

            openFileDialog1.InitialDirectory = Application.StartupPath;
            openFileDialog1.Filter = "CSV Files| *.csv|All Files| *.*";
            place_data = ReadListofPlaces();
            tbTotalData.Text = place_data.Count.ToString();
            totalRequest = getTotalRequest(place_data.Count, (int)numericMaxGrab.Value);
            tbTotalRequest.Text = totalRequest.ToString();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are You Sure to Proceed ?", "Confirmation Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (checkInput(tbTitle.Text) && totalRequest < 2400)
                {
                    groupBox1.Enabled = false;
                    richTextBox1.Clear();
                    matrix_data.Clear();
                    progressBar1.Value = 0;
                    progressBar1.Maximum = totalRequest;

                    String title = tbTitle.Text;
                    String mode = cbMode.SelectedItem.ToString();
                    String apiKey = cbKey.SelectedItem.ToString();

                    try
                    {
                        int request = 0;
                        int originOffset = 0;
                        int destOffset = 0;
                        int MAXIMUM_COUNTER = place_data.Count;
                        int MAXIMUM_DEST = (int)numericMaxGrab.Value;

                        while (originOffset < MAXIMUM_COUNTER)
                        {
                            while (destOffset < MAXIMUM_COUNTER)
                            {
                                int DEST_COUNTER = (destOffset + MAXIMUM_DEST > MAXIMUM_COUNTER) ? MAXIMUM_COUNTER : (destOffset + MAXIMUM_DEST);

                                List<String> origin = new List<String>();
                                origin.Add(place_data[originOffset].placeid);

                                List<String> destinations = new List<String>();
                                for (int i = destOffset; i < DEST_COUNTER; i++)
                                {
                                    if (i != originOffset) destinations.Add(place_data[i].placeid);
                                }

                                String query = DistanceMatrix.getDistanceMatrixQuery(apiKey, origin, destinations, mode);

                                //richTextBox1.AppendText(query + "\n");

                                //START JSON PARSING
                                String distanceMatrixResult = await doTaskAsync(query);

                                JObject distanceMatrixJSON = JObject.Parse(distanceMatrixResult);
                                String distanceMatrixStatus = (String)distanceMatrixJSON["status"];

                                if (distanceMatrixStatus == "OK")
                                {
                                    JArray distanceMatrixArray = (JArray)distanceMatrixJSON["rows"][0]["elements"];
                                    int idx = 0;
                                    foreach (var item in distanceMatrixArray)
                                    {
                                        String status = (String)item["status"];
                                        String curOrigin = place_data[originOffset].placeid;
                                        String curDest = destinations[idx].Split(':')[1];
                                        if (status == "OK")
                                        {
                                            int distance, traveltime, fare;
                                            String currency;

                                            if (item["distance"] != null)
                                                distance = (int)item["distance"]["value"];
                                            else
                                                distance = -1;

                                            if (item["duration"] != null)
                                                traveltime = (int)item["duration"]["value"];
                                            else
                                                traveltime = -1;

                                            if (item["fare"] != null)
                                            {
                                                fare = (int)item["fare"]["value"];
                                                currency = (String)item["fare"]["currency"];
                                            }
                                            else
                                            {
                                                fare = -1;
                                                currency = "NONE";
                                            }

                                            MatrixData data = new MatrixData(curOrigin, curDest, distance, traveltime, currency, fare);
                                            matrix_data.Add(data);
                                        }
                                        else
                                        {
                                            MatrixData data = new MatrixData(curOrigin, curDest);
                                            matrix_data.Add(data);
                                        }
                                        idx++;
                                    }
                                }
                                else
                                {
                                    break;
                                }

                                richTextBox1.AppendText("Request-" + (request + 1).ToString() + " Success... \n");
                                richTextBox1.ScrollToCaret();
                                destOffset = DEST_COUNTER;
                                request++;
                                progressBar1.Value++;
                            }
                            destOffset = 0;
                            originOffset++;
                        }

                        savetoLog(title);
                        exportCSV("Matrix/City/" + getDateTimeDirectory() + "_" + title + ".csv", matrix_data);

                        richTextBox1.AppendText("Done...\n");
                        System.Media.SoundPlayer player = new System.Media.SoundPlayer("alert.wav");
                        player.Play();
                    }
                    catch (Exception)
                    {
                        if (checkConnection()) richTextBox1.AppendText("INVALID REQUEST...\n");
                        else richTextBox1.AppendText("No Connection...\n");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Input!!");
                }
            }
            groupBox1.Enabled = true;
            tbTitle.Text = "";
        }

        public List<DistanceMatrix> ReadListofPlaces(String filename = "Data/contoh.csv")
        {
            List<DistanceMatrix> result = new List<DistanceMatrix>();
            using (var sr = new StreamReader(filename, System.Text.Encoding.UTF8))
            {
                var csv = new CsvReader(sr);
                csv.Configuration.Delimiter = "\t";
                while (csv.Read())
                {
                    String city = csv.GetField<String>(2);
                    String country = csv.GetField<String>(3);
                    String name = csv.GetField<String>(4);
                    String placeid = csv.GetField<String>(5);
                    String address = csv.GetField<String>(6);
                    DistanceMatrix temp = new DistanceMatrix(city, country, name, placeid, address);
                    result.Add(temp);
                }
            }
            return result;
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                String oldFileName = textBox1.Text;
                try
                {
                    String fileName = openFileDialog1.FileName;
                    textBox1.Text = fileName;
                    place_data = ReadListofPlaces(fileName);
                    totalRequest = getTotalRequest(place_data.Count, (int)numericMaxGrab.Value);
                    tbTotalData.Text = place_data.Count.ToString();
                    tbTotalRequest.Text = totalRequest.ToString();
                }
                catch (Exception)
                {
                    textBox1.Text = oldFileName;
                    place_data = ReadListofPlaces(oldFileName);
                    MessageBox.Show("Invalid CSV Files!!!");
                }
            }
        }

        public async Task<string> doTaskAsync(string url)
        {
            string responseString = await client.GetStringAsync(url);
            return responseString;
        }

        public bool checkInput(String text)
        {
            if (text.Trim().Length > 0)
                return true;
            return false;
        }

        public int getTotalRequest(int data, int max_grab) => data * (int)(Math.Ceiling((double)(data - 1) / max_grab));

        public String getDateTimeDirectory()
        {
            return DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Hour.ToString().PadLeft(2, '0') + "." + DateTime.Now.Minute.ToString().PadLeft(2, '0') + "." + DateTime.Now.Second.ToString().PadLeft(2, '0');
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

        private void savetoLog(String title)
        {
            StreamWriter writer = new StreamWriter("Matrix/log.txt", true);
            writer.WriteLine("City;" + title + "; " + getDateTimeDirectory());
            writer.Close();
        }

        private void exportCSV(String filename, List<MatrixData> data)
        {
            using (var sw = new StreamWriter(filename, false, System.Text.Encoding.UTF8))
            {
                var writer = new CsvWriter(sw);
                writer.WriteHeader(typeof(MatrixData));
                foreach (MatrixData item in data)
                {
                    writer.WriteRecord(item);
                }
            }
            richTextBox1.AppendText("Successfully Saved Data...\n");
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

        private void OpenFile(string folderPath)
        {
            folderPath = Application.StartupPath + "\\" + folderPath;
            ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe", folderPath);
            Process.Start(startInfo);
        }

        private void coba()
        {
            string json = @"{
                            'CPU': 'Intel',
                            'PSU': '500W',
                            'Drives': [
                              'DVD read/writer'
                              /*(broken)*/,
                              '500 gigabyte hard drive',
                              '200 gigabype hard drive'
                            ]
                            }";
            JObject reader = JObject.Parse(json);
            String test = (String)reader["CPUU"];
            if (test == null)
            {
                MessageBox.Show("NULL");
            }
            else
            {
                MessageBox.Show("NOT NULL");
            }
        }

        private void exampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCalculateMatrix f = new FormCalculateMatrix();
            f.Show();
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile("Matrix\\log.txt");
        }

        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFolder("Matrix");
        }

        private void cobaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            coba();
        }
    }
}
