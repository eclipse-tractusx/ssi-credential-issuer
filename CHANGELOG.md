# Changelog

## [1.0.0-rc.2](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.0.0-rc.1...v1.0.0-rc.2) (2024-04-23)


### Features

* **ssi:** adjust framework creation endpoint ([#70](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/70)) ([2d06fe6](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/2d06fe65365b644a209900a464c6823cb0db372e))


### Bug Fixes

* adjust the hosting url for rc and pen ([#76](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/76)) ([061272e](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/061272e1854f3acc75d079fd339b4728cc1d401d))
* **ssi:** adjust schemas ([#72](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/72)) ([ba63179](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ba63179300aa835bcb0f0d5c874c927ea48c89c9))


### Miscellaneous Chores

* release 1.0.0-rc.2 ([5d95922](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/5d95922bf381ebfd97262be7bb33d6c780862630))

## [1.0.0-rc.1](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.0.0-rc.1...v1.0.0-rc.1) (2024-04-15)


### Features

* establish a database to handle credential requests, verified credentials, document proof, and managing lifecycle ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a GET endpoint for retrieving own credential requests with their current status ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a GET endpoint to retrieve supported credential types, allowing customers to see all credentials that can be requested ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a job to store newly created verified credentials inside the holder wallet ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a notification system for credential expiry to alert holders ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a processes worker to create credentials and submit them for signature by the issuer wallet ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish an admin endpoint to retrieve credential requests for the purpose of approval or rejection ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish endpoints to approve or reject customer credential requests ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish POST endpoints for credential requests for BPN (Business Partner Number), Membership, and Framework Agreement credentials ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* implement a job to run expiry validation and revocation of credentials ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* **known issue:** upload of documents with credential requests currently not working ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))


### Miscellaneous Chores

* release 1.0.0-rc.1 ([e74c880](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e74c880fef9245fca685c102541e46420893db2e))
