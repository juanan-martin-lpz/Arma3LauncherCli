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
    public class FileHashXXHash : IFileHash
    {
        public string ComputeHash(string Filename)
        {
            try
            {
                string firma_base64 = "";

                xxHash firma = new xxHash(64); ;
                
                using (FileStream streamFichero = File.OpenRead(Filename))
                {
                    byte[] f = firma.ComputeHash(streamFichero);
                    firma_base64 = Convert.ToBase64String(f);
                }

                return firma_base64;
            }
            catch (Exception x)
            {
                throw x;
            }
        }

        public async Task<string> ComputeHashAsync(string Filename)
        {
            try
            {
                string firma_base64 = "";

                xxHash firma = new xxHash(64); ;

                using (FileStream streamFichero = File.OpenRead(Filename))
                {
                    byte[] f = await firma.ComputeHashAsync(streamFichero);
                    firma_base64 = Convert.ToBase64String(f);
                }

                return firma_base64;
            }
            catch (Exception x)
            {
                throw x;
            }
        }
    }
}
