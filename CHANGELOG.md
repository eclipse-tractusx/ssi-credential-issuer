# Changelog

## [1.0.0-rc.6](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.0.0-rc.5...v1.0.0-rc.6) (2024-05-24)


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
* **issuer:** add filter to /api/issuer ([#120](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/120)) ([ea5d91a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ea5d91a30b18d70c0bcc46555141db6762f6af56))
* **known issue:** upload of documents with credential requests currently not working ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* **revocation:** add endpoints to revoke credentials ([#43](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/43)) ([dc9c70d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/dc9c70da4c0bcba979c71b5c636526c13041c774))
* **ssi:** adjust framework creation endpoint ([#70](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/70)) ([2d06fe6](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/2d06fe65365b644a209900a464c6823cb0db372e))


### Bug Fixes

* adjust bpn schema ([#84](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/84)) ([e32ec3a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e32ec3a47e94133294a8e7035f81e5d8fbe305e3))
* adjust multiple ssi detail handling ([#116](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/116)) ([7e8df9d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/7e8df9dd35953fc5ed3c199dbd6357cc574feec4))
* **approval:** send mail and notification to requester ([#101](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/101)) ([0fe249c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/0fe249ceb5728be69055320718ff9b3deb7a3f52))
* **callback:** set the correct base address for the callback ([#83](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/83)) ([9f79c54](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/9f79c541873c951eb6335aba6b5b1adda0ee25e9)), closes [#71](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/71)
* **credential:** adjust puris credential naming ([#148](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/148)) ([57c36b1](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/57c36b15f126a32a5d20321b7d7d72910d2f5f3c)), closes [#147](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/147)
* **credential:** remove duplicate credential ([#113](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/113)) ([f2cc13d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f2cc13dd810970095c3969a7996c4f00d22f967a))
* **credentials:** remove quality credential ([#97](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/97)) ([e6a817d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e6a817d61ac8a713b9be623a361a26e2e4354964)), closes [#95](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/95)
* **framework:** adjust framework credential creation ([#138](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/138)) ([d07abc5](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/d07abc51da218889eb9c09869b7659a878a9166a))
* **helm:** change ingress to work without tls enabled ([377cd66](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/377cd6634245e9f6a7f650e61064c97f46e10a37))
* **ingress:** adjust api path ([#157](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/157)) ([8b9bffa](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/8b9bffa23dffdaf6bb0afc9a30ad401f8e4a507e))
* **notification:** adjust notification creation url ([#98](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/98)) ([ae966e9](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ae966e97395a38e56d88e5479e34c0dac6bc3914))
* **ownedCredential:** add credential id to response ([#149](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/149)) ([f4c1bca](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f4c1bca5374e40900e45969b5b1a2a56a5879f56))
* **qualityCredential:** re add quality credential ([#114](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/114)) ([d962baf](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/d962bafd9df92dd5cbaf12a5aa93fa37c4ec29f7)), closes [#107](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/107)
* return pending credentials ([#117](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/117)) ([21defc7](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/21defc7ab1238c0dd250c0f69cd3c55cc1cf47cf)), closes [#109](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/109)
* **revocation:** save database changes afert revocation ([#159](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/159)) ([41cb14d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/41cb14dff374b16199c888fbf1145e9f28af66bc))
* **seeding:** set consortia to seeding paths ([#96](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/96)) ([8e16f04](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/8e16f04ee8310318149d27318cbdf1c1dd4bf8c8))
* **ssi:** adjust schemas ([#72](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/72)) ([ba63179](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ba63179300aa835bcb0f0d5c874c927ea48c89c9))


### Miscellaneous Chores

* release 1.0.0-rc.1 ([e74c880](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e74c880fef9245fca685c102541e46420893db2e))
* release 1.0.0-rc.3 ([1d3132c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/1d3132c4ddb34db4f4c71613f6360906c4fb8664))
* release 1.0.0-rc.6 ([2a609eb](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/2a609ebbf591f99bd602646122751fcb82979241))

## [1.0.0-rc.5](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.0.0-rc.4...v1.0.0-rc.5) (2024-05-23)


### Features

* **issuer:** add filter to /api/issuer ([#120](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/120)) ([ea5d91a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ea5d91a30b18d70c0bcc46555141db6762f6af56))

### Bug Fixes

* **credential:** adjust puris credential naming ([#148](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/148)) ([57c36b1](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/57c36b15f126a32a5d20321b7d7d72910d2f5f3c)), closes [#147](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/147)
* **framework:** adjust framework credential creation ([#138](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/138)) ([d07abc5](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/d07abc51da218889eb9c09869b7659a878a9166a))
* **helm:** change ingress to work without tls enabled ([377cd66](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/377cd6634245e9f6a7f650e61064c97f46e10a37))
* **ownedCredential:** add credential id to response ([#149](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/149)) ([f4c1bca](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f4c1bca5374e40900e45969b5b1a2a56a5879f56))


### Miscellaneous Chores

* release 1.0.0-rc.5 ([46e479e](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/46e479efeed064503484d74b94ac13d9e1951bb1))

## [1.0.0-rc.4](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.0.0-rc.3...v1.0.0-rc.4) (2024-05-13)


### Bug Fixes

* adjust multiple ssi detail handling ([#116](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/116)) ([7e8df9d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/7e8df9dd35953fc5ed3c199dbd6357cc574feec4))
* **approval:** send mail and notification to requester ([#101](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/101)) ([0fe249c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/0fe249ceb5728be69055320718ff9b3deb7a3f52))
* **credential:** remove duplicate credential ([#113](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/113)) ([f2cc13d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f2cc13dd810970095c3969a7996c4f00d22f967a))
* **credentials:** remove quality credential ([#97](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/97)) ([e6a817d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e6a817d61ac8a713b9be623a361a26e2e4354964)), closes [#95](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/95)
* **notification:** adjust notification creation url ([#98](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/98)) ([ae966e9](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ae966e97395a38e56d88e5479e34c0dac6bc3914))
* **qualityCredential:** re add quality credential ([#114](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/114)) ([d962baf](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/d962bafd9df92dd5cbaf12a5aa93fa37c4ec29f7)), closes [#107](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/107)
* return pending credentials ([#117](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/117)) ([21defc7](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/21defc7ab1238c0dd250c0f69cd3c55cc1cf47cf)), closes [#109](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/109)
* **seeding:** set consortia to seeding paths ([#96](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/96)) ([8e16f04](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/8e16f04ee8310318149d27318cbdf1c1dd4bf8c8))


### Miscellaneous Chores

* release 1.0.0-rc.4 ([f159102](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f1591024624317e403fab442539a1b7a332a4c16))

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
