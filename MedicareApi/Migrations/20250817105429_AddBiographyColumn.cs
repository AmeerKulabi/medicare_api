using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicareApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBiographyColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfessionalBiography",
                table: "Doctors",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfessionalBiography",
                table: "Doctors");
        }
    }
}
