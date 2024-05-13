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

#nullable disable

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _100rc4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"() CASCADE;");

            migrationBuilder.Sql("delete from issuer.verified_credential_type_assigned_use_cases where verified_credential_type_id = 6");
            migrationBuilder.Sql("delete from issuer.verified_credential_type_assigned_external_types where verified_credential_type_id = 6");
            migrationBuilder.Sql("delete from issuer.verified_credential_type_assigned_kinds where verified_credential_type_id = 6");
            migrationBuilder.Sql("delete from issuer.verified_credential_external_type_detail_versions where id = '37aa6259-b452-4d50-b09e-827929dcfa15'");
            migrationBuilder.Sql("delete from issuer.use_cases where id = 'c065a349-f649-47f8-94d5-1a504a855419'");

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_external_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "issuer",
                table: "verified_credential_types",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240419\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240419\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_DOCUMENT$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_document20240419\" (\"id\", \"date_created\", \"document_hash\", \"document_content\", \"document_name\", \"media_type_id\", \"document_type_id\", \"document_status_id\", \"identity_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"document_hash\", \r\n  NEW.\"document_content\", \r\n  NEW.\"document_name\", \r\n  NEW.\"media_type_id\", \r\n  NEW.\"document_type_id\", \r\n  NEW.\"document_status_id\", \r\n  NEW.\"identity_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_DOCUMENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_DOCUMENT AFTER INSERT\r\nON \"issuer\".\"documents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_DOCUMENT$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_document20240419\" (\"id\", \"date_created\", \"document_hash\", \"document_content\", \"document_name\", \"media_type_id\", \"document_type_id\", \"document_status_id\", \"identity_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"document_hash\", \r\n  NEW.\"document_content\", \r\n  NEW.\"document_name\", \r\n  NEW.\"media_type_id\", \r\n  NEW.\"document_type_id\", \r\n  NEW.\"document_status_id\", \r\n  NEW.\"identity_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_TIMESTAMP, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_DOCUMENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_DOCUMENT AFTER UPDATE\r\nON \"issuer\".\"documents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"() CASCADE;");
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"() CASCADE;");

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_external_types",
                columns: new[] { "id", "label" },
                values: new object[] { 6, "QUALITY_CREDENTIAL" });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_types",
                columns: new[] { "id", "label" },
                values: new object[] { 6, "FRAMEWORK_AGREEMENT_QUALITY" });

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240419\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240419\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_DOCUMENT$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_document20240419\" (\"id\", \"date_created\", \"document_hash\", \"document_content\", \"document_name\", \"media_type_id\", \"document_type_id\", \"document_status_id\", \"identity_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"document_hash\", \r\n  NEW.\"document_content\", \r\n  NEW.\"document_name\", \r\n  NEW.\"media_type_id\", \r\n  NEW.\"document_type_id\", \r\n  NEW.\"document_status_id\", \r\n  NEW.\"identity_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_DOCUMENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_DOCUMENT AFTER INSERT\r\nON \"issuer\".\"documents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"();");
            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_DOCUMENT$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_document20240419\" (\"id\", \"date_created\", \"document_hash\", \"document_content\", \"document_name\", \"media_type_id\", \"document_type_id\", \"document_status_id\", \"identity_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v2id\", \"audit_v2operation_id\", \"audit_v2date_last_changed\", \"audit_v2last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"document_hash\", \r\n  NEW.\"document_content\", \r\n  NEW.\"document_name\", \r\n  NEW.\"media_type_id\", \r\n  NEW.\"document_type_id\", \r\n  NEW.\"document_status_id\", \r\n  NEW.\"identity_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_DOCUMENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_DOCUMENT AFTER UPDATE\r\nON \"issuer\".\"documents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"();");
        }
    }
}
