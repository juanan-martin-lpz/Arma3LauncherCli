using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

namespace ServerManagementClient
{
    public class Mod : INotifyPropertyChanged
    {
        int version;
        string nombre;

        string path;

        string absolutePath;

        ReadOnlyCollection<ModFile> files;


        public Mod()
        {

        }
        
        public Mod(string p)
        {

            absolutePath = p;

            DirectoryInfo info = new DirectoryInfo(p);

            path = info.Name;

            var di = info.EnumerateFiles("*.*", SearchOption.AllDirectories);

            List<ModFile> lst = new List<ModFile>();


            foreach(FileInfo fi in di)
            {
                lst.Add(new ModFile() { Basename = fi.Name, RelativePath = Path.GetFileName(fi.DirectoryName), Size = fi.Length });                
            }

            files = new ReadOnlyCollection<ModFile>(lst);
        }

        
        public string Nombre
        {
            get
            {
                return nombre;
            }

            set
            {
                nombre = value; NotifyPropertyChanged("Nombre");
            }
        }

        public string RelativePath
        {
            get
            {
                return path;
            }

            set
            {
                path = value; NotifyPropertyChanged("RelativePath");
            }
        }

        public string AbsolutePath
        {
            get
            {
                return absolutePath;
            }
        }

        public int Version
        {
            get
            {
                return version;
            }

            set
            {
               version = value; NotifyPropertyChanged("Version");
            }
        }


        public ReadOnlyCollection<ModFile> Files
        {
            get
            {
                return files;
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

        public string FromRepository { get; set; }

        public bool Selected
        {
            get;
            set;
        }

        public Mod Clone()
        {
            Mod m = new Mod();
            m.RelativePath = this.RelativePath;
            m.Nombre = this.Nombre;
            m.Version = this.Version;

            return m;
        }

    }
}
