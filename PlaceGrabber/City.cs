using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceGrabber
{
    class City
    {
        public String country { get; set; }
        public String name { get; set; }
        public String latitude { get; set; }
        public String longitude { get; set; }

        public City(String country, String name, String latitude, String longitude)
        {
            this.country = country;
            this.name = name;
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public override string ToString() {
            return name;
        }
    }

    class ListCity : List<City>
    {
        public ListCity()
        {
            StreamReader sr = new StreamReader("Data/city.txt");
            while (sr.Peek() != -1)
            {
                String[] data = sr.ReadLine().Split(';');
                Add(new City(data[0],data[1],data[2],data[3]));
            }
            sr.Close();
        }
    }

    class Country
    {
        public static List<String> getAllCountry()
        {
            List<String> listcountry = new List<String>();
            StreamReader sr = new StreamReader("Data/country.txt");
            while (sr.Peek() != -1)
            {
                listcountry.Add(sr.ReadLine());
            }
            sr.Close();
            return listcountry;
        }
    }
}
