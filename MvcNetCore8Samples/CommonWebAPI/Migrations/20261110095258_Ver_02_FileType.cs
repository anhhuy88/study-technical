using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommonWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class Ver_02_FileType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FileType",
                table: "FileDatas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileType",
                table: "FileDatas");
        }
    }
}
