using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nodsoft.Cutter.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkBlocking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_blocked",
                table: "links",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "ip_addresses", "raw_object" },
                values: new object[] { new List<IPAddress>(), System.Text.Json.JsonDocument.Parse("{\"legacy\": true}", new System.Text.Json.JsonDocumentOptions()) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_blocked",
                table: "links");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "ip_addresses", "raw_object" },
                values: new object[] { new List<IPAddress>(), System.Text.Json.JsonDocument.Parse("{\"legacy\": true}", new System.Text.Json.JsonDocumentOptions()) });
        }
    }
}
