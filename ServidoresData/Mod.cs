using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

namespace ServidoresData
{
    public class Mod : INotifyPropertyChanged
    {
        int version;
        string nombre;

        string path;

        string absolutePath;

        ReadOnlyCollection<ModFile> files;

        Repository repo;
		
        public string FromRepository { get; set; }

        public Mod()
        {
        	repo = null;
        }
        
        public Mod(Repository parent)
        {
            repo = parent;
        }

        public Mod(string p, Repository parent)
        {
            repo = parent;

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

        public Mod(string fromRepository)
        {
            FromRepository = fromRepository;
            repo = null;
        }
        
        public Repository Repository
        {
            get { return repo;  }
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


        public bool Selected
        {
            get;
            set;
        }
        public void Check() 
        { 
        
        }

        public void Publish()
        {
            /*
            DirectoryInfo di = Directory.CreateDirectory(repo.RelativePath + @"\" + this.RelativePath);

            foreach(ModFile file in files)
            {
                if (!Directory.Exists(di.Name + @"\" + file.RelativePath))
                {
                    Directory.CreateDirectory(di.Name + @"\" + file.RelativePath);
                }

                DirectoryInfo dst = new DirectoryInfo(di.Name + @"\" + file.RelativePath);

                FileStream fso = new FileStream(file.Basename,FileMode.Open);
                FileStream fsd = new FileStream(dst.Name + @"\" + file.Basename, FileMode.Open);

                
                fso.CopyTo(fsd);

                
                file.ComputeHash();    
            }
            */
        }

        public void Remove()
        {

        }

        public Mod Clone()
        {
            Mod m = new Mod(this.Repository);
            m.RelativePath = this.RelativePath;
            m.Nombre = this.Nombre;
            m.Version = this.Version;

            return m;
        }

    }
}
