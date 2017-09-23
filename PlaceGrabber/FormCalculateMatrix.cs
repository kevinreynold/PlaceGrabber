using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlaceGrabber
{
    public partial class FormCalculateMatrix : Form
    {
        public FormCalculateMatrix()
        {
            InitializeComponent();
        }

        private void FormCalculateMatrix_Load(object sender, EventArgs e)
        {
            cbKey.DataSource = ItineraryData.getApiKey();
            cbKey.SelectedIndex = 1;
            cbMode.SelectedIndex = 1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();

            String key = cbKey.SelectedItem.ToString();
            List<String> origin = new List<String>();
            origin.Add(tbOrigin.Text);
            List<String> destination = new List<String>();
            destination.Add(tbDestination.Text);
            String mode = cbMode.SelectedItem.ToString();

            String query = DistanceMatrix.getDistanceMatrixQuery(key, origin, destination, mode);
            richTextBox1.AppendText(query);
        }
    }
}
