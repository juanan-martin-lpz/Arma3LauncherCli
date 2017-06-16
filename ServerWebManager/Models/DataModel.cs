namespace ServerWebManager.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using ServerWebManager.Models;

    public partial class DataModel : DbContext
    {
        public DataModel() : base("name=DataModel")
        {
            //Database.SetInitializer<DataModel>(new DataModelContextDbInitializer());
        }
        

        public virtual DbSet<ModFileSet> ModFileSet { get; set; }
        public virtual DbSet<ModSet> ModSet { get; set; }
        public virtual DbSet<RepositorySet> RepositorySet { get; set; }
        public virtual DbSet<ServerSet> ServerSet { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModSet>()
                .HasMany(e => e.ModFileSet)
                .WithRequired(e => e.ModSet)
                .HasForeignKey(e => e.ModId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RepositorySet>()
                .HasMany(e => e.ModSet)
                .WithRequired(e => e.RepositorySet)
                .HasForeignKey(e => e.RepositoryId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RepositorySet>()
                .HasMany(e => e.ServerSet)
                .WithMany(e => e.RepositorySet)
                .Map(m => m.ToTable("ServerRepository").MapLeftKey("Repositories_Id").MapRightKey("Server_Id"));
        }
    }
}
