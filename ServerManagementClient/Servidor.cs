using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ServerManagementClient
{
	public enum StatusInfo
	{
		Unknown = -1,
		Stopped = 0,
		Running = 1
	}
	
    public class Servidor : INotifyPropertyChanged
    {

        string arma;
        DateTime fecha;
        string nombre;
        string puerto;
        string ip;
        string password;
        string repo;
        

        StatusInfo status;
        string publishedat;

        ObservableCollection<Mod> modlist;

        public Servidor(/* ServerConfig config */)
        {
            modlist = new ObservableCollection<Mod>();
            
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

            if (! path.EndsWith(@"\"))
            {
                path = path + @"\";
            }

            if (ModList.Count > 0)
            {
                foreach (Mod s in ModList)
                {
                    if ((s.FromRepository == "") || (s.FromRepository == null))
                    {
                        lista += path + s.Nombre + ";";
                    }
                    else
                    {
                        string ptrimmed = path.TrimEnd("\\".ToCharArray());

                        string p = ptrimmed.Substring(0,ptrimmed.LastIndexOf(@"\"));

                        lista += p + @"\" + s.FromRepository + @"\" + s.Nombre + ";";
                    }
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
            
            foreach(Mod m in modlist)
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
