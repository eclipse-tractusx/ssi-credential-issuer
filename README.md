# SSI-Credential-Issuer

This repository contains the backend code for the SSI Credential Issuer written in C#.

For **information about the SSI Credential Issuer**, please refer to the documentation, especially the context and scope section in the [architecture documentation](./docs/architecture).

For **installation** details, please refer to the [README.md](./charts/ssi-credential-issuer/README.md) of the provided helm chart.

## How to build and run

Install the [.NET 8.0 SDK](https://www.microsoft.com/net/download).

Run the following command from the CLI:

```console
dotnet build src
```

Make sure the necessary config is added to the settings of the service you want to run.
Run the following command from the CLI in the directory of the service you want to run:

```console
dotnet run
```

## Notice for Docker image

This application provides container images for demonstration purposes.

See Docker notice files for more information:

- [credential-issuer-service](./docker//notice-credential-issuer-service.md)
- [credential-issuer-processes-worker](./docker/notice-credential-issuer-processes-worker.md)
- [credential-expiry-app](./docker/notice-credential-expiry-app.md)
- [credential-issuer-migrations](./docker/notice-credential-issuer-migrations.md)

## Contributing

See [Contribution details](/docs/technical-documentation/dev-process/How%20to%20contribute.md).

## License

Distributed under the Apache 2.0 License.
See [LICENSE](./LICENSE) for more information
