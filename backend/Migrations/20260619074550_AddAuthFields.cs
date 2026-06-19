using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PasswordHash",
                table: "Medias",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Medias",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Medias",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Medias");
        }
    }
}
