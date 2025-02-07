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

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Tests.Shared;
using System.Security.Claims;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Tests.Identity;

public class MandatoryIdentityClaimHandlerTests
{
    private readonly IClaimsIdentityDataBuilder _claimsIdentityDataBuilder;
    private readonly IMockLogger<MandatoryIdentityClaimHandler> _mockLogger;
    private readonly ILogger<MandatoryIdentityClaimHandler> _logger;
    private readonly string Bpnl = "BPNL000000001TEST";

    public MandatoryIdentityClaimHandlerTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _claimsIdentityDataBuilder = new ClaimsIdentityDataBuilder();

        _mockLogger = A.Fake<IMockLogger<MandatoryIdentityClaimHandler>>();
        _logger = new MockLogger<MandatoryIdentityClaimHandler>(_mockLogger);
    }

    [Fact]
    public async Task HandleValidRequirement_WithoutUsername_ReturnsExpected()
    {
        // Arrange
        var principal = new ClaimsPrincipal(Array.Empty<ClaimsIdentity>());

        var context = new AuthorizationHandlerContext(Enumerable.Repeat(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity), 1), principal, null);
        var sut = new MandatoryIdentityClaimHandler(_claimsIdentityDataBuilder, _logger);

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().Be(false);
        _claimsIdentityDataBuilder.Status.Should().Be(IClaimsIdentityDataBuilderStatus.Empty);

        Assert.Throws<UnexpectedConditionException>(() => _claimsIdentityDataBuilder.IdentityId);
        Assert.Throws<UnexpectedConditionException>(() => _claimsIdentityDataBuilder.Bpnl);
        A.CallTo(() => _mockLogger.Log(LogLevel.Information, A<Exception>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task HandleValidRequirement_WithAllSet_ReturnsExpected()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity[]
        {
            new(new[]
            {
                new Claim("preferred_username", "eb4f6b1d-cde2-4e7b-86d5-e678421c0bd3"),
                new Claim("bpn", Bpnl)
            })
        });

        var context = new AuthorizationHandlerContext(Enumerable.Repeat(new MandatoryIdentityClaimRequirement(PolicyTypeId.ValidIdentity), 1), principal, null);
        var sut = new MandatoryIdentityClaimHandler(_claimsIdentityDataBuilder, _logger);

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().Be(true);
        _claimsIdentityDataBuilder.Status.Should().Be(IClaimsIdentityDataBuilderStatus.Complete);

        _claimsIdentityDataBuilder.IdentityId.Should().Be("eb4f6b1d-cde2-4e7b-86d5-e678421c0bd3");
        _claimsIdentityDataBuilder.Bpnl.Should().Be(Bpnl);
        A.CallTo(() => _mockLogger.Log(A<LogLevel>._, A<Exception>._, A<string>._))
            .MustNotHaveHappened();
    }
}
