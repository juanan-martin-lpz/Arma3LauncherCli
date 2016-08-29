using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;

namespace ServidoresData
{
    public class ServerConfig : INotifyPropertyChanged
    {

        string _path;

        string _modpath;
        string _config;
        string _sconfig;
        string _profile;
        string _repo;
        
        public string Path
        {
            get
            {
                return _path;
            }

            set
            {
                _path = value; NotifyPropertyChanged("Path");
            }
        }

        public string ModPath
        {
            get
            {
                return _modpath;
            }

            set
            {
                _modpath = value; NotifyPropertyChanged("ModPath");
            }
        }

        public string ConfigFile
        {
            get
            {
                return _config;
            }

            set
            {
                _config = value; NotifyPropertyChanged("ConfigFile");
            }
        }

        public string ServerConfigFile
        {
            get
            {
                return _sconfig;
            }

            set
            {
                _sconfig = value; NotifyPropertyChanged("ServerConfigFile");
            }
        }

        public string ProfileFile
        {
            get
            {
                return _profile;
            }

            set
            {
                _profile = value; NotifyPropertyChanged("ProfileFile");
            }
        }

        public string RepoBasePath
        {
            get
            {
                return _repo;
            }

            set
            {
                _repo = value; NotifyPropertyChanged("RepoBasePath");
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

        public bool IsValid()
        {
            if (Directory.GetFiles(_path).Contains("arma3.exe") || Directory.GetFiles(_path).Contains("arma3server.exe"))
            {
                return true;
            }
            else
            { 
                return false;  
            }
        }
    }
}
