using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SBAPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInspectionPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InspectionPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    InspectionResultId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PhotoData = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PhotoMimeType = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Caption = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionPhotos_InspectionResults_InspectionResultId",
                        column: x => x.InspectionResultId,
                        principalTable: "InspectionResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InspectionPhotos_InspectionResultId",
                table: "InspectionPhotos",
                column: "InspectionResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionPhotos");
        }
    }
}
