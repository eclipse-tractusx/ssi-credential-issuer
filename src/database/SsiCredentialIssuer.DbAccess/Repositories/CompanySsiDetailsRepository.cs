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

using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;

public class CompanySsiDetailsRepository(IssuerDbContext context)
    : ICompanySsiDetailsRepository
{
    /// <inheritdoc />
    public IAsyncEnumerable<UseCaseParticipationData> GetUseCaseParticipationForCompany(string bpnl, DateTimeOffset minExpiry) =>
        context.VerifiedCredentialTypes
            .Where(t => t.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.FRAMEWORK)
            .Select(t => new
            {
                t.VerifiedCredentialTypeAssignedUseCase!.UseCase,
                TypeId = t.Id,
                ExternalTypeDetails = t.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeDetailVersions
            })
            .Select(x => new UseCaseParticipationData(
                x.UseCase!.Name,
                x.UseCase.Shortname,
                x.TypeId,
                x.ExternalTypeDetails
                    .Select(e =>
                        new CompanySsiExternalTypeDetailData(
                            new ExternalTypeDetailData(
                                e.Id,
                                e.VerifiedCredentialExternalTypeId,
                                e.Version,
                                e.Template,
                                e.ValidFrom,
                                e.Expiry),
                            e.CompanySsiDetails
                                .Where(ssi =>
                                    ssi.Bpnl == bpnl &&
                                    ssi.VerifiedCredentialTypeId == x.TypeId &&
                                    (ssi.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE || ssi.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING) &&
                                    ssi.VerifiedCredentialExternalTypeDetailVersionId == e.Id &&
                                    (ssi.ExpiryDate == null || ssi.ExpiryDate > minExpiry))
                                .Select(ssi =>
                                    new CompanySsiDetailData(
                                        ssi.Id,
                                        ssi.CompanySsiDetailStatusId,
                                        ssi.ExpiryDate,
                                        ssi.Documents.Select(d => new DocumentData(
                                            d.Id,
                                            d.DocumentName,
                                            d.DocumentTypeId))))
                        ))
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<CertificateParticipationData> GetSsiCertificates(string bpnl, DateTimeOffset minExpiry) =>
        context.VerifiedCredentialTypes
            .Where(types => types.VerifiedCredentialTypeAssignedKind != null && types.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId != VerifiedCredentialTypeKindId.FRAMEWORK)
            .Select(t => new
            {
                TypeId = t.Id,
                ExternalTypeDetails = t.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeDetailVersions
            })
            .Select(x => new CertificateParticipationData(
                x.TypeId,
                x.ExternalTypeDetails
                    .Select(e =>
                        new CompanySsiExternalTypeDetailData(
                            new ExternalTypeDetailData(
                                e.Id,
                                e.VerifiedCredentialExternalTypeId,
                                e.Version,
                                e.Template,
                                e.ValidFrom,
                                e.Expiry),
                            e.CompanySsiDetails
                                .Where(ssi =>
                                    ssi.Bpnl == bpnl &&
                                    ssi.VerifiedCredentialTypeId == x.TypeId &&
                                    (ssi.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE || ssi.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING) &&
                                    (ssi.ExpiryDate == null || ssi.ExpiryDate > minExpiry))
                                .Select(ssi =>
                                    new CompanySsiDetailData(
                                        ssi.Id,
                                        ssi.CompanySsiDetailStatusId,
                                        ssi.ExpiryDate,
                                        ssi.Documents.Select(d => new DocumentData(
                                                d.Id,
                                                d.DocumentName,
                                                d.DocumentTypeId))))
                        ))
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public CompanySsiDetail CreateSsiDetails(string bpnl, VerifiedCredentialTypeId verifiedCredentialTypeId, CompanySsiDetailStatusId companySsiDetailStatusId, string issuerBpn, string userId, Action<CompanySsiDetail>? setOptionalFields)
    {
        var detail = new CompanySsiDetail(Guid.NewGuid(), bpnl, verifiedCredentialTypeId, companySsiDetailStatusId, issuerBpn, userId, DateTimeOffset.UtcNow);
        setOptionalFields?.Invoke(detail);
        return context.CompanySsiDetails.Add(detail).Entity;
    }

    /// <inheritdoc />
    public Task<bool> CheckSsiDetailsExistsForCompany(string bpnl, VerifiedCredentialTypeId verifiedCredentialTypeId, VerifiedCredentialTypeKindId kindId, Guid? verifiedCredentialExternalTypeUseCaseDetailId) =>
        context.CompanySsiDetails
            .AnyAsync(x =>
                x.Bpnl == bpnl &&
                x.VerifiedCredentialTypeId == verifiedCredentialTypeId &&
                x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == kindId &&
                x.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE &&
                (verifiedCredentialExternalTypeUseCaseDetailId == null || x.VerifiedCredentialExternalTypeDetailVersionId == verifiedCredentialExternalTypeUseCaseDetailId));

    /// <inheritdoc />
    public Task<(bool Exists, string? Version, string? Template, IEnumerable<VerifiedCredentialExternalTypeId> ExternalTypeIds, DateTimeOffset Expiry, bool PendingCredentialRequestExists)> CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(Guid verifiedCredentialExternalTypeUseCaseDetailId, VerifiedCredentialTypeId verifiedCredentialTypeId, string bpnl) =>
        context.VerifiedCredentialExternalTypeDetailVersions
            .Where(x =>
                x.Id == verifiedCredentialExternalTypeUseCaseDetailId &&
                x.VerifiedCredentialExternalType!.VerifiedCredentialTypeAssignedExternalTypes.Any(y => y.VerifiedCredentialTypeId == verifiedCredentialTypeId))
            .Select(x => new ValueTuple<bool, string?, string?, IEnumerable<VerifiedCredentialExternalTypeId>, DateTimeOffset, bool>(
                true,
                x.Version,
                x.Template,
                x.VerifiedCredentialExternalType!.VerifiedCredentialTypeAssignedExternalTypes.Select(y => y.VerifiedCredentialExternalTypeId),
                x.Expiry,
                x.CompanySsiDetails.Any(ssi => ssi.Bpnl == bpnl && ssi.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING)))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool Exists, IEnumerable<Guid> DetailVersionIds)> CheckSsiCertificateType(VerifiedCredentialTypeId credentialTypeId) =>
        context.VerifiedCredentialTypeAssignedKinds
            .Where(x =>
                x.VerifiedCredentialTypeId == credentialTypeId &&
                x.VerifiedCredentialTypeKindId != VerifiedCredentialTypeKindId.FRAMEWORK)
            .Select(x => new ValueTuple<bool, IEnumerable<Guid>>(
                true,
                x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeDetailVersions.Select(v => v.Id)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IQueryable<CompanySsiDetail> GetAllCredentialDetails(CompanySsiDetailStatusId? companySsiDetailStatusId, VerifiedCredentialTypeId? credentialTypeId, CompanySsiDetailApprovalType? approvalType) =>
        context.CompanySsiDetails.AsNoTracking()
            .Where(c =>
                (!companySsiDetailStatusId.HasValue || c.CompanySsiDetailStatusId == companySsiDetailStatusId.Value) &&
                (!credentialTypeId.HasValue || c.VerifiedCredentialTypeId == credentialTypeId) &&
                (!approvalType.HasValue || (approvalType.Value == CompanySsiDetailApprovalType.Automatic && c.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.FRAMEWORK) || (approvalType.Value == CompanySsiDetailApprovalType.Manual && c.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId != VerifiedCredentialTypeKindId.FRAMEWORK)));

    /// <inheritdoc />
    public IAsyncEnumerable<OwnedVerifiedCredentialData> GetOwnCredentialDetails(string bpnl) =>
        context.CompanySsiDetails.AsNoTracking()
            .Where(c => c.Bpnl == bpnl)
            .Select(c => new OwnedVerifiedCredentialData(
                c.Id,
                c.VerifiedCredentialTypeId,
                c.CompanySsiDetailStatusId,
                c.ExpiryDate,
                c.IssuerBpn))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public Task<(bool exists, SsiApprovalData data)> GetSsiApprovalData(Guid credentialId) =>
        context.CompanySsiDetails
            .Where(x => x.Id == credentialId)
            .Select(x => new ValueTuple<bool, SsiApprovalData>(
                true,
                new SsiApprovalData(
                    x.CompanySsiDetailStatusId,
                    x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalTypeId,
                    x.ProcessId,
                    x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind == null ? null : x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId,
                    x.Bpnl,
                    x.CreatorUserId,
                    x.CompanySsiProcessData!.Schema,
                    x.VerifiedCredentialExternalTypeDetailVersion == null ?
                        null :
                        new DetailData(
                            x.VerifiedCredentialExternalTypeDetailVersion!.VerifiedCredentialExternalTypeId,
                            x.VerifiedCredentialExternalTypeDetailVersion.Template,
                            x.VerifiedCredentialExternalTypeDetailVersion.Version,
                            x.VerifiedCredentialExternalTypeDetailVersion.Expiry
                        )
                )
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool Exists, CompanySsiDetailStatusId Status, VerifiedCredentialExternalTypeId Type, string UserId, Guid? ProcessId, IEnumerable<Guid> ProcessStepIds)> GetSsiRejectionData(Guid credentialId) =>
        context.CompanySsiDetails
            .Where(x => x.Id == credentialId)
            .Select(x => new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialExternalTypeId, string, Guid?, IEnumerable<Guid>>(
                true,
                x.CompanySsiDetailStatusId,
                x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalTypeId,
                x.CreatorUserId,
                x.ProcessId,
                x.Process!.ProcessSteps.Where(ps => ps.ProcessStepStatusId == ProcessStepStatusId.TODO).Select(p => p.Id)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyCompanySsiDetails(Guid id, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields)
    {
        var entity = new CompanySsiDetail(id, null!, default, default, null!, null!, DateTimeOffset.MinValue);
        initialize?.Invoke(entity);
        context.Attach(entity);
        updateFields.Invoke(entity);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes(string bpnl) =>
        context.VerifiedCredentialTypes
            .Where(x =>
                x.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId != VerifiedCredentialTypeKindId.FRAMEWORK &&
                !x.CompanySsiDetails.Any(ssi =>
                    ssi.Bpnl == bpnl &&
                    (ssi.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING || ssi.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE)))
            .Select(x => x.Id)
            .ToAsyncEnumerable();

    public IAsyncEnumerable<CredentialExpiryData> GetExpiryData(DateTimeOffset now, DateTimeOffset inactiveVcsToDelete, DateTimeOffset expiredVcsToDelete)
    {
        var oneDay = now.AddDays(1);
        var twoWeeks = now.AddDays(14);
        var oneMonth = now.AddMonths(2);

        return context.CompanySsiDetails
            .Select(x => new
            {
                Details = x,
                IsVcToDecline = x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.PENDING && x.VerifiedCredentialExternalTypeDetailVersion!.Expiry < now,
                IsVcToDelete = x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE && x.DateCreated < inactiveVcsToDelete || (x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE || x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.INACTIVE) && x.ExpiryDate < expiredVcsToDelete,
                IsOneDayNotification = x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE && x.ExpiryDate <= oneDay && (x.ExpiryCheckTypeId == ExpiryCheckTypeId.TWO_WEEKS || x.ExpiryCheckTypeId == ExpiryCheckTypeId.ONE_MONTH || x.ExpiryCheckTypeId == null),
                IsTwoWeeksNotification = x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE && x.ExpiryDate > oneDay && x.ExpiryDate <= twoWeeks && (x.ExpiryCheckTypeId == ExpiryCheckTypeId.ONE_MONTH || x.ExpiryCheckTypeId == null),
                IsOneMonthNotification = x.CompanySsiDetailStatusId == CompanySsiDetailStatusId.ACTIVE && x.ExpiryDate > twoWeeks && x.ExpiryDate <= oneMonth && x.ExpiryCheckTypeId == null
            })
            .Where(x => x.IsVcToDecline || x.IsVcToDelete || x.IsOneDayNotification || x.IsTwoWeeksNotification || x.IsOneMonthNotification)
            .Select(x => new CredentialExpiryData(
                x.Details.Id,
                x.Details.CreatorUserId,
                x.Details.ExpiryDate,
                x.Details.ExpiryCheckTypeId,
                x.Details.VerifiedCredentialExternalTypeDetailVersion!.Version,
                x.Details.Bpnl,
                x.Details.CompanySsiDetailStatusId,
                x.Details.VerifiedCredentialType!.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalTypeId,
                new CredentialScheduleData(
                    x.IsVcToDelete,
                    x.IsOneDayNotification,
                    x.IsTwoWeeksNotification,
                    x.IsOneMonthNotification,
                    x.IsVcToDecline
                )))
            .ToAsyncEnumerable();
    }

    public void RemoveSsiDetail(Guid companySsiDetailId, string bpnl, string userId) =>
        context.CompanySsiDetails.Remove(new CompanySsiDetail(companySsiDetailId, bpnl, default, default, bpnl, userId, DateTimeOffset.MinValue));

    public void CreateProcessData(Guid companySsiDetailId, JsonDocument schema, VerifiedCredentialTypeKindId credentialTypeKindId, Action<CompanySsiProcessData>? setOptionalFields)
    {
        var companySsiDetailData = new CompanySsiProcessData(companySsiDetailId, schema, credentialTypeKindId);
        context.CompanySsiProcessData.Add(companySsiDetailData);
        setOptionalFields?.Invoke(companySsiDetailData);
    }

    public void AttachAndModifyProcessData(Guid companySsiDetailId, Action<CompanySsiProcessData>? initialize, Action<CompanySsiProcessData> setOptionalFields)
    {
        var companySsiDetailData = new CompanySsiProcessData(companySsiDetailId, null!, default);
        initialize?.Invoke(companySsiDetailData);
        context.CompanySsiProcessData.Attach(companySsiDetailData);
        setOptionalFields(companySsiDetailData);
    }
}
