using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace ServerManager
{
    public class CatalogoRemoto : ICatalogo<ItemCatalogo>
    {
        private string baseurl;
        private string name;
        private string catalogName;

        private ObservableCollection<ItemCatalogo> _items = new ObservableCollection<ItemCatalogo>();

        public CatalogoRemoto()
        {

        }

        public CatalogoRemoto(string Url,string Name,string CatalogName)
        {
            baseurl = Url;
            name = Name;
            catalogName = CatalogName;
        }

        public string BaseUrl()
        {
            return baseurl;
        }

        public void Generate(string from)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ItemCatalogo> Items()
        {
            return _items;
        }

        public void Load()
        {
            string tmpFile = Path.GetTempFileName();

            // Access Web API and get the data

            //

            string jsoncontent;

            if (File.Exists(tmpFile))
            {
                jsoncontent = File.ReadAllText(tmpFile);
                _items = (ObservableCollection<ItemCatalogo>)JsonConvert.DeserializeObject<ObservableCollection<ItemCatalogo>>(jsoncontent);
            }

            File.Delete(tmpFile);

        }

        public string Name()
        {
            return name;
        }

        public void Write()
        {
            throw new NotImplementedException();
        }
    }
}
