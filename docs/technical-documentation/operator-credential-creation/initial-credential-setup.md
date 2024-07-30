# Initial Credential Setup

After the initial wallet creation which is executed by the portal the process will create the bpn and membership credential.

The portal will request a bpn credential via the endpoint `POST: api/issuer/bpn` which will create a process and a process step. The process will currently fail at step 4 since the issuer wallet is the same as the holder wallet. This will be fixed in the future. For now you can execute the following query to resolve the issue

```sql

SELECT process_id
FROM issuer.company_ssi_details
where bpnl = 'operator bpn'
and verified_credential_type_id = 7

```

take the process id and insert it into the following query

```sql

UPDATE issuer.process_steps
SET process_step_status_id=2
WHERE process_step_type_id = 4 and process_step_status_id = 4;

INSERT INTO issuer.process_steps(id, process_step_type_id, process_step_status_id, process_id, date_created, date_last_changed, message)
VALUES ('8ddd7518-4532-409e-920a-c2b5029408a7', 5, 1, 'your process id', now(), null, null);

```

After that the issuer component will do the callback to the portal with the successfully created bpn credential. The portal will than request the creation of the membership credential via `POST: api/issuer/membership`. The same as for the bpn credential applies for the membership credential. The error can be fixed with the following queries

```sql

SELECT process_id
FROM issuer.company_ssi_details
where bpnl = 'operator bpn'
and verified_credential_type_id = 4

```

take the process id and insert it into the following query

```sql

UPDATE issuer.process_steps
SET process_step_status_id=2
WHERE process_step_type_id = 4 and process_step_status_id = 4;

INSERT INTO issuer.process_steps(id, process_step_type_id, process_step_status_id, process_id, date_created, date_last_changed, message)
VALUES ('8ddd7518-4532-409e-920a-c2b5029408a7', 5, 1, 'your process id', now(), null, null);

```

**Warning**: Currently the application of the wallet must be set to `catena-x-portal`. This value is not configurable and must be existing in the wallet (see [#226](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/226)).
## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer