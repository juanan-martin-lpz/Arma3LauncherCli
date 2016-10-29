using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidoresData
{
    public class PWDProgressChangedEventArgs
    {
        public int BytesReaded { get; set; }
        public int TotalBytes { get; set; }
        public int Percentage { get; set; }


        public PWDProgressChangedEventArgs()
        {

        }

        public PWDProgressChangedEventArgs(int bytesreaded, int totalbytes, int percentage)
        {
            BytesReaded = bytesreaded;
            TotalBytes = totalbytes;
            Percentage = percentage;
        }
    }
}
