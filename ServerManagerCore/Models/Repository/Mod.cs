using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ServerManagerCore.Models.Repository
{
    public class Mod
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string RelativePath { get; set; }

        public int RepositoryId { get; set; }
    }
}
