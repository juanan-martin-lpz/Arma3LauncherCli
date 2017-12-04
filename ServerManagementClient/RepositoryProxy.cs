using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;

namespace ServerManagementClient
{
    public class ModView
    {
        public string Arma { get; set; }
        public bool IsSelected { get; set; }
        public string Name { get; set; }
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class RepositoryProxy : INotifyPropertyChanged
    {
        bool _update;
        string nombre;
        DirectoryInfo basepath;

        List<ModProxy> _mods;

        public RepositoryProxy()
        {
            _mods = new List<ModProxy>();

        }


        public RepositoryProxy(string from, string Repo)
        {
            _mods = new List<ModProxy>();
            basepath = new DirectoryInfo(from);
            nombre = Repo;
        }


        
        public string Nombre { get; set; }

        public List<ModProxy> Mods { get { return _mods; } set { _mods = value; } }
        
        public bool MustUpdate
        {
            get
            {
                return _update;
            }
            set
            {
                _update = value;
                NotifyPropertyChanged("MustUpdate");
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
    }
}
