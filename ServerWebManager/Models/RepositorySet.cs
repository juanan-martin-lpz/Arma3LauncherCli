namespace ServerWebManager.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RepositorySet")]
    public partial class RepositorySet
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RepositorySet()
        {
            ModSet = new HashSet<ModSet>();
            ServerSet = new HashSet<ServerSet>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string RelativePath { get; set; }

        [Required]
        public string Image { get; set; }

        public int LoadOrder { get; set; }

        public virtual ICollection<ModSet> ModSet { get;}

        public virtual ICollection<ServerSet> ServerSet { get;}
    }
}
