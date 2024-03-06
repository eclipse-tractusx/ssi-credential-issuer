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

using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.AuditEntities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Attributes;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;

[AuditEntityV1(typeof(AuditCompanySsiDetail20240228))]
public class CompanySsiDetail : IAuditableV1, IBaseEntity
{
    private CompanySsiDetail()
    {
        Bpnl = null!;
        Documents = new HashSet<Document>();
    }

    public CompanySsiDetail(Guid id, string bpnl, VerifiedCredentialTypeId verifiedCredentialTypeId, CompanySsiDetailStatusId companySsiDetailStatusId, Guid creatorUserId, DateTimeOffset dateCreated)
        : this()
    {
        Id = id;
        Bpnl = bpnl;
        VerifiedCredentialTypeId = verifiedCredentialTypeId;
        CompanySsiDetailStatusId = companySsiDetailStatusId;
        CreatorUserId = creatorUserId;
        DateCreated = dateCreated;
    }

    public Guid Id { get; set; }
    public string Bpnl { get; set; }
    public VerifiedCredentialTypeId VerifiedCredentialTypeId { get; set; }
    public CompanySsiDetailStatusId CompanySsiDetailStatusId { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public Guid CreatorUserId { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    public Guid? VerifiedCredentialExternalTypeDetailVersionId { get; set; }

    public ExpiryCheckTypeId? ExpiryCheckTypeId { get; set; }
    public Guid? ProcessId { get; set; }
    public Guid? ExternalCredentialId { get; set; }
    public string? Credential { get; set; }

    [LastChangedV1]
    public DateTimeOffset? DateLastChanged { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    // Navigation Properties
    public virtual VerifiedCredentialType? VerifiedCredentialType { get; set; }
    public virtual ExpiryCheckType? ExpiryCheckType { get; set; }
    public virtual CompanySsiDetailStatus? CompanySsiDetailStatus { get; set; }
    public virtual Process? Process { get; set; }
    public virtual VerifiedCredentialExternalTypeDetailVersion? VerifiedCredentialExternalTypeDetailVersion { get; set; }
    public virtual CompanySsiProcessData? CompanySsiProcessData { get; set; }
    public virtual ICollection<Document> Documents { get; private set; }
}
