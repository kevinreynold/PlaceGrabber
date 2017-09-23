using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceGrabber
{
    class MatrixData
    {
        public String origin { get; set; } //dalam placeid
        public String destination { get; set; } //dalam placeid

        public int distance { get; set; } //dalam meter (-1 apabila tidak diketahui)
        public int traveltime { get; set; } //dalam detik

        public String currency { get; set; } //singkatan ("NONE" apabila tidak dketahui)
        public int fare { get; set; } //dalam satuan negara masing-masing (-1 apabila tidak diketahui)

        public MatrixData(String origin, String destination, int distance, int traveltime, String currency, int fare)
        {
            this.origin = origin;
            this.destination = destination;
            this.distance = distance;
            this.traveltime = traveltime;
            this.currency = currency;
            this.fare = fare;
        }

        public MatrixData(String origin, String destination)
        {
            this.origin = origin;
            this.destination = destination;
            this.distance = -1;
            this.traveltime = -1;
            this.currency = "NONE";
            this.fare = -1;
        }
    }
}
