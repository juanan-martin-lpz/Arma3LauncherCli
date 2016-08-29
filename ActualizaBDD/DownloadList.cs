using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ActualizaBDD
{
    public class DownloadList
    {
        ObservableCollection<WebDownload> _lista;

        public DownloadList()
        {
            _lista = new ObservableCollection<WebDownload>();
        }

        public ObservableCollection<WebDownload> Downloads
        {
            get { return _lista; }
        }
        
    }
}
