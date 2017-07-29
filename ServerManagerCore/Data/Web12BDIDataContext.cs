using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServerManagerCore.Models;
using ServerManagerCore.Models.Server;
using ServerManagerCore.Models.Repository;

namespace ServerManagerCore.Data
{
    public class Web12BDIDataContext : DbContext
    {
        public Web12BDIDataContext(DbContextOptions<Web12BDIDataContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

        }

        public DbSet<Server> Servers { get; set; }
        public DbSet<Repository> Repositories { get; set; }
        public DbSet<Mod> Mods { get; set; }


    }
}
