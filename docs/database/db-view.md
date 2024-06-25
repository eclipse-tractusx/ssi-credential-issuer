
# Database View

- [Database View](#database-view)
  - [Database Overview](#database-overview)
  - [Database Structure](#database-structure)
    - [COMPANY\_SSI\_DETAIL\_ASSIGNED\_DOCUMENTS](#company_ssi_detail_assigned_documents)
    - [COMPANY\_SSI\_DETAIL\_STATUSES](#company_ssi_detail_statuses)
      - [Possible Values](#possible-values)
    - [COMPANY\_SSI\_DETAILS](#company_ssi_details)
    - [COMPANY\_SSI\_PROCESS\_DATA](#company_ssi_process_data)
    - [DOCUMENT\_STATUS](#document_status)
      - [Possible Values](#possible-values-1)
    - [DOCUMENT\_TYPES](#document_types)
      - [Possible Values](#possible-values-2)
    - [DOCUMENTS](#documents)
    - [EXPIRY\_CHECK\_TYPES](#expiry_check_types)
      - [Possible Values](#possible-values-3)
    - [MEDIA\_TYPES](#media_types)
    - [PROCESS\_STEP\_STATUSES](#process_step_statuses)
      - [Possible Values](#possible-values-4)
    - [PROCESS\_STEP\_TYPES](#process_step_types)
      - [Possible Values](#possible-values-5)
    - [PROCESS\_STEPS](#process_steps)
    - [PROCESS\_TYPES](#process_types)
      - [Possible Values](#possible-values-6)
    - [PROCESSES](#processes)
    - [USE\_CASES](#use_cases)
    - [VERIFIED\_CREDENTIAL\_EXTERNAL\_TYPE\_DETAIL\_VERSIONS](#verified_credential_external_type_detail_versions)
    - [VERIFIED\_CREDENTIAL\_TYPE\_ASSIGNED\_EXTERNAL\_TYPES](#verified_credential_type_assigned_external_types)
    - [VERIFIED\_CREDENTIAL\_EXTERNAL\_TYPES](#verified_credential_external_types)
    - [VERIFIED\_CREDENTIAL\_TYPE\_ASSIGNED\_KINDS](#verified_credential_type_assigned_kinds)
    - [VERIFIED\_CREDENTIAL\_TYPE\_ASSIGNED\_USE\_CASES](#verified_credential_type_assigned_use_cases)
    - [VERIFIED\_CREDENTIAL\_TYPE\_KINDS](#verified_credential_type_kinds)
    - [VERIFIED\_CREDENTIAL\_TYPES](#verified_credential_types)
    - [Enum Value Tables](#enum-value-tables)
    - [Mapping Tables](#mapping-tables)
    - [Credentials](#credentials)
    - [Process Handling](#process-handling)
  - [NOTICE](#notice)

## Database Overview

```mermaid
erDiagram
    COMPANY_SSI_DETAIL_ASSIGNED_DOCUMENTS {
        uuid document_id PK
        uuid company_ssi_detail_id PK
    }
    COMPANY_SSI_DETAIL_STATUSES {
        integer id PK
        text label
    }
    COMPANY_SSI_DETAILS {
        uuid id PK
        text bpnl
        text issuer_bpn
        integer verified_credential_type_id FK
        integer company_ssi_detail_status_id FK
        timestamp date_created
        text creator_user_id
        timestamp expiry_date
        uuid verified_credential_external_type_detail_version_id FK
        integer expiry_check_type_id FK
        uuid process_id
        uuid external_credential_id
        text credential
        timestamp date_last_changed
        text last_editor_id
    }
    COMPANY_SSI_PROCESS_DATA {
        uuid company_ssi_detail_id PK
        jsonb schema
        integer credential_type_kind_id FK
        text client_id
        bytea client_secret
        bytea initialization_vector
        integer encryption_mode
        text holder_wallet_url
        text callback_url
    }
    DOCUMENT_STATUS {
        integer id PK
        text label
    }
    DOCUMENT_TYPES {
        integer id PK
        text label
    }
    DOCUMENTS {
        uuid id PK
        timestamp date_created
        bytea document_hash
        bytea document_content
        text document_name
        integer media_type_id FK
        integer document_type_id FK
        integer document_status_id FK
        text identity_id
        timestamp date_last_changed
        text last_editor_id
    }
    EXPIRY_CHECK_TYPES {
        integer id PK
        text label
    }
    MEDIA_TYPES {
        integer id PK
        text label
    }
    PROCESS_STEP_STATUSES {
        integer id PK
        text label
    }
    PROCESS_STEP_TYPES {
        integer id PK
        text label
    }
    PROCESS_STEPS {
        uuid id PK
        integer process_step_type_id FK
        integer process_step_status_id FK
        uuid process_id FK
        timestamp date_created
        timestamp date_last_changed
        text message
    }
    PROCESS_TYPES {
        integer id PK
        text label
    }
    PROCESSES {
        uuid id PK
        integer process_type_id FK
        timestamp lock_expiry_date
        uuid version
    }
    USE_CASES {
        uuid id PK
        text name
        text shortname
    }
    VERIFIED_CREDENTIAL_EXTERNAL_TYPE_DETAIL_VERSIONS {
        uuid id PK
        integer verified_credential_external_type_id FK
        text version
        text template
        timestamp valid_from
        timestamp expiry
    }
    VERIFIED_CREDENTIAL_TYPE_ASSIGNED_EXTERNAL_TYPES {
        int verified_credential_type_id PK
        int verified_credential_external_type_id PK
    }
    VERIFIED_CREDENTIAL_EXTERNAL_TYPES {
        integer id PK
        text label
    }
    VERIFIED_CREDENTIAL_TYPE_ASSIGNED_KINDS {
        int verified_credential_type_id PK
        int verified_credential_type_kind_id PK
    }
    VERIFIED_CREDENTIAL_TYPE_ASSIGNED_USE_CASES {
        int verified_credential_type_id PK
        uuid use_case_id PK
    }
    VERIFIED_CREDENTIAL_TYPE_KINDS {
        integer id PK
        text label
    }
    VERIFIED_CREDENTIAL_TYPES {
        integer id PK
        text label
    }
    COMPANY_SSI_DETAIL_ASSIGNED_DOCUMENTS ||--|| COMPANY_SSI_DETAILS : company_ssi_detail_id
    COMPANY_SSI_DETAIL_ASSIGNED_DOCUMENTS ||--|| DOCUMENTS : document_id
    COMPANY_SSI_DETAILS ||--|| VERIFIED_CREDENTIAL_TYPES : verified_credential_type_id
    COMPANY_SSI_DETAILS ||--|| COMPANY_SSI_DETAIL_STATUSES : company_ssi_detail_status_id
    COMPANY_SSI_DETAILS ||--|| VERIFIED_CREDENTIAL_EXTERNAL_TYPE_DETAIL_VERSIONS : verified_credential_external_type_detail_version_id
    COMPANY_SSI_DETAILS ||--|| EXPIRY_CHECK_TYPES : expiry_check_type_id
    COMPANY_SSI_DETAILS ||--|| PROCESSES : process_id
    COMPANY_SSI_PROCESS_DATA ||--|| COMPANY_SSI_DETAILS : company_ssi_detail_id
    COMPANY_SSI_PROCESS_DATA ||--|| VERIFIED_CREDENTIAL_TYPE_KINDS : credential_type_kind_id
    DOCUMENTS ||--|| DOCUMENT_STATUS : document_status_id
    DOCUMENTS ||--|| DOCUMENT_TYPES : document_type_id
    PROCESS_STEPS ||--|| PROCESS_STEP_STATUSES : process_step_status_id
    PROCESS_STEPS ||--|| PROCESS_STEP_TYPES : process_step_type_id
    PROCESS_STEPS ||--|| PROCESSES : process_id
    PROCESSES ||--|| PROCESS_TYPES : process_type_id
    VERIFIED_CREDENTIAL_EXTERNAL_TYPE_DETAIL_VERSIONS ||--|| VERIFIED_CREDENTIAL_EXTERNAL_TYPES : verified_credential_external_type_id
    VERIFIED_CREDENTIAL_TYPE_ASSIGNED_EXTERNAL_TYPES ||--|| VERIFIED_CREDENTIAL_EXTERNAL_TYPES : has
    VERIFIED_CREDENTIAL_TYPE_ASSIGNED_EXTERNAL_TYPES ||--|| VERIFIED_CREDENTIAL_TYPES : has
    VERIFIED_CREDENTIAL_TYPE_ASSIGNED_KINDS ||--|| VERIFIED_CREDENTIAL_TYPES : verified_credential_type_id
    VERIFIED_CREDENTIAL_TYPE_ASSIGNED_KINDS ||--|| VERIFIED_CREDENTIAL_TYPE_KINDS : verified_credential_type_kind_id
    VERIFIED_CREDENTIAL_TYPE_ASSIGNED_USE_CASES ||--|| USE_CASES : use_case_id
    VERIFIED_CREDENTIAL_TYPE_ASSIGNED_USE_CASES ||--|| VERIFIED_CREDENTIAL_TYPES : verified_credential_type_id

```

## Database Structure

The database is organized into several key tables, each serving a specific purpose:

### COMPANY_SSI_DETAIL_ASSIGNED_DOCUMENTS

document_id (UUID): A unique identifier for the document. This is a primary key and a foreign key referencing id in the DOCUMENTS table.
company_ssi_detail_id (UUID): A unique identifier for the company SSI detail. This is a primary key and a foreign key referencing id in the COMPANY_SSI_DETAILS table.

### COMPANY_SSI_DETAIL_STATUSES

id (INTEGER): A unique identifier for the status. This is the primary key of the table.
label (TEXT): The label of the status.

#### Possible Values

- `PENDING`: The credential is pending.
- `ACTIVE`: The credential is active.
- `REVOKED`: The credential was revoked, either by the holder or by the issuer.
- `INACTIVE`: The credential is inactive.

### COMPANY_SSI_DETAILS

id (UUID): A unique identifier for the company SSI detail. This is the primary key of the table.
bpnl (TEXT): The BP number of the company.
issuer_bpn (TEXT): The BP number of the issuer.
verified_credential_type_id (INTEGER): A foreign key referencing id in the VERIFIED_CREDENTIAL_TYPES table.
company_ssi_detail_status_id (INTEGER): A foreign key referencing id in the COMPANY_SSI_DETAIL_STATUSES table.
date_created (TIMESTAMP): The timestamp when the company SSI detail was created.
creator_user_id (TEXT): The user ID of the creator.
expiry_date (TIMESTAMP): The expiry date of the company SSI detail.
verified_credential_external_type_detail_version_id (UUID): A foreign key referencing id in the VERIFIED_CREDENTIAL_EXTERNAL_TYPE_DETAIL_VERSIONS table.
expiry_check_type_id (INTEGER): A foreign key referencing id in the EXPIRY_CHECK_TYPES table.
process_id (UUID): A foreign key referencing id in the PROCESSES table.
external_credential_id (UUID): A unique identifier for the external credential.
credential (TEXT): The credential information.
date_last_changed (TIMESTAMP): The timestamp when the company SSI detail was last changed.
last_editor_id (TEXT): The user ID of the last editor.

### COMPANY_SSI_PROCESS_DATA

company_ssi_detail_id (UUID): A unique identifier for the company SSI detail. This is the primary key and a foreign key referencing id in the COMPANY_SSI_DETAILS table.
schema (JSONB): The schema of the credential.
credential_type_kind_id (INTEGER): A foreign key referencing id in the VERIFIED_CREDENTIAL_TYPE_KINDS table.
client_id (TEXT): The client ID.
client_secret (BYTEA): The client secret.
initialization_vector (BYTEA): The initialization vector for encryption.
encryption_mode (INTEGER): The encryption mode.
holder_wallet_url (TEXT): The URL of the holder's wallet.
callback_url (TEXT): The callback URL.

### DOCUMENT_STATUS

id (INTEGER): A unique identifier for the document status. This is the primary key of the table.
label (TEXT): The label of the document status.
DOCUMENT_TYPES

#### Possible Values

- `ACTIVE`: The document is active.
- `INACTIVE`: The document is inactive.

### DOCUMENT_TYPES

id (INTEGER): A unique identifier for the document type. This is the primary key of the table.
label (TEXT): The label of the document type.

#### Possible Values

- `PRESENTATION`: Represents a presentation document uploaded by the customer/requester to present a proof of certification etc.
- `CREDENTIAL`: Represents a credential document created by the issuer (unsigned).
- `VERIFIED_CREDENTIAL`: Represents a verified credential document (signed by the issuer wallet and official credential document).

### DOCUMENTS

id (UUID): A unique identifier for the document. This is the primary key of the table.
date_created (TIMESTAMP): The timestamp when the document was created.
document_hash (BYTEA): The hash of the document content for verification.
document_content (BYTEA): The binary content of the document.
document_name (TEXT): The name of the document.
media_type_id (INTEGER): A foreign key referencing id in the MEDIA_TYPES table.
document_type_id (INTEGER): A foreign key referencing id in the DOCUMENT_TYPES table.
document_status_id (INTEGER): A foreign key referencing id in the DOCUMENT_STATUS table.
identity_id (TEXT): The identity ID associated with the document.
date_last_changed (TIMESTAMP): The timestamp when the document was last changed.
last_editor_id (TEXT): The user ID of the last editor.

### EXPIRY_CHECK_TYPES

id (INTEGER): A unique identifier for the expiry check type. This is the primary key of the table.
label (TEXT): The label of the expiry check type.

#### Possible Values

- `ONE_MONTH`: The expiry check was done one month prior to the expiry of the credential.
- `TWO_WEEKS`: The expiry check was done two weeks prior to the expiry of the credential.
- `ONE_DAY`: The expiry check was done one month prior to the expiry of the credential.

### MEDIA_TYPES

id (INTEGER): A unique identifier for the media type. This is the primary key of the table.
label (TEXT): The label of the media type.

### PROCESS_STEP_STATUSES

id (INTEGER): A unique identifier for the process step status. This is the primary key of the table.
label (TEXT): The label of the process step status.

#### Possible Values

- `TODO`: The process step is still to be executed.
- `DONE`: The process step was already executed successfully.
- `SKIPPED`: The execution of the process step was skipped.
- `FAILED`: The process step execution failed due to an error.
- `DUPLICATE`: The process step did already exist.

### PROCESS_STEP_TYPES

id (INTEGER): A unique identifier for the process step type. This is the primary key of the table.
label (TEXT): The label of the process step type.

#### Possible Values

- `CREATE_CREDENTIAL`: Creates a credential in the issuer wallet.
- `SIGN_CREDENTIAL`: Signs the credential in the issuer wallet.
- `SAVE_CREDENTIAL_DOCUMENT`: Saves the credential in the database.
- `CREATE_CREDENTIAL_FOR_HOLDER`: Creates the credential in the holder wallet.
- `TRIGGER_CALLBACK`: Triggers the callback to the portal.
- `REVOKE_CREDENTIAL`: Revokes the credential.
- `TRIGGER_NOTIFICATION`: Triggers the notification sending.
- `TRIGGER_MAIL`: Triggers the mail sending.

### PROCESS_STEPS

id (UUID): A unique identifier for the process step. This is the primary key of the table.
process_step_type_id (INTEGER): A foreign key referencing id in the PROCESS_STEP_TYPES table.
process_step_status_id (INTEGER): A foreign key referencing id in the PROCESS_STEP_STATUSES table.
process_id (UUID): A foreign key referencing id in the PROCESSES table.
date_created (TIMESTAMP): The timestamp when the process step was created.
date_last_changed (TIMESTAMP): The timestamp when the process step was last changed.
message (TEXT): A message associated with the process step.

### PROCESS_TYPES

id (INTEGER): A unique identifier for the process type. This is the primary key of the table.
label (TEXT): The label of the process type.

#### Possible Values

- `CREATE_CREDENTIAL`: Process to create credentials.
- `DECLINE_CREDENTIAL`: Process to revoke credentials.

### PROCESSES

id (UUID): A unique identifier for the process. This is the primary key of the table.
process_type_id (INTEGER): A foreign key referencing id in the PROCESS_TYPES table.
lock_expiry_date (TIMESTAMP): The lock expiry date of the process.
version (UUID): The version of the process.

### USE_CASES

id (UUID): A unique identifier for the use case. This is the primary key of the table.
name (TEXT): The name of the use case.
shortname (TEXT): The short name of the use case.

### VERIFIED_CREDENTIAL_EXTERNAL_TYPE_DETAIL_VERSIONS

id (UUID): A unique identifier for the external type detail version. This is the primary key of the table.
verified_credential_external_type_id (INTEGER): A foreign key referencing id in the VERIFIED_CREDENTIAL_EXTERNAL_TYPES table.
version (TEXT): The version of the external type detail.
template (TEXT): The template url of the external type detail.
valid_from (TIMESTAMP): The validity start date of the external type detail version.
expiry (TIMESTAMP): The expiry date of the external type detail version.

### VERIFIED_CREDENTIAL_TYPE_ASSIGNED_EXTERNAL_TYPES

verified_credential_type_id (INTEGER): A unique identifier for the verified credential type. This is a primary key and a foreign key referencing id in the VERIFIED_CREDENTIAL_TYPES table.
verified_credential_external_type_id (INTEGER): A unique identifier for the verified credential external type. This is a primary key and a foreign key referencing id in the VERIFIED_CREDENTIAL_EXTERNAL_TYPES table.

### VERIFIED_CREDENTIAL_EXTERNAL_TYPES

id (INTEGER): A unique identifier for the external type. This is the primary key of the table.
label (TEXT): The label of the external type.

### VERIFIED_CREDENTIAL_TYPE_ASSIGNED_KINDS

verified_credential_type_id (INTEGER): A unique identifier for the verified credential type. This is a primary key and a foreign key referencing id in the VERIFIED_CREDENTIAL_TYPES table.
verified_credential_type_kind_id (INTEGER): A unique identifier for the verified credential type kind. This is a primary key and a foreign key referencing id in the VERIFIED_CREDENTIAL_TYPE_KINDS table.

### VERIFIED_CREDENTIAL_TYPE_ASSIGNED_USE_CASES

verified_credential_type_id (INTEGER): A unique identifier for the verified credential type. This is a primary key and a foreign key referencing id in the VERIFIED_CREDENTIAL_TYPES table.
use_case_id (UUID): A unique identifier for the use case. This is a primary key and a foreign key referencing id in the USE_CASES table.

### VERIFIED_CREDENTIAL_TYPE_KINDS

id (INTEGER): A unique identifier for the credential type kind. This is the primary key of the table.
label (TEXT): The label of the credential type kind.

### VERIFIED_CREDENTIAL_TYPES

id (INTEGER): A unique identifier for the credential type. This is the primary key of the table.
label (TEXT): The label of the credential type.

### Enum Value Tables

`company_ssi_detail_status`, `document_status`, `document_types`, `expiry_check_types`, `media_types`, `process_step_statuses`, `process_step_types`, `process_steps`, `process_types`, `verified_credential_external_types`, `verified_credential_type_kinds`, `verified_credential_types` are tables designed to store enum values. They contain an id and label, derived from the backend enums.

### Mapping Tables

`company_ssi_detail_assigned_documents` and `verified_credential_type_assigned_external_types`, `verified_credential_type_assigned_kinds`, `verified_credential_type_assigned_use_cases` are used to map entities.

### Credentials

The `company_ssi_details` table is utilized to safe the credential requests and set their status.

### Process Handling

The tables `processes`, `process_steps` are used for the processing of the credential creation and revocation.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer
