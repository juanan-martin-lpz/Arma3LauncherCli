using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Data.HashFunction;

namespace ServerManager
{
    public class ItemCatalogo
    {
        public string Name { get; set; }
        public string Basename { get; set; }
        public string FullName { get; set; }
        public string Hash { get; set; }
        public long Size { get; set; }

        public ItemCatalogo()
        {

        }

        public ItemCatalogo(string fullname)
        {
            Name = Path.GetFileName(fullname);
            FullName = fullname;
            Basename = Path.GetDirectoryName(FullName);
            FileInfo f = new FileInfo(FullName);
            Size = f.Length;
            //

            //
        }

        /*
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
                 */
    }
}
