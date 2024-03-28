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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using ErrorParameter = Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.ErrorParameter;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;

public class IssuerBusinessLogic : IIssuerBusinessLogic
{
    private const string StatusList = "StatusList2021";
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly IEnumerable<string> Context = new[] { "https://www.w3.org/2018/credentials/v1", "https://w3id.org/catenax/credentials/v1.0.0" };
    private static readonly Regex UrlPathInvalidCharsRegex = new("""[""<>#%{}|\\^~\[\]`]+""", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    private readonly IIssuerRepositories _repositories;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IPortalService _portalService;
    private readonly IssuerSettings _settings;
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
    public IssuerBusinessLogic(
        IIssuerRepositories repositories,
        IIdentityService identityService,
        IDateTimeProvider dateTimeProvider,
        IHttpClientFactory clientFactory,
        IPortalService portalService,
        IOptions<IssuerSettings> options)
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
                                (InvalidOperationException _) => throw ConflictException.Create(CredentialErrors.MULTIPLE_SSI_DETAIL))))
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
                                (InvalidOperationException _) => throw ConflictException.Create(CredentialErrors.MULTIPLE_SSI_DETAIL))))
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

        var processId = CreateProcess();

        var expiry = GetExpiryDate(data.DetailData?.ExpiryDate);
        UpdateIssuanceDate(credentialId, data, companySsiRepository);
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
                c.ExpiryDate = expiry;
                c.ProcessId = processId;
            });
        var typeValue = data.Type.GetEnumValue() ?? throw UnexpectedConditionException.Create(CredentialErrors.CREDENTIAL_TYPE_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialType", data.Type.ToString()) });
        var mailParameters = new MailParameter[]
        {
            new("requestName", typeValue),
            new("credentialType", typeValue),
            new("expiryDate", expiry.ToString("o", CultureInfo.InvariantCulture))
        };
        await _portalService.TriggerMail("CredentialApproval", _identity.IdentityId, mailParameters, cancellationToken).ConfigureAwait(false);
        var content = JsonSerializer.Serialize(new { data.Type, CredentialId = credentialId }, Options);
        await _portalService.AddNotification(content, _identity.IdentityId, NotificationTypeId.CREDENTIAL_APPROVAL, cancellationToken).ConfigureAwait(false);
        await _repositories.SaveAsync().ConfigureAwait(false);
    }

    private void UpdateIssuanceDate(Guid credentialId, SsiApprovalData data,
        ICompanySsiDetailsRepository companySsiRepository)
    {
        var frameworkCredential = data.Schema!.Deserialize<FrameworkCredential>();
        if (frameworkCredential == null)
        {
            throw UnexpectedConditionException.Create(CredentialErrors.SCHEMA_NOT_FRAMEWORK);
        }

        var newCredential = frameworkCredential with { IssuanceDate = _dateTimeProvider.OffsetNow };
        companySsiRepository.AttachAndModifyProcessData(credentialId, c => c.Schema = JsonDocument.Parse(JsonSerializer.Serialize(frameworkCredential, Options)), c => c.Schema = JsonDocument.Parse(JsonSerializer.Serialize(newCredential, Options)));
    }

    private Guid CreateProcess()
    {
        var processStepRepository = _repositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_CREDENTIAL, ProcessStepStatusId.TODO, processId);
        return processId;
    }

    private static void ValidateApprovalData(Guid credentialId, bool exists, SsiApprovalData data)
    {
        if (!exists)
        {
            throw NotFoundException.Create(CredentialErrors.SSI_DETAILS_NOT_FOUND, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        if (data.Status != CompanySsiDetailStatusId.PENDING)
        {
            throw ConflictException.Create(CredentialErrors.CREDENTIAL_NOT_PENDING, new ErrorParameter[] { new("credentialId", credentialId.ToString()), new("status", CompanySsiDetailStatusId.PENDING.ToString()) });
        }

        if (string.IsNullOrWhiteSpace(data.Bpn))
        {
            throw UnexpectedConditionException.Create(CredentialErrors.BPN_NOT_SET);
        }

        if (data.DetailData == null && data.Kind == VerifiedCredentialTypeKindId.FRAMEWORK)
        {
            throw ConflictException.Create(CredentialErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET);
        }

        if (data.Kind != VerifiedCredentialTypeKindId.FRAMEWORK && data.Kind != VerifiedCredentialTypeKindId.MEMBERSHIP && data.Kind != VerifiedCredentialTypeKindId.BPN)
        {
            throw ConflictException.Create(CredentialErrors.KIND_NOT_SUPPORTED, new ErrorParameter[] { new("kind", data.Kind != null ? data.Kind.Value.ToString() : "empty kind") });
        }

        if (data.Kind == VerifiedCredentialTypeKindId.FRAMEWORK && string.IsNullOrWhiteSpace(data.DetailData!.Version))
        {
            throw ConflictException.Create(CredentialErrors.EMPTY_VERSION);
        }

        if (data.ProcessId is not null)
        {
            throw UnexpectedConditionException.Create(CredentialErrors.ALREADY_LINKED_PROCESS);
        }

        if (data.Schema is null)
        {
            throw UnexpectedConditionException.Create(CredentialErrors.SCHEMA_NOT_SET);
        }
    }

    private DateTimeOffset GetExpiryDate(DateTimeOffset? expiryDate)
    {
        var now = _dateTimeProvider.OffsetNow;
        var future = now.AddMonths(12);
        var expiry = expiryDate ?? future;

        if (expiry < now)
        {
            throw ConflictException.Create(CredentialErrors.EXPIRY_DATE_IN_PAST);
        }

        return expiry > future ? future : expiry;
    }

    /// <inheritdoc />
    public async Task RejectCredential(Guid credentialId, CancellationToken cancellationToken)
    {
        var companySsiRepository = _repositories.GetInstance<ICompanySsiDetailsRepository>();
        var (exists, status, type, processId, processStepIds) = await companySsiRepository.GetSsiRejectionData(credentialId).ConfigureAwait(false);
        if (!exists)
        {
            throw NotFoundException.Create(CredentialErrors.SSI_DETAILS_NOT_FOUND, new ErrorParameter[] { new("credentialId", credentialId.ToString()) });
        }

        if (status != CompanySsiDetailStatusId.PENDING)
        {
            throw ConflictException.Create(CredentialErrors.CREDENTIAL_NOT_PENDING, new ErrorParameter[] { new("credentialId", credentialId.ToString()), new("status", CompanySsiDetailStatusId.PENDING.ToString()) });
        }

        var typeValue = type.GetEnumValue() ?? throw UnexpectedConditionException.Create(CredentialErrors.CREDENTIAL_TYPE_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialType", type.ToString()) });
        var content = JsonSerializer.Serialize(new { Type = type, CredentialId = credentialId }, Options);
        await _portalService.AddNotification(content, _identity.IdentityId, NotificationTypeId.CREDENTIAL_REJECTED, cancellationToken).ConfigureAwait(false);

        var mailParameters = new MailParameter[]
        {
            new("requestName", typeValue),
            new("reason", "Declined by the Operator")
        };

        await _portalService.TriggerMail("CredentialRejected", _identity.IdentityId, mailParameters, cancellationToken).ConfigureAwait(false);

        companySsiRepository.AttachAndModifyCompanySsiDetails(credentialId, c =>
            {
                c.CompanySsiDetailStatusId = status;
            },
            c =>
            {
                c.CompanySsiDetailStatusId = CompanySsiDetailStatusId.INACTIVE;
                c.DateLastChanged = _dateTimeProvider.OffsetNow;
            });

        if (processId is not null)
        {
            _repositories.GetInstance<IProcessStepRepository>().AttachAndModifyProcessSteps(
                processStepIds.Select(p => new ValueTuple<Guid, Action<ProcessStep>?, Action<ProcessStep>>(
                    p,
                    ps => ps.ProcessStepStatusId = ProcessStepStatusId.TODO,
                    ps => ps.ProcessStepStatusId = ProcessStepStatusId.SKIPPED
                )));
        }

        await _repositories.SaveAsync().ConfigureAwait(false);
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
            ),
            new CredentialStatus(
                _settings.StatusListUrl,
                StatusList)
        );
        var schema = JsonSerializer.Serialize(schemaData, Options);
        return await HandleCredentialProcessCreation(requestData.BusinessPartnerNumber, VerifiedCredentialTypeKindId.BPN, VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, schema, requestData.TechnicalUserDetails, null, requestData.CallbackUrl, companyCredentialDetailsRepository);
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
            ),
            new CredentialStatus(
                _settings.StatusListUrl,
                StatusList)
        );
        var schema = JsonSerializer.Serialize(schemaData, Options);
        return await HandleCredentialProcessCreation(requestData.HolderBpn, VerifiedCredentialTypeKindId.MEMBERSHIP, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, schema, requestData.TechnicalUserDetails, null, requestData.CallbackUrl, companyCredentialDetailsRepository);
    }

    public async Task<Guid> CreateFrameworkCredential(CreateFrameworkCredentialRequest requestData, CancellationToken cancellationToken)
    {
        var companyCredentialDetailsRepository = _repositories.GetInstance<ICompanySsiDetailsRepository>();
        var result = await companyCredentialDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(requestData.UseCaseFrameworkVersionId, requestData.UseCaseFrameworkId).ConfigureAwait(false);
        if (!result.Exists)
        {
            throw ControllerArgumentException.Create(CredentialErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND, new ErrorParameter[] { new("verifiedCredentialExternalTypeDetailId", requestData.UseCaseFrameworkId.ToString()) });
        }

        if (result.Expiry < _dateTimeProvider.OffsetNow)
        {
            throw ControllerArgumentException.Create(CredentialErrors.EXPIRY_DATE_IN_PAST);
        }

        if (string.IsNullOrWhiteSpace(result.Version))
        {
            throw ControllerArgumentException.Create(CredentialErrors.EMPTY_VERSION);
        }

        if (string.IsNullOrWhiteSpace(result.Template))
        {
            throw ControllerArgumentException.Create(CredentialErrors.EMPTY_TEMPLATE);
        }

        if (result.ExternalTypeIds.Count() != 1)
        {
            throw ControllerArgumentException.Create(CredentialErrors.MULTIPLE_USE_CASES);
        }

        var externalTypeId = result.ExternalTypeIds.Single().GetEnumValue();
        if (externalTypeId is null)
        {
            throw ControllerArgumentException.Create(CredentialErrors.EMPTY_EXTERNAL_TYPE_ID);
        }

        var holderDid = await GetHolderInformation(requestData.Holder, cancellationToken).ConfigureAwait(false);
        var schemaData = new FrameworkCredential(
            Guid.NewGuid(),
            Context,
            new[] { "VerifiableCredential", externalTypeId },
            externalTypeId,
            $"Framework Credential for UseCase {externalTypeId}",
            DateTimeOffset.UtcNow,
            result.Expiry,
            _settings.IssuerDid,
            new FrameworkCredentialSubject(
                holderDid,
                requestData.HolderBpn,
                "UseCaseFramework",
                externalTypeId,
                result.Template!,
                result.Version!
            ),
            new CredentialStatus(
                _settings.StatusListUrl,
                StatusList)
        );
        var schema = JsonSerializer.Serialize(schemaData, Options);
        return await HandleCredentialProcessCreation(_identity.Bpnl, VerifiedCredentialTypeKindId.FRAMEWORK, requestData.UseCaseFrameworkId, schema, requestData.TechnicalUserDetails, requestData.UseCaseFrameworkVersionId, requestData.CallbackUrl, companyCredentialDetailsRepository);
    }

    private async Task<string> GetHolderInformation(string didDocumentLocation, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(didDocumentLocation, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) || UrlPathInvalidCharsRegex.IsMatch(uri.AbsolutePath))
        {
            throw ControllerArgumentException.Create(CredentialErrors.INVALID_DID_LOCATION, null, nameof(didDocumentLocation));
        }

        var client = _clientFactory.CreateClient("didDocumentDownload");
        var result = await client.GetAsync(uri, cancellationToken)
            .CatchingIntoServiceExceptionFor("get-did-document").ConfigureAwait(false);
        var did = await result.Content.ReadFromJsonAsync<DidDocument>(Options, cancellationToken).ConfigureAwait(false);
        if (did == null)
        {
            throw ConflictException.Create(CredentialErrors.DID_NOT_SET);
        }

        return did.Id;
    }

    private async Task<Guid> HandleCredentialProcessCreation(
        string bpnl,
        VerifiedCredentialTypeKindId kindId,
        VerifiedCredentialTypeId typeId,
        string schema,
        TechnicalUserDetails? technicalUserDetails,
        Guid? detailVersionId,
        string? callbackUrl,
        ICompanySsiDetailsRepository companyCredentialDetailsRepository)
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

        Guid? processId = null;
        var status = CompanySsiDetailStatusId.PENDING;
        if (kindId != VerifiedCredentialTypeKindId.FRAMEWORK)
        {
            processId = CreateProcess();
            status = CompanySsiDetailStatusId.ACTIVE;
        }

        var ssiDetailId = companyCredentialDetailsRepository.CreateSsiDetails(
            bpnl,
            typeId,
            status,
            _settings.IssuerBpn,
            _identity.IdentityId,
            c =>
            {
                c.VerifiedCredentialExternalTypeDetailVersionId = detailVersionId;
                c.ProcessId = processId;
            }).Id;
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
                c.HolderWalletUrl = technicalUserDetails.WalletUrl;
                c.EncryptionMode = cryptoConfig.Index;
                c.InitializationVector = initializationVector;
                c.CallbackUrl = callbackUrl;
            });

        await _repositories.SaveAsync().ConfigureAwait(false);
        return ssiDetailId;
    }
}
