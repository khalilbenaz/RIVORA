using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVR.Framework.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeyHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeyHash",
                table: "ApiKeys",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_KeyHash",
                table: "ApiKeys",
                column: "KeyHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiKeys_KeyHash",
                table: "ApiKeys");

            migrationBuilder.DropColumn(
                name: "KeyHash",
                table: "ApiKeys");
        }
    }
}
