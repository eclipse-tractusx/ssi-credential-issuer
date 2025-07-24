# Helm chart for SSI Credential Issuer

This helm chart installs the Catena-X SSI Credential Issuer application.

For further information please refer to [Technical Documentation](/docs/technical-documentation).

For information about the initial credential creation for the Operator, please refer to [initial credential setup](/docs/technical-documentation/operator-credential-creation/initial-credential-setup.md).

Information about the connection to the Decentralized Identity Verification (DIV, formerly known as DIM) Wallet: this version was last tested with the 2.2.1 version of the [SSI DIM Middle Layer](https://github.com/SAP/ssi-dim-middle-layer).

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
    version: 1.4.0
```

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| https://charts.bitnami.com/bitnami | postgresql | 12.12.x |

## Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| affinity.podAntiAffinity | object | `{"preferredDuringSchedulingIgnoredDuringExecution":[{"podAffinityTerm":{"labelSelector":{"matchExpressions":[{"key":"app.kubernetes.io/name","operator":"DoesNotExist"}]},"topologyKey":"kubernetes.io/hostname"},"weight":100}]}` | Following Catena-X Helm Best Practices, [reference](https://kubernetes.io/docs/concepts/scheduling-eviction/assign-pod-node/#affinity-and-anti-affinity). |
| centralidp | object | `{"address":"https://centralidp.example.org","authRealm":"CX-Central","jwtBearerOptions":{"metadataPath":"/auth/realms/CX-Central/.well-known/openid-configuration","refreshInterval":"00:00:30","requireHttpsMetadata":"true","tokenValidationParameters":{"validAudience":"Cl24-CX-SSI-CredentialIssuer","validIssuerPath":"/auth/realms/CX-Central"}},"tokenPath":"/auth/realms/CX-Central/protocol/openid-connect/token","useAuthTrail":true}` | Provide details about centralidp (CX IAM) Keycloak instance. |
| centralidp.address | string | `"https://centralidp.example.org"` | Provide centralidp base address (CX IAM), without trailing '/auth'. |
| centralidp.useAuthTrail | bool | `true` | Flag if the api should be used with an leading /auth path |
| credentialExpiry.expiry.expiredVcsToDeleteInMonth | int | `12` |  |
| credentialExpiry.expiry.inactiveVcsToDeleteInWeeks | int | `12` |  |
| credentialExpiry.image.name | string | `"docker.io/tractusx/ssi-credential-expiry-app"` |  |
| credentialExpiry.image.pullSecrets | list | `[]` |  |
| credentialExpiry.image.tag | string | `""` |  |
| credentialExpiry.imagePullPolicy | string | `"IfNotPresent"` |  |
| credentialExpiry.logging.default | string | `"Information"` |  |
| credentialExpiry.name | string | `"expiry"` |  |
| credentialExpiry.processIdentity.identityId | string | `"ac1cf001-7fbc-1f2f-817f-bce058020006"` |  |
| credentialExpiry.resources | object | `{"limits":{"cpu":"45m","memory":"105M"},"requests":{"cpu":"15m","memory":"105M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| dbConnection.schema | string | `"issuer"` |  |
| dbConnection.sslMode | string | `"Disable"` |  |
| dotnetEnvironment | string | `"Production"` |  |
| existingSecret | string | `""` | Secret containing the client-secrets for the connection to portal and wallet as well as encryptionKeys for issuer.credential and processesworker.wallet |
| externalDatabase.database | string | `"issuer"` | Database name. |
| externalDatabase.existingSecret | string | `"issuer-external-db"` | Secret containing the password non-root username, (default 'issuer'). |
| externalDatabase.host | string | `"issuer-postgres-ext"` | External PostgreSQL configuration IMPORTANT: non-root db user needs to be created beforehand on external database. And the init script (02-init-db.sql) available in templates/configmap-postgres-init.yaml needs to be executed beforehand. Database host ('-primary' is added as postfix). |
| externalDatabase.password | string | `""` | Password for the non-root username (default 'issuer'). Secret-key 'password'. |
| externalDatabase.port | int | `5432` | Database port number. |
| externalDatabase.username | string | `"issuer"` | Non-root username for issuer. |
| ingress.enabled | bool | `false` | SSI Credential Issuer ingress parameters, enable ingress record generation for ssi-credential-issuer. |
| ingress.hosts[0] | object | `{"host":"","paths":[{"backend":{"port":8080},"path":"/api","pathType":"Prefix"}]}` | Provide default path for the ingress record. |
| ingress.tls | list | `[]` | Ingress TLS configuration |
| livenessProbe.failureThreshold | int | `3` |  |
| livenessProbe.initialDelaySeconds | int | `10` |  |
| livenessProbe.periodSeconds | int | `10` |  |
| livenessProbe.successThreshold | int | `1` |  |
| livenessProbe.timeoutSeconds | int | `10` |  |
| migrations.image.name | string | `"docker.io/tractusx/ssi-credential-issuer-migrations"` |  |
| migrations.image.pullSecrets | list | `[]` |  |
| migrations.image.tag | string | `""` |  |
| migrations.imagePullPolicy | string | `"IfNotPresent"` |  |
| migrations.logging.default | string | `"Information"` |  |
| migrations.name | string | `"migrations"` |  |
| migrations.resources | object | `{"limits":{"cpu":"45m","memory":"200M"},"requests":{"cpu":"15m","memory":"200M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| migrations.seeding.seedTestData | object | `{"useDefault":false,"useOwnConfigMap":{"configMap":"","filename":""}}` | Option to seed testdata |
| migrations.seeding.seedTestData.useDefault | bool | `false` | If set to true the data configured in the config map 'configmap-seeding-testdata.yaml' will be taken to insert the default test data |
| migrations.seeding.seedTestData.useOwnConfigMap.configMap | string | `""` | ConfigMap containing json files for the tables to seed, e.g. use_cases.json, verified_credential_external_type_detail_versions.test.json, etc. |
| migrations.seeding.seedTestData.useOwnConfigMap.filename | string | `""` | Filename identifying the test data files e.g. for companies.test.json the value would be "test" |
| nodeSelector | object | `{}` | Node labels for pod assignment |
| portContainer | int | `8080` |  |
| portService | int | `8080` |  |
| portalBackendAddress | string | `"https://portal-backend.example.org"` | Provide portal-backend base address. |
| postgresql.architecture | string | `"replication"` |  |
| postgresql.audit.logLinePrefix | string | `"%m %u %d "` |  |
| postgresql.audit.pgAuditLog | string | `"write, ddl"` |  |
| postgresql.auth.database | string | `"issuer"` | Database name. |
| postgresql.auth.existingSecret | string | `"{{ .Release.Name }}-issuer-postgres"` | Secret containing the passwords for root usernames postgres and non-root username issuer. Should not be changed without changing the "issuer-postgresSecretName" template as well. |
| postgresql.auth.password | string | `""` | Password for the non-root username 'issuer'. Secret-key 'password'. |
| postgresql.auth.postgrespassword | string | `""` | Password for the root username 'postgres'. Secret-key 'postgres-password'. |
| postgresql.auth.replicationPassword | string | `""` | Password for the non-root username 'repl_user'. Secret-key 'replication-password'. |
| postgresql.auth.username | string | `"issuer"` | Non-root username. |
| postgresql.commonLabels."app.kubernetes.io/version" | string | `"15"` |  |
| postgresql.enabled | bool | `true` | PostgreSQL chart configuration; default configurations: host: "issuer-postgresql-primary", port: 5432; Switch to enable or disable the PostgreSQL helm chart. |
| postgresql.image | object | `{"tag":"15-debian-12"}` | Setting image tag to major to get latest minor updates |
| postgresql.primary.extendedConfiguration | string | `""` | Extended PostgreSQL Primary configuration (increase of max_connections recommended - default is 100) |
| postgresql.primary.initdb.scriptsConfigMap | string | `"{{ .Release.Name }}-issuer-cm-postgres"` |  |
| postgresql.readReplicas.extendedConfiguration | string | `""` | Extended PostgreSQL read only replicas configuration (increase of max_connections recommended - default is 100) |
| processesworker.image.name | string | `"docker.io/tractusx/ssi-credential-issuer-processes-worker"` |  |
| processesworker.image.pullSecrets | list | `[]` |  |
| processesworker.image.tag | string | `""` |  |
| processesworker.imagePullPolicy | string | `"IfNotPresent"` |  |
| processesworker.logging.default | string | `"Information"` |  |
| processesworker.name | string | `"processesworker"` |  |
| processesworker.portal.clientId | string | `"portal-client-id"` | Provide portal client-id from CX IAM centralidp. |
| processesworker.portal.clientSecret | string | `""` | Client-secret for portal client-id. Secret-key 'portal-client-secret'. |
| processesworker.portal.grantType | string | `"client_credentials"` |  |
| processesworker.portal.scope | string | `"openid"` |  |
| processesworker.processIdentity.identityId | string | `"ac1cf001-7fbc-1f2f-817f-bce058020006"` |  |
| processesworker.resources | object | `{"limits":{"cpu":"45m","memory":"200M"},"requests":{"cpu":"15m","memory":"200M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| processesworker.wallet.application | string | `"catena-x-portal"` | the application set in the wallet |
| processesworker.wallet.clientId | string | `"wallet-client-id"` | Provide wallet client-id from CX IAM centralidp. |
| processesworker.wallet.clientSecret | string | `""` | Client-secret for wallet client-id. Secret-key 'wallet-client-secret'. |
| processesworker.wallet.createCredentialPath | string | `"api/v2.0.0/credentials"` | path to create a credential |
| processesworker.wallet.createSignedCredentialPath | string | `"/api/v2.0.0/credentials"` | path to create a specific credential which is directly signed |
| processesworker.wallet.credentialRequestsReceivedAutoApprovePath | string | `"/api/v2.0.0/dcp/credentialRequestsReceived/{0}/autoApprove"` | path to credential request received auto approve; {0} will be replaced by the credential request received id |
| processesworker.wallet.credentialRequestsReceivedDetailPath | string | `"/api/v2.0.0/dcp/credentialRequestsReceived/{0}"` | path to credential request received detail; {0} will be replaced by the credential request received id |
| processesworker.wallet.credentialRequestsReceivedPath | string | `"/api/v2.0.0/dcp/credentialRequestsReceived"` | path to credential request received; |
| processesworker.wallet.getCredentialPath | string | `"/api/v2.0.0/credentials/{0}"` | path to get a specific credential; {0} will be replaced by the credential id |
| processesworker.wallet.grantType | string | `"client_credentials"` |  |
| processesworker.wallet.requestCredentialPath | string | `"/api/v2.0.0/dcp/requestCredentials/{0}"` | path to request a credential; {0} will be replaced by application name |
| processesworker.wallet.revokeCredentialPath | string | `"/api/v2.0.0/credentials/{0}"` | path to revoke a specific credential; {0} will be replaced by the credential id |
| processesworker.wallet.scope | string | `"openid"` |  |
| readinessProbe.failureThreshold | int | `3` |  |
| readinessProbe.initialDelaySeconds | int | `10` |  |
| readinessProbe.periodSeconds | int | `10` |  |
| readinessProbe.successThreshold | int | `1` |  |
| readinessProbe.timeoutSeconds | int | `1` |  |
| replicaCount | int | `3` |  |
| service.credential.encryptionConfigIndex | int | `0` |  |
| service.credential.encryptionConfigs.index0.cipherMode | string | `"CBC"` |  |
| service.credential.encryptionConfigs.index0.encryptionKey | string | `""` | EncryptionKey for wallet. Secret-key 'credential-encryption-key0'. Expected format is 256 bit (64 digits) hex. |
| service.credential.encryptionConfigs.index0.index | int | `0` |  |
| service.credential.encryptionConfigs.index0.paddingMode | string | `"PKCS7"` |  |
| service.credential.issuerBpn | string | `"BPNL00000001TEST"` |  |
| service.credential.issuerDid | string | `"did:web:example"` |  |
| service.credential.statusListType | string | `"StatusList2021"` | valid types are:  StatusList2021, BitstringStatusList |
| service.credential.statusListUrl | string | `"https://example.org/statuslist"` |  |
| service.healthChecks.liveness.path | string | `"/healthz"` |  |
| service.healthChecks.readyness.path | string | `"/ready"` |  |
| service.healthChecks.startup.path | string | `"/health/startup"` |  |
| service.healthChecks.startup.tags[0].name | string | `"HEALTHCHECKS__0__TAGS__1"` |  |
| service.healthChecks.startup.tags[0].value | string | `"issuerdb"` |  |
| service.image.name | string | `"docker.io/tractusx/ssi-credential-issuer-service"` |  |
| service.image.pullSecrets | list | `[]` |  |
| service.image.tag | string | `""` |  |
| service.imagePullPolicy | string | `"IfNotPresent"` |  |
| service.logging.businessLogic | string | `"Information"` |  |
| service.logging.default | string | `"Information"` |  |
| service.portal.clientId | string | `"portal-client-id"` | Provide portal client-id from CX IAM centralidp. |
| service.portal.clientSecret | string | `""` | Client-secret for portal client-id. Secret-key 'portal-client-secret'. |
| service.portal.grantType | string | `"client_credentials"` |  |
| service.portal.scope | string | `"openid"` |  |
| service.resources | object | `{"limits":{"cpu":"45m","memory":"400M"},"requests":{"cpu":"15m","memory":"400M"}}` | We recommend to review the default resource limits as this should a conscious choice. |
| service.swaggerEnabled | bool | `false` |  |
| startupProbe | object | `{"failureThreshold":30,"initialDelaySeconds":10,"periodSeconds":10,"successThreshold":1,"timeoutSeconds":1}` | Following Catena-X Helm Best Practices, [reference](https://github.com/helm/charts/blob/master/stable/nginx-ingress/values.yaml#L210). |
| tolerations | list | `[]` | Tolerations for pod assignment |
| updateStrategy.rollingUpdate.maxSurge | int | `1` |  |
| updateStrategy.rollingUpdate.maxUnavailable | int | `0` |  |
| updateStrategy.type | string | `"RollingUpdate"` | Update strategy type, rolling update configuration parameters, [reference](https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/#update-strategies). |
| walletAddress | string | `"https://wallet.example.org"` |  |
| walletTokenAddress | string | `"https://wallet.example.org/oauth/token"` |  |

Autogenerated with [helm docs](https://github.com/norwoodj/helm-docs)
