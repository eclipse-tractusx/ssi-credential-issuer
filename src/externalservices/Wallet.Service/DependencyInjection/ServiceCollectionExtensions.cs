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
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWalletService(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<WalletSettings>()
            .Bind(config.GetSection("Wallet"))
            .ValidateOnStart();

        services.AddTransient<LoggingHandler<WalletService>>();

        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<WalletSettings>>();
        return services
            .AddScoped<IBasicAuthTokenService, BasicAuthTokenService>()
            .AddScoped<IWalletBusinessLogic, WalletBusinessLogic>()
            .AddScoped<IWalletService, WalletService>()
            .AddCustomHttpClientWithAuthentication<WalletService>(settings.Value.BaseAddress);
    }
}
