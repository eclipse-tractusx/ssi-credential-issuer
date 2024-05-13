# Security Assessment SSI Credential Issuer

|                           |                                                                                                |
| ------------------------- | ---------------------------------------------------------------------------------------------- |
| Contact for product       | [@evegufy](https://github.com/evegufy) <br> [@jjeroch](https://github.com/jjeroch)             |
| Security responsible      | tbd |
| Version number of product | 1.0.0                                                                                          |
| Dates of assessment       | tbd: Assessment                                                                      |
| Status of assessment      | Assessment Report                                                                            |

## Product Description

The SSI Credential Issuer product is an REST API project with two Process Worker processes, so a pure backend component (without implementation of an user interface).

The main purpose of the product is to provide authenticated CX Users the possibility to create credentials inside the issuer and holder wallet. Furthermore, it handles the revocation and expiry handling for credentials.

The SSI Credential Issuer comprises the technical foundation for functional interaction, monitoring, auditing and further functionalities.

The product can be run anywhere: it can be deployed as a docker image, e.g. on Kubernetes (platform-independent, cloud, on prem or local).

The SSI Credential Issuer is using following key frameworks:

- .Net
- Entity Framework
[Development Concept](/Development%20Concept.md)

## Data Flow Diagram

```mermaid
flowchart LR

    CU(Company user or Service Account)
    K("Keycloak (REST API)")
    IS(Issuer Service)
    CS(Credential Service)
    RS(Revocation Service)
    EW(Expiry Worker)
    IW(Issuer Wallet)
    HW(3rd Party Holder Wallets)
    P(Portal Backend)
    PHD[(Issuer DB \n Postgres \n EF Core for mapping \n objects to SQL)]

    subgraph centralidp[centralidp Keycloak]
     K
    end

    subgraph companyrealm[SharedIdP Keycloak or ownIdP]
     CU
    end

    subgraph SSI-Issuer-Component Product
     IS
     CS
     RS
     EW
     PHD
    end

    subgraph External Systems
     P
     IW
     HW
    end

    K-->|"Authentication & Authorization Data \n (Using JWT)"|IS
    K-->|"Authentication & Authorization Data \n (Using JWT)"|CS
    K-->|"Authentication & Authorization Data \n (Using JWT)"|RS
    CU-->|"Consumption of central, read-only REST API \n [HTTPS]"|IS
    CU-->|"Consumption of central, read-only REST API \n [HTTPS]"|CS
    CU-->|"Consumption of central, read-only REST API \n [HTTPS]"|RS
    IS-->|"Read and write credentials"|PHD
    IS-->|"Read and write credentials"|IW
    IS-->|"Read and write credentials"|HW
    EW-->|"Read and write credentials"|IW
    RS-->|"Read and write credentials"|IW
    P-->|"Create and revoke credentials"|IS
    IS-->|"Create notifications and mails"|P
    CS-->|"Read credentials and document"|PHD
    RS-->|"Read and update credential data"|PHD
    CU-->|"IAM with OIDC \n [HTTPS]"|K
```

### Changes compared to last Security Assessment

N/A

### Features for Upcoming Versions

N/A

## Threats & Risks

TBD

### Mitigated Threats

N/A

### Performed Security Checks

- Static Application Security Testing (SAST) - CodeQL
- Software Composition Analysis (SCA) - Dependabot
- Container Scan conducted - Trivy
- Infrastructure as Code - KICS
- Secret Scanning - GitGuardian
- Dynamic Application Security Testing (DAST) - OWASP ZAP (Unauthenticated)

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer
