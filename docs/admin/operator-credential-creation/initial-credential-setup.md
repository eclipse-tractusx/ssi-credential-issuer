# Initial Credential Setup

After the initial wallet creation which is executed by the portal the process will create the bpn and membership credential.

The portal will request a bpn credential via the endpoint `POST: api/issuer/bpn` which will create a process and a process step.
After the process has successfully finished the issuer component will do the callback to the portal with the successfully created bpn credential. The portal will than request the creation of the membership credential via `POST: api/issuer/membership`. The same as for the bpn credential applies for the membership credential.

**Note**: Since the issuer and holder of the credentials are the same, process step `OFFER_CREDENTIAL_TO_HOLDER` will be skipped.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer