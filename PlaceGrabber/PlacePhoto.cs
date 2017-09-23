using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PlaceGrabber
{
    class PlacePhoto
    {
        //search
        public static String querySearch = "https://maps.googleapis.com/maps/api/place/photo?";

        //result
        public String name;
        public String extension;
        public String dir;

        public PlacePhoto(String name, String dir)
        {
            this.name = name;
            this.extension = "jpg";
            this.dir = dir;
        }

        public PlacePhoto()
        {
            this.name = "nowhere";
            this.extension = "jpg";
            this.dir = "";
        }

        public override string ToString()
        {
            return "";
        }

        public String fileName()
        {
            return name + "." + extension;
        }

        public static String getPlacePhotoQuery(String apiKey, String photoreferenceParam, String maxwidthParam="1024")
        {
            String result = "";
            result += querySearch;

            String maxwidth = "maxwidth=" + maxwidthParam;
            String radius = "&photoreference=" + photoreferenceParam;
            String key = "&key=" + apiKey;

            result += maxwidth + radius + key;
            return result;
        }

        public static void SaveImage(String apiKey, String photoreference, String filename)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(getPlacePhotoQuery(apiKey,photoreference));
            Bitmap bitmap; bitmap = new Bitmap(stream);

            if (bitmap != null)
                bitmap.Save(filename, ImageFormat.Jpeg);

            stream.Flush();
            stream.Close();
            client.Dispose();
        }
    }
}
