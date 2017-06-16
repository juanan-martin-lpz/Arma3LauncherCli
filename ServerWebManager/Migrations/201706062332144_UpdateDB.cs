namespace ServerWebManager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ModFileSet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Basename = c.String(nullable: false),
                        RelativePath = c.String(nullable: false),
                        Size = c.Int(nullable: false),
                        Signature = c.String(nullable: false),
                        ModId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ModSet", t => t.ModId)
                .Index(t => t.ModId);
            
            CreateTable(
                "dbo.ModSet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        Image = c.String(nullable: false),
                        RelativePath = c.String(nullable: false),
                        RepositoryId = c.Int(nullable: false),
                        LoadOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RepositorySet", t => t.RepositoryId)
                .Index(t => t.RepositoryId);
            
            CreateTable(
                "dbo.RepositorySet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        RelativePath = c.String(nullable: false),
                        Image = c.String(nullable: false),
                        LoadOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ServerSet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Port = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        RelativePath = c.String(nullable: false),
                        Private = c.Boolean(nullable: false),
                        OwnerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ServerRepository",
                c => new
                    {
                        Repositories_Id = c.Int(nullable: false),
                        Server_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Repositories_Id, t.Server_Id })
                .ForeignKey("dbo.RepositorySet", t => t.Repositories_Id, cascadeDelete: true)
                .ForeignKey("dbo.ServerSet", t => t.Server_Id, cascadeDelete: true)
                .Index(t => t.Repositories_Id)
                .Index(t => t.Server_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ServerRepository", "Server_Id", "dbo.ServerSet");
            DropForeignKey("dbo.ServerRepository", "Repositories_Id", "dbo.RepositorySet");
            DropForeignKey("dbo.ModSet", "RepositoryId", "dbo.RepositorySet");
            DropForeignKey("dbo.ModFileSet", "ModId", "dbo.ModSet");
            DropIndex("dbo.ServerRepository", new[] { "Server_Id" });
            DropIndex("dbo.ServerRepository", new[] { "Repositories_Id" });
            DropIndex("dbo.ModSet", new[] { "RepositoryId" });
            DropIndex("dbo.ModFileSet", new[] { "ModId" });
            DropTable("dbo.ServerRepository");
            DropTable("dbo.ServerSet");
            DropTable("dbo.RepositorySet");
            DropTable("dbo.ModSet");
            DropTable("dbo.ModFileSet");
        }
    }
}
