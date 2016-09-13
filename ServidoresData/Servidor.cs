using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ServidoresData
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
        string nombre;
        string puerto;
        string ip;
        string password;
        string repo;

        StatusInfo status;
        string publishedat;

        ServerConfig _config;

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
                    lista += path + s.Nombre + ";";
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
        
        public void Publish(Repository target)
        {
        	//target.CreateDB();

            //target.Publish();
        	
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


        public ServerConfig Config
        {
            get { return _config;}
            set
            { if ((value != null) && (value != _config))
                {
                    _config = value;
                }    
            }
        }

        public void Start()
        {

            if (_config == null)
            {
                throw new Exception("No existe configuracion para el lanzamiento");
            }

            string executable = _config.Path + @"\" + "arma3server.exe";
            string srvcfg = @" -ServerCfg" + _config.ServerConfigFile + @" -ServerBasic=" + _config.ConfigFile;
            string profile = @" -Profile=" + _config.ProfileFile;
            string mods = @" -mod=" + ModListToString();

            string parametros = srvcfg + profile + mods;

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(executable);
            info.UseShellExecute = true;

            info.RedirectStandardError = true;
            
            info.Arguments = parametros;
            //info.WorkingDirectory = dir_trabajo;

            // black
            //
            //p.EnableRaisingEvents = true;
            //p.Exited += new EventHandler(Utiles.arma_Exited);
            //
            p.StartInfo = info;
            //while (!p.Start()) ; // mio

            
            p.Start();

            int pid = p.Id;
            // Guardamos la info en una carpeta, al estilo Linux. El nombre de la carpeta es el pid
            // Dentro guardamos un archivo con informacion acerca del lanzamiento -> launch_object
            // Otro archivo con datos de telemetria (memoria,etc..) actualizable por timer+evento -> telemetry
            // La carpeta y el contenido es eliminado en el Stop
             
            p.WaitForExit();

        }

        public void Stop()
        {
        	
        }
        
        public void Remove()
        {
        	
        }
    }
}
