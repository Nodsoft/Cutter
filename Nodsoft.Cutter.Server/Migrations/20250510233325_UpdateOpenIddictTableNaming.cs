using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nodsoft.Cutter.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOpenIddictTableNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                table: "OpenIddictTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_tokens",
                table: "OpenIddictTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_scopes",
                table: "OpenIddictScopes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_authorizations",
                table: "OpenIddictAuthorizations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_open_iddict_applications",
                table: "OpenIddictApplications");

            migrationBuilder.RenameTable(
                name: "OpenIddictTokens",
                newName: "openiddict_tokens");

            migrationBuilder.RenameTable(
                name: "OpenIddictScopes",
                newName: "openiddict_scopes");

            migrationBuilder.RenameTable(
                name: "OpenIddictAuthorizations",
                newName: "openiddict_authorizations");

            migrationBuilder.RenameTable(
                name: "OpenIddictApplications",
                newName: "openiddict_applications");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_reference_id",
                table: "openiddict_tokens",
                newName: "ix_openiddict_tokens_reference_id");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_authorization_id",
                table: "openiddict_tokens",
                newName: "ix_openiddict_tokens_authorization_id");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_tokens_application_id_status_subject_type",
                table: "openiddict_tokens",
                newName: "ix_openiddict_tokens_application_id_status_subject_type");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_scopes_name",
                table: "openiddict_scopes",
                newName: "ix_openiddict_scopes_name");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_authorizations_application_id_status_subject_type",
                table: "openiddict_authorizations",
                newName: "ix_openiddict_authorizations_application_id_status_subject_type");

            migrationBuilder.RenameIndex(
                name: "ix_open_iddict_applications_client_id",
                table: "openiddict_applications",
                newName: "ix_openiddict_applications_client_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_openiddict_tokens",
                table: "openiddict_tokens",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_openiddict_scopes",
                table: "openiddict_scopes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_openiddict_authorizations",
                table: "openiddict_authorizations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_openiddict_applications",
                table: "openiddict_applications",
                column: "id");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "ip_addresses", "raw_object" },
                values: new object[] { new List<IPAddress>(), System.Text.Json.JsonDocument.Parse("{\"legacy\": true}", new System.Text.Json.JsonDocumentOptions()) });

            migrationBuilder.AddForeignKey(
                name: "fk_openiddict_authorizations_openiddict_applications_applicati",
                table: "openiddict_authorizations",
                column: "application_id",
                principalTable: "openiddict_applications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_openiddict_tokens_openiddict_applications_application_id",
                table: "openiddict_tokens",
                column: "application_id",
                principalTable: "openiddict_applications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_openiddict_tokens_openiddict_authorizations_authorization_id",
                table: "openiddict_tokens",
                column: "authorization_id",
                principalTable: "openiddict_authorizations",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_openiddict_authorizations_openiddict_applications_applicati",
                table: "openiddict_authorizations");

            migrationBuilder.DropForeignKey(
                name: "fk_openiddict_tokens_openiddict_applications_application_id",
                table: "openiddict_tokens");

            migrationBuilder.DropForeignKey(
                name: "fk_openiddict_tokens_openiddict_authorizations_authorization_id",
                table: "openiddict_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_openiddict_tokens",
                table: "openiddict_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_openiddict_scopes",
                table: "openiddict_scopes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_openiddict_authorizations",
                table: "openiddict_authorizations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_openiddict_applications",
                table: "openiddict_applications");

            migrationBuilder.RenameTable(
                name: "openiddict_tokens",
                newName: "OpenIddictTokens");

            migrationBuilder.RenameTable(
                name: "openiddict_scopes",
                newName: "OpenIddictScopes");

            migrationBuilder.RenameTable(
                name: "openiddict_authorizations",
                newName: "OpenIddictAuthorizations");

            migrationBuilder.RenameTable(
                name: "openiddict_applications",
                newName: "OpenIddictApplications");

            migrationBuilder.RenameIndex(
                name: "ix_openiddict_tokens_reference_id",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_reference_id");

            migrationBuilder.RenameIndex(
                name: "ix_openiddict_tokens_authorization_id",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_authorization_id");

            migrationBuilder.RenameIndex(
                name: "ix_openiddict_tokens_application_id_status_subject_type",
                table: "OpenIddictTokens",
                newName: "ix_open_iddict_tokens_application_id_status_subject_type");

            migrationBuilder.RenameIndex(
                name: "ix_openiddict_scopes_name",
                table: "OpenIddictScopes",
                newName: "ix_open_iddict_scopes_name");

            migrationBuilder.RenameIndex(
                name: "ix_openiddict_authorizations_application_id_status_subject_type",
                table: "OpenIddictAuthorizations",
                newName: "ix_open_iddict_authorizations_application_id_status_subject_type");

            migrationBuilder.RenameIndex(
                name: "ix_openiddict_applications_client_id",
                table: "OpenIddictApplications",
                newName: "ix_open_iddict_applications_client_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_tokens",
                table: "OpenIddictTokens",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_scopes",
                table: "OpenIddictScopes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_authorizations",
                table: "OpenIddictAuthorizations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_open_iddict_applications",
                table: "OpenIddictApplications",
                column: "id");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "ip_addresses", "raw_object" },
                values: new object[] { new List<IPAddress>(), System.Text.Json.JsonDocument.Parse("{\"legacy\": true}", new System.Text.Json.JsonDocumentOptions()) });

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                table: "OpenIddictAuthorizations",
                column: "application_id",
                principalTable: "OpenIddictApplications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                table: "OpenIddictTokens",
                column: "application_id",
                principalTable: "OpenIddictApplications",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                table: "OpenIddictTokens",
                column: "authorization_id",
                principalTable: "OpenIddictAuthorizations",
                principalColumn: "id");
        }
    }
}
