using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerManagementClient
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ModProxy
    {
        public string Nombre { get; set; }

        public string Icon { get; set; }


    }
}
