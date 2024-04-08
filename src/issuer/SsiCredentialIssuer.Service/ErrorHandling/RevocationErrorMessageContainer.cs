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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Service.ErrorHandling;

[ExcludeFromCodeCoverage]
public class RevocationErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<RevocationDataErrors, string> {
        { RevocationDataErrors.CREDENTIAL_NOT_FOUND, "Credential {credentialId} does not exist" },
        { RevocationDataErrors.EXTERNAL_CREDENTIAL_ID_NOT_SET, "External Credential Id must be set for {credentialId}" },
        { RevocationDataErrors.NOT_ALLOWED_TO_REVOKE_CREDENTIAL, "Not allowed to revoke credential" }
    }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(RevocationDataErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum RevocationDataErrors
{
    CREDENTIAL_NOT_FOUND,
    EXTERNAL_CREDENTIAL_ID_NOT_SET,
    NOT_ALLOWED_TO_REVOKE_CREDENTIAL
}
