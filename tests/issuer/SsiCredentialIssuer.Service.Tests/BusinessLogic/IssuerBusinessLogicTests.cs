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

using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Tests.BusinessLogic;

public class IssuerBusinessLogicTests
{
    private static readonly IEnumerable<string> Context = new[] { "https://www.w3.org/2018/credentials/v1", "https://w3id.org/catenax/credentials/v1.0.0" };
    private static readonly JsonSerializerOptions Options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private static readonly Guid CredentialId = Guid.NewGuid();
    private static readonly string Bpnl = "BPNL00000001TEST";
    private static readonly string IssuerBpnl = "BPNL000001ISSUER";

    private readonly IFixture _fixture;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IProcessStepRepository _processStepRepository;

    private readonly IIssuerBusinessLogic _sut;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IPortalService _portalService;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly IIdentityData _identity;

    public IssuerBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize<JsonDocument>(x => x.FromFactory(() => JsonDocument.Parse("{}")));

        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();
        _identity = A.Fake<IIdentityData>();

        _identityService = A.Fake<IIdentityService>();
        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        _clientFactory = A.Fake<IHttpClientFactory>();
        _portalService = A.Fake<IPortalService>();

        A.CallTo(() => _issuerRepositories.GetInstance<ICompanySsiDetailsRepository>()).Returns(_companySsiDetailsRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);

        var identityId = Guid.NewGuid();

        A.CallTo(() => _identity.IdentityId).Returns(identityId.ToString());
        A.CallTo(() => _identity.CompanyUserId).Returns(identityId);
        A.CallTo(() => _identity.IsServiceAccount).Returns(false);
        A.CallTo(() => _identity.Bpnl).Returns(Bpnl);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        var options = A.Fake<IOptions<IssuerSettings>>();
        A.CallTo(() => options.Value).Returns(new IssuerSettings
        {
            EncryptionConfigs = Enumerable.Repeat(new EncryptionModeConfig
            {
                Index = 0,
                CipherMode = CipherMode.ECB,
                PaddingMode = PaddingMode.PKCS7,
                EncryptionKey = "zlWxjv54PrNDbjYx7d3m4nz88qmCHG0AhYwu0UYSFGTo9psPbcVsNiqr14zhRgSd"
            }, 1),
            MaxPageSize = 15,
            IssuerDid = "did:web:example:org:bpn:18273z682734rt",
            IssuerBpn = IssuerBpnl,
            EncrptionConfigIndex = 0,
            StatusListUrl = "https://example.org/statuslist"
        });

        _sut = new IssuerBusinessLogic(_issuerRepositories, _identityService, _dateTimeProvider, _clientFactory, _portalService, options);
    }

    #region GetUseCaseParticipationAsync

    [Fact]
    public async Task GetUseCaseParticipationAsync_ReturnsExpected()
    {
        // Arrange
        Setup_GetUseCaseParticipationAsync();

        // Act
        var result = await _sut.GetUseCaseParticipationAsync();

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region GetSsiCertificatesAsync

    [Fact]
    public async Task GetSsiCertificatesAsync_ReturnsExpected()
    {
        // Arrange
        Setup_GetSsiCertificatesAsync();

        // Act
        var result = await _sut.GetSsiCertificatesAsync();

        // Assert
        result.Should().HaveCount(5);
    }

    #endregion

    #region ApproveCredential

    [Fact]
    public async Task ApproveCredential_WithoutExistingSsiDetail_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingId = Guid.NewGuid();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(notExistingId))
            .Returns(default((bool, SsiApprovalData)));
        Task Act() => _sut.ApproveCredential(notExistingId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.SSI_DETAILS_NOT_FOUND.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(CompanySsiDetailStatusId.ACTIVE)]
    [InlineData(CompanySsiDetailStatusId.INACTIVE)]
    public async Task ApproveCredential_WithStatusNotPending_ThrowsConflictException(CompanySsiDetailStatusId statusId)
    {
        // Arrange
        var alreadyActiveId = Guid.NewGuid();
        var approvalData = _fixture.Build<SsiApprovalData>()
            .With(x => x.Status, statusId)
            .Create();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(alreadyActiveId))
            .Returns((true, approvalData));
        Task Act() => _sut.ApproveCredential(alreadyActiveId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.CREDENTIAL_NOT_PENDING.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ApproveCredential_WithBpnNotSetActiveSsiDetail_ThrowsConflictException()
    {
        // Arrange
        var alreadyActiveId = Guid.NewGuid();
        var approvalData = _fixture.Build<SsiApprovalData>()
            .With(x => x.Status, CompanySsiDetailStatusId.PENDING)
            .With(x => x.Bpn, (string?)null)
            .Create();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(alreadyActiveId))
            .Returns((true, approvalData));
        Task Act() => _sut.ApproveCredential(alreadyActiveId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.BPN_NOT_SET.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ApproveCredential_WithExpiryInThePast_ReturnsExpected()
    {
        // Arrange
        const VerifiedCredentialTypeId typeId = VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK;
        var now = DateTimeOffset.Now;
        var detailData = new DetailData(
            VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL,
            "test",
            "1.0.0",
            DateTimeOffset.Now.AddDays(-5)
        );

        var schema = CreateSchema();
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            typeId,
            null,
            VerifiedCredentialTypeKindId.FRAMEWORK,
            Bpnl,
            JsonDocument.Parse(schema),
            detailData
        );

        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(CredentialId))
            .Returns((true, data));
        Task Act() => _sut.ApproveCredential(CredentialId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.EXPIRY_DATE_IN_PAST.ToString());

        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _portalService.AddNotification(A<string>._, A<Guid>._, A<NotificationTypeId>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ApproveCredential_WithInvalidCredentialType_ThrowsException()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var useCaseData = new DetailData(
            VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL,
            "test",
            "1.0.0",
            DateTimeOffset.UtcNow
        );

        var schema = CreateSchema();
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            default,
            null,
            VerifiedCredentialTypeKindId.FRAMEWORK,
            Bpnl,
            JsonDocument.Parse(schema),
            useCaseData
        );

        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(CredentialId))
            .Returns((true, data));

        // Act
        Task Act() => _sut.ApproveCredential(CredentialId, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be(IssuerErrors.CREDENTIAL_TYPE_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task ApproveCredential_WithDetailVersionNotSet_ThrowsConflictException()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK,
            null,
            VerifiedCredentialTypeKindId.FRAMEWORK,
            Bpnl,
            null,
            null
        );

        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(CredentialId))
            .Returns((true, data));
        Task Act() => _sut.ApproveCredential(CredentialId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL))
            .MustNotHaveHappened();
        ex.Message.Should().Be(IssuerErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET.ToString());
    }

    [Fact]
    public async Task ApproveCredential_WithAlreadyLinkedProcess_ThrowsConflictException()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK,
            Guid.NewGuid(),
            VerifiedCredentialTypeKindId.FRAMEWORK,
            Bpnl,
            null,
            new DetailData(
                VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL,
                "test",
                "1.0.0",
                DateTimeOffset.UtcNow
            )
        );

        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(CredentialId))
            .Returns((true, data));
        Task Act() => _sut.ApproveCredential(CredentialId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL))
            .MustNotHaveHappened();
        ex.Message.Should().Be(IssuerErrors.ALREADY_LINKED_PROCESS.ToString());
    }

    [Theory]
    [InlineData(VerifiedCredentialTypeKindId.FRAMEWORK, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL)]
    public async Task ApproveCredential_WithValid_ReturnsExpected(VerifiedCredentialTypeKindId kindId, VerifiedCredentialTypeId typeId, VerifiedCredentialExternalTypeId externalTypeId)
    {
        // Arrange
        var schema = CreateSchema();
        var processData = new CompanySsiProcessData(CredentialId, JsonDocument.Parse(schema), VerifiedCredentialTypeKindId.FRAMEWORK);
        var now = DateTimeOffset.UtcNow;
        var detailData = new DetailData(
            externalTypeId,
            "test",
            "1.0.0",
            DateTimeOffset.UtcNow
        );

        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            typeId,
            null,
            kindId,
            Bpnl,
            JsonDocument.Parse(schema),
            detailData
        );

        var detail = new CompanySsiDetail(CredentialId, _identity.Bpnl, typeId, CompanySsiDetailStatusId.PENDING, "", Guid.NewGuid().ToString(), DateTimeOffset.Now);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(CredentialId))
            .Returns((true, data));
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(CredentialId, A<Action<CompanySsiDetail>?>._, A<Action<CompanySsiDetail>>._!))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(detail);
                updateFields.Invoke(detail);
            });
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyProcessData(CredentialId, A<Action<CompanySsiProcessData>?>._, A<Action<CompanySsiProcessData>>._!))
            .Invokes((Guid _, Action<CompanySsiProcessData>? initialize, Action<CompanySsiProcessData> updateFields) =>
            {
                initialize?.Invoke(processData);
                updateFields.Invoke(processData);
            });

        // Act
        await _sut.ApproveCredential(CredentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _portalService.AddNotification(A<string>._, A<Guid>._, NotificationTypeId.CREDENTIAL_APPROVAL, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL))
            .MustHaveHappenedOnceExactly();

        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.ACTIVE);
        detail.DateLastChanged.Should().Be(now);
        processData.Schema.Deserialize<FrameworkCredential>()!.IssuanceDate.Should().Be(now);
    }

    private static string CreateSchema()
    {
        var schemaData = new FrameworkCredential(
            Guid.NewGuid(),
            Context,
            new[] { "VerifiableCredential", VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL.ToString() },
            VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL.ToString(),
            $"Framework Credential for UseCase {VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL}",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            "issuer",
            new FrameworkCredentialSubject(
                "test",
                "123",
                "UseCaseFramework",
                VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL.ToString(),
                "template",
                "1.0"
            ),
            new CredentialStatus(
                "https://example.com/statusList",
                "StatusList2021")
        );

        var schema = JsonSerializer.Serialize(schemaData, Options);
        return schema;
    }

    #endregion

    #region RejectCredential

    [Fact]
    public async Task RejectCredential_WithoutExistingSsiDetail_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingId = Guid.NewGuid();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(notExistingId))
            .Returns(default((bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId, Guid?, IEnumerable<Guid>)));
        Task Act() => _sut.RejectCredential(notExistingId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.SSI_DETAILS_NOT_FOUND.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialRejected", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(CompanySsiDetailStatusId.ACTIVE)]
    [InlineData(CompanySsiDetailStatusId.INACTIVE)]
    public async Task RejectCredential_WithNotPendingSsiDetail_ThrowsNotFoundException(CompanySsiDetailStatusId status)
    {
        // Arrange
        var alreadyInactiveId = Guid.NewGuid();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(alreadyInactiveId))
            .Returns((
                true,
                status,
                VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK,
                null,
                Enumerable.Empty<Guid>()
                ));
        Task Act() => _sut.RejectCredential(alreadyInactiveId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.CREDENTIAL_NOT_PENDING.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialRejected", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task RejectCredential_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var detail = new CompanySsiDetail(CredentialId, _identity.Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, CompanySsiDetailStatusId.PENDING, IssuerBpnl, Guid.NewGuid().ToString(), DateTimeOffset.Now);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(CredentialId))
            .Returns((
                true,
                CompanySsiDetailStatusId.PENDING,
                VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK,
                null,
                Enumerable.Empty<Guid>()));
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(CredentialId, A<Action<CompanySsiDetail>?>._, A<Action<CompanySsiDetail>>._!))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(detail);
                updateFields.Invoke(detail);
            });

        // Act
        await _sut.RejectCredential(CredentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _portalService.TriggerMail("CredentialRejected", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalService.AddNotification(A<string>._, A<Guid>._, NotificationTypeId.CREDENTIAL_REJECTED, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.INACTIVE);
        detail.DateLastChanged.Should().Be(now);
    }

    [Fact]
    public async Task RejectCredential_WithValidRequestAndPendingProcessStepIds_ReturnsExpectedAndSkipsSteps()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var detail = new CompanySsiDetail(CredentialId, _identity.Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, CompanySsiDetailStatusId.PENDING, IssuerBpnl, Guid.NewGuid().ToString(), DateTimeOffset.Now);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(CredentialId))
            .Returns((
                true,
                CompanySsiDetailStatusId.PENDING,
                VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK,
                Guid.NewGuid(),
                Enumerable.Repeat<Guid>(Guid.NewGuid(), 1)));
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(CredentialId, A<Action<CompanySsiDetail>?>._, A<Action<CompanySsiDetail>>._!))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(detail);
                updateFields.Invoke(detail);
            });

        // Act
        await _sut.RejectCredential(CredentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _portalService.TriggerMail("CredentialRejected", A<Guid>._, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalService.AddNotification(A<string>._, A<Guid>._, NotificationTypeId.CREDENTIAL_REJECTED, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)>>._)).MustHaveHappenedOnceExactly();

        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.INACTIVE);
        detail.DateLastChanged.Should().Be(now);
    }

    #endregion

    #region GetCertificateTypes

    [Fact]
    public async Task GetCertificateTypes_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _companySsiDetailsRepository.GetCertificateTypes(A<string>._))
            .Returns(Enum.GetValues<VerifiedCredentialTypeId>().ToAsyncEnumerable());

        // Act
        var result = await _sut.GetCertificateTypes().ToListAsync();

        // Assert
        result.Should().HaveCount(10);
    }

    #endregion

    #region CreateBpnCredential

    [Fact]
    public async Task CreateBpnCredential_ReturnsExpected()
    {
        // Arrange
        var didId = Guid.NewGuid().ToString();
        var didDocument = new DidDocument(didId);
        var data = new CreateBpnCredentialRequest("https://example.org/holder/BPNL12343546/did.json", Bpnl, null, null);
        HttpRequestMessage? request = null;
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(didDocument))
        }, requestMessage => request = requestMessage);

        // Act
        await _sut.CreateBpnCredential(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("schema.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.Bpnl, VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, CompanySsiDetailStatusId.ACTIVE, IssuerBpnl, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AssignDocumentToCompanySsiDetails(A<Guid>._, A<Guid>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("htt://test.com")]
    [InlineData("http://test.com")]
    [InlineData("abc://test.com")]
    [InlineData("test.com/example")]
    [InlineData("test")]
    [InlineData("https://testsite.test/<script>alert(\"TEST\")")]
    [InlineData("https://testsite.test?test=avd")]
    [InlineData("https://ab..test")]
    [InlineData("https://ab.com..test")]
    public async Task CreateBpnCredential_WithInvalidUri_ReturnsExpected(string holderUrl)
    {
        // Arrange
        var didId = Guid.NewGuid().ToString();
        var didDocument = new DidDocument(didId);
        var data = new CreateBpnCredentialRequest(holderUrl, Bpnl, null, null);
        HttpRequestMessage? request = null;
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(didDocument))
        }, requestMessage => request = requestMessage);
        Task Act() => _sut.CreateBpnCredential(data, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.INVALID_DID_LOCATION.ToString());
        ex.ParamName.Should().Be("didDocumentLocation");
        A.CallTo(() => _documentRepository.CreateDocument("schema.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.Bpnl, VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, CompanySsiDetailStatusId.ACTIVE, IssuerBpnl, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
            .MustNotHaveHappened();
        A.CallTo(() => _documentRepository.AssignDocumentToCompanySsiDetails(A<Guid>._, A<Guid>._))
            .MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion

    #region CreateMembershipCredential

    [Fact]
    public async Task CreateMembershipCredential_ReturnsExpected()
    {
        // Arrange
        var didId = Guid.NewGuid().ToString();
        var didDocument = new DidDocument(didId);
        var data = new CreateMembershipCredentialRequest("https://example.org/holder/BPNL12343546/did.json", Bpnl, "Test", null, null);
        HttpRequestMessage? request = null;
        A.CallTo(() => _companySsiDetailsRepository.GetCertificateTypes(A<string>._))
            .Returns(Enum.GetValues<VerifiedCredentialTypeId>().ToAsyncEnumerable());
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(didDocument))
        }, requestMessage => request = requestMessage);

        // Act
        await _sut.CreateMembershipCredential(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("schema.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.Bpnl, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, CompanySsiDetailStatusId.ACTIVE, IssuerBpnl, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AssignDocumentToCompanySsiDetails(A<Guid>._, A<Guid>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region CreateFrameworkCredential

    [Fact]
    public async Task CreateFrameworkCredential_WithVersionNotExisting_ThrowsControllerArgumentException()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var data = new CreateFrameworkCredentialRequest("BPNL0012HOLDER", Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, useCaseId, null, null);
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(useCaseId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns(default((bool, string?, string?, IEnumerable<VerifiedCredentialExternalTypeId>, DateTimeOffset)));
        Task Act() => _sut.CreateFrameworkCredential(data, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task CreateFrameworkCredential_WithExpiryInPast_ThrowsControllerArgumentException()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var now = DateTimeOffset.Now;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        var data = new CreateFrameworkCredentialRequest("BPNL0012HOLDER", Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, useCaseId, null, null);
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(useCaseId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns((true, null, null, Enumerable.Empty<VerifiedCredentialExternalTypeId>(), now.AddDays(-5)));
        Task Act() => _sut.CreateFrameworkCredential(data, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.EXPIRY_DATE_IN_PAST.ToString());
    }

    [Fact]
    public async Task CreateFrameworkCredential_WithEmptyVersion_ThrowsControllerArgumentException()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var now = DateTimeOffset.Now;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        var data = new CreateFrameworkCredentialRequest("BPNL0012HOLDER", Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, useCaseId, null, null);
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(useCaseId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns((true, null, null, Enumerable.Empty<VerifiedCredentialExternalTypeId>(), now.AddDays(5)));
        Task Act() => _sut.CreateFrameworkCredential(data, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.EMPTY_VERSION.ToString());
    }

    [Fact]
    public async Task CreateFrameworkCredential_WithEmptyTemplate_ThrowsControllerArgumentException()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var now = DateTimeOffset.Now;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        var data = new CreateFrameworkCredentialRequest("BPNL0012HOLDER", Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, useCaseId, null, null);
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(useCaseId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns((true, "1.0.0", null, Enumerable.Empty<VerifiedCredentialExternalTypeId>(), now.AddDays(5)));
        Task Act() => _sut.CreateFrameworkCredential(data, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.EMPTY_TEMPLATE.ToString());
    }

    [Fact]
    public async Task CreateFrameworkCredential_WithMoreThanOneUseCase_ThrowsControllerArgumentException()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var now = DateTimeOffset.Now;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        var data = new CreateFrameworkCredentialRequest("BPNL0012HOLDER", Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, useCaseId, null, null);
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(useCaseId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns((true, "1.0.0", "https://example.org/tempalte", new[] { VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL, VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL }, now.AddDays(5)));
        Task Act() => _sut.CreateFrameworkCredential(data, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.MULTIPLE_USE_CASES.ToString());
    }

    [Fact]
    public async Task CreateFrameworkCredential_WithNoUseCase_ThrowsControllerArgumentException()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var now = DateTimeOffset.Now;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        var data = new CreateFrameworkCredentialRequest("BPNL0012HOLDER", Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, useCaseId, null, null);
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(useCaseId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns((true, "1.0.0", "https://example.org/tempalte", Enumerable.Empty<VerifiedCredentialExternalTypeId>(), now.AddDays(5)));
        Task Act() => _sut.CreateFrameworkCredential(data, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act);

        // Assert
        ex.Message.Should().Be(IssuerErrors.MULTIPLE_USE_CASES.ToString());
    }

    [Fact]
    public async Task CreateFrameworkCredential_ReturnsExpected()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var didId = Guid.NewGuid().ToString();
        var didDocument = new DidDocument(didId);
        var now = DateTimeOffset.Now;
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        var data = new CreateFrameworkCredentialRequest("https://example.org/holder/BPNL12343546/did.json", Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, useCaseId, null, null);
        HttpRequestMessage? request = null;
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(useCaseId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns((true, "1.0.0", "https://example.org/tempalte", Enumerable.Repeat(VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL, 1), now.AddDays(5)));
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(didDocument))
        }, requestMessage => request = requestMessage);

        // Act
        await _sut.CreateFrameworkCredential(data, CancellationToken.None);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("schema.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, CompanySsiDetailStatusId.PENDING, IssuerBpnl, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AssignDocumentToCompanySsiDetails(A<Guid>._, A<Guid>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region Setup

    private void Setup_GetUseCaseParticipationAsync()
    {
        var verifiedCredentials = _fixture.Build<CompanySsiExternalTypeDetailTransferData>()
            .With(x => x.SsiDetailData, _fixture.CreateMany<CompanySsiDetailTransferData>(1))
            .CreateMany(5);
        A.CallTo(() => _companySsiDetailsRepository.GetUseCaseParticipationForCompany(Bpnl, A<DateTimeOffset>._))
            .Returns(_fixture.Build<UseCaseParticipationTransferData>().With(x => x.VerifiedCredentials, verifiedCredentials).CreateMany(5).ToAsyncEnumerable());
    }

    private void Setup_GetSsiCertificatesAsync()
    {
        A.CallTo(() => _companySsiDetailsRepository.GetSsiCertificates(Bpnl, A<DateTimeOffset>._))
            .Returns(_fixture.Build<SsiCertificateTransferData>().With(x => x.Credentials, Enumerable.Repeat(new SsiCertificateExternalTypeDetailTransferData(_fixture.Create<ExternalTypeDetailData>(), _fixture.CreateMany<CompanySsiDetailTransferData>(1)), 1)).CreateMany(5).ToAsyncEnumerable());
    }

    private void ConfigureHttpClientFactoryFixture(HttpResponseMessage httpResponseMessage, Action<HttpRequestMessage?>? setMessage = null)
    {
        var messageHandler = A.Fake<HttpMessageHandler>();
        A.CallTo(messageHandler) // mock protected method
            .Where(x => x.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(call =>
            {
                var message = call.Arguments.Get<HttpRequestMessage>(0);
                setMessage?.Invoke(message);
                return Task.FromResult(httpResponseMessage);
            });
        var httpClient = new HttpClient(messageHandler);
        _fixture.Inject(httpClient);

        A.CallTo(() => _clientFactory.CreateClient("didDocumentDownload")).Returns(httpClient);
    }

    #endregion
}
