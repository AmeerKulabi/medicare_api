using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicareApi.Migrations
{
    /// <inheritdoc />
    public partial class DoctorTableChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Availability",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "LicenseState",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "ServicesOffered",
                table: "Doctors",
                newName: "ClinicType");

            migrationBuilder.RenameColumn(
                name: "PracticeType",
                table: "Doctors",
                newName: "City");

            migrationBuilder.AlterColumn<int>(
                name: "YearsOfExperience",
                table: "Doctors",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GraduationYear",
                table: "Doctors",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ConsultationFee",
                table: "Doctors",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClinicType",
                table: "Doctors",
                newName: "ServicesOffered");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Doctors",
                newName: "PracticeType");

            migrationBuilder.AlterColumn<string>(
                name: "YearsOfExperience",
                table: "Doctors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GraduationYear",
                table: "Doctors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConsultationFee",
                table: "Doctors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Availability",
                table: "Doctors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseState",
                table: "Doctors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Doctors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Doctors",
                type: "TEXT",
                nullable: true);
        }
    }
}
