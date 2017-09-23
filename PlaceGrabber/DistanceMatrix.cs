using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceGrabber
{
    public class DistanceMatrix
    {
        //search
        public static String querySearch = "https://maps.googleapis.com/maps/api/distancematrix/json?";

        //result
        public String city { get; set; }
        public String country { get; set; }
        public String name { get; set; }
        public String placeid { get; set; }
        public String address { get; set; }

        public DistanceMatrix(String city, String country, String name, String placeid, String address)
        {
            this.city = city;
            this.country = country;
            this.name = name;
            this.placeid = placeid;
            this.address = address;
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static String getDistanceMatrixQuery(String apiKey, List<String> origins, List<String> destinations, String modes, String unit = "metric", bool arrival_time = true, bool departure_time = false)
        {
            for (int i = 0; i < origins.Count; i++)
            {
                origins[i] = "place_id:" + origins[i];
            }

            for (int i = 0; i < destinations.Count; i++)
            {
                destinations[i] = "place_id:" + destinations[i];
            }

            String result = "";
            result += querySearch;

            String units = "unit=" + unit;
            String origin = "&origins=";
            for (int i = 0; i < origins.Count; i++)
            {
                origin += origins[i];
                if (i != origins.Count - 1) origin += "|";
            }
            String destination = "&destinations=";
            for (int i = 0; i < destinations.Count; i++)
            {
                destination += destinations[i];
                if (i != destinations.Count - 1) destination += "|";
            }
            String mode = "&mode=" + modes;
            String arrival = arrival_time ? "&arrival_time=" + getDiffTime().ToString() : "";
            String departure = departure_time ? "&departure_time=" + getDiffTime().ToString() : "";
            String key = "&key=" + apiKey;

            result += units + origin + destination + mode + arrival + departure + key;
            return result;
        }

        public static int getDiffTime(int hours = 0, int minutes = 0, int seconds = 0)
        {
            DateTime awal = new DateTime(1970, 1, 1, 0, 0, 0);
            DateTime akhir = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, hours, minutes, seconds);
            akhir.AddDays(1);
            //if (DateTime.Now.TimeOfDay > akhir.TimeOfDay) akhir.AddDays(1);

            return (int)akhir.Subtract(awal).TotalSeconds;
        }
    }
}
