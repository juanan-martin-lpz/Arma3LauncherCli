using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServidoresData
{
    [JsonObject(MemberSerialization.OptOut)]
    public class RepositoryProxy
    {
        List<ModProxy> _mods;
        public RepositoryProxy()
        {
            _mods = new List<ModProxy>();
        }
        public string Nombre { get; set; }
        public List<ModProxy> Mods { get { return _mods; } set { _mods = value; } }

    }
}
