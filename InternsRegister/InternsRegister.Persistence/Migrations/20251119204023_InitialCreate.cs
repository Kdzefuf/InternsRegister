using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternsRegister.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Directions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Interns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    firstname = table.Column<string>(type: "text", nullable: false),
                    lastname = table.Column<string>(type: "text", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    birthdate = table.Column<DateOnly>(type: "date", nullable: false),
                    internshipdirectionid = table.Column<Guid>(type: "uuid", nullable: true),
                    currentprojectid = table.Column<Guid>(type: "uuid", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interns", x => x.id);
                    table.ForeignKey(
                        name: "FK_Interns_Directions_internshipdirectionid",
                        column: x => x.internshipdirectionid,
                        principalTable: "Directions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Interns_Projects_currentprojectid",
                        column: x => x.currentprojectid,
                        principalTable: "Projects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Directions_name",
                table: "Directions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interns_currentprojectid",
                table: "Interns",
                column: "currentprojectid");

            migrationBuilder.CreateIndex(
                name: "IX_Interns_email",
                table: "Interns",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interns_internshipdirectionid",
                table: "Interns",
                column: "internshipdirectionid");

            migrationBuilder.CreateIndex(
                name: "IX_Interns_phone",
                table: "Interns",
                column: "phone",
                unique: true,
                filter: "Phone IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_name",
                table: "Projects",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interns");

            migrationBuilder.DropTable(
                name: "Directions");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
