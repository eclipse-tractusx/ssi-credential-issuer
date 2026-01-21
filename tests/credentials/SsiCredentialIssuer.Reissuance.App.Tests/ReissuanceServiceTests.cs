/********************************************************************************
 * Copyright (c) 2025 Cofinity-X GmbH
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Models;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Services.Tests;

public class ReissuanceServiceTests
{
    private readonly IFixture _fixture;
    private readonly ReissuanceService _sut;
    private readonly IIssuerRepositories _repositories;
    private readonly ICompanySsiDetailsRepository _ssiDetailsRepository;
    private readonly IIssuerBusinessLogic _issuerBusinessLogic;
    private readonly ReissuanceServiceSettings _settings;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ReissuanceServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _repositories = A.Fake<IIssuerRepositories>();
        _ssiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();
        _issuerBusinessLogic = A.Fake<IIssuerBusinessLogic>();

        A.CallTo(() => _repositories.GetInstance<ICompanySsiDetailsRepository>()).Returns(_ssiDetailsRepository);

        var serviceProvider = A.Fake<IServiceProvider>();
        var dateTimeProvider = A.Fake<IDateTimeProvider>();
        A.CallTo(() => dateTimeProvider.OffsetNow).Returns(DateTimeOffset.UtcNow);
        A.CallTo(() => serviceProvider.GetService(typeof(IIssuerRepositories))).Returns(_repositories);
        A.CallTo(() => serviceProvider.GetService(typeof(IIssuerBusinessLogic))).Returns(_issuerBusinessLogic);
        A.CallTo(() => serviceProvider.GetService(typeof(IDateTimeProvider))).Returns(dateTimeProvider);

        var scope = A.Fake<IServiceScope>();
        A.CallTo(() => scope.ServiceProvider).Returns(serviceProvider);

        _serviceScopeFactory = A.Fake<IServiceScopeFactory>();
        A.CallTo(() => _serviceScopeFactory.CreateScope()).Returns(scope);

        _settings = new ReissuanceServiceSettings { ExpiredVcsToReissueInDays = 30 };

        _sut = new ReissuanceService(
            _serviceScopeFactory,
            A.Fake<ILogger<ReissuanceService>>(),
            Options.Create(_settings));
    }

    [Fact]
    public async Task ExecuteAsync_WithBpnCredential_ReissuesSuccessfully()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var bpnl = "BPNL00000001TEST";
        var holderDid = "did:web:example.com:BPNL00000001TEST";
        var json = JsonSerializer.SerializeToDocument(new
        {
            credentialSubject = new { id = holderDid }
        });

        var data = new ReissueCredential(
            credentialId,
            "creator",
            DateTimeOffset.UtcNow.AddDays(-31),
            null,
            Guid.NewGuid(),
            bpnl,
            CompanySsiDetailStatusId.ACTIVE,
            VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            VerifiedCredentialExternalTypeId.BUSINESS_PARTNER_NUMBER,
            json);

        A.CallTo(() => _ssiDetailsRepository.GetExpiryCredentials(A<DateTimeOffset>._))
            .Returns(new[] { data }.ToAsyncEnumerable());

        var newId = Guid.NewGuid();
        A.CallTo(() => _issuerBusinessLogic.CreateBpnCredential(A<CreateBpnCredentialRequest>._, A<CancellationToken>._))
            .Returns(newId);

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _issuerBusinessLogic.CreateBpnCredential(
            A<CreateBpnCredentialRequest>.That.Matches(x => x.BusinessPartnerNumber == bpnl && x.Holder == "https://example.com/BPNL00000001TEST/did.json"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        A.CallTo(() => _ssiDetailsRepository.AttachAndModifyCompanySsiDetails(credentialId, null, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? _, Action<CompanySsiDetail> modify) =>
            {
                var detail = new CompanySsiDetail(credentialId, null!, default, default, null!, null!, default);
                modify(detail);
                detail.ReissuedCredentialId.Should().Be(newId);
            });
        A.CallTo(() => _ssiDetailsRepository.AttachAndModifyCompanySsiDetails(credentialId, null, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _repositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WithMembershipCredential_ReissuesSuccessfully()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var bpnl = "BPNL00000001TEST";
        var holderDid = "did:web:example.com";
        var memberOf = "did:web:association.com";
        var json = JsonSerializer.SerializeToDocument(new
        {
            credentialSubject = new { id = holderDid, memberOf = memberOf }
        });

        var data = new ReissueCredential(
            credentialId,
            "creator",
            DateTimeOffset.UtcNow.AddDays(-31),
            null,
            Guid.NewGuid(),
            bpnl,
            CompanySsiDetailStatusId.ACTIVE,
            VerifiedCredentialTypeId.MEMBERSHIP,
            VerifiedCredentialExternalTypeId.MEMBERSHIP_CREDENTIAL,
            json);

        A.CallTo(() => _ssiDetailsRepository.GetExpiryCredentials(A<DateTimeOffset>._))
            .Returns(new[] { data }.ToAsyncEnumerable());

        var newId = Guid.NewGuid();
        A.CallTo(() => _issuerBusinessLogic.CreateMembershipCredential(A<CreateMembershipCredentialRequest>._, A<CancellationToken>._))
            .Returns(newId);

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _issuerBusinessLogic.CreateMembershipCredential(
            A<CreateMembershipCredentialRequest>.That.Matches(x => x.HolderBpn == bpnl && x.MemberOf == memberOf),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _ssiDetailsRepository.AttachAndModifyCompanySsiDetails(credentialId, null, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? _, Action<CompanySsiDetail> modify) =>
            {
                var detail = new CompanySsiDetail(credentialId, null!, default, default, null!, null!, default);
                modify(detail);
                detail.ReissuedCredentialId.Should().Be(newId);
            });
        A.CallTo(() => _ssiDetailsRepository.AttachAndModifyCompanySsiDetails(credentialId, null, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _repositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WithFrameworkCredential_ReissuesSuccessfully()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var bpnl = "BPNL00000001TEST";
        var holderDid = "did:web:example.com";
        var detailVersionId = Guid.NewGuid();
        var json = JsonSerializer.SerializeToDocument(new
        {
            credentialSubject = new { id = holderDid }
        });

        var data = new ReissueCredential(
            credentialId,
            "creator",
            DateTimeOffset.UtcNow.AddDays(-31),
            null,
            detailVersionId,
            bpnl,
            CompanySsiDetailStatusId.ACTIVE,
            VerifiedCredentialTypeId.DATA_EXCHANGE_GOVERNANCE_CREDENTIAL,
            VerifiedCredentialExternalTypeId.DATA_EXCHANGE_GOVERNANCE_CREDENTIAL,
            json);

        A.CallTo(() => _ssiDetailsRepository.GetExpiryCredentials(A<DateTimeOffset>._))
            .Returns(new[] { data }.ToAsyncEnumerable());

        var newId = Guid.NewGuid();
        A.CallTo(() => _issuerBusinessLogic.CreateFrameworkCredentialBySystem(A<CreateFrameworkCredentialRequest>._, A<CancellationToken>._))
            .Returns(newId);

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _issuerBusinessLogic.CreateFrameworkCredentialBySystem(
            A<CreateFrameworkCredentialRequest>.That.Matches(x => x.HolderBpn == bpnl && x.UseCaseFrameworkVersionId == detailVersionId),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _ssiDetailsRepository.AttachAndModifyCompanySsiDetails(credentialId, null, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? _, Action<CompanySsiDetail> modify) =>
            {
                var detail = new CompanySsiDetail(credentialId, null!, default, default, null!, null!, default);
                modify(detail);
                detail.ReissuedCredentialId.Should().Be(newId);
            });
        A.CallTo(() => _ssiDetailsRepository.AttachAndModifyCompanySsiDetails(credentialId, null, A<Action<CompanySsiDetail>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _repositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedType_LogsWarning()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var data = new ReissueCredential(
            credentialId,
            "creator",
            DateTimeOffset.UtcNow.AddDays(-31),
            null,
            Guid.NewGuid(),
            "BPNL00000001TEST",
            CompanySsiDetailStatusId.ACTIVE,
            VerifiedCredentialTypeId.TRACEABILITY_FRAMEWORK, // Unsupported
            VerifiedCredentialExternalTypeId.TRACEABILITY_CREDENTIAL,
            JsonSerializer.SerializeToDocument(new { credentialSubject = new { id = "did:web:example.com" } }));

        A.CallTo(() => _ssiDetailsRepository.GetExpiryCredentials(A<DateTimeOffset>._))
            .Returns(new[] { data }.ToAsyncEnumerable());

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _issuerBusinessLogic.CreateBpnCredential(A<CreateBpnCredentialRequest>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _repositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithMalformedJson_ContinuesProcessing()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var data = new ReissueCredential(
            credentialId,
            "creator",
            DateTimeOffset.UtcNow.AddDays(-31),
            null,
            Guid.NewGuid(),
            "BPNL00000001TEST",
            CompanySsiDetailStatusId.ACTIVE,
            VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            VerifiedCredentialExternalTypeId.BUSINESS_PARTNER_NUMBER,
            JsonSerializer.SerializeToDocument(new { something = "invalid" })); // Missing credentialSubject

        A.CallTo(() => _ssiDetailsRepository.GetExpiryCredentials(A<DateTimeOffset>._))
            .Returns(new[] { data }.ToAsyncEnumerable());

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _repositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidDid_ContinuesProcessing()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var data = new ReissueCredential(
            credentialId,
            "creator",
            DateTimeOffset.UtcNow.AddDays(-31),
            null,
            Guid.NewGuid(),
            "BPNL00000001TEST",
            CompanySsiDetailStatusId.ACTIVE,
            VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            VerifiedCredentialExternalTypeId.BUSINESS_PARTNER_NUMBER,
            JsonSerializer.SerializeToDocument(new { credentialSubject = new { id = "not-a-did" } }));

        A.CallTo(() => _ssiDetailsRepository.GetExpiryCredentials(A<DateTimeOffset>._))
            .Returns(new[] { data }.ToAsyncEnumerable());

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _repositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithBusinessLogicException_ContinuesProcessing()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var json = JsonSerializer.SerializeToDocument(new { credentialSubject = new { id = "did:web:example.com" } });
        var data = new ReissueCredential(
            credentialId,
            "creator",
            DateTimeOffset.UtcNow.AddDays(-31),
            null,
            Guid.NewGuid(),
            "BPNL00000001TEST",
            CompanySsiDetailStatusId.ACTIVE,
            VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            VerifiedCredentialExternalTypeId.BUSINESS_PARTNER_NUMBER,
            json);

        A.CallTo(() => _ssiDetailsRepository.GetExpiryCredentials(A<DateTimeOffset>._))
            .Returns(new[] { data }.ToAsyncEnumerable());

        A.CallTo(() => _issuerBusinessLogic.CreateBpnCredential(A<CreateBpnCredentialRequest>._, A<CancellationToken>._))
            .Throws(new Exception("Wallet service down"));

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _repositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WithTopLevelException_Throws()
    {
        // Arrange
        A.CallTo(() => _serviceScopeFactory.CreateScope()).Throws(new Exception("Fatal error"));

        // Act
        var act = () => _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Fatal error");
    }

    [Fact]
    public async Task ExecuteAsync_WithSinglePartDid_ReissuesSuccessfully()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var bpnl = "BPNL00000001TEST";
        var holderDid = "did:web:example.com";
        var json = JsonSerializer.SerializeToDocument(new
        {
            credentialSubject = new { id = holderDid }
        });

        var data = new ReissueCredential(
            credentialId,
            "creator",
            DateTimeOffset.UtcNow.AddDays(-31),
            null,
            Guid.NewGuid(),
            bpnl,
            CompanySsiDetailStatusId.ACTIVE,
            VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            VerifiedCredentialExternalTypeId.BUSINESS_PARTNER_NUMBER,
            json);

        A.CallTo(() => _ssiDetailsRepository.GetExpiryCredentials(A<DateTimeOffset>._))
            .Returns(new[] { data }.ToAsyncEnumerable());

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _issuerBusinessLogic.CreateBpnCredential(
            A<CreateBpnCredentialRequest>.That.Matches(x => x.Holder == "https://example.com/.well-known/did.json"),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullJson_ContinuesProcessing()
    {
        // Arrange
        var credentialId = Guid.NewGuid();
        var data = new ReissueCredential(
            credentialId,
            "creator",
            DateTimeOffset.UtcNow.AddDays(-31),
            null,
            Guid.NewGuid(),
            "BPNL00000001TEST",
            CompanySsiDetailStatusId.ACTIVE,
            VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            VerifiedCredentialExternalTypeId.BUSINESS_PARTNER_NUMBER,
            null); // Null JSON

        A.CallTo(() => _ssiDetailsRepository.GetExpiryCredentials(A<DateTimeOffset>._))
            .Returns(new[] { data }.ToAsyncEnumerable());

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _repositories.SaveAsync()).MustNotHaveHappened();
    }
}
