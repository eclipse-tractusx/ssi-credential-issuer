/********************************************************************************
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
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Processes.ProcessIdentity.Tests;

public class ProcessIdentityIdServiceTests
{
    private readonly Guid _identityId;
    private readonly ProcessIdentityIdService _sut;

    public ProcessIdentityIdServiceTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _identityId = Guid.NewGuid();
        var options = Options.Create(new ProcessExecutionServiceSettings
        {
            IdentityId = _identityId.ToString(),
            LockExpirySeconds = 60
        });
        _sut = new ProcessIdentityIdService(options);
    }

    [Fact]
    public void GetIdentityId_ReturnsValid()
    {
        _sut.IdentityId.Should().Be(_identityId.ToString());
    }
}
