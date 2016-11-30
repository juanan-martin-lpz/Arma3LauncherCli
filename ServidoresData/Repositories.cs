using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace ServidoresData
{
    public class Repositories : INotifyPropertyChanged, IDisposable
    {
        DirectoryInfo basepath;
        RepositoryBay bay;

        Dictionary<string, Repository> repositoryList;

        List<RepositoryProxy> repolight;
        private string webrepository = "http://188.165.254.137/WebRepo";

        public Repositories()
        {
            repositoryList = new Dictionary<string, Repository>();
            repolight = new List<RepositoryProxy>();

            bay = new RepositoryBay();

            try
            {
                

                WinWebDownload dl = new WinWebDownload();
                dl.DownloadFile(webrepository, @"/repositories.json", bay.ToDirectoryInfo().FullName + @"\repositories.json");

                string json = File.ReadAllText(bay.ToDirectoryInfo().FullName + @"\repositories.json");

                repolight = JsonConvert.DeserializeObject<List<RepositoryProxy>>(json);

                double oldsum = 0;
                double newsum = 0;

                foreach (RepositoryProxy r in repolight)
                {
                    string repofolder = bay.GetDirectoryForRepo(r.Nombre).FullName;
                    string repo = r.Nombre;

                    WinWebDownload dltstamp = new WinWebDownload();

                    if (File.Exists(repofolder + @"\timestamp_" + repo + @".txt"))
                    {
                        File.Copy(repofolder + @"\timestamp_" + repo + @".txt", repofolder + @"\timestamp_" + repo + @".old");
                        File.Delete(repofolder + @"\timestamp_" + repo + @".txt");
                    }
                    else
                    {
                        r.MustUpdate = true;
                        dltstamp.DownloadFile(webrepository, repo + @"/timestamp.txt", repofolder + @"\timestamp_" + repo + @".txt");
                        continue;
                    }

                    dltstamp.DownloadFile(webrepository, repo + @"/timestamp.txt", repofolder + @"\timestamp_" + repo + @".txt");

                    if (File.Exists(repofolder + @"\timestamp_" + r.Nombre + @".old"))
                    {
                        oldsum = System.Convert.ToDouble(File.ReadAllText(bay.GetDirectoryForRepo(r.Nombre).FullName + @"\timestamp_" + r.Nombre + @".old"));
                    }

                    newsum = System.Convert.ToDouble(File.ReadAllText(repofolder + @"\timestamp_" + r.Nombre + @".txt"));

                    r.MustUpdate = (oldsum <= newsum) ? true : false;

                    if (File.Exists(repofolder + @"\timestamp_" + repo + @".old"))
                    {
                        File.Delete(repofolder + @"\timestamp_" + repo + @".old");
                    }
                }
            }
            catch
            {
                repolight = new List<RepositoryProxy>();
            }

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
                Repository re = new Repository(r.FullName, bay, r.Name);
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


        public List<RepositoryProxy> RepositoryProxyList
        {
            get { return repolight; }
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

        public List<Repository> RepositoryList
        {
            get { return repositoryList.Values.ToList<Repository>(); }
            
        }
    }
}
