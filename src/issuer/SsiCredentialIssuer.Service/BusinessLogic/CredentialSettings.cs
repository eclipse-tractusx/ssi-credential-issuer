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

using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Validation;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;

public class CredentialSettings
{
    /// <summary>
    /// The Did of the issuer
    /// </summary>
    public string IssuerDid { get; set; } = null!;

    /// <summary>
    /// The maximum amount of elements for a page
    /// </summary>
    public int MaxPageSize { get; set; }

    [Required]
    public IEnumerable<EncryptionModeConfig> EncryptionConfigs { get; set; } = null!;

    [Required]
    public int EncrptionConfigIndex { get; set; }
}

public static class CompanyDataSettingsExtensions
{
    [ExcludeFromCodeCoverage]
    public static IServiceCollection ConfigureCredentialSettings(
        this IServiceCollection services,
        IConfigurationSection section
    )
    {
        services.AddOptions<CredentialSettings>()
            .Bind(section)
            .ValidateDistinctValues(section)
            .ValidateEnumEnumeration(section)
            .ValidateOnStart();
        return services;
    }
}
