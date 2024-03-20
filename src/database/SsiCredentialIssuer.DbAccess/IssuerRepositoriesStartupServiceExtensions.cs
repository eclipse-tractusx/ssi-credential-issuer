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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.DependencyInjection;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;

public static class IssuerRepositoriesStartupServiceExtensions
{
    public static IServiceCollection AddIssuerRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IIssuerRepositories, IssuerRepositories>()
            .AddDbAuditing()
            .AddDbContext<IssuerDbContext>(o => o
                    .UseNpgsql(configuration.GetConnectionString("IssuerDb")))
            .AddHealthChecks()
            .AddDbContextCheck<IssuerDbContext>("IssuerDbContext", tags: new[] { "issuerdb" });
        return services;
    }
}
