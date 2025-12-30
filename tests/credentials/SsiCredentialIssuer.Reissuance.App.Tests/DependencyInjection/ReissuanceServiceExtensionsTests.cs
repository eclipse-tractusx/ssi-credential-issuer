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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Services;
using Xunit;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Tests.DependencyInjection;

public class ReissuanceServiceExtensionsTests
{
    [Fact]
    public void AddReissuanceService_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Reissuance:ExpiredVcsToReissueInDays", "30"}
            })
            .Build();
        var section = config.GetSection("Reissuance");

        // Act
        services.AddReissuanceService(section);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<ReissuanceService>().Should().NotBeNull();
        serviceProvider.GetService<IDateTimeProvider>().Should().BeOfType<UtcDateTimeProvider>();

        var options = serviceProvider.GetService<IOptions<ReissuanceServiceSettings>>();
        options.Should().NotBeNull();
        options!.Value.ExpiredVcsToReissueInDays.Should().Be(30);
    }
}
