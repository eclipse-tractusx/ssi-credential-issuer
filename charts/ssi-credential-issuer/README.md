# Helm chart for SSI Credential Issuer

This helm chart installs the Catena-X SSI Credential Issuer application.

For further information please refer to [Technical Documentation](./docs/technical-documentation).

The referenced container images are for demonstration purposes only.

## Installation

To install the chart with the release name `ssi-credential-issuer`:

```shell
$ helm repo add tractusx-dev https://eclipse-tractusx.github.io/charts/dev
$ helm install ssi-credential-issuer tractusx-dev/ssi-credential-issuer
```

To install the helm chart into your cluster with your values:

```shell
$ helm install -f your-values.yaml ssi-credential-issuer tractusx-dev/ssi-credential-issuer
```

To use the helm chart as a dependency:

```yaml
dependencies:
  - name: ssi-credential-issuer
    repository: https://eclipse-tractusx.github.io/charts/dev
    version: 1.0.0
```

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| https://charts.bitnami.com/bitnami | postgresql | 12.12.x |

## Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| portalBackendAddress | string | `"https://portal-backend.example.org"` | Provide portal-backend base address. |
| walletAddress | string | `"https://wallet.example.org"` |  |
| walletTokenAddress | string | `"https://wallet.example.org/oauth/token"` |  |
| issuer.image.name | string | `"docker.io/tractusx/ssi-credential-issuer-service"` |  |
| issuer.image.tag | string | `""` |  |
| issuer.imagePullPolicy | string | `"IfNotPresent"` |  |
| issuer.resources | object | `{"limits":{"cpu":"45m","memory":"400M"},"requests":{"cpu":"15m","memory":"400M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| issuer.logging.businessLogic | string | `"Information"` |  |
| issuer.logging.default | string | `"Information"` |  |
| issuer.healthChecks.startup.path | string | `"/health/startup"` |  |
| issuer.healthChecks.startup.tags[0].name | string | `"HEALTHCHECKS__0__TAGS__1"` |  |
| issuer.healthChecks.startup.tags[0].value | string | `"issuerdb"` |  |
| issuer.healthChecks.liveness.path | string | `"/healthz"` |  |
| issuer.healthChecks.readyness.path | string | `"/ready"` |  |
| issuer.swaggerEnabled | bool | `false` |  |
| issuer.portal.scope | string | `"openid"` |  |
| issuer.portal.grantType | string | `"client_credentials"` |  |
| issuer.portal.clientId | string | `"portal-client-id"` | Provide portal client-id from CX IAM centralidp. |
| issuer.portal.clientSecret | string | `""` | Client-secret for portal client-id. Secret-key 'portal-client-secret'. |
| issuer.credential.issuerDid | string | `"did:web:example"` |  |
| issuer.credential.issuerBpn | string | `"BPNL00000001TEST"` |  |
| issuer.credential.statusListUrl | string | `"https://example.org/statuslist"` |  |
| issuer.credential.encryptionConfigIndex | int | `0` |  |
| issuer.credential.encryptionConfigs.index0.index | int | `0` |  |
| issuer.credential.encryptionConfigs.index0.cipherMode | string | `"CBC"` |  |
| issuer.credential.encryptionConfigs.index0.paddingMode | string | `"PKCS7"` |  |
| issuer.credential.encryptionConfigs.index0.encryptionKey | string | `""` | EncryptionKey for wallet. Secret-key 'credential-encryption-key0'. Expected format is 256 bit (64 digits) hex. |
| issuermigrations.name | string | `"migrations"` |  |
| issuermigrations.image.name | string | `"docker.io/tractusx/ssi-credential-issuer-migrations"` |  |
| issuermigrations.image.tag | string | `""` |  |
| issuermigrations.imagePullPolicy | string | `"IfNotPresent"` |  |
| issuermigrations.resources | object | `{"limits":{"cpu":"45m","memory":"200M"},"requests":{"cpu":"15m","memory":"200M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| issuermigrations.seeding.testDataEnvironments | string | `""` |  |
| issuermigrations.seeding.testDataPaths | string | `"Seeder/Data"` |  |
| issuermigrations.logging.default | string | `"Information"` |  |
| issuermigrations.processIdentity.identityId | string | `"ac1cf001-7fbc-1f2f-817f-bce058020006"` |  |
| processesworker.name | string | `"processesworker"` |  |
| processesworker.image.name | string | `"docker.io/tractusx/ssi-credential-issuer-processes-worker"` |  |
| processesworker.image.tag | string | `""` |  |
| processesworker.imagePullPolicy | string | `"IfNotPresent"` |  |
| processesworker.resources | object | `{"limits":{"cpu":"45m","memory":"200M"},"requests":{"cpu":"15m","memory":"200M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| processesworker.logging.default | string | `"Information"` |  |
| processesworker.portal.scope | string | `"openid"` |  |
| processesworker.portal.grantType | string | `"client_credentials"` |  |
| processesworker.portal.clientId | string | `"portal-client-id"` | Provide portal client-id from CX IAM centralidp. |
| processesworker.portal.clientSecret | string | `""` | Client-secret for portal client-id. Secret-key 'portal-client-secret'. |
| processesworker.processIdentity.identityId | string | `"ac1cf001-7fbc-1f2f-817f-bce058020006"` |  |
| processesworker.wallet.scope | string | `"openid"` |  |
| processesworker.wallet.grantType | string | `"client_credentials"` |  |
| processesworker.wallet.clientId | string | `"wallet-client-id"` | Provide wallet client-id from CX IAM centralidp. |
| processesworker.wallet.clientSecret | string | `""` | Client-secret for wallet client-id. Secret-key 'wallet-client-secret'. |
| processesworker.wallet.encryptionConfigIndex | int | `0` |  |
| processesworker.wallet.encryptionConfigs.index0.index | int | `0` |  |
| processesworker.wallet.encryptionConfigs.index0.cipherMode | string | `"CBC"` |  |
| processesworker.wallet.encryptionConfigs.index0.paddingMode | string | `"PKCS7"` |  |
| processesworker.wallet.encryptionConfigs.index0.encryptionKey | string | `""` | EncryptionKey for wallet. Secret-key 'process-wallet-encryption-key0'. Expected format is 256 bit (64 digits) hex. |
| credentialExpiry.name | string | `"expiry"` |  |
| credentialExpiry.image.name | string | `"docker.io/tractusx/ssi-credential-expiry-app"` |  |
| credentialExpiry.image.tag | string | `""` |  |
| credentialExpiry.imagePullPolicy | string | `"IfNotPresent"` |  |
| credentialExpiry.resources | object | `{"limits":{"cpu":"45m","memory":"105M"},"requests":{"cpu":"15m","memory":"105M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| credentialExpiry.processIdentity.identityId | string | `"ac1cf001-7fbc-1f2f-817f-bce058020006"` |  |
| credentialExpiry.logging.default | string | `"Information"` |  |
| credentialExpiry.expiry.expiredVcsToDeleteInMonth | int | `12` |  |
| credentialExpiry.expiry.inactiveVcsToDeleteInWeeks | int | `12` |  |
| existingSecret | string | `""` | Secret containing the client-secrets for the connection to portal and wallet as well as encryptionKeys for issuer.credential and processesworker.wallet |
| dotnetEnvironment | string | `"Production"` |  |
| dbConnection.schema | string | `"issuer"` |  |
| dbConnection.sslMode | string | `"Disable"` |  |
| postgresql.enabled | bool | `true` | PostgreSQL chart configuration; default configurations: host: "issuer-postgresql-primary", port: 5432; Switch to enable or disable the PostgreSQL helm chart. |
| postgresql.image | object | `{"tag":"15-debian-12"}` | Setting image tag to major to get latest minor updates |
| postgresql.commonLabels."app.kubernetes.io/version" | string | `"15"` |  |
| postgresql.auth.username | string | `"issuer"` | Non-root username. |
| postgresql.auth.database | string | `"issuer"` | Database name. |
| postgresql.auth.existingSecret | string | `"{{ .Release.Name }}-issuer-postgres"` | Secret containing the passwords for root usernames postgres and non-root username issuer. Should not be changed without changing the "issuer-postgresSecretName" template as well. |
| postgresql.auth.postgrespassword | string | `""` | Password for the root username 'postgres'. Secret-key 'postgres-password'. |
| postgresql.auth.password | string | `""` | Password for the non-root username 'issuer'. Secret-key 'password'. |
| postgresql.auth.replicationPassword | string | `""` | Password for the non-root username 'repl_user'. Secret-key 'replication-password'. |
| postgresql.architecture | string | `"replication"` |  |
| postgresql.audit.pgAuditLog | string | `"write, ddl"` |  |
| postgresql.audit.logLinePrefix | string | `"%m %u %d "` |  |
| postgresql.primary.extendedConfiguration | string | `""` | Extended PostgreSQL Primary configuration (increase of max_connections recommended - default is 100) |
| postgresql.primary.initdb.scriptsConfigMap | string | `"{{ .Release.Name }}-issuer-cm-postgres"` |  |
| postgresql.readReplicas.extendedConfiguration | string | `""` | Extended PostgreSQL read only replicas configuration (increase of max_connections recommended - default is 100) |
| externalDatabase.host | string | `"issuer-postgres-ext"` | External PostgreSQL configuration IMPORTANT: non-root db user needs to be created beforehand on external database. And the init script (02-init-db.sql) available in templates/configmap-postgres-init.yaml needs to be executed beforehand. Database host ('-primary' is added as postfix). |
| externalDatabase.port | int | `5432` | Database port number. |
| externalDatabase.username | string | `"issuer"` | Non-root username for issuer. |
| externalDatabase.database | string | `"issuer"` | Database name. |
| externalDatabase.password | string | `""` | Password for the non-root username (default 'issuer'). Secret-key 'password'. |
| externalDatabase.existingSecret | string | `"issuer-external-db"` | Secret containing the password non-root username, (default 'issuer'). |
| centralidp | object | `{"address":"https://centralidp.example.org","authRealm":"CX-Central","jwtBearerOptions":{"metadataPath":"/auth/realms/CX-Central/.well-known/openid-configuration","refreshInterval":"00:00:30","requireHttpsMetadata":"true","tokenValidationParameters":{"validAudience":"Cl24-CX-SSI-CredentialIssuer","validIssuerPath":"/auth/realms/CX-Central"}},"tokenPath":"/auth/realms/CX-Central/protocol/openid-connect/token","useAuthTrail":true}` | Provide details about centralidp (CX IAM) Keycloak instance. |
| centralidp.address | string | `"https://centralidp.example.org"` | Provide centralidp base address (CX IAM), without trailing '/auth'. |
| centralidp.useAuthTrail | bool | `true` | Flag if the api should be used with an leading /auth path |
| ingress.enabled | bool | `false` | SSI Credential Issuer ingress parameters, enable ingress record generation for ssi-credential-issuer. |
| ingress.tls | list | `[]` | Ingress TLS configuration |
| ingress.hosts[0] | object | `{"host":"","paths":[{"backend":{"port":8080},"path":"/api","pathType":"Prefix"}]}` | Provide default path for the ingress record. |
| portContainer | int | `8080` |  |
| portService | int | `8080` |  |
| replicaCount | int | `3` |  |
| nodeSelector | object | `{}` | Node labels for pod assignment |
| tolerations | list | `[]` | Tolerations for pod assignment |
| affinity.podAntiAffinity | object | `{"preferredDuringSchedulingIgnoredDuringExecution":[{"podAffinityTerm":{"labelSelector":{"matchExpressions":[{"key":"app.kubernetes.io/name","operator":"DoesNotExist"}]},"topologyKey":"kubernetes.io/hostname"},"weight":100}]}` | Following Catena-X Helm Best Practices, [reference](https://kubernetes.io/docs/concepts/scheduling-eviction/assign-pod-node/#affinity-and-anti-affinity). |
| updateStrategy.type | string | `"RollingUpdate"` | Update strategy type, rolling update configuration parameters, [reference](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/#update-strategies). |
| updateStrategy.rollingUpdate.maxSurge | int | `1` |  |
| updateStrategy.rollingUpdate.maxUnavailable | int | `0` |  |
| startupProbe | object | `{"failureThreshold":30,"initialDelaySeconds":10,"periodSeconds":10,"successThreshold":1,"timeoutSeconds":1}` | Following Catena-X Helm Best Practices, [reference](https://github.com/helm/charts/blob/master/stable/nginx-ingress/values.yaml#L210). |
| livenessProbe.failureThreshold | int | `3` |  |
| livenessProbe.initialDelaySeconds | int | `10` |  |
| livenessProbe.periodSeconds | int | `10` |  |
| livenessProbe.successThreshold | int | `1` |  |
| livenessProbe.timeoutSeconds | int | `10` |  |
| readinessProbe.failureThreshold | int | `3` |  |
| readinessProbe.initialDelaySeconds | int | `10` |  |
| readinessProbe.periodSeconds | int | `10` |  |
| readinessProbe.successThreshold | int | `1` |  |
| readinessProbe.timeoutSeconds | int | `1` |  |

Autogenerated with [helm docs](https://github.com/norwoodj/helm-docs)
