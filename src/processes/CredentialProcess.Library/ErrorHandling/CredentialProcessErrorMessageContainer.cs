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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.CredentialProcess.Library.ErrorHandling;

[ExcludeFromCodeCoverage]
public class CredentialProcessErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<CredentialProcessErrors, string>
    {
        { CredentialProcessErrors.EXTERNAL_CREDENTIAL_ID_NOT_SET, "ExternalCredentialId must be set here" },
        { CredentialProcessErrors.CREDENTIAL_NOT_SET, "Credential must be set here" },
        { CredentialProcessErrors.WALLET_INFO_NOT_SET, "Wallet information must be set" },
        { CredentialProcessErrors.WALLET_SECRET_NOT_SET, "Wallet secret must be set" },
        { CredentialProcessErrors.CALLBACK_URL_NOT_SET, "CallbackUrl must be set" },
        { CredentialProcessErrors.CREDENTIAL_REQUEST_ID_NOT_SET, "External Credential Request Id must be set here" }
    }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type => typeof(CredentialProcessErrors);
    public IReadOnlyDictionary<int, string> MessageContainer => _messageContainer;
}

public enum CredentialProcessErrors
{
    EXTERNAL_CREDENTIAL_ID_NOT_SET,
    CREDENTIAL_NOT_SET,
    WALLET_INFO_NOT_SET,
    WALLET_SECRET_NOT_SET,
    CALLBACK_URL_NOT_SET,
    CREDENTIAL_REQUEST_ID_NOT_SET
}
