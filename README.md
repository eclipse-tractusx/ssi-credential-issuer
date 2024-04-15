# SSI-Credential-Issuer

This repository contains the backend code for the SSI Credential Issuer written in C#.

## How to build and run

Install the [.NET 7.0 SDK](https://www.microsoft.com/net/download).

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

## License

Distributed under the Apache 2.0 License.
See [LICENSE](./LICENSE) for more information
