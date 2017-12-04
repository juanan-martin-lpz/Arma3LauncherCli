using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Data.HashFunction;

namespace ServerManagementClient
{
    public class ModFile : INotifyPropertyChanged
    {

        string basename;
        string relativePath;
        long size;
        string hash;


        public string Basename
        {
            get
            {
                return basename;
            }

            set
            {
                basename = value; NotifyPropertyChanged("Basename");
            }
        }

        public string RelativePath
        {
            get
            {
                return relativePath;
            }

            set
            {
                relativePath = value; NotifyPropertyChanged("RelativePath");
            }
        }


        public long Size
        {
            get
            {
                return size;
            }

            set
            {
                size = value; NotifyPropertyChanged("Size");
            }
        }


        public string Hash
        {
            get
            {
                return hash;
            }

            set
            {
                hash = value; NotifyPropertyChanged("Hash");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void ComputeHash()
        {

            try
            {
                string firma_base64 = "";

                xxHash firma = null;

                if (firma == null) { firma = new xxHash(); }

                //using (MD5 firma = MD5.Create())
                using (FileStream streamFichero = File.OpenRead(basename))
                {
                    byte[] f = firma.ComputeHash(streamFichero);
                    firma_base64 = Convert.ToBase64String(f);
                }

                hash = firma_base64;
            }
            catch (Exception x)
            {
                throw x;
            }
        }
    }
}
