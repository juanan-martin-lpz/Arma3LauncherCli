using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;

namespace ServerManagementClient
{
    public class RepositoryBay : INotifyPropertyChanged
    {
        string _repobay;
        DirectoryInfo rp;

        List<DirectoryInfo> _localrepos;


        public RepositoryBay()
        {
            _repobay = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\12bdi_launcher\Repositories";

            rp = new DirectoryInfo(_repobay);

            if (!rp.Exists)
            {
                rp.Create();
            }

            _localrepos = new List<DirectoryInfo>();

            populateLocalRepos();
        }

        private void populateLocalRepos()
        {
            _localrepos = rp.GetDirectories().ToList();
        }

        public DirectoryInfo GetDirectoryForRepo(string name)
        {
            var target = from d in _localrepos where d.Name == name select d;

            if (target.Count() > 0)
            {
                return target.ElementAt(0);
            }
            else
            {
                DirectoryInfo di = createRepoInfo(name);

                populateLocalRepos();

                target = from d in _localrepos where d.Name == name select d;

                if (target.Count() > 0)
                {
                    return target.ElementAt(0);
                }
                else
                {
                    return di;
                }
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


        public DirectoryInfo ToDirectoryInfo()
        {
            return new DirectoryInfo(_repobay);
        }

        private DirectoryInfo createRepoInfo(string name)
        {
            string repo = _repobay + @"\" + name;
            DirectoryInfo d = new DirectoryInfo(repo);

            if (!d.Exists)
            {
                d.Create();
            }

            return d;
        }
    }
}
