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

using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Tests.Setup;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.Tests.Controllers;

public class IssuerControllerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    private const string BaseUrl = "/api/issuer";
    private readonly HttpClient _client = factory.CreateClient();

    #region GetCertificateTypes

    [Fact]
    public async Task GetCertificateTypes()
    {
        // Act
        var types = await _client.GetFromJsonAsync<IEnumerable<VerifiedCredentialTypeId>>($"{BaseUrl}/certificateTypes", JsonOptions);

        // Assert
        types.Should().NotBeNull().And.HaveCount(3).And.Satisfy(
            x => x == VerifiedCredentialTypeId.MEMBERSHIP,
            x => x == VerifiedCredentialTypeId.BUSINESS_PARTNER_NUMBER,
            x => x == VerifiedCredentialTypeId.FRAMEWORK_AGREEMENT
        );
    }

    #endregion

    #region Swagger

    [Fact]
    public async Task CheckSwagger_ReturnsExpected()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/swagger/v1/swagger.json");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
