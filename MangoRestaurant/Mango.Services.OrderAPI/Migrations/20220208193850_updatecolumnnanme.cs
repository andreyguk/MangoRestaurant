using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mango.Services.OrderAPI.Migrations
{
    public partial class updatecolumnnanme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpiryMinthYear",
                table: "OrderHeaders",
                newName: "ExpiryMonthYear");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpiryMonthYear",
                table: "OrderHeaders",
                newName: "ExpiryMinthYear");
        }
    }
}
