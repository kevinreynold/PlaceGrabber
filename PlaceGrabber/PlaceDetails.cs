using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceGrabber
{
    class PlaceDetails
    {
        //search
        public static String querySearch = "https://maps.googleapis.com/maps/api/place/details/json?";

        //result
        public String name;
        public String placeid;
        public String address;
        public String phonenumber;
        public float rating;
        public ListOpeningHours opening_hours = new ListOpeningHours();
        public List<String> types = new List<String>();
        public String url;

        public PlaceDetails(String name, String placeid, String address, String phonenumber, float rating, ListOpeningHours opening_hours, List<String> types, String url)
        {
            this.name = name;
            this.placeid = placeid;
            this.address = address;
            this.phonenumber = phonenumber;
            this.rating = rating;
            this.opening_hours = opening_hours;
            this.types = types;
            this.url = url;
        }

        public override string ToString()
        {
            String res = "";
            res += "Place_ID\t\t: " + placeid;
            res += "\nName\t\t: " + name;
            res += "\nAddress\t\t: " + address;
            res += "\nPhone Number\t: " + phonenumber;
            res += "\nRating\t\t: " + rating;
            res += "\nUrl\t\t: " + url;
            res += "\n\nOpening Hours\t:";
            for (int i = 0; i < opening_hours.Count; i++)
            {
                if (i == 3)
                    res += "\n - " + opening_hours[i].day + "\t: " + opening_hours[i].open + " - " + opening_hours[i].close;
                else
                    res += "\n - " + opening_hours[i].day + "\t: " + opening_hours[i].open + " - " + opening_hours[i].close;
            }
            res += "\n\nTypes         : ";
            for (int j = 0; j < types.Count; j++)
            {
                res += "\n - " + types[j];
            }
            return res;
        }

        public static String getPlaceDetailsQuery(String apiKey, String place_id)
        {
            String result = "";
            result += querySearch;
            
            String key = "key=" + apiKey;
            String id = "&placeid=" + place_id;

            result += key + id;
            return result;
        }
    }

    class OpeningHours
    {
        public int day_index;
        public String day;
        public String open, close;

        public OpeningHours(int index)
        {
            day_index = index;
            day = new DateTime(2017, 10, day_index + 1).DayOfWeek.ToString();
            open = close = "0";
        }
    }

    class ListOpeningHours : List<OpeningHours>
    {
        public ListOpeningHours(){
            for (int i = 0; i<7; i++)
			{
                Add(new OpeningHours(i));
			}    
        }
    }
}
