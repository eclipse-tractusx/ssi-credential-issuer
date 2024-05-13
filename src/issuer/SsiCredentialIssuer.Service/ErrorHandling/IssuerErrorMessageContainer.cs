/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;

[ExcludeFromCodeCoverage]
public class IssuerErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<IssuerErrors, string> {
        { IssuerErrors.INVALID_COMPANY, "company {companyId} is not a valid company" },
        { IssuerErrors.INVALID_COMPANY_STATUS, "Company Status is Incorrect" },
        { IssuerErrors.USE_CASE_NOT_FOUND, "UseCaseId {useCaseId} is not available" },
        { IssuerErrors.INVALID_LANGUAGECODE, "language {languageShortName} is not a valid languagecode" },
        { IssuerErrors.COMPANY_NOT_FOUND, "company {companyId} does not exist" },
        { IssuerErrors.COMPANY_ROLE_IDS_CONSENT_STATUS_NULL, "neither CompanyRoleIds nor ConsentStatusDetails should ever be null here" },
        { IssuerErrors.MISSING_AGREEMENTS, "All agreements need to get signed as Active or InActive. Missing consents: [{missingConsents}]" },
        { IssuerErrors.UNASSIGN_ALL_ROLES, "Company can't unassign from all roles, Atleast one Company role need to signed as active" },
        { IssuerErrors.AGREEMENTS_NOT_ASSIGNED_WITH_ROLES, "Agreements not associated with requested companyRoles: [{companyRoles}]" },
        { IssuerErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND, "VerifiedCredentialExternalTypeDetail {verifiedCredentialExternalTypeDetailId} does not exist" },
        { IssuerErrors.EXPIRY_DATE_IN_PAST, "The expiry date must not be in the past" },
        { IssuerErrors.CREDENTIAL_NO_CERTIFICATE, "{credentialTypeId} is not assigned to a certificate" },
        { IssuerErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET, "The VerifiedCredentialExternalTypeDetailId must be set" },
        { IssuerErrors.CREDENTIAL_ALREADY_EXISTING, "Credential request already existing" },
        { IssuerErrors.CREDENTIAL_TYPE_NOT_FOUND, "VerifiedCredentialType {verifiedCredentialType} does not exists" },
        { IssuerErrors.SSI_DETAILS_NOT_FOUND, "CompanySsiDetail {credentialId} does not exists" },
        { IssuerErrors.CREDENTIAL_NOT_PENDING, "Credential {credentialId} must be {status}" },
        { IssuerErrors.BPN_NOT_SET, "Bpn should be set for company" },
        { IssuerErrors.EXPIRY_DATE_NOT_SET, "Expiry date must always be set for use cases" },
        { IssuerErrors.EMPTY_VERSION, "External Detail Version must not be null" },
        { IssuerErrors.EMPTY_TEMPLATE, "Template must not be null" },
        { IssuerErrors.KIND_NOT_SUPPORTED, "{kind} is currently not supported" },
        { IssuerErrors.MULTIPLE_USE_CASES, "There must only be one use case" },
        { IssuerErrors.DID_NOT_SET, "Did must not be null" },
        { IssuerErrors.ALREADY_LINKED_PROCESS, "Credential should not already be linked to a process" },
        { IssuerErrors.INVALID_DID_LOCATION, "The did url location must be a valid url" },
        { IssuerErrors.EMPTY_EXTERNAL_TYPE_ID, "External Type ID must be set" },
        { IssuerErrors.SCHEMA_NOT_SET, "The json schema must be set when approving a credential" },
        { IssuerErrors.SCHEMA_NOT_FRAMEWORK, "The schema must be a framework credential" },
        { IssuerErrors.PENDING_CREDENTIAL_ALREADY_EXISTS, "Pending Credential request for version {versionId} and framework {frameworkId} does already exist" }
    }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(IssuerErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum IssuerErrors
{
    INVALID_COMPANY,
    INVALID_COMPANY_STATUS,
    USE_CASE_NOT_FOUND,
    INVALID_LANGUAGECODE,
    COMPANY_NOT_FOUND,
    COMPANY_ROLE_IDS_CONSENT_STATUS_NULL,
    MISSING_AGREEMENTS,
    UNASSIGN_ALL_ROLES,
    AGREEMENTS_NOT_ASSIGNED_WITH_ROLES,
    EXTERNAL_TYPE_DETAIL_NOT_FOUND,
    EXPIRY_DATE_IN_PAST,
    CREDENTIAL_NO_CERTIFICATE,
    EXTERNAL_TYPE_DETAIL_ID_NOT_SET,
    CREDENTIAL_ALREADY_EXISTING,
    CREDENTIAL_TYPE_NOT_FOUND,
    SSI_DETAILS_NOT_FOUND,
    CREDENTIAL_NOT_PENDING,
    BPN_NOT_SET,
    EXPIRY_DATE_NOT_SET,
    EMPTY_VERSION,
    EMPTY_TEMPLATE,
    KIND_NOT_SUPPORTED,
    MULTIPLE_USE_CASES,
    DID_NOT_SET,
    ALREADY_LINKED_PROCESS,
    INVALID_DID_LOCATION,
    EMPTY_EXTERNAL_TYPE_ID,
    SCHEMA_NOT_SET,
    SCHEMA_NOT_FRAMEWORK,
    PENDING_CREDENTIAL_ALREADY_EXISTS
}
