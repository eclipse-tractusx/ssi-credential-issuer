# Building Block View

## Summary

In the following image you see the overall system overview of the SSI Credential Issuer

```mermaid
flowchart LR

    C(Customer)
    ING(Ingress)
    IS(Issuer Service)
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
    ES-->|"Revokes Credentials"|IW
    ES-->|"Revokes Credentials"|HW
    ES-->|"Creates Mails & Notifications"|P
    ES-->|"Reads & updates credentials"|PHD
    PW-->|"Read, Write & Sign Credentials"|IW
    PW-->|"Write Credentials"|HW
    PW-->|"Creates Mails & Notifications"|P
    PW-->|"Reads credential requests, \n saves documents"|PHD
    IS-->|"Read credentialTypes and versions, \n saves credential requests, \n revokes credential"|PHD
    ES-->|"Updates credentials"|PHD

```

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer
