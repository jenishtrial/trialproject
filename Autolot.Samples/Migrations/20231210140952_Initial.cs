﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Autolot.Samples.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Drivers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Makes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Makes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Black"),
                    IsDrivable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Display = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, computedColumnSql: "[PetName] + ' (' + [Color] + ')'", stored: true),
                    DateBuilt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "getdate()"),
                    PetName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MakeId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventory_Makes_MakeId",
                        column: x => x.MakeId,
                        principalSchema: "dbo",
                        principalTable: "Makes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventoryToDrivers",
                schema: "dbo",
                columns: table => new
                {
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryToDrivers", x => new { x.InventoryId, x.DriverId });
                    table.ForeignKey(
                        name: "FK_InventoryDriver_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalSchema: "dbo",
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryDriver_Inventory_InventoryId",
                        column: x => x.InventoryId,
                        principalSchema: "dbo",
                        principalTable: "Inventory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Radios",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HasTweeters = table.Column<bool>(type: "bit", nullable: false),
                    HasSubWoofers = table.Column<bool>(type: "bit", nullable: false),
                    RadioId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InventoryId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Radios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Radios_Inventory_InventoryId",
                        column: x => x.InventoryId,
                        principalSchema: "dbo",
                        principalTable: "Inventory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_MakeId",
                schema: "dbo",
                table: "Inventory",
                column: "MakeId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryToDrivers_DriverId",
                schema: "dbo",
                table: "InventoryToDrivers",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Radios_CarId",
                schema: "dbo",
                table: "Radios",
                column: "InventoryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryToDrivers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Radios",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Drivers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Inventory",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Makes",
                schema: "dbo");
        }
    }
}
