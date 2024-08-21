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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Handlers;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Services;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Tests.Services;

public class ReissuanceServiceTests
{
    private readonly ICredentialIssuerHandler _credentialIssuerHandler;
    private readonly IReissuanceService _reissuanceService;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<ReissuanceService> _logger;

    public ReissuanceServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        var serviceScopeFactory = fixture.Create<IServiceScopeFactory>();
        var serviceScope = fixture.Create<IServiceScope>();
        var options = A.Fake<IOptions<ReissuanceExpirySettings>>();
        _logger = A.Fake<ILogger<ReissuanceService>>();
        var settings = new ReissuanceExpirySettings();
        var serviceProvider = fixture.Create<IServiceProvider>();
        _dateTimeProvider = A.Fake<IDateTimeProvider>();

        _credentialIssuerHandler = A.Fake<ICredentialIssuerHandler>();
        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();
        _credentialIssuerHandler = A.Fake<ICredentialIssuerHandler>();

        A.CallTo(() => options.Value).Returns(settings);
        A.CallTo(() => serviceProvider.GetService(typeof(IDateTimeProvider))).Returns(_dateTimeProvider);
        A.CallTo(() => serviceProvider.GetService(typeof(IIssuerRepositories))).Returns(_issuerRepositories);
        A.CallTo(() => _issuerRepositories.GetInstance<ICompanySsiDetailsRepository>()).Returns(_companySsiDetailsRepository);
        A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
        A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(DateTimeOffset.UtcNow);

        _reissuanceService = new ReissuanceService(serviceScopeFactory, _credentialIssuerHandler, options, _logger);
    }

    [Fact]
    public async void ExecuteAsync_ProcessCredentials_NoCredentialsAboutToExpire()
    {
        // Act
        await _reissuanceService.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _credentialIssuerHandler.HandleCredentialProcessCreation(A<IssuerCredentialRequest>._)).MustNotHaveHappened();
    }

    [Fact]
    public async void ExecuteAsync_ProcessCredentials_CreateBpnCredential()
    {
        // Arrange
        var schema = "{\"id\":\"6f05cac6-c073-4562-8540-8fc883807808\",\"name\":\"BpnCredential\",\"type\":[\"VerifiableCredential\",\"BpnCredential\"],\"issuer\":\"did:web:localhost:BPNL000000000000\",\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://w3id.org/catenax/credentials/v1.0.0\"],\"description\":\"BpnCredential\",\"issuanceDate\":\"2024-08-19T07:32:37.598099+00:00\",\"expirationDate\":\"2025-08-19T07:32:37.598079+00:00\",\"credentialStatus\":{\"id\":\"example.com\",\"type\":\"StatusList2021\"},\"credentialSubject\":{\"id\":\"did:web:localhost:BPNL000000000000\",\"bpn\":\"BPNL000000000000\",\"holderIdentifier\":\"BPNL000000000000\"}}";
        IssuerCredentialRequest? issuerCredentialRequest = null;
        var credentialsAboutToExpire = new CredentialAboutToExpireData(
            Guid.NewGuid(),
            "BPNL000000000000",
            Entities.Enums.VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            Entities.Enums.VerifiedCredentialTypeKindId.BPN,
            JsonDocument.Parse(schema),
            "BPNL000000000000",
            "http://localhost",
            Guid.NewGuid(),
            "callback.com"
        );

        A.CallTo(() => _companySsiDetailsRepository.GetCredentialsAboutToExpire(A<DateTimeOffset>._)).Returns((new[] { credentialsAboutToExpire }.ToAsyncEnumerable()));
        A.CallTo(() => _credentialIssuerHandler.HandleCredentialProcessCreation(A<IssuerCredentialRequest>._))
            .Invokes((IssuerCredentialRequest credentialRequest) =>
            {
                issuerCredentialRequest = credentialRequest;
            });

        // Act
        await _reissuanceService.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _credentialIssuerHandler.HandleCredentialProcessCreation(A<IssuerCredentialRequest>._)).MustHaveHappenedOnceExactly();
        Assert.NotNull(issuerCredentialRequest);
        issuerCredentialRequest.Id.Should().Be(credentialsAboutToExpire.Id);
        issuerCredentialRequest.Bpnl.Should().Be(credentialsAboutToExpire.HolderBpn);
        issuerCredentialRequest.TypeId.Should().Be(credentialsAboutToExpire.VerifiedCredentialTypeId);
    }

    [Fact]
    public async void ExecuteAsync_ProcessCredentials_HandleException()
    {
        // Arrange
        var schema = "{\"id\":\"6f05cac6-c073-4562-8540-8fc883807808\",\"name\":\"BpnCredential\",\"type\":[\"VerifiableCredential\",\"BpnCredential\"],\"issuer\":\"did:web:localhost:BPNL000000000000\",\"@context\":[\"https://www.w3.org/2018/credentials/v1\",\"https://w3id.org/catenax/credentials/v1.0.0\"],\"description\":\"BpnCredential\",\"issuanceDate\":\"2024-08-19T07:32:37.598099+00:00\",\"expirationDate\":\"2025-08-19T07:32:37.598079+00:00\",\"credentialStatus\":{\"id\":\"example.com\",\"type\":\"StatusList2021\"},\"credentialSubject\":{\"id\":\"did:web:localhost:BPNL000000000000\",\"bpn\":\"BPNL000000000000\",\"holderIdentifier\":\"BPNL000000000000\"}}";
        IssuerCredentialRequest? issuerCredentialRequest = null;

        var credentialsAboutToExpire = new CredentialAboutToExpireData(
            Guid.NewGuid(),
            "BPNL000000000000",
            Entities.Enums.VerifiedCredentialTypeId.MEMBERSHIP,
            Entities.Enums.VerifiedCredentialTypeKindId.MEMBERSHIP,
            JsonDocument.Parse(schema),
            "BPNL000000000000",
            "http://localhost",
            Guid.NewGuid(),
            "callback.com"
        );

        A.CallTo(() => _companySsiDetailsRepository.GetCredentialsAboutToExpire(A<DateTimeOffset>._)).Returns((new[] { credentialsAboutToExpire }.ToAsyncEnumerable()));
        A.CallTo(() => _credentialIssuerHandler.HandleCredentialProcessCreation(A<IssuerCredentialRequest>._)).Throws(new Exception());

        // Act
        await _reissuanceService.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _credentialIssuerHandler.HandleCredentialProcessCreation(A<IssuerCredentialRequest>._)).MustHaveHappenedOnceExactly();
        A.CallTo(_logger).Where(call => call.Method.Name == "Log" && call.GetArgument<LogLevel>(0) == LogLevel.Error).MustHaveHappened(1, Times.Exactly);

    }
}
