/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _291AddReissuance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() CASCADE;");

            migrationBuilder.AddColumn<Guid>(
                name: "reissued_credential_id",
                schema: "issuer",
                table: "company_ssi_details",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "audit_company_ssi_detail20240902",
                schema: "issuer",
                columns: table => new
                {
                    audit_v2id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bpnl = table.Column<string>(type: "text", nullable: false),
                    issuer_bpn = table.Column<string>(type: "text", nullable: false),
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    company_ssi_detail_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_user_id = table.Column<string>(type: "text", nullable: false),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_credential_external_type_detail_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expiry_check_type_id = table.Column<int>(type: "integer", nullable: true),
                    process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_credential_id = table.Column<Guid>(type: "uuid", nullable: true),
                    credential = table.Column<string>(type: "text", nullable: true),
                    reissued_credential_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<string>(type: "text", nullable: true),
                    audit_v2last_editor_id = table.Column<string>(type: "text", nullable: true),
                    audit_v2operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v2date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_ssi_detail20240902", x => x.audit_v2id);
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[] { 6, "REVOKE_REISSUED_CREDENTIAL" });

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_reissued_credential_id",
                schema: "issuer",
                table: "company_ssi_details",
                column: "reissued_credential_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_company_ssi_details_company_ssi_details_reissued_credential",
                schema: "issuer",
                table: "company_ssi_details",
                column: "reissued_credential_id",
                principalSchema: "issuer",
                principalTable: "company_ssi_details",
                principalColumn: "id");

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240902\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"reissued_credential_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"reissued_credential_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240902\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"reissued_credential_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"reissued_credential_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "fk_company_ssi_details_company_ssi_details_reissued_credential",
                schema: "issuer",
                table: "company_ssi_details");

            migrationBuilder.DropTable(
                name: "audit_company_ssi_detail20240902",
                schema: "issuer");

            migrationBuilder.DropIndex(
                name: "ix_company_ssi_details_reissued_credential_id",
                schema: "issuer",
                table: "company_ssi_details");

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "reissued_credential_id",
                schema: "issuer",
                table: "company_ssi_details");

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240419\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240419\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"();");
        }
    }
}
