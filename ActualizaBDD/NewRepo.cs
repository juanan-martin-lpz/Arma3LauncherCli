using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ActualizaBDD
{
    public class FileMD5
    {
        public FileInfo info {get; set;}
        public string MD5 {get; set;}

        public FileMD5(FileInfo i, string md5)
        {
            info = i;
            MD5 = md5;
        }
    }

    public class NewRepo
    {
        public NewRepo()
        {

        }

        public void index(string parentFolder,string prefix = "")
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(parentFolder);

            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            var result = fileList.Select(f => new FileMD5(f,Utiles.calcula_firma_MD5(f.FullName)));

            
            
            foreach (FileMD5 f in result)
            {
                Console.Write(f.info.FullName + "-" + f.MD5 + "\n" );
            }
            
        }


    }
}
