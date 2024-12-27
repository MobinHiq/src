using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CongestionTaxCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RegistrationNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TaxRules",
                columns: new[] { "Id", "Amount", "City", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { 1, 8m, "Gothenburg", new TimeSpan(0, 6, 29, 59, 0), new TimeSpan(0, 6, 0, 0, 0) },
                    { 2, 13m, "Gothenburg", new TimeSpan(0, 6, 59, 59, 0), new TimeSpan(0, 6, 30, 0, 0) },
                    { 3, 18m, "Gothenburg", new TimeSpan(0, 7, 59, 59, 0), new TimeSpan(0, 7, 0, 0, 0) },
                    { 4, 13m, "Gothenburg", new TimeSpan(0, 8, 29, 59, 0), new TimeSpan(0, 8, 0, 0, 0) },
                    { 5, 8m, "Gothenburg", new TimeSpan(0, 14, 59, 59, 0), new TimeSpan(0, 8, 30, 0, 0) },
                    { 6, 13m, "Gothenburg", new TimeSpan(0, 15, 29, 59, 0), new TimeSpan(0, 15, 0, 0, 0) },
                    { 7, 18m, "Gothenburg", new TimeSpan(0, 16, 59, 59, 0), new TimeSpan(0, 15, 30, 0, 0) },
                    { 8, 13m, "Gothenburg", new TimeSpan(0, 17, 59, 59, 0), new TimeSpan(0, 17, 0, 0, 0) },
                    { 9, 8m, "Gothenburg", new TimeSpan(0, 18, 29, 59, 0), new TimeSpan(0, 18, 0, 0, 0) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxRules");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
