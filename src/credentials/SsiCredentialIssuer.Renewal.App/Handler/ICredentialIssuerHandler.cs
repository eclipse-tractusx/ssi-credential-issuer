namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Renewal.App.Handlers;

public interface ICredentialIssuerHandler
{
    public Task HandleCredentialProcessCreation(IssuerCredentialRequest issuerCredentialRequest);
}
