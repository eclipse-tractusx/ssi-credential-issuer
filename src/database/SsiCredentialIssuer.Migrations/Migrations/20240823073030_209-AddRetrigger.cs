using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _209AddRetrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "issuer",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 6, "RETRIGGER_CREATE_SIGNED_CREDENTIAL" },
                    { 7, "RETRIGGER_SAVE_CREDENTIAL_DOCUMENT" },
                    { 8, "RETRIGGER_CREATE_CREDENTIAL_FOR_HOLDER" },
                    { 9, "RETRIGGER_TRIGGER_CALLBACK" },
                    { 103, "RETRIGGER_REVOKE_CREDENTIAL" },
                    { 104, "RETRIGGER_TRIGGER_NOTIFICATION" },
                    { 105, "RETRIGGER_TRIGGER_MAIL" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 105);
        }
    }
}
