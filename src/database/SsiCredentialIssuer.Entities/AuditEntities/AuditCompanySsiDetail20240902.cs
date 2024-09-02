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

using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.AuditEntities;

public class AuditCompanySsiDetail20240902 : IAuditEntityV2
{
    /// <inheritdoc />
    [Key]
    public Guid AuditV2Id { get; set; }

    public Guid Id { get; set; }
    public string Bpnl { get; set; } = null!;
    public string IssuerBpn { get; set; } = null!;
    public VerifiedCredentialTypeId VerifiedCredentialTypeId { get; set; }
    public CompanySsiDetailStatusId CompanySsiDetailStatusId { get; set; }
    public DateTimeOffset DateCreated { get; private set; }
    public string CreatorUserId { get; set; } = null!;
    public DateTimeOffset? ExpiryDate { get; set; }
    public Guid? VerifiedCredentialExternalTypeDetailVersionId { get; set; }

    public ExpiryCheckTypeId? ExpiryCheckTypeId { get; set; }
    public Guid? ProcessId { get; set; }
    public Guid? ExternalCredentialId { get; set; }
    public string? Credential { get; set; }
    public Guid? ReissuedCredentialId { get; set; }
    public DateTimeOffset? DateLastChanged { get; set; }
    public string? LastEditorId { get; set; }

    /// <inheritdoc />
    public string? AuditV2LastEditorId { get; set; }

    /// <inheritdoc />
    public AuditOperationId AuditV2OperationId { get; set; }

    /// <inheritdoc />
    public DateTimeOffset AuditV2DateLastChanged { get; set; }
}
