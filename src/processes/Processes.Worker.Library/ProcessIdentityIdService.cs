using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Auditing.Identity;

namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Processes.Worker.Library;

public class ProcessIdentityIdService : IIdentityIdService
{
    private readonly ProcessExecutionServiceSettings _settings;

    public ProcessIdentityIdService(IOptions<ProcessExecutionServiceSettings> options)
    {
        _settings = options.Value;
    }

    public Guid IdentityId => _settings.IdentityId;
}
