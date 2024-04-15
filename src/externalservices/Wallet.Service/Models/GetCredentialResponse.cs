using System.Text.Json;
using System.Text.Json.Serialization;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Wallet.Service.Models;

public record GetCredentialResponse(
    [property: JsonPropertyName("verifiableCredential")] string VerifiableCredential,
    [property: JsonPropertyName("credential")] JsonDocument Credential,
    [property: JsonPropertyName("signing_key_id")] string SigningKeyId
);
