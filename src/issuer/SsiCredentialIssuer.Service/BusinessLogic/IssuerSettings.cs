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
using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.BusinessLogic;

public class IssuerSettings
{
    /// <summary>
    /// The Did of the issuer
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string IssuerDid { get; set; } = null!;

    /// <summary>
    /// The maximum amount of elements for a page
    /// </summary>
    public int MaxPageSize { get; set; }

    [Required]
    public IEnumerable<EncryptionModeConfig> EncryptionConfigs { get; set; } = null!;

    [Required]
    public int EncryptionConfigIndex { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string StatusListUrl { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string StatusListType { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string IssuerBpn { get; set; } = null!;
}

public static class CompanyDataSettingsExtensions
{
    public static IServiceCollection ConfigureCredentialSettings(
        this IServiceCollection services,
        IConfigurationSection section)
    {
        services.AddOptions<IssuerSettings>()
            .Bind(section)
            .EnvironmentalValidation(section);
        return services;
    }
}
