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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Tests.Setup;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Tests.BusinessLogic;

public class CredentialBusinessLogicTests
{
    private static readonly Guid CredentialId = Guid.NewGuid();
    private static readonly string Bpnl = "BPNL00000001TEST";

    private readonly IFixture _fixture;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IProcessStepRepository _processStepRepository;

    private readonly ICredentialBusinessLogic _sut;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IHttpClientFactory _clientFactory;
    private readonly IPortalService _portalService;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly IIdentityData _identity;

    public CredentialBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

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

        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid());
        A.CallTo(() => _identity.Bpnl).Returns(Bpnl);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        var options = A.Fake<IOptions<CredentialSettings>>();
        A.CallTo(() => options.Value).Returns(new CredentialSettings
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
            EncrptionConfigIndex = 0
        });

        _sut = new CredentialBusinessLogic(_issuerRepositories, _identityService, _dateTimeProvider, _clientFactory, _portalService, options);
    }

    #region GetUseCaseParticipationAsync

    [Fact]
    public async Task GetUseCaseParticipationAsync_ReturnsExpected()
    {
        // Arrange
        Setup_GetUseCaseParticipationAsync();

        // Act
        var result = await _sut.GetUseCaseParticipationAsync().ConfigureAwait(false);

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
        var result = await _sut.GetSsiCertificatesAsync().ConfigureAwait(false);

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
            .Returns(new ValueTuple<bool, SsiApprovalData>());
        async Task Act() => await _sut.ApproveCredential(notExistingId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.SSI_DETAILS_NOT_FOUND.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IDictionary<string, string>>._, A<CancellationToken>._)).MustNotHaveHappened();
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
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, approvalData));
        async Task Act() => await _sut.ApproveCredential(alreadyActiveId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.CREDENTIAL_NOT_PENDING.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IDictionary<string, string>>._, A<CancellationToken>._)).MustNotHaveHappened();
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
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, approvalData));
        async Task Act() => await _sut.ApproveCredential(alreadyActiveId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.BPN_NOT_SET.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IDictionary<string, string>>._, A<CancellationToken>._)).MustNotHaveHappened();
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

        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            typeId,
            VerifiedCredentialTypeKindId.FRAMEWORK,
            Bpnl,
            detailData
        );

        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(CredentialId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, data));
        async Task Act() => await _sut.ApproveCredential(CredentialId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.EXPIRY_DATE_IN_PAST.ToString());

        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IDictionary<string, string>>._, A<CancellationToken>._)).MustNotHaveHappened();
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

        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            default,
            VerifiedCredentialTypeKindId.FRAMEWORK,
            Bpnl,
            useCaseData
        );

        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(CredentialId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, data));

        // Act
        async Task Act() => await _sut.ApproveCredential(CredentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act).ConfigureAwait(false);
        ex.Message.Should().Be(CompanyDataErrors.CREDENTIAL_TYPE_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task ApproveCredential_WithDetailVersionNotSet_ThrowsConflictException()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var data = new SsiApprovalData(
            CompanySsiDetailStatusId.PENDING,
            VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK,
            VerifiedCredentialTypeKindId.FRAMEWORK,
            Bpnl,
            null
        );

        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(CredentialId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, data));
        async Task Act() => await _sut.ApproveCredential(CredentialId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IDictionary<string, string>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();

        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL))
            .MustNotHaveHappened();
        ex.Message.Should().Be(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_ID_NOT_SET.ToString());
    }

    [Theory]
    [InlineData(VerifiedCredentialTypeKindId.FRAMEWORK, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL)]
    [InlineData(VerifiedCredentialTypeKindId.MEMBERSHIP, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, VerifiedCredentialExternalTypeId.VEHICLE_DISMANTLE)]
    [InlineData(VerifiedCredentialTypeKindId.BPN, VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, VerifiedCredentialExternalTypeId.BUSINESS_PARTNER_NUMBER)]
    public async Task ApproveCredential_WithValid_ReturnsExpected(VerifiedCredentialTypeKindId kindId, VerifiedCredentialTypeId typeId, VerifiedCredentialExternalTypeId externalTypeId)
    {
        // Arrange
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
            kindId,
            Bpnl,
            detailData
        );

        var detail = new CompanySsiDetail(CredentialId, _identity.Bpnl, typeId, CompanySsiDetailStatusId.PENDING, Guid.NewGuid(), DateTimeOffset.Now);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiApprovalData(CredentialId))
            .Returns(new ValueTuple<bool, SsiApprovalData>(true, data));
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(CredentialId, A<Action<CompanySsiDetail>?>._, A<Action<CompanySsiDetail>>._!))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(detail);
                updateFields.Invoke(detail);
            });

        // Act
        await _sut.ApproveCredential(CredentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalService.AddNotification(A<string>._, A<Guid>._, NotificationTypeId.CREDENTIAL_APPROVAL, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalService.TriggerMail("CredentialApproval", A<Guid>._, A<IDictionary<string, string>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL))
            .MustHaveHappenedOnceExactly();

        detail.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.ACTIVE);
        detail.DateLastChanged.Should().Be(now);
    }

    #endregion

    #region RejectCredential

    [Fact]
    public async Task RejectCredential_WithoutExistingSsiDetail_ThrowsNotFoundException()
    {
        // Arrange
        var notExistingId = Guid.NewGuid();
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(notExistingId))
            .Returns(new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId>());
        async Task Act() => await _sut.RejectCredential(notExistingId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.SSI_DETAILS_NOT_FOUND.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialRejected", A<Guid>._, A<IDictionary<string, string>>._, A<CancellationToken>._)).MustNotHaveHappened();
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
            .Returns(new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId>(true, status, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK));
        async Task Act() => await _sut.RejectCredential(alreadyInactiveId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.CREDENTIAL_NOT_PENDING.ToString());
        A.CallTo(() => _portalService.TriggerMail("CredentialRejected", A<Guid>._, A<IDictionary<string, string>>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task RejectCredential_WithValidRequest_ReturnsExpected()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var detail = new CompanySsiDetail(CredentialId, _identity.Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, CompanySsiDetailStatusId.PENDING, Guid.NewGuid(), DateTimeOffset.Now);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetSsiRejectionData(CredentialId))
            .Returns(new ValueTuple<bool, CompanySsiDetailStatusId, VerifiedCredentialTypeId>(true, CompanySsiDetailStatusId.PENDING, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK));
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(CredentialId, A<Action<CompanySsiDetail>?>._, A<Action<CompanySsiDetail>>._!))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(detail);
                updateFields.Invoke(detail);
            });

        // Act
        await _sut.RejectCredential(CredentialId, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _portalService.TriggerMail("CredentialRejected", A<Guid>._, A<IDictionary<string, string>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalService.AddNotification(A<string>._, A<Guid>._, NotificationTypeId.CREDENTIAL_REJECTED, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();

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
        var result = await _sut.GetCertificateTypes().ToListAsync().ConfigureAwait(false);

        // Assert
        result.Should().HaveCount(7);
    }

    #endregion

    #region CreateBpnCredential

    [Fact]
    public async Task CreateBpnCredential_ReturnsExpected()
    {
        // Arrange
        var didId = Guid.NewGuid().ToString();
        var didDocument = new DidDocument(didId);
        var data = new CreateBpnCredentialRequest("https://example.org/holder/BPNL12343546", Bpnl, null, null);
        HttpRequestMessage? request = null;
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(didDocument))
        }, requestMessage => request = requestMessage);

        // Act
        await _sut.CreateBpnCredential(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("schema.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.Bpnl, VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, A<Guid>._, CompanySsiDetailStatusId.ACTIVE, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _documentRepository.AssignDocumentToCompanySsiDetails(A<Guid>._, A<Guid>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    #endregion

    #region CreateMembershipCredential

    [Fact]
    public async Task CreateMembershipCredential_ReturnsExpected()
    {
        // Arrange
        var didId = Guid.NewGuid().ToString();
        var didDocument = new DidDocument(didId);
        var data = new CreateMembershipCredentialRequest("https://example.org/holder/BPNL12343546", Bpnl, "Test", null, null);
        HttpRequestMessage? request = null;
        A.CallTo(() => _companySsiDetailsRepository.GetCertificateTypes(A<string>._))
            .Returns(Enum.GetValues<VerifiedCredentialTypeId>().ToAsyncEnumerable());
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(didDocument))
        }, requestMessage => request = requestMessage);

        // Act
        await _sut.CreateMembershipCredential(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("schema.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.Bpnl, VerifiedCredentialTypeId.DISMANTLER_CERTIFICATE, A<Guid>._, CompanySsiDetailStatusId.ACTIVE, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
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
            .Returns(new ValueTuple<bool, string?, string?, IEnumerable<string>, DateTimeOffset>());
        async Task Act() => await _sut.CreateFrameworkCredential(data, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.EXTERNAL_TYPE_DETAIL_NOT_FOUND.ToString());
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
            .Returns(new ValueTuple<bool, string?, string?, IEnumerable<string>, DateTimeOffset>(true, null, null, Enumerable.Empty<string>(), now.AddDays(-5)));
        async Task Act() => await _sut.CreateFrameworkCredential(data, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.EXPIRY_DATE_IN_PAST.ToString());
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
            .Returns(new ValueTuple<bool, string?, string?, IEnumerable<string>, DateTimeOffset>(true, null, null, Enumerable.Empty<string>(), now.AddDays(5)));
        async Task Act() => await _sut.CreateFrameworkCredential(data, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.EMPTY_VERSION.ToString());
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
            .Returns(new ValueTuple<bool, string?, string?, IEnumerable<string>, DateTimeOffset>(true, "1.0.0", null, Enumerable.Empty<string>(), now.AddDays(5)));
        async Task Act() => await _sut.CreateFrameworkCredential(data, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.EMPTY_TEMPLATE.ToString());
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
            .Returns(new ValueTuple<bool, string?, string?, IEnumerable<string>, DateTimeOffset>(true, "1.0.0", "https://example.org/tempalte", new[] { "test", "test" }, now.AddDays(5)));
        async Task Act() => await _sut.CreateFrameworkCredential(data, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.MULTIPLE_USE_CASES.ToString());
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
            .Returns(new ValueTuple<bool, string?, string?, IEnumerable<string>, DateTimeOffset>(true, "1.0.0", "https://example.org/tempalte", Enumerable.Empty<string>(), now.AddDays(5)));
        async Task Act() => await _sut.CreateFrameworkCredential(data, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ControllerArgumentException>(Act).ConfigureAwait(false);

        // Assert
        ex.Message.Should().Be(CompanyDataErrors.MULTIPLE_USE_CASES.ToString());
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
        var data = new CreateFrameworkCredentialRequest("https://example.org/holder/BPNL12343546", Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, useCaseId, null, null);
        HttpRequestMessage? request = null;
        A.CallTo(() => _companySsiDetailsRepository.CheckCredentialTypeIdExistsForExternalTypeDetailVersionId(useCaseId, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK))
            .Returns(new ValueTuple<bool, string?, string?, IEnumerable<string>, DateTimeOffset>(true, "1.0.0", "https://example.org/tempalte", Enumerable.Repeat("Test", 1), now.AddDays(5)));
        ConfigureHttpClientFactoryFixture(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(didDocument))
        }, requestMessage => request = requestMessage);

        // Act
        await _sut.CreateFrameworkCredential(data, CancellationToken.None).ConfigureAwait(false);

        // Assert
        A.CallTo(() => _documentRepository.CreateDocument("schema.json", A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, A<Action<Document>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(_identity.Bpnl, VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, A<Guid>._, CompanySsiDetailStatusId.PENDING, _identity.IdentityId, A<Action<CompanySsiDetail>>._))
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
