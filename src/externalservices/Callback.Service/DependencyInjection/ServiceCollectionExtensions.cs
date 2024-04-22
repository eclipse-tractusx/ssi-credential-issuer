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
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.Services;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Callback.Service.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCallbackService(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<CallbackSettings>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<LoggingHandler<CallbackService>>();

        return services
            .AddScoped<ICallbackService, CallbackService>()
            .AddCustomHttpClientWithAuthentication<CallbackService>(null);
    }
}
