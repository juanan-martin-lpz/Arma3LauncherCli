using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;

namespace ServidoresData
{
    public class RepositorySource
    {
        string _url;
        string _pwd_server;
        private ObservableCollection<Servidor> _servidores;

        private string webrepository = "http://188.165.254.137/WebRepo";
        DirectoryInfo servidores_path;


        public RepositorySource(string url, RepositoryBay Bay, string pwd_server)
        {
            _url = url;
            _pwd_server = pwd_server;

            servidores_path = Bay.ToDirectoryInfo();

            _servidores = new ObservableCollection<Servidor>();
        }

        public ObservableCollection<Servidor> Servidores
        {
            get { return _servidores; }
        }

        public string Address
        {
            get { return webrepository; }
        }

        public void Connect()
        {
            WebDownload dl = new WebDownload();
            dl.DownloadFile(webrepository, @"/servidores2.txt", servidores_path.Parent.FullName + @"\Servidores2.txt");
            readServidores();
        }

        public string GetRepositoryCatalog(string repo)
        {
            string nombre_bdd = Path.GetTempPath() + "ficheros_" + DateTime.Now.Ticks + ".db4o";

            WebDownload dl = new WebDownload();
            dl.DownloadFile(webrepository, repo + @"/ficheros.db4o", nombre_bdd);

            return nombre_bdd;
        }

        private void ServidoresCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //readServidores();
        }

        private void ModlistCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //readModList();
        }

        private void readServidores()
        {
            try
            {

                using (StreamReader r = new StreamReader(Path.Combine(servidores_path.Parent.FullName , "Servidores2.txt")))
                {
                    string l;

                    while (!r.EndOfStream)
                    {
                        l = r.ReadLine();

                        if (l.Length > 0)
                        {
                            if (!l.TrimStart().StartsWith("#"))
                            {
                                Servidor s = new Servidor(/* null */);

                                string[] items = l.Split('|');

                                s.Arma = items[0];
                                s.Nombre = items[1];
                                s.IP = items[2];
                                s.Puerto = items[3];
                                s.Password = _pwd_server;
                                s.Repository = items[4];

                                string[] lmods = items[5].Length > 0 ? items[5].Split(';') : null;

                                if (lmods != null)
                                {
                                    foreach (string m in lmods)
                                    {
                                        ServidoresData.Mod mo = new ServidoresData.Mod();
                                        mo.Nombre = m;
                                        s.ModList.Add(mo);
                                    }
                                }
                                _servidores.Add(s);
                            }
                        }
                    }
                }

            }
            catch
            {

            }
        }
    }
}
