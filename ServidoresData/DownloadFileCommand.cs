using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Net;
using System.IO;
using System.Threading;

namespace ServidoresData
{
    public class DownloadFileCommand : CommandBase
    {
        PortableWebDownload wd;

        public CommandCompletedEventHandler DownloadFileCommandCompleted;
        
        public event CommandBeforeExecuteEventHandler DownloadFileBeforeExecute;

        public DownloadFileCommand(string Server, string Repo, string Filename, string Target, IProgress<int> p) : base(p)
        {
            wd = new PortableWebDownload(Server, Repo, Filename, Target);
            wd.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
            wd.DownloadProgressChanged += new PortableDownloadProgressChangedEventHandler(ProgressChanged);
            wd.WebDownloadProgressChanged += new ProgressChangedEventHandler(OnWebDownloadProgressChange);


            _prg = (Progress<int>)p;

            
        }

        public override void Execute()
        {
            beforeexecargs.Server = wd.Server;
            beforeexecargs.Repo = wd.Repository;
            beforeexecargs.SourceFilename = wd.Filename;
            beforeexecargs.TargetFile = new FileInfo(wd.Target);

            OnDownloadFileBeforeExecute();


            //sem.Wait();

            wd.DownloadAsync();
            
        }

        
        private void ProgressChanged(object sender, PWDProgressChangedEventArgs e)
        {
            
            IProgress<int> prg = (IProgress<int>)_prg;

            
            prg.Report(e.Percentage);
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //sem.Release();
            completedargs.Message = wd.Filename + @" descargado con exito";
            OnDownloadFileCommandCompleted();
        }

        private void OnDownloadFileCommandCompleted()
        {
            if (DownloadFileCommandCompleted != null)
            {
                DownloadFileCommandCompleted(this, completedargs);
            }
        }

        private void OnDownloadFileBeforeExecute()
        {
            if (DownloadFileBeforeExecute != null)
            {
                DownloadFileBeforeExecute(this, beforeexecargs);
            }
        }

        private void OnWebDownloadProgressChange(object sender, ProgressChangedEventArgs e)
        {
            
            IProgress<int> prg = _prg;

            prg.Report(e.ProgressPercentage);

        }
    }
}
