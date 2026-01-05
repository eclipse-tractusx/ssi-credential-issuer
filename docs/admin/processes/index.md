# Summary

The main process worker project is the `Processes.Worker` which runs all the processes. It therefor looks up `process_steps` in status `TODO` and their respective `processes` and executes those.

## Processes

The process worker supports the following processes:

- [CreateCredential](../processes/01.%20create_credential.md) - handles the creation of new credentials
- [DeleteCredential](../processes/02.%20delete_credential.md) - handles the revocation of credentials
- [ReissueCredential](../processes/03.%20reissue_credential.md) - handles the reissuance of expiring credentials

## Retriggering

The process has a logic to retrigger failing steps. For this a retrigger step is created which can be triggered via an api call to retrigger the step. This logic is implemented separately for each process. In general the retriggering of a step is possible if for example external services are not available. The retrigger logic for each process can be found in the process file.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer
