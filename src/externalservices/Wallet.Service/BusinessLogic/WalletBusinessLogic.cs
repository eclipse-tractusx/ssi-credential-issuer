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

using Json.Schema;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess;
using Org.Eclipse.TractusX.SsiCredentialIssuer.DBAccess.Repositories;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Enums;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.DependencyInjection;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Services;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using EncryptionInformation = Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models.EncryptionInformation;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.BusinessLogic;

public class WalletBusinessLogic(
    IWalletService walletService,
    IIssuerRepositories repositories,
    IOptions<WalletSettings> options)
    : IWalletBusinessLogic
{
    private readonly WalletSettings _settings = options.Value;

    public async Task CreateCredential(Guid companySsiDetailId, JsonDocument schema, CancellationToken cancellationToken)
    {
        var credentialId = await walletService.CreateCredential(schema, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        repositories.GetInstance<ICompanySsiDetailsRepository>().AttachAndModifyCompanySsiDetails(companySsiDetailId, c => c.ExternalCredentialId = null, c => c.ExternalCredentialId = credentialId);
    }

    public async Task SignCredential(Guid companySsiDetailId, Guid credentialId, CancellationToken cancellationToken)
    {
        var credential = await walletService.SignCredential(credentialId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        repositories.GetInstance<ICompanySsiDetailsRepository>().AttachAndModifyCompanySsiDetails(companySsiDetailId, c => c.Credential = null, c => c.Credential = credential);
    }

    public async Task GetCredential(Guid credentialId, Guid externalCredentialId, VerifiedCredentialTypeKindId kindId, CancellationToken cancellationToken)
    {
        var credential = await walletService.GetCredential(externalCredentialId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await ValidateSchema(kindId, credential, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        credential.WriteTo(writer);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var documentContent = stream.ToArray();
        var hash = SHA512.HashData(documentContent);
        var documentRepository = repositories.GetInstance<IDocumentRepository>();
        var docId = documentRepository.CreateDocument("signed-credential.json", documentContent, hash, MediaTypeId.JSON, DocumentTypeId.VERIFIED_CREDENTIAL, null).Id;
        documentRepository.AssignDocumentToCompanySsiDetails(docId, credentialId);
    }

    private static async Task ValidateSchema(VerifiedCredentialTypeKindId kindId, JsonDocument content, CancellationToken cancellationToken)
    {
        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (location == null)
        {
            throw new UnexpectedConditionException("Assembly location must be set");
        }

        var path = Path.Combine(location, "Schemas", $"{kindId}Credential.schema.json");
        var schemaJson = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        var schema = JsonSchema.FromText(schemaJson);
        SchemaRegistry.Global.Register(schema);
        var results = schema.Evaluate(content);
        if (!results.IsValid)
        {
            throw new ServiceException($"Invalid schema for type {kindId}");
        }
    }

    public async Task CreateCredentialForHolder(Guid companySsiDetailId, string holderWalletUrl, string clientId, EncryptionInformation encryptionInformation, string credential, CancellationToken cancellationToken)
    {
        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var secret = cryptoHelper.Decrypt(encryptionInformation.Secret, encryptionInformation.InitializationVector);

        await walletService
            .CreateCredentialForHolder(holderWalletUrl, clientId, secret, credential, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        repositories.GetInstance<ICompanySsiDetailsRepository>().AttachAndModifyProcessData(companySsiDetailId,
            c =>
            {
                c.ClientId = clientId;
                c.ClientSecret = encryptionInformation.Secret;
                c.InitializationVector = encryptionInformation.InitializationVector;
                c.EncryptionMode = encryptionInformation.EncryptionMode;
            },
            c =>
            {
                c.ClientId = null;
                c.ClientSecret = null;
                c.InitializationVector = null;
                c.EncryptionMode = null;
            });
    }
}
