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
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Models;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Services;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Tests.Shared;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Tests;

public class CallbackServiceTests
{
    #region Initialization

    private readonly ITokenService _tokenService;
    private readonly IOptions<CallbackSettings> _options;
    private readonly IFixture _fixture;

    public CallbackServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _options = Options.Create(new CallbackSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            TokenAddress = "https://example.org/token"
        });
        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region TriggerCallback

    [Fact]
    public async Task TriggerCallback_WithValid_DoesNotThrowException()
    {
        // Arrange
        var data = new IssuerResponseData("Test1", IssuerResponseStatus.SUCCESSFUL, "test 123");
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<CallbackService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CallbackService(_tokenService, _options);

        // Act
        await sut.TriggerCallback("https://example.org/callback", data, CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(IssuerResponseData) &&
            ((x.Content as JsonContent)!.Value as IssuerResponseData)!.Status == IssuerResponseStatus.SUCCESSFUL &&
            ((x.Content as JsonContent)!.Value as IssuerResponseData)!.Bpn == "Test1"
        );
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict, "{ \"message\": \"Framework test!\" }", "call to external system callback failed with statuscode 409")]
    [InlineData(HttpStatusCode.BadRequest, "{ \"test\": \"123\" }", "call to external system callback failed with statuscode 400")]
    [InlineData(HttpStatusCode.BadRequest, "this is no json", "call to external system callback failed with statuscode 400")]
    [InlineData(HttpStatusCode.Forbidden, null, "call to external system callback failed with statuscode 403")]
    public async Task TriggerCallback_WithConflict_ThrowsServiceExceptionWithErrorContent(HttpStatusCode statusCode, string? content, string message)
    {
        // Arrange
        var httpMessageHandlerMock = content == null
            ? new HttpMessageHandlerMock(statusCode)
            : new HttpMessageHandlerMock(statusCode, new StringContent(content));
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<CallbackService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CallbackService(_tokenService, _options);
        Task Act() => sut.TriggerCallback("https://example.org/callback", _fixture.Create<IssuerResponseData>(), CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);

        // Assert
        ex.Message.Should().Be(message);
        ex.StatusCode.Should().Be(statusCode);
    }

    #endregion
}
