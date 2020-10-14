using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Reader.Migrations
{
    public partial class AddedItemRead : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Read",
                table: "Item",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Read",
                table: "Item");
        }
    }
}
