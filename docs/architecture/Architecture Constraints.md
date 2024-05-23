# Architecture Constraints

## General

- The SSI Credential Issuer is a central API for the handling of credentials. It handles the wallet communication for the creation and revocation of credentials of the issuer and holder. Another purpose is the expiry handling and automatic revocation of already expired credentials. There is no plan to implement an UI at the current stage.

- Run anywhere: can be deployed as a docker image, e. g. on Kubernetes (platform-independent, cloud, on prem or local).

## Developer

- OpenSource software first - FOSS licenses approved by the eclipse foundation has to be used. It could represent the initial set that the CX community agrees on to regulate the content contribution under FOSS licenses.

- Coding guidelines for FE and BE are defined and are to be followed for all portal related developments.

- Apache License 2.0 - Apache License 2.0 is one of the approved licenses which should be used to respect and guarantee Intellectual property (IP).

- Code Analysis, Linting and Code Coverage - Consistent style increases readability and maintainability of the code base. Hence, we use analyzers to enforce consistency and style rules. We enforce the code style and rules in the CI to avoid merging code that does not comply with standards.

## Code analysis, linting and code coverage

As part of the standard reviews, following code analysis and security checks have been executed:

- SonarCloud Code Analysis
- Thread Modelling Analysis
- Static Application Security Testing (SAST)
- Dynamic Application Security Testing (DAST)
- Secret Scans
- Software Composition Analysis (SCA)
- Container Scans
- Infrastructure as Code (IaC)

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer
