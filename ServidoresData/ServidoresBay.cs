using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

namespace ServidoresData
{
    class ServidoresBay
    {
        string _srvbay;
        DirectoryInfo rp;

        List<DirectoryInfo> _runningServers;


        public ServidoresBay()
        {
            _srvbay = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\12bdi\Servers";

            rp = new DirectoryInfo(_srvbay);

            if (!rp.Exists)
            {
                rp.Create();
            }

            _runningServers = new List<DirectoryInfo>();

            populateRunningServers();
        }

        private void populateRunningServers()
        {
            _runningServers = rp.GetDirectories().ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public DirectoryInfo ToDirectoryInfo()
        {
            return new DirectoryInfo(_srvbay);
        }

        private DirectoryInfo createRepoInfo(string name)
        {
            string repo = _srvbay + @"\" + name;
            DirectoryInfo d = new DirectoryInfo(repo);

            if (!d.Exists)
            {
                d.Create();
            }

            return d;
        }
    }
}
