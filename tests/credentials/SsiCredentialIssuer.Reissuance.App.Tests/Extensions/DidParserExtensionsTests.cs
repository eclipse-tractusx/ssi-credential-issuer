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

using FluentAssertions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Extensions;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Tests.Extensions;

public class DidParserExtensionsTests
{
    [Theory]
    [InlineData("did:web:example.com", "did:web:example.com", "web", "example.com")]
    [InlineData("did:web:example.com:BPNL00000001TEST", "did:web:example.com:BPNL00000001TEST", "web", "example.com:BPNL00000001TEST")]
    [InlineData("did:example:123456789abcdefghi", "did:example:123456789abcdefghi", "example", "123456789abcdefghi")]
    [InlineData("did:example:123456789;service=agent/path?query=1#fragment", "did:example:123456789", "example", "123456789")]
    public void Parse_WithValidDid_ReturnsExpected(string input, string expectedDid, string expectedMethod, string expectedId)
    {
        // Act
        var result = input.Parse();

        // Assert
        result.Should().NotBeNull();
        result!.Did.Should().Be(expectedDid);
        result.Method.Should().Be(expectedMethod);
        result.Id.Should().Be(expectedId);
        result.DidUrl.Should().Be(input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-did")]
    [InlineData("did:web")]
    [InlineData("did:web:")]
    public void Parse_WithInvalidDid_ReturnsNull(string? input)
    {
        // Act
        var result = input?.Parse();

        // Assert
        result.Should().BeNull();
    }
}
