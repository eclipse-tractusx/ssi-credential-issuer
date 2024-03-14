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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Linq;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using ErrorParameter = Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.ErrorParameter;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;

public class CredentialBusinessLogic : ICredentialBusinessLogic
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly IEnumerable<string> Context = new[] { "https://www.w3.org/2018/credentials/v1", "https://w3id.org/catenax/credentials/v1.0.0" };

    private readonly IIssuerRepositories _repositories;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IPortalService _portalService;
    private readonly CredentialSettings _settings;
    private readonly IIdentityData _identity;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="repositories"></param>
    /// <param name="dateTimeProvider"></param>
    /// <param name="identityService"></param>
    /// <param name="clientFactory"></param>
    /// <param name="portalService" />
    /// <param name="options"></param>
    public CredentialBusinessLogic(
        IIssuerRepositories repositories,
        IIdentityService identityService,
        IDateTimeProvider dateTimeProvider,
        IHttpClientFactory clientFactory,
        IPortalService portalService,
        IOptions<CredentialSettings> options)
    {
        _repositories = repositories;
        _identity = identityService.IdentityData;
        _dateTimeProvider = dateTimeProvider;
        _clientFactory = clientFactory;
        _portalService = portalService;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UseCaseParticipationData>> GetUseCaseParticipationAsync() =>
        await _repositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetUseCaseParticipationForCompany(_identity.Bpnl, _dateTimeProvider.OffsetNow)
            .Select(x => new UseCaseParticipationData(
                x.UseCase,
                x.Description,
                x.CredentialType,
                x.VerifiedCredentials
                    .Select(y =>
                        new CompanySsiExternalTypeDetailData(
                            y.ExternalDetailData,
                            y.SsiDetailData.CatchingInto(
                                data => data
                                    .Select(d => new CompanySsiDetailData(
                                        d.CredentialId,
                                        d.ParticipationStatus,
                                        d.ExpiryDate,
                                        d.Documents))
                                    .SingleOrDefault(),
                                (InvalidOperationException _) => throw ConflictException.Create(CompanyDataErrors.MULTIPLE_SSI_DETAIL))))
                    .ToList()))
            .ToListAsync()
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IEnumerable<CertificateParticipationData>> GetSsiCertificatesAsync() =>
        await _repositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetSsiCertificates(_identity.Bpnl, _dateTimeProvider.OffsetNow)
            .Select(x => new CertificateParticipationData(
                x.CredentialType,
                x.Credentials
                    .Select(y =>
                        new CompanySsiExternalTypeDetailData(
                            y.ExternalDetailData,
                            y.SsiDetailData.CatchingInto(
                                data => data
                                    .Select(d => new CompanySsiDetailData(
                                        d.CredentialId,
                                        d.ParticipationStatus,
                                        d.ExpiryDate,
                                        d.Documents))
                                    .SingleOrDefault(),
                                (InvalidOperationException _) => throw ConflictException.Create(CompanyDataErrors.MULTIPLE_SSI_DETAIL))))
                    .ToList()))
            .ToListAsync()
            .ConfigureAwait(false);

    /// <inheritdoc />
    public Task<Pagination.Response<CredentialDetailData>> GetCredentials(int page, int size, CompanySsiDetailStatusId? companySsiDetailStatusId, VerifiedCredentialTypeId? credentialTypeId, CompanySsiDetailSorting? sorting)
    {
        var query = _repositories
            .GetInstance<ICompanySsiDetailsRepository>()
            .GetAllCredentialDetails(companySsiDetailStatusId, credentialTypeId);
        var sortedQuery = sorting switch
        {
            CompanySsiDetailSorting.BpnlAsc or null => query.OrderBy(c => c.Bpnl),
            CompanySsiDetailSorting.BpnlDesc => query.OrderByDescending(c => c.Bpnl),
            _ => query
        };

        return Pagination.CreateResponseAsync(page, size, _settings.MaxPageSize, (skip, take) =>
            new Pagination.AsyncSource<CredentialDetailData>
            (
                query.CountAsync(),
                sortedQuery
                    .Skip(skip)
                    .Take(take)
                    .Select(c => new CredentialDetailData(
                        c.Id,
                        c.Bpnl,
                        c.VerifiedCredentialTypeId,
                        c.VerifiedCredentialType!.VerifiedCredentialTypeAssignedUseCase!.UseCase!.Name,
                        c.CompanySsiDetailStatusId,
                        c.ExpiryDate,
                        c.Documents.Select(d => new DocumentData(d.Id, d.DocumentName, d.DocumentTypeId)),
                        c.VerifiedCredentialExternalTypeDetailVersion == null
                            ? null
                            : new ExternalTypeDetailData(
                                c.VerifiedCredentialExternalTypeDetailVersion.Id,
                                c.VerifiedCredentialExternalTypeDetailVersion.VerifiedCredentialExternalTypeId,
                                c.VerifiedCredentialExternalTypeDetailVersion.Version,
                                c.VerifiedCredentialExternalTypeDetailVersion.Template,
                                c.VerifiedCredentialExternalTypeDetailVersion.ValidFrom,
                                c.VerifiedCredentialExternalTypeDetailVersion.Expiry))
                    ).AsAsyncEnumerable()
            ));
    }

    /// <inheritdoc />
    public async Task ApproveCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        var companySsiRepository = _repositories.GetInstance<ICompanySsiDetailsRepository>();
        var (exists, data) = await companySsiRepository.GetSsiApprovalData(credentialId).ConfigureAwait(false);
        ValidateApprovalData(credentialId, exists, data);

        var processStepRepository = _repositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_CREDENTIAL, ProcessStepStatusId.TODO, processId);

        var expiryDate = GetExpiryDate(data.DetailData?.ExpiryDate);
        companySsiRepository.AttachAndModifyCompanySsiDetails(credentialId, c =>
            {
                c.CompanySsiDetailStatusId = data.Status;
                c.ExpiryDate = DateTimeOffset.MinValue;
                c.ProcessId = null;
            },
            c =>
            {
                c.CompanySsiDetailStatusId = CompanySsiDetailStatusId.ACTIVE;
                c.DateLastChanged = _dateTimeProvider.OffsetNow;
                c.ExpiryDate = expiryDate;
                c.ProcessId = processId;
            });

        var typeValue = data.Type.GetEnumValue() ?? throw UnexpectedConditionException.Create(CompanyDataErrors.CREDENTIAL_TYPE_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialType", data.Type.ToString()) });
        var content = JsonSerializer.Serialize(new { data.Type, CredentialId = credentialId }, Options);
        await _portalService.AddNotification(content, _identity.IdentityId, NotificationTypeId.CREDENTIAL_APPROVAL, cancellationToken).ConfigureAwait(false);
        var mailParameters = new Dictionary<string, string>
        {
            { "requestName", typeValue },
            { "credentialType", typeValue },
            { "expiryDate", expiryDate.ToString("o", CultureInfo.InvariantCulture) }
        };
        await _portalService.TriggerMail("CredentialApproval", _identity.IdentityId, mailParameters, cancellationToken).ConfigureAwait(false);
        await _repositories.SaveAsync().ConfigureAwait(false);
    }

    private static void ValidateApprovalData(Guid credentialId, bool exists, SsiApprovalData data)
    {
        if (!exists)
        {
            throw NotFoundException.Create(CompanyDataErrors.SSI_DETAILS_NOT_FOUND, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        if (data.Status != CompanySsiDetailStatusId.PENDING)
        {
            throw ConflictException.Create(CompanyDataErrors.CREDENTIAL_NOT_PENDING, new ErrorParameter[] { new("credentialId", credentialId.ToString()), new("status", CompanySsiDetailStatusId.PENDING.ToString()) });
        }

        if (string.IsNullOrWhiteSpace(data.Bpn))
        {
            throw UnexpectedConditionException.Create(CompanyDataErrors.BPN_NOT_SET);
        }

        if (data.DetailData == null && data.Kind == VerifiedCredentialTypeKindId.FRAMEWORK)
        {
            throw ConflictException.Create(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET);
        }

        if (data.Kind != VerifiedCredentialTypeKindId.FRAMEWORK && data.Kind != VerifiedCredentialTypeKindId.MEMBERSHIP && data.Kind != VerifiedCredentialTypeKindId.BPN)
        {
            throw ConflictException.Create(CompanyDataErrors.KIND_NOT_SUPPORTED, new ErrorParameter[] { new("kind", data.Kind != null ? data.Kind.Value.ToString() : "empty kind") });
        }

        if (data.Kind == VerifiedCredentialTypeKindId.FRAMEWORK && string.IsNullOrWhiteSpace(data.DetailData!.Version))
        {
            throw ConflictException.Create(CompanyDataErrors.EMPTY_VERSION);
        }
    }

    private DateTimeOffset GetExpiryDate(DateTimeOffset? expiryDate)
    {
        var now = _dateTimeProvider.OffsetNow;
        var future = now.AddMonths(12);
        var expiry = expiryDate ?? future;

        if (expiry < now)
        {
            throw ConflictException.Create(CompanyDataErrors.EXPIRY_DATE_IN_PAST);
        }

        return expiry > future ? future : expiry;
    }

    /// <inheritdoc />
    public async Task RejectCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        var companySsiRepository = _repositories.GetInstance<ICompanySsiDetailsRepository>();
        var (exists, status, type) = await companySsiRepository.GetSsiRejectionData(credentialId).ConfigureAwait(false);
        if (!exists)
        {
            throw NotFoundException.Create(CompanyDataErrors.SSI_DETAILS_NOT_FOUND, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        if (status != CompanySsiDetailStatusId.PENDING)
        {
            throw ConflictException.Create(CompanyDataErrors.CREDENTIAL_NOT_PENDING, new ErrorParameter[] { new("credentialId", credentialId.ToString()), new("status", CompanySsiDetailStatusId.PENDING.ToString()) });
        }

        var typeValue = type.GetEnumValue() ?? throw UnexpectedConditionException.Create(CompanyDataErrors.CREDENTIAL_TYPE_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialType", type.ToString()) });
        var content = JsonSerializer.Serialize(new { Type = type, CredentialId = credentialId }, Options);
        await _portalService.AddNotification(content, _identity.IdentityId, NotificationTypeId.CREDENTIAL_REJECTED, cancellationToken).ConfigureAwait(false);

        companySsiRepository.AttachAndModifyCompanySsiDetails(credentialId, c =>
            {
                c.CompanySsiDetailStatusId = status;
            },
            c =>
            {
                c.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
                c.DateLastChanged = _dateTimeProvider.OffsetNow;
            });

        await _repositories.SaveAsync().ConfigureAwait(false);

        var mailParameters = new Dictionary<string, string>
        {
            { "requestName", typeValue },
            { "reason", "Declined by the Operator" }
        };

        await _portalService.TriggerMail("CredentialRejected", _identity.IdentityId, mailParameters, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<VerifiedCredentialTypeId> GetCertificateTypes() =>
        _repositories.GetInstance<ICompanySsiDetailsRepository>().GetCertificateTypes(_identity.Bpnl);

    public async Task<Guid> CreateBpnCredential(CreateBpnCredentialRequest requestData, CancellationToken cancellationToken)
    {
        var companyCredentialDetailsRepository = _repositories.GetInstance<ICompanySsiDetailsRepository>();
        var holderDid = await GetHolderInformation(requestData.Holder, cancellationToken).ConfigureAwait(false);
        var schemaData = new BpnCredential(
            Guid.NewGuid(),
            Context,
            new[] { "VerifiableCredential", "BpnCredential" },
            "BpnCredential",
            "Bpn Credential",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMonths(12),
            _settings.IssuerDid,
            new BpnCredentialSubject(
                holderDid,
                requestData.BusinessPartnerNumber,
                requestData.BusinessPartnerNumber
            )
        );
        var schema = JsonSerializer.Serialize(schemaData, Options);
        return await HandleCredentialProcessCreation(VerifiedCredentialTypeKindId.BPN, VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, schema, requestData.TechnicalUserDetails, null, companyCredentialDetailsRepository);
    }

    public async Task<Guid> CreateMembershipCredential(CreateMembershipCredentialRequest requestData, CancellationToken cancellationToken)
    {
        var companyCredentialDetailsRepository = _repositories.GetInstance<ICompanySsiDetailsRepository>();

        var holderDid = await GetHolderInformation(requestData.Holder, cancellationToken).ConfigureAwait(false);
        var schemaData = new MembershipCredential(
            Guid.NewGuid(),
            Context,
            new[] { "VerifiableCredential", "MembershipCredential" },
            "MembershipCredential",
            "Membership Credential",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMonths(12),
            _settings.IssuerDid,
            new MembershipCredentialSubject(
                holderDid,
                requestData.HolderBpn,
                requestData.MemberOf
            )
        );
        var schema = JsonSerializer.Serialize(schemaData, Options);
        return await HandleCredentialProcessCreation(VerifiedCredentialTypeKindId.MEMBERSHIP, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, schema, requestData.TechnicalUserDetails, null, companyCredentialDetailsRepository);
    }

    public async Task<Guid> CreateFrameworkCredential(CreateFrameworkCredentialRequest requestData, CancellationToken cancellationToken)
    {
        var companyCredentialDetailsRepository = _repositories.GetInstance<ICompanySsiDetailsRepository>();
        var result = await companyCredentialDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(requestData.UseCaseFrameworkVersionId, requestData.UseCaseFrameworkId).ConfigureAwait(false);
        if (!result.Exists)
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialExternalTypeDetailId", requestData.UseCaseFrameworkId.ToString()) });
        }

        if (result.Expiry < _dateTimeProvider.OffsetNow)
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.EXPIRY_DATE_IN_PAST);
        }

        if (string.IsNullOrWhiteSpace(result.Version))
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.EMPTY_VERSION);
        }

        if (string.IsNullOrWhiteSpace(result.Template))
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.EMPTY_TEMPLATE);
        }

        if (result.UseCase.Count() != 1)
        {
            throw ControllerArgumentException.Create(CompanyDataErrors.MULTIPLE_USE_CASES);
        }

        var useCase = result.UseCase.Single();
        var holderDid = await GetHolderInformation(requestData.Holder, cancellationToken).ConfigureAwait(false);
        var schemaData = new FrameworkCredential(
            Guid.NewGuid(),
            Context,
            new[] { "VerifiableCredential", $"{useCase}Credential" },
            $"{useCase}Credential",
            $"Framework Credential for UseCase {useCase}",
            DateTimeOffset.UtcNow,
            result.Expiry,
            _settings.IssuerDid,
            new FrameworkCredentialSubject(
                holderDid,
                requestData.HolderBpn,
                "UseCaseFramework",
                useCase,
                result.Template!,
                result.Version!
            )
        );
        var schema = JsonSerializer.Serialize(schemaData, Options);
        return await HandleCredentialProcessCreation(VerifiedCredentialTypeKindId.FRAMEWORK, requestData.UseCaseFrameworkId, schema, requestData.TechnicalUserDetails, requestData.UseCaseFrameworkVersionId, companyCredentialDetailsRepository);
    }

    private async Task<string> GetHolderInformation(string didDocumentLocation, CancellationToken cancellationToken)
    {
        var client = _clientFactory.CreateClient("didDocumentDownload");
        var result = await client.GetAsync(didDocumentLocation, cancellationToken)
            .CatchingIntoServiceExceptionFor("get-did-document").ConfigureAwait(false);
        var did = await result.Content.ReadFromJsonAsync<DidDocument>(Options, cancellationToken).ConfigureAwait(false);
        if (did == null)
        {
            throw ConflictException.Create(CompanyDataErrors.DID_NOT_SET);
        }

        return did.Id;
    }

    private async Task<Guid> HandleCredentialProcessCreation(VerifiedCredentialTypeKindId kindId, VerifiedCredentialTypeId typeId, string schema, TechnicalUserDetails? technicalUserDetails, Guid? detailVersionId, ICompanySsiDetailsRepository companyCredentialDetailsRepository)
    {
        var documentContent = System.Text.Encoding.UTF8.GetBytes(schema);
        var hash = SHA512.HashData(documentContent);
        var documentRepository = _repositories.GetInstance<IDocumentRepository>();
        var docId = documentRepository.CreateDocument("schema.json", documentContent,
            hash, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, x =>
            {
                x.CompanyUserId = _identity.IdentityId;
                x.DocumentStatusId = DocumentStatusId.ACTIVE;
            }).Id;

        var ssiDetailId = companyCredentialDetailsRepository.CreateSsiDetails(
            _identity.Bpnl,
            typeId, docId, CompanySsiDetailStatusId.PENDING,
            _identity.IdentityId,
            c => c.VerifiedCredentialExternalTypeDetailVersionId = detailVersionId).Id;
        documentRepository.AssignDocumentToCompanySsiDetails(docId, ssiDetailId);

        companyCredentialDetailsRepository.CreateProcessData(ssiDetailId, JsonDocument.Parse(schema), kindId,
            c =>
            {
                if (technicalUserDetails == null)
                {
                    return;
                }

                var cryptoConfig = _settings.EncryptionConfigs.SingleOrDefault(x => x.Index == _settings.EncrptionConfigIndex) ?? throw new ConfigurationException($"EncryptionModeIndex {_settings.EncrptionConfigIndex} is not configured");
                var (secret, initializationVector) = CryptoHelper.Encrypt(technicalUserDetails.ClientSecret, Convert.FromHexString(cryptoConfig.EncryptionKey), cryptoConfig.CipherMode, cryptoConfig.PaddingMode);

                c.ClientId = technicalUserDetails.ClientId;
                c.ClientSecret = secret;
                c.InitializationVector = initializationVector;
                c.HolderWalletUrl = technicalUserDetails.WalletUrl;
            });

        await _repositories.SaveAsync().ConfigureAwait(false);
        return ssiDetailId;
    }
}
