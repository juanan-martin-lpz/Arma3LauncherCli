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
            try
            {
                wd = new PortableWebDownload(Server, Repo, Filename, Target);
                wd.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
                wd.DownloadProgressChanged += new ProgressChangedEventHandler(ProgressChanged);

                //wd.WebDownloadProgressChanged += new ProgressChangedEventHandler(OnWebDownloadProgressChange);

                _prg = (Progress<int>)p;
            }
            catch (Exception ex)
            {
                _finished = true;
                Progreso = 100;
                throw ex;
            }       

        }

        public override void Execute()
        {
            beforeexecargs.Server = wd.Server;
            beforeexecargs.Repo = wd.Repository;
            beforeexecargs.SourceFilename = wd.Filename;
            beforeexecargs.TargetFile = new FileInfo(wd.Target);

            OnDownloadFileBeforeExecute();

            try
            {
                Task<bool> t1 = wd.DownloadAsync().ContinueWith<bool>((prev) =>
                {
                   bool result = prev.Result;

                   if (!result)
                   {
                       Progreso = 100;
                       NotifyPropertyChanged("Progreso");

                       _finished = true;

                       Console.WriteLine("La descarga del fichero {0} ha fallado", wd.Filename);
                       DownloadCompleted(this, new AsyncCompletedEventArgs(null, true, this));
                   }

                   Progreso = 100;

                    return result;
                });

                t1.Start();

                t1.Wait();

                
            }
            catch (Exception ex)
            {
                Progreso = 100;
                NotifyPropertyChanged("Progreso");

                _finished = true;

                Console.WriteLine("La descarga del fichero {0} ha fallado", wd.Filename);
            }                 
        }

        
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
            IProgress<int> prg = (IProgress<int>)_prg;

            Progreso = e.ProgressPercentage;
            NotifyPropertyChanged("Progreso");
            prg.Report(e.ProgressPercentage);
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //sem.Release();

            _finished = true;

            if (e.Cancelled)
            {
                completedargs = new CommandCompletedEventArgs(null, true, this);
                completedargs.Message = wd.Filename + @" ha fallado";

            }
            else
            {
                completedargs = new CommandCompletedEventArgs(null, false, this);
                completedargs.Message = wd.Filename + @" descargado con exito";
            }

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

        

        /*
        private void OnWebDownloadProgressChange(object sender, ProgressChangedEventArgs e)
        {
            
            IProgress<int> prg = _prg;

            prg.Report(e.ProgressPercentage);

        }
        */
    }
}
