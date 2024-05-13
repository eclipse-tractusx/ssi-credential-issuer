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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;

public interface IIssuerBusinessLogic
{
    IAsyncEnumerable<UseCaseParticipationData> GetUseCaseParticipationAsync();

    IAsyncEnumerable<CertificateParticipationData> GetSsiCertificatesAsync();

    Task<Pagination.Response<CredentialDetailData>> GetCredentials(int page, int size, CompanySsiDetailStatusId? companySsiDetailStatusId, VerifiedCredentialTypeId? credentialTypeId, CompanySsiDetailApprovalType? approvalType, CompanySsiDetailSorting? sorting);
    IAsyncEnumerable<OwnedVerifiedCredentialData> GetCredentialsForBpn();

    Task ApproveCredential(Guid credentialId, CancellationToken cancellationToken);

    Task RejectCredential(Guid credentialId, CancellationToken cancellationToken);

    IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes();
    Task<Guid> CreateBpnCredential(CreateBpnCredentialRequest requestData, CancellationToken cancellationToken);
    Task<Guid> CreateMembershipCredential(CreateMembershipCredentialRequest requestData, CancellationToken cancellationToken);
    Task<Guid> CreateFrameworkCredential(CreateFrameworkCredentialRequest requestData, CancellationToken cancellationToken);
}
