using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nodsoft.Cutter.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveModelManagedSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed rows remain in place; they are now owned by the table-creation migrations.
            // Also align the token type length with the current OpenIddict model.
            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "openiddict_tokens",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "openiddict_tokens",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);
        }
    }
}
