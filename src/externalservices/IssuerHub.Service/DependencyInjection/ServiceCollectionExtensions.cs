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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.Services;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIssuerHubService(this IServiceCollection services, IConfiguration config)
    {
        var section = config.GetSection("IssuerHub");
        services.AddOptions<IssuerHubSettings>()
            .Bind(section)
            .EnvironmentalValidation(section);

        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<IssuerHubSettings>>();

        services.AddScoped<IIssuerHubService, IssuerHubService>();
        services.AddHttpClient(nameof(IssuerHubService), c =>
        {
            c.BaseAddress = new Uri(settings.Value.BaseAddress);
            c.DefaultRequestHeaders.Add("x-api-key", settings.Value.ApiKey);
        });

        return services;
    }

    /// <summary>
    /// Registers the IssuerHub service only if the CredentialBackend is configured as "IssuerHub".
    /// This allows backward-compatible deployment with the Wallet backend without requiring IssuerHub configuration.
    /// </summary>
    public static IServiceCollection AddIssuerHubServiceIfConfigured(this IServiceCollection services, IConfiguration config)
    {
        var backendType = config.GetValue<string>("CredentialBackend") ?? "Wallet";
        if (string.Equals(backendType, "IssuerHub", StringComparison.OrdinalIgnoreCase))
        {
            return services.AddIssuerHubService(config);
        }

        return services;
    }
}
