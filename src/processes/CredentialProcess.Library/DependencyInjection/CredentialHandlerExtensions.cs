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
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Backend;
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Creation;
using Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.Expiry;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.DependencyInjection;

public static class CredentialHandlerExtensions
{
    public static IServiceCollection AddCredentialCreationProcessHandler(this IServiceCollection services)
    {
        services
            .AddTransient<ICredentialCreationProcessHandler, CredentialCreationProcessHandler>();

        return services;
    }

    public static IServiceCollection AddCredentialExpiryProcessHandler(this IServiceCollection services)
    {
        services
            .AddTransient<ICredentialExpiryProcessHandler, CredentialExpiryProcessHandler>();

        return services;
    }

    /// <summary>
    /// Registers the credential backend based on the "CredentialBackend" configuration value.
    /// Use "IssuerHub" to use the Identity Hub Issuer Service, or "Wallet" (default) for the legacy Wallet.Service.
    /// </summary>
    public static IServiceCollection AddCredentialBackend(this IServiceCollection services, IConfiguration config)
    {
        var backendType = config.GetValue<string>("CredentialBackend") ?? "Wallet";
        if (string.Equals(backendType, "IssuerHub", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ICredentialBackend, IssuerHubCredentialBackend>();
        }
        else
        {
            services.AddScoped<ICredentialBackend, WalletCredentialBackend>();
        }

        return services;
    }
}
