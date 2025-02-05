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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;

public interface ICompanySsiDetailsRepository
{
    /// <summary>
    /// Gets the company credential details for the given company id
    /// </summary>
    /// <param name="bpnl">Bpnl of the company</param>
    /// <param name="minExpiry">The minimum datetime the expiry date should have</param>
    /// <returns>AsyncEnumerable of UseCaseParticipation</returns>
    IAsyncEnumerable<UseCaseParticipationData> GetUseCaseParticipationForCompany(string bpnl, DateTimeOffset minExpiry);

    /// <summary>
    /// Gets the company credential details for the given company id
    /// </summary>
    /// <param name="bpnl">Bpnl of the company</param>
    /// <param name="minExpiry">The minimum datetime the expiry date should have</param>
    /// <returns>AsyncEnumerable of SsiCertificateData</returns>
    IAsyncEnumerable<CertificateParticipationData> GetSsiCertificates(string bpnl, DateTimeOffset minExpiry);

    /// <summary>
    /// Creates the credential details
    /// </summary>
    /// <param name="bpnl">Id of the company</param>
    /// <param name="verifiedCredentialTypeId">Id of the credential types</param>
    /// <param name="companySsiDetailStatusId">id of detail status</param>
    /// <param name="issuerBpn">bpn of the credential issuer</param>
    /// <param name="userId">Id of the creator</param>
    /// <param name="setOptionalFields">sets the optional fields</param>
    /// <returns>The created entity</returns>
    CompanySsiDetail CreateSsiDetails(string bpnl, VerifiedCredentialTypeId verifiedCredentialTypeId, CompanySsiDetailStatusId companySsiDetailStatusId, string issuerBpn, string userId, Action<CompanySsiDetail>? setOptionalFields);

    /// <summary>
    /// Checks whether the credential details are already exists for the company and the given version
    /// </summary>
    /// <param name="bpnl">Bpnl of the company</param>
    /// <param name="verifiedCredentialTypeId">Id of the verifiedCredentialType</param>
    /// <param name="kindId">Id of the credentialTypeKind</param>
    /// <param name="verifiedCredentialExternalTypeUseCaseDetailId">Id of the verifiedCredentialExternalType Detail Id</param>
    /// <returns><c>true</c> if the details already exists, otherwise <c>false</c></returns>
    Task<bool> CheckSsiDetailsExistsForCompany(string bpnl, VerifiedCredentialTypeId verifiedCredentialTypeId, VerifiedCredentialTypeKindId kindId, Guid? verifiedCredentialExternalTypeUseCaseDetailId);

    /// <summary>
    /// Checks whether the given externalTypeDetail exists and returns the CredentialTypeId
    /// </summary>
    /// <param name="verifiedCredentialExternalTypeUseCaseDetailId">Id of vc external type use case detail id</param>
    /// <param name="verifiedCredentialTypeId">Id of the vc type</param>
    /// <param name="bpnl">The business partner number of the current user</param>
    /// <returns>Returns a valueTuple with identifiers if the externalTypeUseCaseDetailId exists and the corresponding credentialTypeId</returns>
    Task<(bool Exists, string? Version, string? Template, IEnumerable<VerifiedCredentialExternalTypeId> ExternalTypeIds, DateTimeOffset Expiry, bool PendingCredentialRequestExists)> CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(Guid verifiedCredentialExternalTypeUseCaseDetailId, VerifiedCredentialTypeId verifiedCredentialTypeId, string bpnl);

    /// <summary>
    /// Checks whether the given credentialTypeId is a <see cref="VerifiedCredentialTypeKindId"/> Certificate
    /// </summary>
    /// <param name="credentialTypeId">Id of the credentialTypeId</param>
    /// <returns><c>true</c> if the tpye is a certificate, otherwise <c>false</c></returns>
    Task<(bool Exists, IEnumerable<Guid> DetailVersionIds)> CheckSsiCertificateType(VerifiedCredentialTypeId credentialTypeId);

    /// <summary>
    /// Gets all credential details
    /// </summary>
    /// <param name="sorting">The sorting of the result</param>
    /// <param name="companySsiDetailStatusId">The status of the details</param>
    /// <param name="credentialTypeId">OPTIONAL: The type of the credential that should be returned</param>
    /// <param name="approvalType">OPTIONAL: The approval type of the credential</param>
    /// <returns>Returns data to create the pagination</returns>
    Func<int, int, Task<Pagination.Source<CredentialDetailData>?>> GetAllCredentialDetails(CompanySsiDetailSorting? sorting, CompanySsiDetailStatusId? companySsiDetailStatusId, VerifiedCredentialTypeId? credentialTypeId, CompanySsiDetailApprovalType? approvalType);

    /// <summary>
    /// Gets all credentials for a specific bpn
    /// </summary>
    /// <param name="bpnl">The bpn to filter the credentials for</param>
    IAsyncEnumerable<OwnedVerifiedCredentialData> GetOwnCredentialDetails(string bpnl);

    Task<(bool exists, SsiApprovalData data)> GetSsiApprovalData(Guid credentialId);
    Task<(bool Exists, CompanySsiDetailStatusId Status, VerifiedCredentialExternalTypeId Type, string UserId, Guid? ProcessId, IEnumerable<Guid> ProcessStepIds)> GetSsiRejectionData(Guid credentialId);
    void AttachAndModifyCompanySsiDetails(Guid id, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields);
    IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes(string bpnl);
    IAsyncEnumerable<CredentialExpiryData> GetExpiryData(DateTimeOffset now, DateTimeOffset inactiveVcsToDelete, DateTimeOffset expiredVcsToDelete);
    void RemoveSsiDetail(Guid companySsiDetailId, string bpnl, string userId);
    void CreateProcessData(Guid companySsiDetailId, JsonDocument schema, VerifiedCredentialTypeKindId credentialTypeKindId, Action<CompanySsiProcessData>? setOptionalFields);
    void AttachAndModifyProcessData(Guid companySsiDetailId, Action<CompanySsiProcessData>? initialize, Action<CompanySsiProcessData> setOptionalFields);
}
