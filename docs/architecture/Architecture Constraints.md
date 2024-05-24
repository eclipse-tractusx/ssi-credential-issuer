# Architecture Constraints Documentation

## Overview

The following document outlines the architecture constraints for the SSI Credential Issuer App. This App serves as a central point for credential handling, including creation, revocation, and expiration management. The constraints outlined in this document are intended to guide the development and deployment of the system to ensure it meets the specified requirements and adheres to the defined standards.

## General Constraints

### System Purpose

- **Credential Management**: The SSI Credential Issuer App is designed to manage digital credentials, handling tasks such as creation, revocation, and automatic expiration of credentials for both issuers and holders.
- **Communication**: The App facilitates communication with wallets, enabling the management of credentials.
- **No User Interface (UI)**: The current development plan does not include the implementation of a user interface. However an user interface interaction got implemented as part of the portal project.

### Deployment

- **Run Anywhere**: The system is designed to be containerized and deployable as a Docker image. This ensures it can run on various platforms, including cloud environments, on-premises infrastructure, or locally.
- **Platform-Independent**: The application is platform-independent, capable of running on Kubernetes or similar orchestration platforms.

## Developer Constraints

### Open Source Software

- **FOSS Licenses**: All software used must be open-source, with licenses approved by the Eclipse Foundation. These licenses form the initial set agreed upon by the CX community to regulate content contributions.
- **Apache License 2.0**: The Apache License 2.0 is selected as the approved license to respect and guarantee intellectual property rights.

### Development Standards

- **Coding Guidelines**: Defined coding guidelines for frontend (FE) and backend (BE) development must be followed for all portal-related developments.
- **Consistency Enforcement**: Code analysis tools, linters, and code coverage metrics are used to enforce coding standards and maintain a consistent style. These standards are enforced through the Continuous Integration (CI) process to prevent the merging of non-compliant code.

## Code Analysis and Security

To ensure code quality and security, the following analyses and checks are performed during standard reviews:

### Code Quality Checks

- **SonarCloud Code Analysis**: Automated code review tool to detect code quality issues.
- **Code Linting**: Tools to enforce coding style and detect syntax errors.
- **Code Coverage**: Metrics to ensure a sufficient percentage of the codebase is covered by automated tests.

### Security Checks

- **Thread Modelling Analysis**: Assessment of potential security threats and vulnerabilities.
- **Static Application Security Testing (SAST)**: Analysis of source code for security vulnerabilities.
- **Dynamic Application Security Testing (DAST)**: Testing of the application in its running state to find security vulnerabilities.
- **Secret Scans**: Detection of sensitive information such as passwords or API keys in the codebase.
- **Software Composition Analysis (SCA)**: Evaluation of open-source components for security risks.
- **Container Scans**: Analysis of Docker container images for vulnerabilities.
- **Infrastructure as Code (IaC)**: Analysis of infrastructure definitions for security and compliance.

## NOTICE

This work is licensed under the [Apache-2.0](https://www.apache.org/licenses/LICENSE-2.0).

- SPDX-License-Identifier: Apache-2.0
- SPDX-FileCopyrightText: 2024 Contributors to the Eclipse Foundation
- Source URL: https://github.com/eclipse-tractusx/ssi-credential-issuer
