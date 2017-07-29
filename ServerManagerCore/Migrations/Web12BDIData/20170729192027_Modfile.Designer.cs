using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ServerManagerCore.Data;

namespace ServerManagerCore.Migrations.Web12BDIData
{
    [DbContext(typeof(Web12BDIDataContext))]
    [Migration("20170729192027_Modfile")]
    partial class Modfile
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ServerManagerCore.Models.Repository.Mod", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DownloadUrl");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("RelativePath")
                        .IsRequired();

                    b.Property<int>("RepositoryId");

                    b.Property<string>("SteamId");

                    b.HasKey("Id");

                    b.HasIndex("RepositoryId");

                    b.ToTable("Mods");
                });

            modelBuilder.Entity("ServerManagerCore.Models.Repository.ModFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Filename")
                        .IsRequired();

                    b.Property<string>("Hash")
                        .IsRequired();

                    b.Property<long>("Length");

                    b.Property<int>("ModId");

                    b.Property<string>("RelativePath")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ModId");

                    b.ToTable("ModFile");
                });

            modelBuilder.Entity("ServerManagerCore.Models.Repository.Repository", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("RelativePath")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Repositories");
                });

            modelBuilder.Entity("ServerManagerCore.Models.Server.Server", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("RelativePath")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("ServerManagerCore.Models.Server.ServerExecParams", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AditionalParams");

                    b.Property<string>("Mods");

                    b.Property<string>("Port")
                        .IsRequired();

                    b.Property<string>("Profile");

                    b.Property<int>("ServerId");

                    b.Property<string>("UserProfile");

                    b.HasKey("Id");

                    b.HasIndex("ServerId")
                        .IsUnique();

                    b.ToTable("ServerExecParams");
                });

            modelBuilder.Entity("ServerManagerCore.Models.Repository.Mod", b =>
                {
                    b.HasOne("ServerManagerCore.Models.Repository.Repository")
                        .WithMany("Mods")
                        .HasForeignKey("RepositoryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerManagerCore.Models.Repository.ModFile", b =>
                {
                    b.HasOne("ServerManagerCore.Models.Repository.Mod")
                        .WithMany("Files")
                        .HasForeignKey("ModId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ServerManagerCore.Models.Server.ServerExecParams", b =>
                {
                    b.HasOne("ServerManagerCore.Models.Server.Server")
                        .WithOne("ExecParams")
                        .HasForeignKey("ServerManagerCore.Models.Server.ServerExecParams", "ServerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
