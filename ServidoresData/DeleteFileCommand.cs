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

        public override void Execute()
        {

            f.Delete();

            completedargs.Message = f.Name + @" eliminado con exito";
            OnDeleteFileCompleted();
        }
        
        private void OnDeleteFileCompleted()
        {
            if (DeleteFileCommandCompleted != null)
            {
                DeleteFileCommandCompleted(this, completedargs);
            }
        }
    }
}
