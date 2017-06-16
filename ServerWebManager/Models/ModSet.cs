namespace ServerWebManager.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ModSet")]
    public partial class ModSet
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ModSet()
        {
            ModFileSet = new HashSet<ModFileSet>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Image { get; set; }

        [Required]
        public string RelativePath { get; set; }

        public int RepositoryId { get; set; }

        public int LoadOrder { get; set; }

        public virtual ICollection<ModFileSet> ModFileSet { get; }

        public virtual RepositorySet RepositorySet { get; }
    }
}
