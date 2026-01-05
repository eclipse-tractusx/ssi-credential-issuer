/********************************************************************************
 * Copyright (c) 2025 Cofinity-X GmbH
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
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Identity;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Services;

/// <summary>
/// Service to provide system identity
/// </summary>
public sealed class SystemIdentityService(IConfiguration config, IOptions<IssuerSettings> settings) : IIdentityService
{
    private readonly IIdentityData _data = new SystemIdentityData(
        config.GetValue<string>("Reissuance:CreatorUserId") ?? Guid.NewGuid().ToString(),
        settings.Value.IssuerBpn);

    /// <inheritdoc />
    public IIdentityData IdentityData => _data;

    private sealed class SystemIdentityData(string id, string bpn) : IIdentityData
    {
        public string IdentityId { get; } = id;
        public Guid? CompanyUserId => Guid.Empty;
        public string Bpnl { get; } = bpn;
        public bool IsServiceAccount => false;
    }
}
