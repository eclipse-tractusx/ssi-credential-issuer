## Known Issues and Limitations

- The database is capable of storing documents of type `PRESENTATION` through a POST API call, even though this functionality is not exposed through any specific API endpoint, indicating an undocumented feature or a future use case not yet realized.

- The DIM Status List is presently included in both the configuration file and the outbound wallet post body, which is against our recommendation as we believe this function should be autonomously managed by the wallet. The status list is defined within the component configuration, suggesting an interim solution with an intention to phase out this approach, reinforcing that the status list should not be integral to the interface in the long term.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer