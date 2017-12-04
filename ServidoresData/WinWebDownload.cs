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

namespace ServidoresData
{
    public class DownloadAsyncProgressChangedEventArgs : ProgressChangedEventArgs
    {
        public string Message { get; set; }

        public double Speed { get; set; }
        public double TotalBytes { get; set; }
        public double ActualBytes { get; set; }

        public DownloadAsyncProgressChangedEventArgs(int progressPercentage, object userState) : base(progressPercentage, userState)
        {
        }
    }

    public class WinWebDownload : INotifyPropertyChanged, IWebDownload
    {
        WebClient webClient;
        public Stopwatch sw;

        private string _fname;
        private long _progress;
        private string _server;
        private string _repo;
        private string _target;

        bool canDownload;

        

        public event AsyncCompletedEventHandler DownloadFileCompleted;
#pragma warning disable CS0067 // El evento 'WinWebDownload.DownloadProgressChanged' nunca se usa
        public event PortableDownloadProgressChangedEventHandler DownloadProgressChanged;
#pragma warning restore CS0067 // El evento 'WinWebDownload.DownloadProgressChanged' nunca se usa
        public event ProgressChangedEventHandler WebDownloadProgressChanged;

        DownloadAsyncProgressChangedEventArgs e;

        static AutoResetEvent block = new AutoResetEvent(true);

        public WinWebDownload()
        {
            sw = new Stopwatch();
            _progress = 0;
        }

        public WinWebDownload(string Srv,string Repo, string FName, string target)
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
            set {  }
        }

        public string Target
        {
            get { return _target; }
            set { }
        }

        public void Download()
        {

            if (canDownload)
            {

                using (webClient = new WebClient())
                {
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);


                    FileInfo fi = new FileInfo(_target);

                    DirectoryInfo di = new DirectoryInfo(fi.DirectoryName);


                    if (!di.Exists)
                    {
                        di.Create();
                    }
                    

                    string urlAddress = _server + "/" + _repo + "/" + _fname;


                    // The variable that will be holding the url address (making sure it starts with http://)
                    Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);
                    
                    // Start the stopwatch which we will be using to calculate the download speed
                    sw.Start();

                    try
                    {
                        // Start downloading the file

                        webClient.DownloadFile(URL, _target);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        public void DownloadAsync()
        {

            if (canDownload)
            {

                using (webClient = new WebClient())
                {
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                    e = new DownloadAsyncProgressChangedEventArgs(0, null);
                    

                    FileInfo fi = new FileInfo(_target);

                    DirectoryInfo di = new DirectoryInfo(fi.DirectoryName);


                    if (!di.Exists)
                    {
                        di.Create();
                    }


                    string urlAddress = _server + "/" + _repo + "/" + _fname;

                    e.Message = "Descargando " + _fname;
                    //DownloadAsyncProgressChanged(e);



                    // The variable that will be holding the url address (making sure it starts with http://)
                    Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                    // Start the stopwatch which we will be using to calculate the download speed
                    sw.Start();

                    try
                    {
                        // Start downloading the file

                        var syncObject = new Object();

                        lock (syncObject)
                        {
                            webClient.DownloadFileAsync(URL, _target, syncObject);  
                            Monitor.Wait(syncObject);
                        }

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }



        public void DownloadFile(string urlAddress, string repodb, string location)
        {
            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                _fname = repodb;

                NotifyPropertyChanged("Filename");

                urlAddress += "/" + repodb;


                // The variable that will be holding the url address (making sure it starts with http://)
                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                // Start the stopwatch which we will be using to calculate the download speed
                sw.Start();

                try
                {
                    // Start downloading the file
                    webClient.DownloadFile(URL, location);
                    webClient.Dispose();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void DownloadFileAsync(string urlAddress, string repodb, string location)
        {
            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                _fname = repodb;

                NotifyPropertyChanged("Filename");

                urlAddress += "/" + repodb;


                // The variable that will be holding the url address (making sure it starts with http://)
                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                // Start the stopwatch which we will be using to calculate the download speed
                sw.Start();

                try
                {
                    // Start downloading the file
                    webClient.DownloadFileAsync(URL, location);
                    webClient.Dispose();
                }
                catch
                {
                    throw;
                }
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

        private void OnProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadAsyncProgressChangedEventArgs v = new DownloadAsyncProgressChangedEventArgs(e.ProgressPercentage, null);

            v.Speed = (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds);

            v.ActualBytes = e.BytesReceived;
            v.TotalBytes = e.TotalBytesToReceive;

            _progress = e.ProgressPercentage;
            NotifyPropertyChanged("Progress");

            DownloadAsyncProgressChanged(v);
        }


 
        private void OnCompleted(object sender, AsyncCompletedEventArgs e)
        {

            if (DownloadFileCompleted != null)
            {
                DownloadFileCompleted(sender, e);
            }
        }

        // The event that will fire whenever the progress of the WebClient is changed
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnProgressChanged(sender, e);   
        }

        // The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
 
            // Reset the stopwatch.
            sw.Reset();

            lock (e.UserState)
            {
               Monitor.Pulse(e.UserState);
            }

            OnCompleted(sender, e);

        }

        private void DownloadAsyncProgressChanged(DownloadAsyncProgressChangedEventArgs e)
        {
            if (WebDownloadProgressChanged != null)
            {
                WebDownloadProgressChanged(this, e);
            }
        }
    }
}
