# Helm chart for Catena-X SSI Credential Issuer

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
    version: 0.1.0-rc.1
```

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| https://charts.bitnami.com/bitnami | postgresql | 12.12.x |

## Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| centralidpAddress | string | `"https://centralidp.example.org"` | Provide centralidp base address (CX IAM), without trailing '/auth'. |
| ingress.enabled | bool | `false` | Policy Hub ingress parameters, enable ingress record generation for policy-hub. |
| ingress.name | string | `"policy-hub"` |  |
| ingress.className | string | `"nginx"` |  |
| ingress.annotations."nginx.ingress.kubernetes.io/use-regex" | string | `"true"` |  |
| ingress.annotations."nginx.ingress.kubernetes.io/enable-cors" | string | `"true"` |  |
| ingress.annotations."nginx.ingress.kubernetes.io/proxy-body-size" | string | `"8m"` |  |
| ingress.annotations."nginx.ingress.kubernetes.io/cors-allow-origin" | string | `"https://*.example.org"` | Provide CORS allowed origin. |
| ingress.tls[0] | object | `{"hosts":["policy-hub.example.org"],"secretName":""}` | Provide tls secret. |
| ingress.tls[0].hosts | list | `["policy-hub.example.org"]` | Provide host for tls secret. |
| ingress.hosts[0] | object | `{"host":"policy-hub.example.org","paths":[{"backend":{"port":8080},"path":"/api/policy-hub","pathType":"Prefix"}]}` | Provide default path for the ingress record. |
| dotnetEnvironment | string | `"Production"` |  |
| dbConnection.schema | string | `"hub"` |  |
| dbConnection.sslMode | string | `"Disable"` |  |
| keycloak.central.authRealm | string | `"CX-Central"` |  |
| keycloak.central.jwtBearerOptions.requireHttpsMetadata | string | `"true"` |  |
| keycloak.central.jwtBearerOptions.metadataPath | string | `"/auth/realms/CX-Central/.well-known/openid-configuration"` |  |
| keycloak.central.jwtBearerOptions.tokenValidationParameters.validIssuerPath | string | `"/auth/realms/CX-Central"` |  |
| keycloak.central.jwtBearerOptions.tokenValidationParameters.validAudience | string | `"Cl23-CX-Policy-Hub"` |  |
| keycloak.central.jwtBearerOptions.refreshInterval | string | `"00:00:30"` |  |
| keycloak.central.tokenPath | string | `"/auth/realms/CX-Central/protocol/openid-connect/token"` |  |
| keycloak.central.useAuthTrail | bool | `true` | Flag if the api should be used with an leading /auth path |
| healthChecks.startup.path | string | `"/health/startup"` |  |
| healthChecks.liveness.path | string | `"/healthz"` |  |
| healthChecks.readyness.path | string | `"/ready"` |  |
| policyhub.image | string | `"docker.io/tractusx/policy-hub-service:0.1.0-rc.3"` |  |
| policyhub.imagePullPolicy | string | `"IfNotPresent"` |  |
| policyhub.resources | object | `{"requests":{"cpu":"15m","memory":"300M"}}` | We recommend not to specify default resource limits and to leave this as a conscious choice for the user. If you do want to specify resource limits, uncomment the following lines and adjust them as necessary. |
| policyhub.logging.businessLogic | string | `"Information"` |  |
| policyhub.logging.default | string | `"Information"` |  |
| policyhub.healthChecks.startup.tags[0].name | string | `"HEALTHCHECKS__0__TAGS__1"` |  |
| policyhub.healthChecks.startup.tags[0].value | string | `"policyhubdb"` |  |
| policyhub.swaggerEnabled | bool | `false` |  |
| policyhubmigrations.image | string | `"docker.io/tractusx/policy-hub-migrations:0.1.0-rc.3"` |  |
| policyhubmigrations.imagePullPolicy | string | `"IfNotPresent"` |  |
| policyhubmigrations.resources | object | `{"requests":{"cpu":"15m","memory":"105M"}}` | We recommend not to specify default resource limits and to leave this as a conscious choice for the user. If you do want to specify resource limits, uncomment the following lines and adjust them as necessary. |
| policyhubmigrations.seeding.testDataEnvironments | string | `""` |  |
| policyhubmigrations.seeding.testDataPaths | string | `"Seeder/Data"` |  |
| policyhubmigrations.logging.default | string | `"Information"` |  |
| postgresql.enabled | bool | `true` | PostgreSQL chart configuration; default configurations: host: "policy-hub-postgresql-primary", port: 5432; Switch to enable or disable the PostgreSQL helm chart. |
| postgresql.auth.username | string | `"hub"` | Non-root username. |
| postgresql.auth.database | string | `"policy-hub"` | Database name. |
| postgresql.auth.existingSecret | string | `"{{ .Release.Name }}-phub-postgres"` | Secret containing the passwords for root usernames postgres and non-root username hub. Should not be changed without changing the "phub-postgresSecretName" template as well. |
| postgresql.architecture | string | `"replication"` |  |
| postgresql.audit.pgAuditLog | string | `"write, ddl"` |  |
| postgresql.audit.logLinePrefix | string | `"%m %u %d "` |  |
| postgresql.primary.extendedConfiguration | string | `""` | Extended PostgreSQL Primary configuration (increase of max_connections recommended - default is 100) |
| postgresql.primary.initdb.scriptsConfigMap | string | `"{{ .Release.Name }}-phub-cm-postgres"` |  |
| postgresql.readReplicas.extendedConfiguration | string | `""` | Extended PostgreSQL read only replicas configuration (increase of max_connections recommended - default is 100) |
| externalDatabase.host | string | `"phub-postgres-ext"` | External PostgreSQL configuration IMPORTANT: non-root db user needs to be created beforehand on external database. And the init script (02-init-db.sql) available in templates/configmap-postgres-init.yaml needs to be executed beforehand. Database host ('-primary' is added as postfix). |
| externalDatabase.port | int | `5432` | Database port number. |
| externalDatabase.user | string | `"hub"` | Non-root username for policy-hub. |
| externalDatabase.database | string | `"policy-hub"` | Database name. |
| externalDatabase.password | string | `""` | Password for the non-root username (default 'hub'). Secret-key 'password'. |
| externalDatabase.existingSecret | string | `"policy-hub-external-db"` | Secret containing the password non-root username, (default 'hub'). |
| externalDatabase.existingSecretPasswordKey | string | `"password"` | Name of an existing secret key containing the database credentials. |
| secrets.postgresql.auth.existingSecret.postgrespassword | string | `""` | Password for the root username 'postgres'. Secret-key 'postgres-password'. |
| secrets.postgresql.auth.existingSecret.password | string | `""` | Password for the non-root username 'hub'. Secret-key 'password'. |
| secrets.postgresql.auth.existingSecret.replicationPassword | string | `""` | Password for the non-root username 'repl_user'. Secret-key 'replication-password'. |
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
