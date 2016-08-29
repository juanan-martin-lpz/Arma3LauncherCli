using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ServidoresData
{
    public class DeleteModCommand : CommandBase
    {
        public event CommandCompletedEventHandler DeleteModCommandCompleted;
        public event CommandBeforeExecuteEventHandler DeleteModBeforeExecute;

        DirectoryInfo target;

        public DeleteModCommand(DirectoryInfo moddir, IProgress<int> prg):base(prg)
        {
            target = moddir;   
        }

        public override void Execute()
        {
            beforeexecargs.TargetDirectory = target;
            beforeexecargs.Mod = target.Name;

            OnDeleteModBeforeExecute();

            IEnumerable<FileInfo> files = target.EnumerateFiles("*", SearchOption.AllDirectories);
            int count = files.Count();
            int i = 0;

            IProgress<int> prg = (IProgress<int>)_prg;
            foreach(FileInfo f in files)
            {
                i++;
                
                prg.Report((i * 100) / count);
                f.Delete();
            }

            completedargs.Message = i.ToString() + @"/" + count.ToString() + " archivos eliminados con exito";
            OnDeleteModCompleted();
        }

        private void OnDeleteModCompleted()
        {
            if (DeleteModCommandCompleted != null)
            {
                DeleteModCommandCompleted(this, completedargs);
            }
        }

        private void OnDeleteModBeforeExecute()
        {
            if (DeleteModBeforeExecute != null)
            {
                DeleteModBeforeExecute(this, beforeexecargs);
            }
        }
    }
}
