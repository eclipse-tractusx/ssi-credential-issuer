# Dev flow with deployment to dev environment

```mermaid
flowchart LR
    subgraph local
    D(Developer)
    end
    subgraph eclipse-tractusx
        direction LR
        D -- PR* to dev*--> SCI(ssi-credential-issuer**)
        click SCI "https://github.com/eclipse-tractusx/ssi-credential-issuer"
    end
    subgraph Argo CD - sync to k8s cluster
    SCI -- auto-sync --> A(Argo CD dev)
    end
```

Note\* Every pull request (PR) requires at least one approving review by a committer

Note\*\* Unit tests and code analysis checks run at pull request

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer
