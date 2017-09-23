using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceGrabber
{
    class ItineraryData
    {
        public String latitude { get; set; }
        public String longitude { get; set; }

        public String city { get; set; }
        public String country { get; set; }
        public String name { get; set; }
        public String placeid { get; set; }
        public String address { get; set; }
        public String phonenumber { get; set; }
        public float rating { get; set; }

        public ListOpeningHours opening_hours = new ListOpeningHours();
        public String Sunday { get; set; }
        public String Monday { get; set; }
        public String Tuesday { get; set; }
        public String Wednesday { get; set; }
        public String Thursday { get; set; }
        public String Friday { get; set; }
        public String Saturday { get; set; }

        public String types { get; set; }
        public String url { get; set; }

        public String photo_name { get; set; }
        public String extension { get; set; }
        public String dir { get; set; }
        public String timestamp { get; set; }

        public ItineraryData(RadarSearch radarsearch, PlaceDetails placedetails, PlacePhoto placephoto, String city="", String country="")
        {
            opening_hours = new ListOpeningHours();

            latitude = radarsearch.latitude;
            longitude = radarsearch.longitude;

            this.city = city;
            this.country = country;

            name = placedetails.name;
            placeid = placedetails.placeid;
            address = placedetails.address;
            phonenumber = placedetails.phonenumber;
            rating = placedetails.rating;
            opening_hours = placedetails.opening_hours;

            byte idx = 0;
            Sunday = (opening_hours[idx].close == "0" && opening_hours[idx].open == "0") ? "Closed" : opening_hours[idx].open + "-" + opening_hours[idx].close; idx++;
            Monday = (opening_hours[idx].close == "0" && opening_hours[idx].open == "0") ? "Closed" : opening_hours[idx].open + "-" + opening_hours[idx].close; idx++;
            Tuesday = (opening_hours[idx].close == "0" && opening_hours[idx].open == "0") ? "Closed" : opening_hours[idx].open + "-" + opening_hours[idx].close; idx++;
            Wednesday = (opening_hours[idx].close == "0" && opening_hours[idx].open == "0") ? "Closed" : opening_hours[idx].open + "-" + opening_hours[idx].close; idx++;
            Thursday = (opening_hours[idx].close == "0" && opening_hours[idx].open == "0") ? "Closed" : opening_hours[idx].open + "-" + opening_hours[idx].close; idx++;
            Friday = (opening_hours[idx].close == "0" && opening_hours[idx].open == "0") ? "Closed" : opening_hours[idx].open + "-" + opening_hours[idx].close; idx++;
            Saturday = (opening_hours[idx].close == "0" && opening_hours[idx].open == "0") ? "Closed" : opening_hours[idx].open + "-" + opening_hours[idx].close; idx++;

            types = "";
            foreach (String type in placedetails.types)
            {
                types += type + ";";
            }

            url = placedetails.url;

            photo_name = placephoto.name;
            extension = placephoto.extension;
            dir = placephoto.dir;
            
            this.timestamp = DateTime.Now.ToString();
        }

        public override string ToString()
        {
            return name;
        }

        public static List<String> getApiKey()
        {
            StreamReader sr = new StreamReader("Data/apikey.txt");
            List<String> key = new List<String>();
            while (sr.Peek() != -1)
            {
                key.Add(sr.ReadLine());
            }
            sr.Close();
            return key;
        }
    }
}
