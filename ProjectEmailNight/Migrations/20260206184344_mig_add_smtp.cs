using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectEmailNight.Migrations
{
    /// <inheritdoc />
    public partial class mig_add_smtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalMessageId",
                table: "Emails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalUser",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalMessageId",
                table: "Emails");

            migrationBuilder.DropColumn(
                name: "IsExternalUser",
                table: "AspNetUsers");
        }
    }
}
