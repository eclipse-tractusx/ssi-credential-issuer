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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Expiry;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;
using System.Collections.Immutable;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Tests;

public class CredentialExpiryProcessHandlerTests
{
    private readonly Guid _credentialId = Guid.NewGuid();

    private readonly IWalletService _walletService;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly ICredentialRepository _credentialRepository;
    private readonly IPortalService _portalService;

    private readonly CredentialExpiryProcessHandler _sut;
    private readonly IFixture _fixture;
    private readonly IDocumentRepository _documentRepository;

    public CredentialExpiryProcessHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _credentialRepository = A.Fake<ICredentialRepository>();
        _documentRepository = A.Fake<IDocumentRepository>();

        A.CallTo(() => _issuerRepositories.GetInstance<ICredentialRepository>()).Returns(_credentialRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);

        _walletService = A.Fake<IWalletService>();
        _portalService = A.Fake<IPortalService>();

        _sut = new CredentialExpiryProcessHandler(_issuerRepositories, _walletService, _portalService);
    }

    #region RevokeCredential

    [Fact]
    public async Task RevokeCredential_WithValidData_ReturnsExpected()
    {
        // Arrange
        var externalCredentialId = Guid.NewGuid();
        var credential = new CompanySsiDetail(_credentialId, "Test", VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER, CompanySsiDetailStatusId.ACTIVE, "Test123", Guid.NewGuid().ToString(), DateTimeOffset.UtcNow);
        var document = _fixture
            .Build<Document>()
            .With(x => x.DocumentStatusId, DocumentStatusId.ACTIVE)
            .Create();
        A.CallTo(() => _credentialRepository.GetRevocationDataById(_credentialId, string.Empty))
            .Returns((true, false, externalCredentialId, credential.CompanySsiDetailStatusId, Enumerable.Repeat((document.Id, document.DocumentStatusId), 1)));
        A.CallTo(() => _credentialRepository.AttachAndModifyCredential(credential.Id, A<Action<CompanySsiDetail>>._, A<Action<CompanySsiDetail>>._))
            .Invokes((Guid _, Action<CompanySsiDetail>? initialize, Action<CompanySsiDetail> modify) =>
            {
                initialize?.Invoke(credential);
                modify(credential);
            });

        A.CallTo(() => _documentRepository.AttachAndModifyDocuments(A<IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)>>._))
            .Invokes((IEnumerable<(Guid DocumentId, Action<Document>? Initialize, Action<Document> Modify)> data) =>
            {
                data.Select(x =>
                    {
                        x.Initialize?.Invoke(document);
                        return document;
                    }
                ).ToImmutableArray();
                data.Select(x =>
                    {
                        x.Modify(document);
                        return document;
                    }
                ).ToImmutableArray();
            });

        // Act
        var result = await _sut.RevokeCredential(_credentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _walletService.RevokeCredentialForIssuer(externalCredentialId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        credential.CompanySsiDetailStatusId.Should().Be(CompanySsiDetailStatusId.REVOKED);
        document.DocumentStatusId.Should().Be(DocumentStatusId.INACTIVE);
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.TRIGGER_NOTIFICATION);
    }

    [Fact]
    public async Task RevokeCredential_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        var externalCredentialId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetRevocationDataById(_credentialId, string.Empty))
            .Returns((false, false, null, default, Enumerable.Empty<ValueTuple<Guid, DocumentStatusId>>()));
        Task Act() => _sut.RevokeCredential(_credentialId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be($"Credential {_credentialId} does not exist");
        A.CallTo(() => _walletService.RevokeCredentialForIssuer(externalCredentialId, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task RevokeCredential_WithEmptyExternalCredentialId_ThrowsConflictException()
    {
        // Arrange
        var externalCredentialId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetRevocationDataById(_credentialId, string.Empty))
            .Returns((true, true, null, default, Enumerable.Empty<ValueTuple<Guid, DocumentStatusId>>()));
        Task Act() => _sut.RevokeCredential(_credentialId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be($"External Credential Id must be set for {_credentialId}");
        A.CallTo(() => _walletService.RevokeCredentialForIssuer(externalCredentialId, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    #endregion

    #region TriggerNotification

    [Fact]
    public async Task TriggerNotification_WithValid_CallsExpected()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetCredentialNotificationData(_credentialId))
            .Returns((VerifiedCredentialExternalTypeId.PCF_CREDENTIAL, requesterId.ToString()));

        // Act
        var result = await _sut.TriggerNotification(_credentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _portalService.AddNotification(A<string>._, requesterId, NotificationTypeId.CREDENTIAL_REJECTED, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.TRIGGER_MAIL);
    }

    #endregion

    #region TriggerMail

    [Fact]
    public async Task TriggerMail_WithValid_CallsExpected()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        A.CallTo(() => _credentialRepository.GetCredentialNotificationData(_credentialId))
            .Returns((VerifiedCredentialExternalTypeId.PCF_CREDENTIAL, requesterId.ToString()));

        // Act
        var result = await _sut.TriggerMail(_credentialId, CancellationToken.None);

        // Assert
        A.CallTo(() => _portalService.TriggerMail("CredentialRejected", requesterId, A<IEnumerable<MailParameter>>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().BeNull();
    }

    #endregion
}
