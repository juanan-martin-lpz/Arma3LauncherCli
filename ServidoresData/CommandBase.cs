using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.IO;

namespace ServidoresData
{
    public class CommandCompletedEventArgs : AsyncCompletedEventArgs
    {
        public string Message { get; set; }

        public CommandCompletedEventArgs(Exception error, bool cancelled, object userState) : base(error, cancelled, userState)
        {

        }
    }

    public class CommandBeforeExecuteEventArgs : AsyncCompletedEventArgs
    {
        public DirectoryInfo TargetDirectory;
        public FileInfo TargetFile;
        public string SourceFilename;
        public string Mod;
        public string Repo;
        public string Server;

        public string Message { get; set; }

        public CommandBeforeExecuteEventArgs(Exception error, bool cancelled, object userState) : base(error, cancelled, userState)
        {

        }
    }

    public abstract class CommandBase : INotifyPropertyChanged
    {

        private int _progreso;

        public delegate void CommandCompletedEventHandler(object sender, CommandCompletedEventArgs e);
        public delegate void CommandBeforeExecuteEventHandler(object sender, CommandBeforeExecuteEventArgs e);
        
        protected CommandCompletedEventArgs completedargs;
        protected CommandBeforeExecuteEventArgs beforeexecargs;
        protected Progress<int> _prg;

        protected bool _finished;

        public string Description { get; set; }

        public CommandBase(IProgress<int> prg)
        {
            completedargs = new CommandCompletedEventArgs(null, false, null);
            beforeexecargs = new CommandBeforeExecuteEventArgs(null, false, null);
            _prg = (Progress<int>) prg;

            _finished = false;
        }

        public virtual void Execute()
        {

        }
        
        public Progress<int> Progress
        {
            get { return _prg; }
            set { _prg = value; }
        }

        public int Progreso
        {
            get { return _progreso; }
            set { _progreso = value;  NotifyPropertyChanged("Progreso"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool Finished
        {
            get { return _finished; }
        }

    }
}
