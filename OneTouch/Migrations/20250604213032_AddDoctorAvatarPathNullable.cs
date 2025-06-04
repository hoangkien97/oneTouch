using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OneTouch.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorAvatarPathNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "Doctors");
        }
    }
}
