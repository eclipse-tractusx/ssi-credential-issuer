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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Expiry.App.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Services;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Expiry.App.Tests;

public class ExpiryCheckServiceTests
{
    private readonly IFixture _fixture;
    private readonly ExpiryCheckService _sut;

    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly IPortalService _portalService;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
    private readonly ExpiryCheckServiceSettings _settings;

    private readonly string Bpnl = "BPNL00000001TEST";
    private static readonly string IssuerBpnl = "BPNL000001ISSUER";

    public ExpiryCheckServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        A.CallTo(() => _issuerRepositories.GetInstance<ICompanySsiDetailsRepository>())
            .Returns(_companySsiDetailsRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<IProcessStepRepository>())
            .Returns(_processStepRepository);

        _dateTimeProvider = A.Fake<IDateTimeProvider>();
        _portalService = A.Fake<IPortalService>();

        var serviceProvider = _fixture.Create<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IIssuerRepositories))).Returns(_issuerRepositories);
        A.CallTo(() => serviceProvider.GetService(typeof(IDateTimeProvider))).Returns(_dateTimeProvider);
        A.CallTo(() => serviceProvider.GetService(typeof(IPortalService))).Returns(_portalService);
        var serviceScope = _fixture.Create<IServiceScope>();
        A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);
        var serviceScopeFactory = _fixture.Create<IServiceScopeFactory>();
        A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);

        _settings = new ExpiryCheckServiceSettings
        {
            ExpiredVcsToDeleteInMonth = 12,
            InactiveVcsToDeleteInWeeks = 8
        };
        _sut = new ExpiryCheckService(serviceScopeFactory, _fixture.Create<ILogger<ExpiryCheckService>>(), Options.Create(_settings));
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveAndEligibleForDeletion_RemovesEntry()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var inactiveVcsToDelete = now.AddDays(-(_settings.InactiveVcsToDeleteInWeeks * 7));
        var credentialId = Guid.NewGuid();
        var credentialScheduleData = _fixture.Build<CredentialScheduleData>()
            .With(x => x.IsVcToDelete, true)
            .Create();
        var data = new CredentialExpiryData[]
        {
            new(credentialId, Guid.NewGuid().ToString(), inactiveVcsToDelete.AddDays(-1), null, null, Bpnl, CompanySsiDetailStatusId.INACTIVE, VerifiedCredentialExternalTypeId.MEMBERSHIP_CREDENTIAL, credentialScheduleData)
        };
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetExpiryData(A<DateTimeOffset>._, A<DateTimeOffset>._, A<DateTimeOffset>._))
            .Returns(data.ToAsyncEnumerable());

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _companySsiDetailsRepository.RemoveSsiDetail(credentialId, A<string>._, A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WithPendingAndExpiryBeforeNow_DeclinesRequest()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var expiredVcsToDeleteInMonth = now.AddMonths(-_settings.ExpiredVcsToDeleteInMonth);
        var creatorUserId = Guid.NewGuid();
        var ssiDetail = new CompanySsiDetail(Guid.NewGuid(), Bpnl, VerifiedCredentialTypeId.MEMBERSHIP, CompanySsiDetailStatusId.PENDING, IssuerBpnl, creatorUserId.ToString(), now)
        {
            ExpiryDate = expiredVcsToDeleteInMonth.AddDays(-2),
            CreatorUserId = creatorUserId.ToString()
        };
        var credentialScheduleData = _fixture.Build<CredentialScheduleData>()
            .With(x => x.IsVcToDecline, true)
            .Create();
        var data = new CredentialExpiryData[]
        {
            new(ssiDetail.Id, ssiDetail.CreatorUserId, ssiDetail.ExpiryDate!.Value, ssiDetail.ExpiryCheckTypeId, null, Bpnl, ssiDetail.CompanySsiDetailStatusId, VerifiedCredentialExternalTypeId.MEMBERSHIP_CREDENTIAL, credentialScheduleData)
        };
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetExpiryData(A<DateTimeOffset>._, A<DateTimeOffset>._, A<DateTimeOffset>._))
            .Returns(data.ToAsyncEnumerable());

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _companySsiDetailsRepository.RemoveSsiDetail(ssiDetail.Id, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcess(ProcessTypeId.DECLINE_CREDENTIAL)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.CreateProcessStep(ProcessStepTypeId.REVOKE_CREDENTIAL, ProcessStepStatusId.TODO, A<Guid>._)).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(1, ExpiryCheckTypeId.ONE_DAY, ExpiryCheckTypeId.TWO_WEEKS)]
    [InlineData(13, ExpiryCheckTypeId.TWO_WEEKS, ExpiryCheckTypeId.ONE_MONTH)]
    [InlineData(27, ExpiryCheckTypeId.ONE_MONTH, null)]
    public async Task ExecuteAsync_WithActiveCloseToExpiry_NotifiesCreator(int days, ExpiryCheckTypeId expiryCheckTypeId, ExpiryCheckTypeId? currentExpiryCheckTypeId)
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var creatorUserId = Guid.NewGuid();
        var ssiDetail = new CompanySsiDetail(Guid.NewGuid(), Bpnl, VerifiedCredentialTypeId.MEMBERSHIP, CompanySsiDetailStatusId.ACTIVE, IssuerBpnl, creatorUserId.ToString(), now)
        {
            ExpiryDate = now.AddDays(-days),
            ExpiryCheckTypeId = currentExpiryCheckTypeId,
            CreatorUserId = creatorUserId.ToString()
        };
        var credentialScheduleData = _fixture.Build<CredentialScheduleData>()
            .With(x => x.IsVcToDecline, false)
            .With(x => x.IsOneDayNotification, expiryCheckTypeId == ExpiryCheckTypeId.ONE_DAY)
            .With(x => x.IsTwoWeeksNotification, expiryCheckTypeId == ExpiryCheckTypeId.TWO_WEEKS)
            .With(x => x.IsOneMonthNotification, expiryCheckTypeId == ExpiryCheckTypeId.ONE_MONTH)
            .Create();
        var data = new CredentialExpiryData[]
        {
            new(ssiDetail.Id, ssiDetail.CreatorUserId, ssiDetail.ExpiryDate!.Value, ssiDetail.ExpiryCheckTypeId, null, Bpnl, ssiDetail.CompanySsiDetailStatusId, VerifiedCredentialExternalTypeId.MEMBERSHIP_CREDENTIAL, credentialScheduleData)
        };
        A.CallTo(() => _dateTimeProvider.OffsetNow).Returns(now);
        A.CallTo(() => _companySsiDetailsRepository.GetExpiryData(A<DateTimeOffset>._, A<DateTimeOffset>._, A<DateTimeOffset>._))
            .Returns(data.ToAsyncEnumerable());
        A.CallTo(() => _companySsiDetailsRepository.AttachAndModifyCompanySsiDetails(A<Guid>._,
                A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> updateFields) =>
            {
                initialize?.Invoke(ssiDetail);
                updateFields.Invoke(ssiDetail);
            });

        // Act
        await _sut.ExecuteAsync(CancellationToken.None);

        // Assert
        A.CallTo(() => _companySsiDetailsRepository.RemoveSsiDetail(ssiDetail.Id, A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalService.AddNotification(A<string>._, creatorUserId, NotificationTypeId.CREDENTIAL_EXPIRY, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _portalService.TriggerMail("CredentialExpiry", creatorUserId, A<IEnumerable<MailParameter>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        ssiDetail.ExpiryCheckTypeId.Should().Be(expiryCheckTypeId);
    }
}
