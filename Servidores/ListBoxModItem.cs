using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServidoresData;
using System.ComponentModel;

namespace Servidores
{
    public class ListBoxModItem : INotifyPropertyChanged
    {
        bool ischecked;
        Mod mod;
        
        public bool IsChecked
        {
            get
            {
                return ischecked;
            }
            set
            {
                ischecked = value;
                NotifyPropertyChanged("IsChecked");
            }
        }

        public Mod Mod
        {
            get
            {
                return mod;
            }
            set
            {
                mod = value;
                NotifyPropertyChanged("Mod");
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
