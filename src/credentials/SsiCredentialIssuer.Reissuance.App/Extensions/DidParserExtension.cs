/********************************************************************************
 * Copyright (c) 2025 Cofinity-X GmbH
 * Copyright (c) 2025 Contributors to the Eclipse Foundation
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

using System.Text.RegularExpressions;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Extensions;

/// <summary>
/// Extension methods for parsing DIDs
/// </summary>
public static class DidParserExtensions
{
    // Define constants for the regex components
    private const string PCT_ENCODED = "(?:%[0-9a-fA-F]{2})";
    private const string ID_CHAR = @"(?:[a-zA-Z0-9._-]|" + PCT_ENCODED + ")";
    private const string METHOD = "([a-z0-9]+)";
    private const string METHOD_ID = @"((?:" + ID_CHAR + @"*:)*(" + ID_CHAR + "+))";
    private const string PARAM_CHAR = "[a-zA-Z0-9_.:%-]";
    private const string PARAM = @";" + PARAM_CHAR + @"+=" + PARAM_CHAR + "*";
    private const string PARAMS = "((" + PARAM + ")*)";
    private const string PATH = @"(/[^#?]*)?";
    private const string QUERY = @"([?][^#]*)?";
    private const string FRAGMENT = "(#.*)?";

    // Compile the full regex for the DID
    private static readonly Regex DidMatcher = new Regex(
        @"^did:" + METHOD + ":" + METHOD_ID + PARAMS + PATH + QUERY + FRAGMENT + "$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100)
    );

    /// <summary>
    /// Parses a DID URL into a <see cref="ParsedDid"/>
    /// </summary>
    /// <param name="didUrl">The DID URL to parse</param>
    /// <returns>The parsed DID or null if invalid</returns>
    public static ParsedDid? Parse(this string didUrl)
    {
        if (string.IsNullOrEmpty(didUrl))
            return null;

        var matchResult = DidMatcher.Match(didUrl);
        if (matchResult.Success)
        {
            return new ParsedDid
            {
                Did = $"did:{matchResult.Groups[1].Value}:{matchResult.Groups[2].Value}",
                Method = matchResult.Groups[1].Value,
                Id = matchResult.Groups[2].Value,
                DidUrl = didUrl
            };
        }

        return null;
    }
}
