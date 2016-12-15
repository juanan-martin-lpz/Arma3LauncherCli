using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ServidoresData
{
    public class DeleteFileCommand : CommandBase 
    {
        public event CommandCompletedEventHandler DeleteFileCommandCompleted;
        FileInfo f;

        public DeleteFileCommand(FileInfo file,IProgress<int> prg) : base(prg)
        {
            f = file;
        }

        public DirectoryInfo Path
        {
            get
            {
                return f.Directory;
            }
        }

        public override void Execute()
        {

            f.Delete();

            Progreso = 100;

            completedargs.Message = f.Name + @" eliminado con exito";

            _finished = true;

            OnDeleteFileCompleted();
        }
        
        private void OnDeleteFileCompleted()
        {
            Progreso = 100;
            _finished = true;

            if (DeleteFileCommandCompleted != null)
            {
                DeleteFileCommandCompleted(this, completedargs);
            }
        }
    }
}
