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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Extensions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using System.Text;
using System.Text.Json;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Tests.BusinessLogic;

public class CredentialBusinessLogicTests
{
    private static readonly string Bpnl = "BPNL00000001TEST";
    private readonly Guid CredentialId = Guid.NewGuid();
    private readonly Guid DocumentId = Guid.NewGuid();

    private readonly IFixture _fixture;

    private readonly ICredentialBusinessLogic _sut;
    private readonly IIdentityService _identityService;
    private readonly IIssuerRepositories _issuerRepositories;
    private readonly IIdentityData _identity;
    private readonly ICredentialRepository _credentialRepository;

    public CredentialBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customize<JsonDocument>(x => x.FromFactory(() => JsonDocument.Parse("{}")));

        _issuerRepositories = A.Fake<IIssuerRepositories>();
        _credentialRepository = A.Fake<ICredentialRepository>();
        _identity = A.Fake<IIdentityData>();

        _identityService = A.Fake<IIdentityService>();
        A.CallTo(() => _issuerRepositories.GetInstance<ICredentialRepository>()).Returns(_credentialRepository);

        A.CallTo(() => _identity.IdentityId).Returns(Guid.NewGuid().ToString());
        A.CallTo(() => _identity.Bpnl).Returns(Bpnl);
        A.CallTo(() => _identityService.IdentityData).Returns(_identity);

        _sut = new CredentialBusinessLogic(_issuerRepositories, _identityService);
    }

    #region GetCredentialDocument

    [Fact]
    public async Task GetCredentialDocument_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetSignedCredentialForCredentialId(CredentialId, Bpnl))
            .Returns(default((bool, bool, IEnumerable<(DocumentStatusId, byte[])>)));
        async Task Act() => await _sut.GetCredentialDocument(CredentialId);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(CredentialErrors.CREDENTIAL_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task GetCredentialDocument_WithDifferentCompany_ThrowsForbiddenException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetSignedCredentialForCredentialId(CredentialId, Bpnl))
            .Returns((true, false, Enumerable.Empty<ValueTuple<DocumentStatusId, byte[]>>()));
        async Task Act() => await _sut.GetCredentialDocument(CredentialId);

        // Act
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);

        // Assert
        ex.Message.Should().Be(CredentialErrors.COMPANY_NOT_ALLOWED.ToString());
    }

    [Fact]
    public async Task GetCredentialDocument_WithoutSignedCredential_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetSignedCredentialForCredentialId(CredentialId, Bpnl))
            .Returns((true, true, Enumerable.Empty<ValueTuple<DocumentStatusId, byte[]>>()));
        async Task Act() => await _sut.GetCredentialDocument(CredentialId);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(CredentialErrors.SIGNED_CREDENTIAL_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task GetCredentialDocument_WithValid_ReturnsExpected()
    {
        // Arrange
        var json = JsonDocument.Parse("{\"test\":\"test\"}");
        var schema = JsonSerializer.Serialize(json, JsonSerializerOptions.Default);
        A.CallTo(() => _credentialRepository.GetSignedCredentialForCredentialId(CredentialId, Bpnl))
            .Returns((true, true, Enumerable.Repeat((DocumentStatusId.ACTIVE, Encoding.UTF8.GetBytes(schema)), 1)));

        // Act
        var doc = await _sut.GetCredentialDocument(CredentialId);

        // Assert
        doc.RootElement.GetRawText().Should().Be("{\"test\":\"test\"}");
    }

    #endregion

    #region GetCredentialDocumentById

    [Fact]
    public async Task GetCredentialDocumentById_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetDocumentById(DocumentId, Bpnl))
            .Returns(default((bool, bool, string, DocumentStatusId, byte[], MediaTypeId)));
        async Task Act() => await _sut.GetCredentialDocumentById(DocumentId);

        // Act
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        ex.Message.Should().Be(CredentialErrors.DOCUMENT_NOT_FOUND.ToString());
    }

    [Fact]
    public async Task GetCredentialDocumentById_WithDifferentCompany_ThrowsForbiddenException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetDocumentById(DocumentId, Bpnl))
            .Returns((true, false, string.Empty, DocumentStatusId.ACTIVE, null!, MediaTypeId.JSON));
        async Task Act() => await _sut.GetCredentialDocumentById(DocumentId);

        // Act
        var ex = await Assert.ThrowsAsync<ForbiddenException>(Act);

        // Assert
        ex.Message.Should().Be(CredentialErrors.DOCUMENT_OTHER_COMPANY.ToString());
    }

    [Fact]
    public async Task GetCredentialDocumentById_WithoutInactiveDocument_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _credentialRepository.GetDocumentById(DocumentId, Bpnl))
            .Returns((true, true, string.Empty, DocumentStatusId.INACTIVE, null!, MediaTypeId.JSON));
        async Task Act() => await _sut.GetCredentialDocumentById(DocumentId);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be(CredentialErrors.DOCUMENT_INACTIVE.ToString());
    }

    [Fact]
    public async Task GetCredentialDocumentById_WithValid_ReturnsExpected()
    {
        // Arrange
        var json = JsonDocument.Parse("{\"test\":\"test\"}");
        var schema = JsonSerializer.Serialize(json, JsonSerializerOptions.Default);
        A.CallTo(() => _credentialRepository.GetDocumentById(DocumentId, Bpnl))
            .Returns((true, true, "test.json", DocumentStatusId.ACTIVE, Encoding.UTF8.GetBytes(schema), MediaTypeId.JSON));

        // Act
        var doc = await _sut.GetCredentialDocumentById(DocumentId);

        // Assert
        doc.MediaType.Should().Be(MediaTypeId.JSON.MapToMediaType());
        doc.FileName.Should().Be("test.json");
    }

    #endregion
}
