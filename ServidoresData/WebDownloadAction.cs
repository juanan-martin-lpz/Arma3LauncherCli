using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.ComponentModel;

namespace ServidoresData
{
    public class WebDownloadAction
    {
        private string repo;
        private string filetodownload;
        private string filetocreate;

        private WinWebDownload wd;

        IProgress<int> prg;

        public WebDownloadAction(string URL, string file, string destination)
        {
            repo = URL;
            filetodownload = file;
            filetocreate = destination;

            wd = new WinWebDownload();

            wd.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnProgressChanged);
            wd.DownloadFileCompleted += new AsyncCompletedEventHandler(OnCompleted);

        }

        public string Title
        {
            get { return filetodownload; }
        }

        public void Execute(IProgress<int> Progress)
        {
            prg = Progress;

            wd.DownloadFile(repo, filetodownload, filetocreate);
        }

        private void OnProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            prg.Report(e.ProgressPercentage);            
        }



        private void OnCompleted(object sender, AsyncCompletedEventArgs e)
        {
            
        }
    }
}
