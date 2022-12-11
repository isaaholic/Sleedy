using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class addresponseS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Response",
                table: "Response");

            migrationBuilder.RenameTable(
                name: "Response",
                newName: "Responses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Responses",
                table: "Responses",
                column: "FacultyName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Responses",
                table: "Responses");

            migrationBuilder.RenameTable(
                name: "Responses",
                newName: "Response");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Response",
                table: "Response",
                column: "FacultyName");
        }
    }
}
