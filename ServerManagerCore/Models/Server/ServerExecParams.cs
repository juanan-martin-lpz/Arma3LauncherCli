using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ServerManagerCore.Models.Server
{
    public class ServerExecParams
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Port { get; set; }

        public string Profile { get; set; }
        public string UserProfile { get; set; }
        public string Mods { get; set; }
        public string AditionalParams { get; set; }

        public int ServerId { get; set; }
    }
}
