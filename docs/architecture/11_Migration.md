# Database Migration Documentation

## Overview

This document describes the database migration structure and history for the SSI Credential Issuer project. The project uses Entity Framework Core with PostgreSQL as the database system.

## Database Schema Details

### Schema Information
- Name: `issuer`
- Default Collation: `en_US.utf8`
- Database Provider: PostgreSQL

### Core Tables

#### 1. company_ssi_details
- Primary Key: `id` (uuid)
- Purpose: Stores company SSI credential information
- Fields:
  - `bpnl` (text, required)
  - `issuer_bpn` (text, required)
  - `verified_credential_type_id` (integer, required)
  - `company_ssi_detail_status_id` (integer, required)
  - `date_created` (timestamp with time zone, required)
  - `creator_user_id` (text, required)
  - `expiry_date` (timestamp with time zone, nullable)
  - `verified_credential_external_type_detail_version_id` (uuid, nullable)
  - `expiry_check_type_id` (integer, nullable)
  - `process_id` (uuid, nullable)
  - `external_credential_id` (uuid, nullable)
  - `credential` (text, nullable)
  - `date_last_changed` (timestamp with time zone, nullable)
  - `last_editor_id` (text, nullable)
  - `credential_request_id` (uuid, nullable)
  - `credential_request_status` (text, nullable)

#### 2. audit_company_ssi_detail20240618
- Primary Key: `audit_v2id` (uuid)
- Purpose: Stores audit history for company_ssi_details
- Fields: Includes all company_ssi_details fields plus audit metadata

#### 3. process_step_types
- Primary Key: `id` (integer)
- Fields:
  - `label` (text, required)
- Predefined Values:
  - REQUEST_CREDENTIAL_FOR_HOLDER (id: 10)
  - RETRIGGER_REQUEST_CREDENTIAL_FOR_HOLDER (id: 11)
  - REQUEST_CREDENTIAL_STATUS_CHECK (id: 12)
  - RETRIGGER_REQUEST_CREDENTIAL_STATUS_CHECK (id: 13)
  - REQUEST_CREDENTIAL_AUTO_APPROVE (id: 14)
  - RETRIGGER_REQUEST_CREDENTIAL_AUTO_APPROVE (id: 15)

## Migration History

### Version 1.5.0-rc.1 (20250618110327)
- Added credential request tracking fields to company_ssi_details table:
  - `credential_request_id` (uuid, nullable)
  - `credential_request_status` (text, nullable)
- Created new audit table `audit_company_ssi_detail20240618` with updated schema including the new credential request fields
- Added new process step types (IDs 10-15) for credential request workflows
- Updated database triggers for insert and update operations on company_ssi_details to include new fields in audit records

## Best Practices

### 1. Naming Conventions

- Database Objects
  - Use `snake_case` naming convention
  - Examples:
    - Tables: `company_ssi_details`, `audit_company_ssi_detail20240618`
    - Columns: `credential_request_id`, `verified_credential_type_id`

- Constraints and Indexes
  - Primary Keys: prefix with `pk_`
    - Example: `pk_audit_company_ssi_detail20240618`
  - Foreign Keys: prefix with `fk_`
    - Example: `fk_company_ssi_details_verified_credential_type_id`
  - Indexes: prefix with `ix_`
    - Example: `ix_company_ssi_details_bpnl`

### 2. Data Integrity

- Foreign Key Constraints
  - Explicitly define relationships between tables
  - Use appropriate ON DELETE rules
    - Cascade: When child records should be deleted with parent
    - No Action: When referential integrity should prevent deletion
  - Example:
    ```sql
    FOREIGN KEY (verified_credential_type_id) REFERENCES credential_types(id) ON DELETE CASCADE
    ```

- Index Strategy
  - Create indexes on:
    - Foreign key columns
    - Frequently queried columns
    - Search/filter fields
  - Consider composite indexes for multi-column queries

### 3. Version Control

- Migration Management
  - Each migration is a separate file with unique timestamp
  - Files contain both Up() and Down() methods for reversibility
  - Clear naming convention: `{Timestamp}_{Description}.cs`

- Documentation Requirements
  - Purpose of migration
  - List of changes
  - Any data transformations
  - Breaking changes or special deployment considerations

## Migration Management

### Creating New Migrations
```bash
dotnet ef migrations add MigrationName --project src/database/SsiCredentialIssuer.Migrations
```

### Applying Migrations
```bash
dotnet ef database update --project src/database/SsiCredentialIssuer.Migrations
```

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2025 Contributors to the Eclipse Foundation
- Source URL: <https://github.com/eclipse-tractusx/ssi-credential-issuer>