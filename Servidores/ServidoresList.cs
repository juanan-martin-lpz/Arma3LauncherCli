using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ServidoresData;

namespace Servidores
{
    public class ServidoresList
    {

        ObservableCollection<Servidor> _lista;

        public ServidoresList()
        {
            _lista = new ObservableCollection<Servidor>();
        }

        public ObservableCollection<Servidor> Servidores
        {
            get { return _lista; }
        }
    }
}
