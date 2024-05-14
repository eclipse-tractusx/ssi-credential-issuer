# Whitebox Overall System

## Summary

In the following image you see the overall system overview of the SSI Credential Issuer

```mermaid
flowchart LR

    C(Customer)
    ING(Ingress)
    IS(Issuer Service)
    CS(Credential Service)
    RS(Revocation Service)
    ES(Expiry Service)
    PW(Process Worker)
    P(Portal)
    HW(Holder Wallet)
    IW(Issuer Wallet)
    PHD[("Postgres Database \n \n (Base data created with \n application seeding)")]

    subgraph SSI Credential Issuer Product   
        ING
        PHD
        IS
        CS
        RS
        ES
        PW
    end

    subgraph External Systems
        P
        HW
        IW
    end

    C-->|"Authentication & Authorization Data \n (Using JWT)"|ING
    ING-->|"Forward Request"|IS
    ING-->|"Forward Request"|CS
    ING-->|"Forward Request"|RS
    ES-->|"Revokes Credentials"|IW
    ES-->|"Revokes Credentials"|HW
    ES-->|"Creates Mails & Notifications"|P
    PW-->|"Read, Write & Sign Credentials"|IW
    PW-->|"Read & Write Credentials"|HW
    PW-->|"Creates Mails & Notifications"|P
    IS-->|"Read credentialTypes and versions, \n saves credential requests"|PHD
    ES-->|"Updates credentials"|PHD
    CS-->|"Read credentials & documents"|PHD
    PW-->|"Reads credential requests, \n saves documents"|PHD

```

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer
