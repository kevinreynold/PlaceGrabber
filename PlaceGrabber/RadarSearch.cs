using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceGrabber
{
    class RadarSearch
    {
        //search
        public static String querySearch = "https://maps.googleapis.com/maps/api/place/radarsearch/json?";

        //result
        public String latitude, longitude;
        public String place_id;

        public RadarSearch(String latitude, String longitude, String place_id)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.place_id = place_id;
        }

        public override string ToString()
        {
            String res = "Location=" + latitude + "," + longitude + "&place_id=" + place_id;
            return res;
        }

        public static String getRadarSearchQuery(String apiKey, String latitudeParam, String longitudeParam, String typeParam=null, String radiusParam = "250000")
        {
            String result = "";
            result += querySearch;

            String location = "location=" + latitudeParam + "," + longitudeParam;
            String radius = "&radius=" + radiusParam;
            String type = "";
            if (typeParam != null) type = "&keyword=" + typeParam;
            String key = "&key=" + apiKey;

            result += location + radius + type + key;
            return result;
        }
    }
}
