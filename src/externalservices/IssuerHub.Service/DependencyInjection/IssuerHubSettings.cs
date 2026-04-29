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

using System.ComponentModel.DataAnnotations;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.IssuerHub.Service.DependencyInjection;

public class IssuerHubSettings
{
    /// <summary>
    /// Base address of the Identity Hub Issuer Service (e.g. https://issuerservice.example.com)
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string BaseAddress { get; set; } = null!;

    /// <summary>
    /// API Key for authenticating with the Identity Hub super-user API
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ApiKey { get; set; } = null!;

    /// <summary>
    /// The participant context ID of the issuer in the Identity Hub
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ParticipantContextId { get; set; } = null!;

    /// <summary>
    /// Base path for the Issuer Admin API (default: /api/issuer)
    /// </summary>
    public string IssuerAdminBasePath { get; set; } = "/api/issuer";

    /// <summary>
    /// Base path for the Identity API (default: /api/identity)
    /// </summary>
    public string IdentityBasePath { get; set; } = "/api/identity";
}
