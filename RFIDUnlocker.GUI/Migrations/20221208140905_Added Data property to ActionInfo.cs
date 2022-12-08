using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RFIDUnlocker.GUI.Migrations
{
    /// <inheritdoc />
    public partial class AddedDatapropertytoActionInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Actions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "Actions");
        }
    }
}
