namespace ServerWebManager.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ModFileSet")]
    public partial class ModFileSet
    {
        public int Id { get; set; }

        [Required]
        public string Basename { get; set; }

        [Required]
        public string RelativePath { get; set; }

        public int Size { get; set; }

        [Required]
        public string Signature { get; set; }

        public int ModId { get; set; }

        public virtual ModSet ModSet { get; set; }
    }
}
