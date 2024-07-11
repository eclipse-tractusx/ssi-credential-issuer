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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Authentication;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Controllers;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.DependencyInjection;
using System.Text.Json.Serialization;

const string VERSION = "v1";

await WebApplicationBuildRunner
    .BuildAndRunWebApplicationAsync<Program>(args, "issuer", VERSION, ".Issuer", builder =>
        {
            builder.Services
                .AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>()
                .AddTransient<IAuthorizationHandler, MandatoryIdentityClaimHandler>()
                .AddTransient<ITokenService, TokenService>()
                .AddClaimsIdentityService()
                .AddEndpointsApiExplorer()
                .AddIssuerRepositories(builder.Configuration)
                .ConfigureHttpJsonOptions(options =>
                    {
                        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    })
                .Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
                    {
                        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    })
                .AddServices(builder.Configuration)
                .AddWalletService(builder.Configuration)
                .AddPortalService(builder.Configuration.GetSection("Portal"))
                .AddSingleton<IErrorMessageService, ErrorMessageService>()
                .AddSingleton<IErrorMessageContainer, IssuerErrorMessageContainer>()
                .AddSingleton<IErrorMessageContainer, RevocationErrorMessageContainer>()
                .AddSingleton<IErrorMessageContainer, CredentialErrorMessageContainer>();
        },
    (app, _) =>
    {
        app.MapGroup("/api")
            .WithOpenApi()
            .MapIssuerApi()
            .MapRevocationApi()
            .MapCredentialApi();
    }).ConfigureAwait(ConfigureAwaitOptions.None);
