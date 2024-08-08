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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Tests.Shared;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.Tests;

public class PortalServiceTests
{
    #region Initialization

    private readonly ITokenService _tokenService;
    private readonly IOptions<PortalSettings> _options;

    public PortalServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new PortalSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            BaseAddress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            TokenAddress = "https://example.org/token"
        });
        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region AddNotification

    [Fact]
    public async Task AddNotification_WithValid_DoesNotThrowException()
    {
        // Arrange
        var requester = Guid.NewGuid();
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<PortalService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new PortalService(_tokenService, _options);

        // Act
        await sut.AddNotification("Test", requester, NotificationTypeId.CREDENTIAL_APPROVAL, CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(NotificationRequest) &&
            ((x.Content as JsonContent)!.Value as NotificationRequest)!.Content == "Test" &&
            ((x.Content as JsonContent)!.Value as NotificationRequest)!.Receiver == requester &&
            ((x.Content as JsonContent)!.Value as NotificationRequest)!.NotificationTypeId == NotificationTypeId.CREDENTIAL_APPROVAL
        );
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system notification failed with statuscode 409 - Message: { \"message\": \"Framework test!\" }")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system notification failed with statuscode 400 - Message: { \"test\": \"123\" }")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system notification failed with statuscode 400 - Message: this is no json")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system notification failed with statuscode 403")]
    public async Task AddNotification_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var requester = Guid.NewGuid();
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<PortalService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new PortalService(_tokenService, _options);

        // Act
        async Task Act() => await sut.AddNotification("Test", requester, NotificationTypeId.CREDENTIAL_APPROVAL, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion

    #region TriggerMail

    [Fact]
    public async Task TriggerMail_WithValid_DoesNotThrowException()
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<PortalService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new PortalService(_tokenService, _options);

        // Act
        await sut.TriggerMail("Test", requesterId, Enumerable.Empty<MailParameter>(), CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(MailData) &&
            ((x.Content as JsonContent)!.Value as MailData)!.Template == "Test" &&
            ((x.Content as JsonContent)!.Value as MailData)!.Requester == requesterId
        );
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system mail failed with statuscode 409")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system mail failed with statuscode 400")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system mail failed with statuscode 400")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system mail failed with statuscode 403")]
    public async Task TriggerMail_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var requesterId = Guid.NewGuid();
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<PortalService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new PortalService(_tokenService, _options);

        // Act
        async Task Act() => await sut.TriggerMail("Test", requesterId, Enumerable.Empty<MailParameter>(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion
}
