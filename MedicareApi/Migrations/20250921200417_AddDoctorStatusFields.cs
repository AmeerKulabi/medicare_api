using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicareApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProfileCompleted",
                table: "Doctors",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Doctors",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProfileCompleted",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Doctors");
        }
    }
}
