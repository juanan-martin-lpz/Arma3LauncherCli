using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ServerManagementClient
{
   public  class ServidorMision : INotifyPropertyChanged
    {
        string arma;
        DateTime fecha;
        string nombre;
        string puerto;
        string ip;
        string password;
        string repo;
        string miscode;
        string briefing;
        string misimg;

        StatusInfo status;
        string publishedat;

        ObservableCollection<Mod> maplist;
        ObservableCollection<Mod> modlist;

        public ServidorMision(/* ServerConfig config */)
        {
            modlist = new ObservableCollection<Mod>();
            maplist = new ObservableCollection<Mod>();

            //_config = config;

            status = StatusInfo.Unknown;
            nombre = "";
            puerto = "";
            publishedat = "";
        }

        public string Image
        {
            get
            {
                if (arma == "2")
                {
                    return "Resources/Images/ListView/a2icon.jpg";
                }
                else
                {
                    return "Resources/Images/ListView/a3icon.jpg";
                }

            }
        }

        public string ImageStatus
        {
            get
            {
                return status == StatusInfo.Running ? "Resources/Images/ListView/iconon.jpg" : "Resources/Images/ListView/iconoff.jpg";
            }
        }

        public string Arma
        {
            get
            {
                return arma;
            }

            set
            {
                arma = value; NotifyPropertyChanged("Arma");
            }
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

        public DateTime FechaJuego
        {
            get { return fecha; }
            set { fecha = value; NotifyPropertyChanged("FechaJuego"); }
        }

        public string Puerto
        {
            get
            {
                return puerto;
            }

            set
            {
                puerto = value; NotifyPropertyChanged("Puerto");
            }
        }

        public string PublishedAt
        {
            get
            {
                return publishedat;
            }

            set
            {
                publishedat = value; NotifyPropertyChanged("PublishedAt");
            }
        }

        public string IP
        {
            get
            {
                return ip;
            }

            set
            {
                ip = value; NotifyPropertyChanged("IP");
            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                password = value; NotifyPropertyChanged("Password");
            }
        }

        public string Repository
        {
            get
            {
                return repo;
            }

            set
            {
                repo = value; NotifyPropertyChanged("Repository");
            }
        }


        public string MissionCode
        {
            get
            {
                return miscode;
            }

            set
            {
                miscode = value; NotifyPropertyChanged("MissionCode");
            }
        }

        public string MissionBriefing
        {
            get
            {
                return briefing;
            }

            set
            {
                briefing = value; NotifyPropertyChanged("MissionBriefing");
            }
        }

        public string MissionIMG
        {
            get
            {
                return misimg;
            }

            set
            {
                misimg = value; NotifyPropertyChanged("MissionIMG");
            }
        }
        public ObservableCollection<Mod> ModList
        {
            get
            {
                return modlist;
            }

            set
            {
                modlist = value; NotifyPropertyChanged("ModList");
            }
        }

        public ObservableCollection<Mod> MapList
        {
            get
            {
                return maplist;
            }

            set
            {
                maplist = value; NotifyPropertyChanged("MapList");
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

        public string ModListToString()
        {
            string lista = "";


            if (ModList.Count > 0)
            {
                foreach (Mod s in ModList)
                {
                    lista += s.Nombre + ";";
                }
            }

            return lista;
        }

        public string ModListToString(string path)
        {
            string lista = "";

            if (!path.EndsWith(@"\"))
            {
                path = path + @"\";
            }

            if (ModList.Count > 0)
            {
                foreach (Mod s in ModList)
                {
                    lista += path + s.Nombre + ";";
                }
            }

            return lista;
        }

        public string MapListToString()
        {
            string lista = "";


            if (MapList.Count > 0)
            {
                foreach (Mod s in MapList)
                {
                    lista += s.Nombre + ";";
                }
            }

            return lista;
        }
        public StatusInfo Status
        {
            get
            {
                return status;
            }
        }

        
        public Servidor Clone()
        {
            Servidor s = new Servidor(/* _config */);

            s.Arma = arma;
            s.Nombre = nombre;
            s.PublishedAt = publishedat;
            s.Puerto = puerto;

            foreach (Mod m in modlist)
            {
                s.ModList.Add(m.Clone());
            }

            return s;
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public void Remove()
        {

        }

    }
}
