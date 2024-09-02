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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Handler;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Tests.Handler;

public class CredentialIssuerHandlerTests
{
    private readonly IOptions<ReissuanceSettings> _options;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICompanySsiDetailsRepository _companySsiDetailsRepository;
    private readonly ICredentialIssuerHandler _credentialIssuerHandler;

    public CredentialIssuerHandlerTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _documentRepository = A.Fake<IDocumentRepository>();
        _companySsiDetailsRepository = A.Fake<ICompanySsiDetailsRepository>();

        _options = A.Fake<IOptions<ReissuanceSettings>>();

        A.CallTo(() => _issuerRepositories.GetInstance<IDocumentRepository>()).Returns(_documentRepository);
        A.CallTo(() => _issuerRepositories.GetInstance<ICompanySsiDetailsRepository>()).Returns(_companySsiDetailsRepository);

        var credentialSettings = new ReissuanceSettings { IssuerBpn = "BPNL000000000000" };
        A.CallTo(() => _options.Value).Returns(credentialSettings);

        _credentialIssuerHandler = new CredentialIssuerHandler(_issuerRepositories, _options);
    }

    [Fact]
    public async Task HandleCredentialProcessCreation_ValidateProcessCreation_NoErrorExpected()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var ssiCredentialDetails = new List<CompanySsiDetail>();
        var schema = "{\"id\": \"21a1aa1f-b2f9-43bb-9c71-00b62bd1f8e0\", \"name\": \"BpnCredential\"}";
        var processStepRepository = A.Fake<IProcessStepRepository>();
        var process = new Process(Guid.NewGuid(), ProcessTypeId.CREATE_CREDENTIAL, Guid.NewGuid());
        var companySsiDetail = new CompanySsiDetail(
            Guid.NewGuid(),
            null!,
            VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            CompanySsiDetailStatusId.ACTIVE,
            _options.Value.IssuerBpn,
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow);

        var request = new IssuerCredentialRequest(
            Guid.NewGuid(),
            "BPNL000000000000",
            VerifiedCredentialTypeKindId.BPN,
            VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            DateTimeOffset.Now,
            "BPNL000000000000",
            schema,
            "holderWalletUrl",
            Guid.NewGuid(),
            "example.callback.cofinity"
        );
        var documentContent = Encoding.ASCII.GetBytes("document content");
        var hash = Encoding.ASCII.GetBytes(documentContent.GetHashCode().ToString());
        var document = new Document(documentId, documentContent, hash, "document", MediaTypeId.JSON, DateTimeOffset.Now, DocumentStatusId.ACTIVE, DocumentTypeId.PRESENTATION);

        A.CallTo(() => _documentRepository.CreateDocument(A<string>._, A<byte[]>._, A<byte[]>._, MediaTypeId.JSON, DocumentTypeId.PRESENTATION, A<Action<Document>?>._)).Returns(document);
        A.CallTo(() => _issuerRepositories.GetInstance<IProcessStepRepository>()).Returns(processStepRepository);
        A.CallTo(() => processStepRepository.CreateProcess(ProcessTypeId.CREATE_CREDENTIAL)).Returns(process);
        A.CallTo(() => _companySsiDetailsRepository.CreateSsiDetails(
            request.Bpnl,
            request.TypeId,
            CompanySsiDetailStatusId.ACTIVE,
            _options.Value.IssuerBpn,
            request.IdentiyId,
            A<Action<CompanySsiDetail>?>._))
            .Invokes((string bpnl, VerifiedCredentialTypeId verifiedCredentialTypeId,
                CompanySsiDetailStatusId companySsiDetailStatusId, string issuerBpn, string userId,
                Action<CompanySsiDetail>? setOptionalFields) =>
            {
                var detail = new CompanySsiDetail(Guid.NewGuid(), bpnl, verifiedCredentialTypeId, companySsiDetailStatusId, issuerBpn, userId, DateTimeOffset.UtcNow);
                setOptionalFields?.Invoke(detail);
                ssiCredentialDetails.Add(detail);
            })
            .Returns(companySsiDetail);

        // Act
        await _credentialIssuerHandler.HandleCredentialProcessCreation(request);

        // Assert
        ssiCredentialDetails.Should().ContainSingle().And.Satisfy(
            c => c.ReissuedCredentialId == request.Id);
        A.CallTo(() => _documentRepository.AssignDocumentToCompanySsiDetails(A<Guid>._, companySsiDetail.Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _companySsiDetailsRepository.CreateProcessData(companySsiDetail.Id, A<JsonDocument>._, A<VerifiedCredentialTypeKindId>._, A<Action<CompanySsiProcessData>?>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _issuerRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
    }
}
