using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using NLog;
using NLog.Targets;

namespace ServerManagementClient
{
    public class Repositories : INotifyPropertyChanged
    {
        DirectoryInfo basepath;
        RepositoryBay bay;

        Dictionary<string, RepositoryProxy> repositoryList;

        List<RepositoryProxy> repolight;
        private string webrepository = "http://188.165.254.137/WebRepo";

        string nombre;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Repositories()
        {
            repositoryList = new Dictionary<string, RepositoryProxy>();
            repolight = new List<RepositoryProxy>();
        }

        public async Task<bool> DownloadRepositoriesJson()
        {
            bay = new RepositoryBay();

            try
            {

                string url = $"{webrepository}/repositories.json";
                string target = $"{bay.ToDirectoryInfo().FullName}\\repositories.json";


                await DownloadAsync(url, target);

                string json = File.ReadAllText(bay.ToDirectoryInfo().FullName + @"\repositories.json");

                repolight = JsonConvert.DeserializeObject<List<RepositoryProxy>>(json);

                return true;

                //double oldsum = 0;
                //double newsum = 0;

                /*
                foreach (RepositoryProxy r in repolight)
                {
                    string repofolder = bay.GetDirectoryForRepo(r.Nombre).FullName;
                    string repo = r.Nombre;

                    WinWebDownload dltstamp = new WinWebDownload();

                    if (!File.Exists(repofolder + @"\timestamp_" + repo + @".txt"))
                    {
                        r.MustUpdate = true;
                        dltstamp.DownloadFile(webrepository, repo + @"/timestamp.txt", repofolder + @"\timestamp_" + repo + @".txt");
                        continue;
                    }

                    if (File.Exists(repofolder + @"\timestamp_" + repo + @".txt"))
                    {
                        File.Copy(repofolder + @"\timestamp_" + repo + @".txt", repofolder + @"\timestamp_" + repo + @".old");
                        File.Delete(repofolder + @"\timestamp_" + repo + @".txt");
                    }
                    else
                    {
                    }


                    dltstamp.DownloadFile(webrepository, repo + @"/timestamp.txt", repofolder + @"\timestamp_" + repo + @".new");
                    
                    oldsum = System.Convert.ToDouble(File.ReadAllText(bay.GetDirectoryForRepo(r.Nombre).FullName + @"\timestamp_" + r.Nombre + @".txt"));
                    
                    newsum = System.Convert.ToDouble(File.ReadAllText(repofolder + @"\timestamp_" + r.Nombre + @".new"));

                    r.MustUpdate = (newsum > oldsum) ? true : false;

                    if (r.MustUpdate)
                    {
                        File.Copy(repofolder + @"\timestamp_" + repo + @".txt", repofolder + @"\timestamp_" + repo + @".old");
                        File.Delete(repofolder + @"\timestamp_" + repo + @".old");
                        File.Copy(repofolder + @"\timestamp_" + repo + @".new", repofolder + @"\timestamp_" + repo + @".txt");
                        File.Delete(repofolder + @"\timestamp_" + repo + @".new");
                    }
                    else
                    {
                        File.Delete(repofolder + @"\timestamp_" + repo + @".new");
                    }
                }
                */
            }
            catch
            {
                repolight = new List<RepositoryProxy>();
                return false;
            }

        }

        private async Task<bool> DownloadAsync(string url, string target)
        {
            using (var client = new HttpClientDownload(url, target))
            {
                try
                {
                    logger.Info($"Descargando el fichero {url} en {target} ");
                    await client.StartDownload();
                    return true;

                }
                catch (Exception e)

                {
                    logger.Error($"La descarga del fichero {url} ha fallado");
                    logger.Error($"{e.Message}");
                    throw;
                }
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

            repositoryList = new Dictionary<string, RepositoryProxy>();

            bay = new RepositoryBay();

            var allRepos = from DirectoryInfo repo in basepath.GetDirectories() select repo;

            
            foreach (DirectoryInfo r in allRepos)
            {
                RepositoryProxy re = new RepositoryProxy(r.FullName, r.Name);
                
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

        public RepositoryProxy GetRepository(string Name)
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

        public RepositoryProxy this[string Name]
        {
            get
            {
                return GetRepository(Name);
            }
        }

        
        public List<RepositoryProxy> RepositoryList
        {
            get { return repositoryList.Values.ToList<RepositoryProxy>(); }
            
        }
    }
}
