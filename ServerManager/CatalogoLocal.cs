using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace ServerManager
{
    public class CatalogoLocal : ICatalogo<ItemCatalogo>
    {
        private string name;
        private string basedir;
        private string jsonfile;
        private ObservableCollection<ItemCatalogo> _items = new ObservableCollection<ItemCatalogo>();
        private DirectoryInfo baseDI;
        private bool isNewCatalog;

        public CatalogoLocal()
        {
            
        }

        public CatalogoLocal(string catalogName, string basefolder, string jsonFilename, bool createIfNotExist = false)
        {
            name = catalogName;
            jsonfile = jsonFilename;

            if (Directory.Exists(basefolder))
            {                
                basedir = basefolder;
                baseDI = new DirectoryInfo(basedir);
                isNewCatalog = false;
            }
            else
            {
                if (createIfNotExist)
                {
                    baseDI = Directory.CreateDirectory(basefolder);
                    isNewCatalog = true;
                }
                else
                {
                    throw new ArgumentException("Basefolder no existe");
                }
            }

        }

        public string BaseUrl()
        {
            return basedir;
        }

        public void Generate(string from)
        {
            DirectoryInfo fromDI = new DirectoryInfo(from);

            if (fromDI.Exists)
            {
                var flist = from d in fromDI.GetDirectories() where d.FullName.Contains("@") select d;

                foreach (DirectoryInfo fol in flist)
                {
                    List<FileInfo> files = fol.EnumerateFiles("*", SearchOption.AllDirectories).ToList<FileInfo>();

                    foreach (FileInfo fichero in files)
                    {


                    }
                }
            }

        }

        public IEnumerable<ItemCatalogo> Items()
        {
            return _items;
        }

        public void Load()
        {
            //throw new NotImplementedException();

            string jsonfname = Path.Combine(basedir, jsonfile);
            string jsoncontent;

            if (File.Exists(jsonfname))
            {
                jsoncontent = File.ReadAllText(jsonfname);
                _items = (ObservableCollection<ItemCatalogo>)JsonConvert.DeserializeObject<ObservableCollection<ItemCatalogo>>(jsoncontent);
            }

        }

        public string Name()
        {
            return name;
        }

        public void Write()
        {
            FileInfo fi = new FileInfo(Path.Combine(basedir, jsonfile));

            if (fi.Exists)
            {
                File.Delete(fi.FullName);
            }

            File.WriteAllText(fi.FullName, JsonConvert.SerializeObject(_items));
        }
    }
}
