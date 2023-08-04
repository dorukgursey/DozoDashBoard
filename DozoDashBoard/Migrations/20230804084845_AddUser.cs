using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DozoDashBoard.Migrations
{
    public partial class AddUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Email", "FirstName", "LastName", "Password", "Role", "UserName" },
                values: new object[] { new Guid("06b41f0d-b431-4231-9e7a-b34a9d35e0c8"), "testuser@example.com", "John", "Doe", "Test123", "User", "TestUser" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: new Guid("06b41f0d-b431-4231-9e7a-b34a9d35e0c8"));
        }
    }
}
