using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Net.Http;


namespace ServidoresData
{

    public class PortableWebDownload : IWebDownload
    {
        public const int BufferSize = 0x2000;

        public const int LargeBufferSize = BufferSize * 1024;

        public Stopwatch sw;

        private string _fname;
        private long _progress;
        private string _server;
        private string _repo;
        private string _target;

        int bufferLength = 1024 * 1024;

        bool canDownload = true;

        
        DownloadAsyncProgressChangedEventArgs e;

        static AutoResetEvent block = new AutoResetEvent(true);

        public event AsyncCompletedEventHandler DownloadFileCompleted;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event ProgressChangedEventHandler WebDownloadProgressChanged;

        public PortableWebDownload()
        {
            sw = new Stopwatch();
            _progress = 0;
        }

        public PortableWebDownload(string Srv, string Repo, string FName, string target)
        {
            sw = new Stopwatch();
            _progress = 0;
            _fname = FName;
            _server = Srv;
            _repo = Repo;
            _target = target;

            canDownload = true;
        }

        public string Filename
        {
            get { return _fname; }
            set { }
        }

        public string Server
        {
            get { return _server; }
        }

        public string Repository
        {
            get { return _repo; }
        }

        public long Progress
        {
            get { return _progress; }
            set { }
        }

        public string Target
        {
            get { return _target; }
            set { }
        }

        public void Download()
        {
            throw new NotImplementedException();
        }

        public async void DownloadAsync()
        {
            string urlAddress = _server + "/" + _repo + "/" + _fname;

            Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

            Action<decimal, decimal, decimal> onProgress = setDownloadProgress;
            Action onFinish = setDownloadFinished;

            Console.WriteLine("Preparando descarga...");

            using (HttpClient client = new HttpClient())
            {
                byte[] buffer;

                FileStream file = new FileStream(_target, FileMode.Create);

                int pos = 1;

                Console.WriteLine("Creado el FileStream....");

                canDownload = true;

                try
                {

                    while (canDownload)
                    {

                        Console.WriteLine("Leyendo....");

                        buffer = await client.GetByteArrayAsyncWithProgress(URL.ToString(), onProgress, onFinish);

                        Console.WriteLine("Leidos {0} bytes", buffer.Length);

                        canDownload = (buffer.Length == 0) ? false : true;

                        await file.WriteAsync(buffer, 0, buffer.Length);

                        pos += buffer.Length;

                        Console.WriteLine("Escritos {0} bytes en {1}", buffer.Length, _target);

                    }

                    file.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ha ocurrido un error al leer el fichero {0} : {1} ", _fname, ex.Message);
                    throw;
                }
            }
        }

        public void DownloadFile(string urlAddress, string repodb, string location)
        {
            throw new NotImplementedException();
        }

        public async void DownloadFileAsync(string urlAddress, string repodb, string location)
        {
            string url = urlAddress + @"/" + repodb + @"/" + location;

            Action<decimal, decimal, decimal> onProgress = setDownloadProgress;
            Action onFinish = setDownloadFinished;

            using (HttpClient client = new HttpClient())
            {

                  await client.GetByteArrayAsyncWithProgress(url, onProgress,onFinish);  
            }

        }   

        protected void setDownloadFinished()
        {
            //canDownload = false;
        }

        protected void setDownloadProgress(decimal BytesReaded, decimal TotalBytes, decimal Percentage)
        {

        }
    }
}
