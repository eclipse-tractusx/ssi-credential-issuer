# Changelog

## [1.0.0-rc.3](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.0.0-rc.1...v1.0.0-rc.3) (2024-04-30)


### Features

* **revocation:** add endpoints to revoke credentials ([#43](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/43)) ([dc9c70d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/dc9c70da4c0bcba979c71b5c636526c13041c774))
* **ssi:** adjust framework creation endpoint ([#70](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/70)) ([2d06fe6](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/2d06fe65365b644a209900a464c6823cb0db372e))


### Bug Fixes

* adjust bpn schema ([#84](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/84)) ([e32ec3a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e32ec3a47e94133294a8e7035f81e5d8fbe305e3))
* **callback:** set the correct base address for the callback ([#83](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/83)) ([9f79c54](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/9f79c541873c951eb6335aba6b5b1adda0ee25e9)), closes [#71](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/71)
* **ssi:** adjust schemas ([#72](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/72)) ([ba63179](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ba63179300aa835bcb0f0d5c874c927ea48c89c9))


### Miscellaneous Chores

* release 1.0.0-rc.3 ([1d3132c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/1d3132c4ddb34db4f4c71613f6360906c4fb8664))

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
