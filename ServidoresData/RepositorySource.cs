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
        private ObservableCollection<ServidorMision> _servidoresMis;

        private string webrepository = "http://188.165.254.137/WebRepo";
        DirectoryInfo servidores_path;
        RepositoryBay _bay;

        public RepositorySource(string url, RepositoryBay Bay, string pwd_server)
        {
            _url = url;
            _pwd_server = pwd_server;


            _bay = Bay;

            servidores_path = Bay.ToDirectoryInfo();

            _servidores = new ObservableCollection<Servidor>();
            _servidoresMis = new ObservableCollection<ServidorMision>();
        }

        public ObservableCollection<Servidor> Servidores
        {
            get { return _servidores; }
        }

        public ObservableCollection<ServidorMision> ServidoresMision
        {
            get { return _servidoresMis; }
        }

        public string Address
        {
            get { return webrepository; }
        }

        public void Connect()
        {
            WinWebDownload dl = new WinWebDownload();
            dl.DownloadFile(webrepository, @"/servidores2.txt", servidores_path.Parent.FullName + @"\Servidores2.txt");
            readServidores();

            dl.DownloadFile(webrepository, @"/servidores3.txt", servidores_path.Parent.FullName + @"\Servidores3.txt");
            readServidoresMision();

        }

        public string GetRepositoryCatalog(string repo)
        {
            string nombre_bdd = Path.GetTempPath() + "ficheros_" + DateTime.Now.Ticks + ".db4o";

            WinWebDownload dl = new WinWebDownload();
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
            WinWebDownload dl = new WinWebDownload();

            try
            {

                using (StreamReader r = new StreamReader(Path.Combine(servidores_path.Parent.FullName, "Servidores2.txt")))
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
                                //
                                /*
                                string[] lbase = items[5].Length > 0 ? items[5].Split(';') : null;

                                if (lbase != null)
                                {
                                    foreach (string m in lbase)
                                    {
                                        DirectoryInfo d = _bay.GetDirectoryForRepo(m);
                                        
                                        ServidoresData.Mod mo = new ServidoresData.Mod();
                                        mo.Nombre = d.FullName;
                                        s.ModList.Add(mo);
                                    }
                                }
                                */
                                //
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

        private void readServidoresMision()
        {
            WinWebDownload dl = new WinWebDownload();

            try
            {

                using (StreamReader r = new StreamReader(Path.Combine(servidores_path.Parent.FullName , "Servidores3.txt")))
                {
                    string l;

                    while (!r.EndOfStream)
                    {
                        l = r.ReadLine();

                        if (l.Length > 0)
                        {
                            if (!l.TrimStart().StartsWith("#"))
                            {
                                ServidorMision s = new ServidorMision(/* null */);

                                string[] items = l.Split('|');

                                s.Arma = items[0];
                                s.FechaJuego = System.Convert.ToDateTime(items[1]);
                                s.Nombre = items[2];
                                s.IP = items[3];
                                s.Puerto = items[4];
                                s.Password = _pwd_server;
                                s.Repository = items[5];
                                s.MissionCode = items[6];

                                string[] lmaps = items[7].Length > 0 ? items[7].Split(';') : null;

                                if (lmaps != null)
                                {
                                    foreach (string m in lmaps)
                                    {
                                        ServidoresData.Mod mo = new ServidoresData.Mod();
                                        mo.Nombre = m;
                                        s.MapList.Add(mo);
                                    }
                                }


                                string[] lmods = items[8].Length > 0 ? items[8].Split(';') : null;

                                if (lmods != null)
                                {
                                    foreach (string m in lmods)
                                    {
                                        ServidoresData.Mod mo = new ServidoresData.Mod();
                                        mo.Nombre = m;
                                        s.ModList.Add(mo);
                                    }
                                }

                                dl.DownloadFile(webrepository, @"/Briefings/" + s.MissionCode + @".txt", servidores_path.Parent.FullName + @"/Briefings/" + s.MissionCode + @".txt");
                                dl.DownloadFile(webrepository, @"/MissionsIMG/" + s.MissionCode + @".jpg", servidores_path.Parent.FullName + @"/MissionsIMG/" + s.MissionCode + @".jpg");

                                using (StreamReader brief = new StreamReader(servidores_path.Parent.FullName + @"/Briefings/" + s.MissionCode + @".txt"))
                                {
                                    s.MissionBriefing = brief.ReadToEnd();
                                }

                                s.MissionIMG = servidores_path.Parent.FullName + @"/MissionsIMG/" + s.MissionCode + @".jpg";

                                _servidoresMis.Add(s);
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
