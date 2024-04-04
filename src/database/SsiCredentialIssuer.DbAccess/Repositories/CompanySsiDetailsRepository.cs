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
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;

public class CompanySsiDetailsRepository : ICompanySsiDetailsRepository
{
    private readonly IssuerDbContext _context;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="dbContext">DB context.</param>
    public CompanySsiDetailsRepository(IssuerDbContext dbContext)
    {
        _context = dbContext;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UseCaseParticipationTransferData> GetUseCaseParticipationForCompany(string bpnl, DateTimeOffset minExpiry) =>
        _context.VerifiedCredentialTypes
            .Where(t => t.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == VerifiedCredentialTypeKindId.FRAMEWORK)
            .Select(t => new
            {
                t.VerifiedCredentialTypeAssignedUseCase!.UseCase,
                TypeId = t.Id,
                ExternalTypeDetails = t.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeDetailVersions
            })
            .Select(x => new UseCaseParticipationTransferData(
                x.UseCase!.Name,
                x.UseCase.Shortname,
                x.TypeId,
                x.ExternalTypeDetails
                    .Select(e =>
                        new CompanySsiExternalTypeDetailTransferData(
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
                                    ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE &&
                                    ssi.VerifiedCredentialExternalTypeDetailVersionId == e.Id &&
                                    ssi.ExpiryDate > minExpiry)
                                .Select(ssi =>
                                    new CompanySsiDetailTransferData(
                                        ssi.Id,
                                        ssi.CompanySsiDetailStatusId,
                                        ssi.ExpiryDate,
                                        ssi.Documents.Select(d => new DocumentData(
                                            d.Id,
                                            d.DocumentName,
                                            d.DocumentTypeId))))
                                .Take(2)
                        ))
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public IAsyncEnumerable<SsiCertificateTransferData> GetSsiCertificates(string bpnl, DateTimeOffset minExpiry) =>
        _context.VerifiedCredentialTypes
            .Where(types => types.VerifiedCredentialTypeAssignedKind != null && types.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId != VerifiedCredentialTypeKindId.FRAMEWORK)
            .Select(t => new
            {
                TypeId = t.Id,
                ExternalTypeDetails = t.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeDetailVersions
            })
            .Select(x => new SsiCertificateTransferData(
                x.TypeId,
                x.ExternalTypeDetails
                    .Select(e =>
                        new SsiCertificateExternalTypeDetailTransferData(
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
                                    ssi.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE &&
                                    ssi.ExpiryDate > minExpiry)
                                .Select(ssi =>
                                    new CompanySsiDetailTransferData(
                                        ssi.Id,
                                        ssi.CompanySsiDetailStatusId,
                                        ssi.ExpiryDate,
                                        ssi.Documents.Select(d => new DocumentData(
                                                d.Id,
                                                d.DocumentName,
                                                d.DocumentTypeId))))
                                .Take(2)
                        ))
            ))
            .ToAsyncEnumerable();

    /// <inheritdoc />
    public CompanySsiDetail CreateSsiDetails(string bpnl, VerifiedCredentialTypeId verifiedCredentialTypeId, CompanySsiDetailStatusId companySsiDetailStatusId, string issuerBpn, Guid userId, Action<CompanySsiDetail>? setOptionalFields)
    {
        var detail = new CompanySsiDetail(Guid.NewGuid(), bpnl, verifiedCredentialTypeId, companySsiDetailStatusId, issuerBpn, userId, DateTimeOffset.UtcNow);
        setOptionalFields?.Invoke(detail);
        return _context.CompanySsiDetails.Add(detail).Entity;
    }

    /// <inheritdoc />
    public Task<bool> CheckSsiDetailsExistsForCompany(string bpnl, VerifiedCredentialTypeId verifiedCredentialTypeId, VerifiedCredentialTypeKindId kindId, Guid? verifiedCredentialExternalTypeUseCaseDetailId) =>
        _context.CompanySsiDetails
            .AnyAsync(x =>
                x.Bpnl == bpnl &&
                x.VerifiedCredentialTypeId == verifiedCredentialTypeId &&
                x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId == kindId &&
                x.CompanySsiDetailStatusId != CompanySsiDetailStatusId.INACTIVE &&
                (verifiedCredentialExternalTypeUseCaseDetailId == null || x.VerifiedCredentialExternalTypeDetailVersionId == verifiedCredentialExternalTypeUseCaseDetailId));

    /// <inheritdoc />
    public Task<(bool Exists, string? Version, string? Template, IEnumerable<VerifiedCredentialExternalTypeId> ExternalTypeIds, DateTimeOffset Expiry)> CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(Guid verifiedCredentialExternalTypeUseCaseDetailId, VerifiedCredentialTypeId verifiedCredentialTypeId) =>
        _context.VerifiedCredentialExternalTypeDetailVersions
            .Where(x =>
                x.Id == verifiedCredentialExternalTypeUseCaseDetailId &&
                x.VerifiedCredentialExternalType!.VerifiedCredentialTypeAssignedExternalTypes.Any(y => y.VerifiedCredentialTypeId == verifiedCredentialTypeId))
            .Select(x => new ValueTuple<bool, string?, string?, IEnumerable<VerifiedCredentialExternalTypeId>, DateTimeOffset>(
                true,
                x.Version,
                x.Template,
                x.VerifiedCredentialExternalType!.VerifiedCredentialTypeAssignedExternalTypes.Select(y => y.VerifiedCredentialExternalTypeId),
                x.Expiry))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public Task<(bool Exists, IEnumerable<Guid> DetailVersionIds)> CheckSsiCertificateType(VerifiedCredentialTypeId credentialTypeId) =>
        _context.VerifiedCredentialTypeAssignedKinds
            .Where(x =>
                x.VerifiedCredentialTypeId == credentialTypeId &&
                x.VerifiedCredentialTypeKindId != VerifiedCredentialTypeKindId.FRAMEWORK)
            .Select(x => new ValueTuple<bool, IEnumerable<Guid>>(
                true,
                x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedExternalType!.VerifiedCredentialExternalType!.VerifiedCredentialExternalTypeDetailVersions.Select(v => v.Id)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public IQueryable<CompanySsiDetail> GetAllCredentialDetails(CompanySsiDetailStatusId? companySsiDetailStatusId, VerifiedCredentialTypeId? credentialTypeId, string? bpnl) =>
        _context.CompanySsiDetails.AsNoTracking()
            .Where(c =>
                (!companySsiDetailStatusId.HasValue || c.CompanySsiDetailStatusId == companySsiDetailStatusId.Value) &&
                (!credentialTypeId.HasValue || c.VerifiedCredentialTypeId == credentialTypeId) &&
                (bpnl == null || EF.Functions.ILike(c.Bpnl, $"%{bpnl.EscapeForILike()}%")));

    /// <inheritdoc />
    public Task<(bool exists, SsiApprovalData data)> GetSsiApprovalData(Guid credentialId) =>
        _context.CompanySsiDetails
            .Where(x => x.Id == credentialId)
            .Select(x => new ValueTuple<bool, SsiApprovalData>(
                true,
                new SsiApprovalData(
                    x.CompanySsiDetailStatusId,
                    x.VerifiedCredentialTypeId,
                    x.ProcessId,
                    x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind == null ? null : x.VerifiedCredentialType!.VerifiedCredentialTypeAssignedKind!.VerifiedCredentialTypeKindId,
                    x.Bpnl,
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
    public Task<(bool Exists, CompanySsiDetailStatusId Status, VerifiedCredentialTypeId Type, Guid? ProcessId, IEnumerable<Guid> ProcessStepIds)> GetSsiRejectionData(Guid credentialId) =>
        _context.CompanySsiDetails
            .Where(x => x.Id == credentialId)
            .Select(x => new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId, Guid?, IEnumerable<Guid>>(
                true,
                x.CompanySsiDetailStatusId,
                x.VerifiedCredentialTypeId,
                x.ProcessId,
                x.Process!.ProcessSteps.Where(ps => ps.ProcessStepStatusId == ProcessStepStatusId.TODO).Select(p => p.Id)
            ))
            .SingleOrDefaultAsync();

    /// <inheritdoc />
    public void AttachAndModifyCompanySsiDetails(Guid id, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields)
    {
        var entity = new CompanySsiDetail(id, null!, default, default, null!, Guid.Empty, DateTimeOffset.MinValue);
        initialize?.Invoke(entity);
        _context.Attach(entity);
        updateFields.Invoke(entity);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes(string bpnl) =>
        _context.VerifiedCredentialTypes
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

        return _context.CompanySsiDetails
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
                x.Details.VerifiedCredentialTypeId,
                new CredentialScheduleData(
                    x.IsVcToDelete,
                    x.IsOneDayNotification,
                    x.IsTwoWeeksNotification,
                    x.IsOneMonthNotification,
                    x.IsVcToDecline
                )))
            .ToAsyncEnumerable();
    }

    public void RemoveSsiDetail(Guid companySsiDetailId) =>
        _context.CompanySsiDetails.Remove(new CompanySsiDetail(companySsiDetailId, null!, default, default, null!, Guid.Empty, DateTimeOffset.MinValue));

    public void CreateProcessData(Guid companySsiDetailId, JsonDocument schema, VerifiedCredentialTypeKindId credentialTypeKindId, Action<CompanySsiProcessData>? setOptionalFields)
    {
        var companySsiDetailData = new CompanySsiProcessData(companySsiDetailId, schema, credentialTypeKindId);
        _context.CompanySsiProcessData.Add(companySsiDetailData);
        setOptionalFields?.Invoke(companySsiDetailData);
    }

    public void AttachAndModifyProcessData(Guid companySsiDetailId, Action<CompanySsiProcessData>? initialize, Action<CompanySsiProcessData> setOptionalFields)
    {
        var companySsiDetailData = new CompanySsiProcessData(companySsiDetailId, null!, default);
        initialize?.Invoke(companySsiDetailData);
        _context.CompanySsiProcessData.Attach(companySsiDetailData);
        setOptionalFields(companySsiDetailData);
    }
}
