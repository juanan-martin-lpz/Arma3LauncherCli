namespace ServerWebManager.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public enum ServerStatus
    {
        Stopped = 0,
        Paused,
        Running,
        Unreachable,
        Initializing
    }

    [Table("ServerSet")]
    public partial class ServerSet
    {
        private ServerStatus _status;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ServerSet()
        {
            _status = ServerStatus.Initializing;

            RepositorySet = new HashSet<RepositorySet>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Port { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string RelativePath { get; set; }

        public ServerStatus Status => _status; // { get { return _status; } }

        public bool Private { get; set; }

        public int OwnerId { get; set; }

        public virtual ICollection<RepositorySet> RepositorySet { get; }
    }
}
