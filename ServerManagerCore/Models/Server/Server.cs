using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ServerManagerCore.Models.Server
{
    public class Server
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string RelativePath { get; set; }

        [Required]
        public ServerExecParams ExecParams { get; set; }

    }
}
