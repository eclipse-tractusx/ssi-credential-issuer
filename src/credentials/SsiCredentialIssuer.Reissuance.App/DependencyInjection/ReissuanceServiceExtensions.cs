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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DateTimeProvider;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Handler;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Services;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.DependencyInjection;

/// <summary>
/// Extension method to register the renewal service and dependent services
/// </summary>
public static class ReissuanceServiceExtensions
{
    /// <summary>
    /// Adds the renewal service
    /// </summary>
    /// <param name="services">the services</param>
    /// <param name="section">Expiry section</param>
    /// <returns>the enriched service collection</returns>
    public static IServiceCollection AddReissuanceService(this IServiceCollection services, IConfigurationSection section)
    {
        services
            .AddOptions<ReissuanceSettings>()
            .ValidateOnStart()
            .ValidateDataAnnotations()
            .Bind(section);
        services
            .AddTransient<IReissuanceService, ReissuanceService>()
            .AddTransient<IDateTimeProvider, UtcDateTimeProvider>()
            .AddTransient<ICredentialIssuerHandler, CredentialIssuerHandler>();

        return services;
    }
}
