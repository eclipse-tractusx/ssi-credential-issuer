namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Reissuance.App.Services;

public interface IReissuanceService
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}
