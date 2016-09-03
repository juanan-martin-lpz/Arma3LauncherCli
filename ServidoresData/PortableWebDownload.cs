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

    class PortableWebDownload : IWebDownload
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

            using (HttpClient client = new HttpClient())
            {
                byte[] buffer;

                FileStream file = new FileStream(_target, FileMode.Create);

                int pos = 0;

                while (canDownload)
                {
                    buffer = await client.GetByteArrayAsyncWithProgress(URL.ToString(), onProgress, onFinish);

                    await file.WriteAsync(buffer, pos, buffer.Length);

                    pos += BufferSize;

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
            canDownload = false;
        }

        protected void setDownloadProgress(decimal BytesReaded, decimal TotalBytes, decimal Percentage)
        {

        }
    }
}
