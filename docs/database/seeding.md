# Seeding Mechanism

## Database Seeding

All data for the SSI Credential Issuer are stored in the database. A seeding process has been implemented to populate the database with initial data.

## Execution

The seeding process is triggered by the SsiCredentialIssuer.Migrations job. During this process, data is sourced from .json files located in a configurable directory. The default directory for the base setup is Seeder -> Data.

## Configuration

To specify the data to be seeded, configure the `BatchInsertSeeder`. This configuration includes the following details:

- Database Table: Identify the target database table
- File Name: Specify the name of the .json file containing the data
- Primary Keys: The primary keys which check for existing entries

To specify the data which should be modified within the seeding, configure the `BatchUpdateSeeder`. This configuration includes the following details:

- Database Table: Identify the target database table
- File Name: Specify the name of the .json file containing the data
- Primary Keys: The primary keys which check for existing entries
- Where Clause: Check for the entries in the table that should be modified
- Update Entries: The function that should be executed to modify the entries.

## Data Integrity

The seeder includes a check to ensure that only data not yet existing in the database is written. This prevents duplication of records.

## Limitations

- While the seeder is designed to add new data, it does not support deletion. Any existing records in the database will not be removed by the seeding process.
- Modifications of the enum values must be made with a migration.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer
