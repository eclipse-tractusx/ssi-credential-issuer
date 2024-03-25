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
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;

[AuditEntityV1(typeof(AuditDocument20240305))]
public class Document : IAuditableV1, IBaseEntity
{
    private Document()
    {
        DocumentHash = null!;
        DocumentName = null!;
        DocumentContent = null!;
        CompanySsiDetails = new HashSet<CompanySsiDetail>();
    }

    public Document(Guid id, byte[] documentContent, byte[] documentHash, string documentName, MediaTypeId mediaTypeId, DateTimeOffset dateCreated, DocumentStatusId documentStatusId, DocumentTypeId documentTypeId)
        : this()
    {
        Id = id;
        DocumentContent = documentContent;
        DocumentHash = documentHash;
        DocumentName = documentName;
        DateCreated = dateCreated;
        DocumentStatusId = documentStatusId;
        DocumentTypeId = documentTypeId;
        MediaTypeId = mediaTypeId;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset DateCreated { get; private set; }

    public byte[] DocumentHash { get; set; }

    public byte[] DocumentContent { get; set; }

    [MaxLength(255)]
    public string DocumentName { get; set; }

    public MediaTypeId MediaTypeId { get; set; }

    public DocumentTypeId DocumentTypeId { get; set; }

    public DocumentStatusId DocumentStatusId { get; set; }

    public Guid? CompanyUserId { get; set; }

    [LastChangedV1]
    public DateTimeOffset? DateLastChanged { get; set; }

    [LastEditorV1]
    public Guid? LastEditorId { get; private set; }

    // Navigation properties
    public virtual DocumentType? DocumentType { get; set; }
    public virtual MediaType? MediaType { get; set; }
    public virtual DocumentStatus? DocumentStatus { get; set; }

    public virtual ICollection<CompanySsiDetail> CompanySsiDetails { get; private set; }
}
