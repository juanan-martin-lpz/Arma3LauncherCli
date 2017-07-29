using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ServerManagerCore.Models.Repository
{
    public class ModFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RelativePath { get; set; }

        [Required]
        public string Filename { get; set; }

        [Required]
        public string Hash { get; set; }

        [Required]
        public long Length { get; set; }

        public int ModId { get; set; }
    }
}
