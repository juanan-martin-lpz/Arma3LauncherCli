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

    
    public class PortableWebDownload    // : IWebDownload
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

        //static SemaphoreSlim sem;

        //DownloadAsyncProgressChangedEventArgs e;
        //public event PortableDownloadProgressChangedEventHandler DownloadProgressChanged;
        public event ProgressChangedEventHandler DownloadProgressChanged;
        public PWDProgressChangedEventArgs ev;



        public event AsyncCompletedEventHandler DownloadFileCompleted;
        //public event DownloadProgressChangedEventHandler WDownloadProgressChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        public event ProgressChangedEventHandler WebDownloadProgressChanged;

        HttpDownload dw;

        Tuple<long, byte[]> result;

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

            //sem = new SemaphoreSlim(6);
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


        public async Task<bool> DownloadAsync()
        {
            string urlAddress = _server + "/" + _repo + "/" + _fname;

            Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

            Console.WriteLine("Preparando descarga...");

            FileInfo fi = new FileInfo(_target);

            DirectoryInfo di = new DirectoryInfo(fi.DirectoryName);

            if (!di.Exists)
            {
                di.Create();
            }

            try
            {
                dw = new HttpDownload(URL.ToString());
                dw.OnDownloadProgress += new HttpDownload.DownloadProgress(downloadProgress);
                dw.OnDownloadFinished += new HttpDownload.DownloadFinished(downloadFinished);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ha ocurrido un error al descargar el fichero {0} : {1} ", _fname, ex.Message);
                return false;
            }

            

            using (HttpClient client = new HttpClient())
            {
                FileStream file = new FileStream(_target, FileMode.Create);

                int pos = 1;

                Console.WriteLine("Creado el FileStream....");

                canDownload = true;

                try
                {
                    while (canDownload)
                    {

                        Console.WriteLine("Esperando respuesta de servidor");

                        result = await dw.GetByteArrayAsync();

                        Console.WriteLine("Leidos {0} bytes", result.Item1);

                        //canDownload = (result.Item1 == 0) ? false : true;

                        file.Write(result.Item2, 0, (int)result.Item1);

                        //await file.WriteAsync(result.Item2, 0, (int) result.Item1);

                        pos += (int)result.Item1;

                        Console.WriteLine("Escritos {0} bytes en {1}", result.Item1, _target);

                    }

                    file.Close();

                    setDownloadFinished();

                    return true;

                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ha ocurrido un error al leer el fichero {0} : {1} ", _fname, ex.Message);

                    file.Close();

                    fi.Delete();

                    canDownload = false;

                    downloadFinished(this, new AsyncCompletedEventArgs(ex, true, this));

                    //Console.WriteLine("Threads libres : {0}", 6 - sem.CurrentCount);

                    return false;
                    //throw;
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

            Action<int, int, int> onProgress = setDownloadProgress;
            Action onFinish = setDownloadFinished;

            using (HttpClient client = new HttpClient())
            {
                // onProgress,onFinish
                await client.GetByteArrayAsyncWithProgress(url, this);  
            }

        }   


        protected void downloadProgress(object sender, DownloadAsyncProgressChangedEventArgs e)
        {
            
            // Elevar el evento al UI
            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged(this, e);
            }
            //
            Console.WriteLine("Progreso de la descarga : {0} ({1}/{2})", _fname, e.ProgressPercentage, e.TotalBytes);

        }


        protected void downloadFinished(object sender, AsyncCompletedEventArgs e)
        {
            canDownload = false;

            Console.WriteLine("Se ha finalizado la descarga del archivo {0}", _fname);

            if (DownloadFileCompleted != null)
            {
                DownloadFileCompleted(this, e);
            }

            //sem.Release();

        }

        protected void setDownloadFinished()
        {
            canDownload = false;

            Console.WriteLine("Se ha finalizado la descarga del archivo {0}", _fname);

            AsyncCompletedEventArgs e = new AsyncCompletedEventArgs(null, false, this);

            if (DownloadFileCompleted != null)
            {
                DownloadFileCompleted(this, e);
            }

            //sem.Release();

        }

        
        
        protected void setDownloadProgress(int BytesReaded, int TotalBytes, int Percentage)
        {

            // Elevar el evento al UI

            /*
            if (ev == null)
            {
                ev = new PWDProgressChangedEventArgs();
            }

            ev.BytesReaded = BytesReaded;
            ev.TotalBytes = TotalBytes;
            ev.Percentage = Percentage;
            */

            ProgressChangedEventArgs e = new ProgressChangedEventArgs(Percentage, null);

            if (DownloadProgressChanged != null)
            {
                DownloadProgressChanged(this, e);
            }
            //
            Console.WriteLine("Progreso de la descarga : {0} ({1}/{2})", _fname,Percentage,BytesReaded,TotalBytes);

        }
    }
}
