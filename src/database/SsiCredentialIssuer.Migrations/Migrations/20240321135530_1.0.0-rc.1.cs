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
using System.Text.Json;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _100rc1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "issuer");

            migrationBuilder.CreateTable(
                name: "audit_company_ssi_detail20240228",
                schema: "issuer",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bpnl = table.Column<string>(type: "text", nullable: false),
                    issuer_bpn = table.Column<string>(type: "text", nullable: false),
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    company_ssi_detail_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_credential_external_type_detail_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expiry_check_type_id = table.Column<int>(type: "integer", nullable: true),
                    process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_credential_id = table.Column<Guid>(type: "uuid", nullable: true),
                    credential = table.Column<string>(type: "text", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_company_ssi_detail20240228", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "audit_document20240305",
                schema: "issuer",
                columns: table => new
                {
                    audit_v1id = table.Column<Guid>(type: "uuid", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    document_hash = table.Column<byte[]>(type: "bytea", nullable: true),
                    document_content = table.Column<byte[]>(type: "bytea", nullable: true),
                    document_name = table.Column<string>(type: "text", nullable: true),
                    media_type_id = table.Column<int>(type: "integer", nullable: true),
                    document_type_id = table.Column<int>(type: "integer", nullable: true),
                    document_status_id = table.Column<int>(type: "integer", nullable: true),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    audit_v1last_editor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    audit_v1operation_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_document20240305", x => x.audit_v1id);
                });

            migrationBuilder.CreateTable(
                name: "company_ssi_detail_statuses",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_ssi_detail_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_status",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_types",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_document_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "expiry_check_types",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expiry_check_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media_types",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_step_statuses",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_step_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_step_types",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_step_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_types",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "use_cases",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    shortname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_use_cases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_external_types",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_external_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_kinds",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_kinds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_types",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    document_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    document_content = table.Column<byte[]>(type: "bytea", nullable: false),
                    document_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    media_type_id = table.Column<int>(type: "integer", nullable: false),
                    document_type_id = table.Column<int>(type: "integer", nullable: false),
                    document_status_id = table.Column<int>(type: "integer", nullable: false),
                    company_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_documents_document_status_document_status_id",
                        column: x => x.document_status_id,
                        principalSchema: "issuer",
                        principalTable: "document_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_documents_document_types_document_type_id",
                        column: x => x.document_type_id,
                        principalSchema: "issuer",
                        principalTable: "document_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_documents_media_types_media_type_id",
                        column: x => x.media_type_id,
                        principalSchema: "issuer",
                        principalTable: "media_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "processes",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_type_id = table.Column<int>(type: "integer", nullable: false),
                    lock_expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processes", x => x.id);
                    table.ForeignKey(
                        name: "fk_processes_process_types_process_type_id",
                        column: x => x.process_type_id,
                        principalSchema: "issuer",
                        principalTable: "process_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_external_type_detail_versions",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    verified_credential_external_type_id = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<string>(type: "text", nullable: true),
                    template = table.Column<string>(type: "text", nullable: true),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expiry = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_external_type_detail_versions", x => x.id);
                    table.ForeignKey(
                        name: "fk_verified_credential_external_type_detail_versions_verified_",
                        column: x => x.verified_credential_external_type_id,
                        principalSchema: "issuer",
                        principalTable: "verified_credential_external_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_assigned_external_types",
                schema: "issuer",
                columns: table => new
                {
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    verified_credential_external_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_assigned_external_types", x => new { x.verified_credential_type_id, x.verified_credential_external_type_id });
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_external_types_verified_c",
                        column: x => x.verified_credential_external_type_id,
                        principalSchema: "issuer",
                        principalTable: "verified_credential_external_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_external_types_verified_c1",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "issuer",
                        principalTable: "verified_credential_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_assigned_kinds",
                schema: "issuer",
                columns: table => new
                {
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    verified_credential_type_kind_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_assigned_kinds", x => new { x.verified_credential_type_id, x.verified_credential_type_kind_id });
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_kinds_verified_credential",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "issuer",
                        principalTable: "verified_credential_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_kinds_verified_credential1",
                        column: x => x.verified_credential_type_kind_id,
                        principalSchema: "issuer",
                        principalTable: "verified_credential_type_kinds",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "verified_credential_type_assigned_use_cases",
                schema: "issuer",
                columns: table => new
                {
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    use_case_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_verified_credential_type_assigned_use_cases", x => new { x.verified_credential_type_id, x.use_case_id });
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_use_cases_use_cases_use_c",
                        column: x => x.use_case_id,
                        principalSchema: "issuer",
                        principalTable: "use_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_verified_credential_type_assigned_use_cases_verified_creden",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "issuer",
                        principalTable: "verified_credential_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_steps",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_step_type_id = table.Column<int>(type: "integer", nullable: false),
                    process_step_status_id = table.Column<int>(type: "integer", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_process_steps_process_step_statuses_process_step_status_id",
                        column: x => x.process_step_status_id,
                        principalSchema: "issuer",
                        principalTable: "process_step_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_process_steps_process_step_types_process_step_type_id",
                        column: x => x.process_step_type_id,
                        principalSchema: "issuer",
                        principalTable: "process_step_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_process_steps_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "issuer",
                        principalTable: "processes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_ssi_details",
                schema: "issuer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bpnl = table.Column<string>(type: "text", nullable: false),
                    issuer_bpn = table.Column<string>(type: "text", nullable: false),
                    verified_credential_type_id = table.Column<int>(type: "integer", nullable: false),
                    company_ssi_detail_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creator_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    verified_credential_external_type_detail_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expiry_check_type_id = table.Column<int>(type: "integer", nullable: true),
                    process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_credential_id = table.Column<Guid>(type: "uuid", nullable: true),
                    credential = table.Column<string>(type: "text", nullable: true),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_editor_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_ssi_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_ssi_details_company_ssi_detail_statuses_company_ssi",
                        column: x => x.company_ssi_detail_status_id,
                        principalSchema: "issuer",
                        principalTable: "company_ssi_detail_statuses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_expiry_check_types_expiry_check_type_id",
                        column: x => x.expiry_check_type_id,
                        principalSchema: "issuer",
                        principalTable: "expiry_check_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "issuer",
                        principalTable: "processes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_verified_credential_external_type_detai",
                        column: x => x.verified_credential_external_type_detail_version_id,
                        principalSchema: "issuer",
                        principalTable: "verified_credential_external_type_detail_versions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_details_verified_credential_types_verified_cred",
                        column: x => x.verified_credential_type_id,
                        principalSchema: "issuer",
                        principalTable: "verified_credential_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_ssi_detail_assigned_documents",
                schema: "issuer",
                columns: table => new
                {
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_ssi_detail_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_ssi_detail_assigned_documents", x => new { x.document_id, x.company_ssi_detail_id });
                    table.ForeignKey(
                        name: "fk_company_ssi_detail_assigned_documents_company_ssi_details_c",
                        column: x => x.company_ssi_detail_id,
                        principalSchema: "issuer",
                        principalTable: "company_ssi_details",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_company_ssi_detail_assigned_documents_documents_document_id",
                        column: x => x.document_id,
                        principalSchema: "issuer",
                        principalTable: "documents",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_ssi_process_data",
                schema: "issuer",
                columns: table => new
                {
                    company_ssi_detail_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schema = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    credential_type_kind_id = table.Column<int>(type: "integer", nullable: false),
                    client_id = table.Column<string>(type: "text", nullable: true),
                    client_secret = table.Column<byte[]>(type: "bytea", nullable: true),
                    initialization_vector = table.Column<byte[]>(type: "bytea", nullable: true),
                    encryption_mode = table.Column<int>(type: "integer", nullable: true),
                    holder_wallet_url = table.Column<string>(type: "text", nullable: true),
                    callback_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_ssi_process_data", x => x.company_ssi_detail_id);
                    table.ForeignKey(
                        name: "fk_company_ssi_process_data_company_ssi_details_company_ssi_de",
                        column: x => x.company_ssi_detail_id,
                        principalSchema: "issuer",
                        principalTable: "company_ssi_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_company_ssi_process_data_verified_credential_type_kinds_cre",
                        column: x => x.credential_type_kind_id,
                        principalSchema: "issuer",
                        principalTable: "verified_credential_type_kinds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "company_ssi_detail_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PENDING" },
                    { 2, "ACTIVE" },
                    { 3, "REVOKED" },
                    { 4, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "document_status",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 2, "ACTIVE" },
                    { 3, "INACTIVE" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "document_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "PRESENTATION" },
                    { 2, "CREDENTIAL" },
                    { 3, "VERIFIED_CREDENTIAL" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "expiry_check_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "ONE_MONTH" },
                    { 2, "TWO_WEEKS" },
                    { 3, "ONE_DAY" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "media_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "JPEG" },
                    { 2, "GIF" },
                    { 3, "PNG" },
                    { 4, "SVG" },
                    { 5, "TIFF" },
                    { 6, "PDF" },
                    { 7, "JSON" },
                    { 8, "PEM" },
                    { 9, "CA_CERT" },
                    { 10, "PKX_CER" },
                    { 11, "OCTET" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "process_step_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "TODO" },
                    { 2, "DONE" },
                    { 3, "SKIPPED" },
                    { 4, "FAILED" },
                    { 5, "DUPLICATE" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "CREATE_CREDENTIAL" },
                    { 2, "SIGN_CREDENTIAL" },
                    { 3, "SAVE_CREDENTIAL_DOCUMENT" },
                    { 4, "CREATE_CREDENTIAL_FOR_HOLDER" },
                    { 5, "TRIGGER_CALLBACK" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 1, "CREATE_CREDENTIAL" });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_external_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "TRACEABILITY_CREDENTIAL" },
                    { 2, "PCF_CREDENTIAL" },
                    { 3, "BEHAVIOR_TWIN_CREDENTIAL" },
                    { 4, "VEHICLE_DISMANTLE" },
                    { 5, "SUSTAINABILITY_CREDENTIAL" },
                    { 6, "QUALITY_CREDENTIAL" },
                    { 7, "BUSINESS_PARTNER_NUMBER" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_type_kinds",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "FRAMEWORK" },
                    { 2, "MEMBERSHIP" },
                    { 3, "BPN" }
                });

            migrationBuilder.InsertData(
                schema: "issuer",
                table: "verified_credential_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "TRACEABILITY_FRAMEWORK" },
                    { 2, "PCF_FRAMEWORK" },
                    { 3, "BEHAVIOR_TWIN_FRAMEWORK" },
                    { 4, "DISMANTLER_CERTIFICATE" },
                    { 5, "SUSTAINABILITY_FRAMEWORK" },
                    { 6, "FRAMEWORK_AGREEMENT_QUALITY" },
                    { 7, "BUSINESS_PARTNER_NUMBER" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_detail_assigned_documents_company_ssi_detail_id",
                schema: "issuer",
                table: "company_ssi_detail_assigned_documents",
                column: "company_ssi_detail_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_company_ssi_detail_status_id",
                schema: "issuer",
                table: "company_ssi_details",
                column: "company_ssi_detail_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_expiry_check_type_id",
                schema: "issuer",
                table: "company_ssi_details",
                column: "expiry_check_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_process_id",
                schema: "issuer",
                table: "company_ssi_details",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_verified_credential_external_type_detai",
                schema: "issuer",
                table: "company_ssi_details",
                column: "verified_credential_external_type_detail_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_details_verified_credential_type_id",
                schema: "issuer",
                table: "company_ssi_details",
                column: "verified_credential_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_ssi_process_data_credential_type_kind_id",
                schema: "issuer",
                table: "company_ssi_process_data",
                column: "credential_type_kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_document_status_id",
                schema: "issuer",
                table: "documents",
                column: "document_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_document_type_id",
                schema: "issuer",
                table: "documents",
                column: "document_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_media_type_id",
                schema: "issuer",
                table: "documents",
                column: "media_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_id",
                schema: "issuer",
                table: "process_steps",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_step_status_id",
                schema: "issuer",
                table: "process_steps",
                column: "process_step_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_step_type_id",
                schema: "issuer",
                table: "process_steps",
                column: "process_step_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_processes_process_type_id",
                schema: "issuer",
                table: "processes",
                column: "process_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_external_type_detail_versions_verified_",
                schema: "issuer",
                table: "verified_credential_external_type_detail_versions",
                columns: new[] { "verified_credential_external_type_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_external_types_verified_c",
                schema: "issuer",
                table: "verified_credential_type_assigned_external_types",
                column: "verified_credential_external_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_external_types_verified_c1",
                schema: "issuer",
                table: "verified_credential_type_assigned_external_types",
                column: "verified_credential_type_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_kinds_verified_credential",
                schema: "issuer",
                table: "verified_credential_type_assigned_kinds",
                column: "verified_credential_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_kinds_verified_credential1",
                schema: "issuer",
                table: "verified_credential_type_assigned_kinds",
                column: "verified_credential_type_kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_use_cases_use_case_id",
                schema: "issuer",
                table: "verified_credential_type_assigned_use_cases",
                column: "use_case_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_verified_credential_type_assigned_use_cases_verified_creden",
                schema: "issuer",
                table: "verified_credential_type_assigned_use_cases",
                column: "verified_credential_type_id",
                unique: true);

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240228\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL AFTER INSERT\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_company_ssi_detail20240228\" (\"id\", \"bpnl\", \"issuer_bpn\", \"verified_credential_type_id\", \"company_ssi_detail_status_id\", \"date_created\", \"creator_user_id\", \"expiry_date\", \"verified_credential_external_type_detail_version_id\", \"expiry_check_type_id\", \"process_id\", \"external_credential_id\", \"credential\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"bpnl\", \r\n  NEW.\"issuer_bpn\", \r\n  NEW.\"verified_credential_type_id\", \r\n  NEW.\"company_ssi_detail_status_id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"creator_user_id\", \r\n  NEW.\"expiry_date\", \r\n  NEW.\"verified_credential_external_type_detail_version_id\", \r\n  NEW.\"expiry_check_type_id\", \r\n  NEW.\"process_id\", \r\n  NEW.\"external_credential_id\", \r\n  NEW.\"credential\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL AFTER UPDATE\r\nON \"issuer\".\"company_ssi_details\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_INSERT_DOCUMENT$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_document20240305\" (\"id\", \"date_created\", \"document_hash\", \"document_content\", \"document_name\", \"media_type_id\", \"document_type_id\", \"document_status_id\", \"company_user_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"document_hash\", \r\n  NEW.\"document_content\", \r\n  NEW.\"document_name\", \r\n  NEW.\"media_type_id\", \r\n  NEW.\"document_type_id\", \r\n  NEW.\"document_status_id\", \r\n  NEW.\"company_user_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  1, \r\n  CURRENT_DATE, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_INSERT_DOCUMENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_INSERT_DOCUMENT AFTER INSERT\r\nON \"issuer\".\"documents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"();");

            migrationBuilder.Sql("CREATE FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"() RETURNS trigger as $LC_TRIGGER_AFTER_UPDATE_DOCUMENT$\r\nBEGIN\r\n  INSERT INTO \"issuer\".\"audit_document20240305\" (\"id\", \"date_created\", \"document_hash\", \"document_content\", \"document_name\", \"media_type_id\", \"document_type_id\", \"document_status_id\", \"company_user_id\", \"date_last_changed\", \"last_editor_id\", \"audit_v1id\", \"audit_v1operation_id\", \"audit_v1date_last_changed\", \"audit_v1last_editor_id\") SELECT NEW.\"id\", \r\n  NEW.\"date_created\", \r\n  NEW.\"document_hash\", \r\n  NEW.\"document_content\", \r\n  NEW.\"document_name\", \r\n  NEW.\"media_type_id\", \r\n  NEW.\"document_type_id\", \r\n  NEW.\"document_status_id\", \r\n  NEW.\"company_user_id\", \r\n  NEW.\"date_last_changed\", \r\n  NEW.\"last_editor_id\", \r\n  gen_random_uuid(), \r\n  2, \r\n  CURRENT_DATE, \r\n  NEW.\"last_editor_id\";\r\nRETURN NEW;\r\nEND;\r\n$LC_TRIGGER_AFTER_UPDATE_DOCUMENT$ LANGUAGE plpgsql;\r\nCREATE TRIGGER LC_TRIGGER_AFTER_UPDATE_DOCUMENT AFTER UPDATE\r\nON \"issuer\".\"documents\"\r\nFOR EACH ROW EXECUTE PROCEDURE \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_COMPANYSSIDETAIL\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_COMPANYSSIDETAIL\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_INSERT_DOCUMENT\"() CASCADE;");

            migrationBuilder.Sql("DROP FUNCTION \"issuer\".\"LC_TRIGGER_AFTER_UPDATE_DOCUMENT\"() CASCADE;");

            migrationBuilder.DropTable(
                name: "audit_company_ssi_detail20240228",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "audit_document20240305",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "company_ssi_detail_assigned_documents",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "company_ssi_process_data",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "process_steps",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "verified_credential_type_assigned_external_types",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "verified_credential_type_assigned_kinds",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "verified_credential_type_assigned_use_cases",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "company_ssi_details",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "process_step_statuses",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "process_step_types",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "verified_credential_type_kinds",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "use_cases",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "document_status",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "document_types",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "media_types",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "company_ssi_detail_statuses",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "expiry_check_types",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "processes",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "verified_credential_external_type_detail_versions",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "verified_credential_types",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "process_types",
                schema: "issuer");

            migrationBuilder.DropTable(
                name: "verified_credential_external_types",
                schema: "issuer");
        }
    }
}
