namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Renewal.App;

public interface IRenewalService
{
    Task ExecuteAsync(CancellationToken stoppingToken);
}
