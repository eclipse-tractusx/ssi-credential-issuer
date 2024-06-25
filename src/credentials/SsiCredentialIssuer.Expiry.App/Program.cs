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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Expiry.App;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Expiry.App.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Portal.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Processes.Worker.Library.DependencyInjection;
using Serilog;

LoggingExtensions.EnsureInitialized();
Log.Information("Building worker");
try
{
    var host = Host
        .CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services
                .AddTransient<ITokenService, TokenService>()
                .AddProcessIdentity(hostContext.Configuration.GetSection("ProcessIdentity"))
                .AddExpiryCheckService(hostContext.Configuration.GetSection("Expiry"))
                .AddPortalService(hostContext.Configuration.GetSection("Portal"))
                .AddIssuerRepositories(hostContext.Configuration);
        })
        .AddLogging()
        .Build();
    Log.Information("Building worker completed");

    using var tokenSource = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        Log.Information("Canceling...");
        tokenSource.Cancel();
        e.Cancel = true;
    };

    Log.Information("Start processing");
    var workerInstance = host.Services.GetRequiredService<ExpiryCheckService>();
    await workerInstance.ExecuteAsync(tokenSource.Token).ConfigureAwait(ConfigureAwaitOptions.None);
    Log.Information("Execution finished shutting down");
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Server Shutting down");
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}
