using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.HashFunction;

namespace ServerManager
{
    public class ItemCatalogo
    {
        private string rpath;

        public string Name { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string Basename { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string FullName { get; set; }

        public string Hash { get; set; }
        public long Size { get; set; }

        private ObservableCollection<ItemCatalogo> _items = new ObservableCollection<ItemCatalogo>();

        public ItemCatalogo()
        {
        }

        public ItemCatalogo(string fullname, ItemCatalogo parent)
        {
            Name = Path.GetFileName(fullname);
            FullName = fullname;
            Basename = Path.GetDirectoryName(FullName);
            FileInfo f = new FileInfo(FullName);

            if (parent != null)
            {
                rpath = fullname.Substring(parent.FullName.Length +1);               
            }
            else
            {
                rpath = ".";
            }

            if (!IsFolder(fullname))
            {
                Size = f.Length;
            }
            else
            {
                Size = 0;
            }

        }
         
        public string RelativePath
        {
            get
            {
                return rpath;
            }
            set
            {
                rpath = value;
            }
        }

        public ObservableCollection<ItemCatalogo> Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public bool IsFolder(string path)
        {
            return ((File.GetAttributes(FullName) & FileAttributes.Directory) == FileAttributes.Directory);
        }       

        public void ComputeHash()
        {
            try
            {
                string firma_base64 = "";

                xxHash firma = null;

                if (firma == null) { firma = new xxHash(); }

                //using (MD5 firma = MD5.Create())
                using (FileStream streamFichero = File.OpenRead(FullName))
                {
                    byte[] f = firma.ComputeHash(streamFichero);
                    firma_base64 = Convert.ToBase64String(f);
                }

                Hash = firma_base64;
            }
            catch (Exception x)
            {
                throw x;
            }
        }
    }
}
