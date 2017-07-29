using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ServerManagerCore.Migrations.Web12BDIData
{
    public partial class Modfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DownloadUrl",
                table: "Mods",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SteamId",
                table: "Mods",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModFile",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Filename = table.Column<string>(nullable: false),
                    Hash = table.Column<string>(nullable: false),
                    Length = table.Column<long>(nullable: false),
                    ModId = table.Column<int>(nullable: false),
                    RelativePath = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModFile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModFile_Mods_ModId",
                        column: x => x.ModId,
                        principalTable: "Mods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModFile_ModId",
                table: "ModFile",
                column: "ModId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModFile");

            migrationBuilder.DropColumn(
                name: "DownloadUrl",
                table: "Mods");

            migrationBuilder.DropColumn(
                name: "SteamId",
                table: "Mods");
        }
    }
}
