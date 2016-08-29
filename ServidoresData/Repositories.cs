using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ServidoresData
{
    public class Repositories : INotifyPropertyChanged, IDisposable
    {
        DirectoryInfo basepath;
        RepositoryBay bay;

        Dictionary<string, Repository> repositoryList;

        public Repositories()
        {
            repositoryList = new Dictionary<string, Repository>();
        }

        // Necesitamos al menos la carpeta base
        public Repositories(string from)
        {
            basepath = new DirectoryInfo(from);

            if (!basepath.Exists)
            {
                throw new System.IO.DriveNotFoundException("La carpeta especificada no existe");
            }

            repositoryList = new Dictionary<string, Repository>();

            bay = new RepositoryBay();

            var allRepos = from DirectoryInfo repo in basepath.GetDirectories() select repo;

            
            foreach (DirectoryInfo r in allRepos)
            {
                Console.WriteLine(r.FullName);
                Repository re = new Repository(r.FullName, bay, r.Name, null);
                Console.WriteLine(re.Nombre);

                repositoryList.Add(r.Name,re);                
            }

            Console.WriteLine(from);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Repository GetRepository(string Name)
        {
            if (repositoryList != null)
            {
                if (repositoryList.ContainsKey(Name))
                {
                    return repositoryList[Name];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public Repository this[string Name]
        {
            get
            {
                return GetRepository(Name);
            }
        }

        public virtual void Dispose()
        {
            foreach(Repository r in repositoryList.Values)
            {
                r.CloseAll();
            }

            GC.SuppressFinalize(this);
        }
    }
}
