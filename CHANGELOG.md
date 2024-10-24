# Changelog

## [1.2.0-rc.1](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.2.0-rc.2...v1.2.0-rc.1) (2024-10-24)


### Date

* Fri May 24 12:37:53 2024 +0200 ([89fcf21](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/89fcf214f7f9b77925816fbb7ac74035c88c8b20))


### Features

* add imagePullSecrets ([#236](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/236)) ([bed4ff8](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/bed4ff875abdcca06fbdbb14779812a465773e10))
* check holder equals issuer ([#275](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/275)) ([fd43a27](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/fd43a27abffef920d3d6b65021070754653b42f3)), closes [#250](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/250)
* **config:** make wallet application and paths configurable ([#230](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/230)) ([7232f27](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/7232f271f8748d281d2909e5016e251217e88e39)), closes [#226](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/226)
* create open api spec on build ([#262](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/262)) ([367db2d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/367db2d3cbfd8a5395a508a31d5db9cd1b8fd975)), closes [#256](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/256)
* **credentials:** add Data_Exchange_Governance credential [#198](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/198) ([3702e5c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/3702e5c5f91e67cf1f84f9f03e549968f7e168b0))
* enhanced the owned-credentials endpoint ([#240](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/240)) ([e41722a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e41722a4e1d02ff631d8b9d1c4940b391f7fd500))
* establish a database to handle credential requests, verified credentials, document proof, and managing lifecycle ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a GET endpoint for retrieving own credential requests with their current status ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a GET endpoint to retrieve supported credential types, allowing customers to see all credentials that can be requested ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a job to store newly created verified credentials inside the holder wallet ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a notification system for credential expiry to alert holders ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish a processes worker to create credentials and submit them for signature by the issuer wallet ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish an admin endpoint to retrieve credential requests for the purpose of approval or rejection ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish endpoints to approve or reject customer credential requests ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* establish POST endpoints for credential requests for BPN (Business Partner Number), Membership, and Framework Agreement credentials ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* **helm:** consolidate structure in values.yaml  ([#172](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/172)) ([1eceb1f](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/1eceb1fbc659d567fa762d6014f67b8fa08e2eed))
* implement a job to run expiry validation and revocation of credentials ([609567a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/609567a6131fdcb1f12ea8a6653b5dbc9963816e))
* **issuer:** add filter to /api/issuer ([#120](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/120)) ([ea5d91a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ea5d91a30b18d70c0bcc46555141db6762f6af56))
* **notification:** adjust notification request parameter ([#233](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/233)) ([37b359d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/37b359d9a289b58e548c6b4935d0e1016872fbff))
* **retrigger:** add retrigger process steps ([#242](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/242)) ([f08ddd5](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f08ddd57af74c9a6d292499e6a062202266f29fc)), closes [#209](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/209)
* **revocation:** add endpoints to revoke credentials ([#43](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/43)) ([dc9c70d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/dc9c70da4c0bcba979c71b5c636526c13041c774))
* **seeding:** add test seeding data ([#121](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/121)) ([c8f07b2](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/c8f07b25772f6bc35603439aad594b7a4b474356)), closes [#118](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/118)
* **ssi:** adjust framework creation endpoint ([#70](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/70)) ([2d06fe6](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/2d06fe65365b644a209900a464c6823cb0db372e))
* **ssi:** merge create and sign credential into one ([#235](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/235)) ([510de92](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/510de9206f916b7eedbc205ff6d3fe9428b73265)), closes [#232](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/232)
* **testdata:** enable seeding via configMap, remove consortia files ([#241](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/241)) ([e3c92d3](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e3c92d3a270d5aca18784315b0f1628bc92806ab)), closes [#205](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/205)


### Bug Fixes

* Added the correct type for owned-credentials method ([dbf0713](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/dbf07137dc2f3b287f7e5c7fa8dd7aa0704b6ffa))
* Added the correct type for owned-credentials method [#193](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/193) ([796415c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/796415c05324bcf9d5f48b1cbf9dadda6ec23704))
* adjust bpn schema ([#84](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/84)) ([e32ec3a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e32ec3a47e94133294a8e7035f81e5d8fbe305e3))
* adjust create credential request structure ([#266](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/266)) ([e466fa3](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e466fa3390b68cc7a51f0aff2be53486a5d6d668)), closes [#265](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/265)
* adjust exceptionhandling for encryption ([#185](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/185)) ([6dcf2f5](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/6dcf2f5c0eb0937042061e4d0420bddd29d4d26f))
* adjust multiple ssi detail handling ([#116](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/116)) ([7e8df9d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/7e8df9dd35953fc5ed3c199dbd6357cc574feec4))
* **approval:** send mail and notification to requester ([#101](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/101)) ([0fe249c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/0fe249ceb5728be69055320718ff9b3deb7a3f52))
* **callback:** set the correct base address for the callback ([#83](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/83)) ([9f79c54](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/9f79c541873c951eb6335aba6b5b1adda0ee25e9)), closes [#71](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/71)
* **credential:** adjust naming for membership credential ([#176](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/176)) ([ea2d55f](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ea2d55fb27dd4ff057b791ed6941d94af4b8d650)), closes [#175](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/175)
* **credential:** adjust puris credential naming ([#148](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/148)) ([57c36b1](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/57c36b15f126a32a5d20321b7d7d72910d2f5f3c)), closes [#147](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/147)
* **credential:** remove duplicate credential ([#113](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/113)) ([f2cc13d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f2cc13dd810970095c3969a7996c4f00d22f967a))
* **credentials:** remove quality credential ([#97](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/97)) ([e6a817d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e6a817d61ac8a713b9be623a361a26e2e4354964)), closes [#95](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/95)
* **credentialType:** rename membership certificate to membership ([#217](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/217)) ([818a9a3](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/818a9a32090322d83cc7ed47e061922f9a1f3d03)), closes [#216](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/216)
* **cronjob expiry:** added the missing environment value for portal ([#196](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/196)) ([dc6b130](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/dc6b13002797dd733694f046f4ec19bc476ecc4e)), closes [#195](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/195)
* **document:** adjust document name for presentation docs ([#174](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/174)) ([e10cbcb](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e10cbcbb03d11e03f9ae5219e1a0163dbf88b280)), closes [#166](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/166)
* **document:** adjust validation to allow the issuer to display documents of credentials ([#229](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/229)) ([a1dd326](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/a1dd326141942de3a873514f6508d42a2400b331)), closes [#225](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/225)
* **encryptionKey:** align credential and wallet config ([#201](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/201)) ([1e1ca59](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/1e1ca59ffcb60ce55c2c68da31c889d8cd490939)), closes [#197](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/197)
* **expiry:** adjust removal of companySsiDetails ([#208](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/208)) ([897a735](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/897a7350f39d378338e411c9b3083218634eac93)), closes [#195](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/195)
* **expiry:** set expiry to max 1 year ([#173](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/173)) ([46d23e8](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/46d23e8cfb192b6cd1aece437d348d42b88d54dd))
* **framework:** adjust framework credential creation ([#138](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/138)) ([d07abc5](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/d07abc51da218889eb9c09869b7659a878a9166a))
* **helm:** change ingress to work without tls enabled ([377cd66](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/377cd6634245e9f6a7f650e61064c97f46e10a37))
* **image-build:** change from emulation to cross-compile ([#181](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/181)) ([aa378a8](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/aa378a8849ce10aee523bd3c998c49ab33e943cc))
* Implement Expiry Date for BPNL and Membership ([4511ec0](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/4511ec0bc20f5d2331c68d0f24b2bafcc9d0666e))
* Implement Expiry Date for BPNL and MembershipMerge [#192](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/192) ([54dbd0e](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/54dbd0e374ca2e0da41ce63c91ee626c57059888))
* **ingress:** adjust api path ([#157](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/157)) ([8b9bffa](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/8b9bffa23dffdaf6bb0afc9a30ad401f8e4a507e))
* **ingress:** adjust environment specific configurations ([#163](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/163)) ([095ab4a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/095ab4a13e5078fc336e1eb909b4142b099247d5))
* **notification:** adjust notification creation url ([#98](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/98)) ([ae966e9](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ae966e97395a38e56d88e5479e34c0dac6bc3914))
* **ownedCredential:** add credential id to response ([#149](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/149)) ([f4c1bca](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f4c1bca5374e40900e45969b5b1a2a56a5879f56))
* **qualityCredential:** re add quality credential ([#114](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/114)) ([d962baf](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/d962bafd9df92dd5cbaf12a5aa93fa37c4ec29f7)), closes [#107](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/107)
* return pending credentials ([#117](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/117)) ([21defc7](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/21defc7ab1238c0dd250c0f69cd3c55cc1cf47cf)), closes [#109](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/109)
* **revocation:** save database changes afert revocation ([#159](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/159)) ([41cb14d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/41cb14dff374b16199c888fbf1145e9f28af66bc))
* **seeding:** set consortia to seeding paths ([#96](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/96)) ([8e16f04](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/8e16f04ee8310318149d27318cbdf1c1dd4bf8c8))
* set companyName for credentialApproval ([#218](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/218)) ([32bb69c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/32bb69ce1364da275cd8538b6fc5b5a75e62961a)), closes [#215](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/215)
* **ssi:** adjust schemas ([#72](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/72)) ([ba63179](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ba63179300aa835bcb0f0d5c874c927ea48c89c9))
* update the template framework pdf link ([#251](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/251)) ([3356250](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/3356250fd09c6e406748298e4fca1f15a59f038e))


### Miscellaneous Chores

* release 1.0.0-rc.1 ([e74c880](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e74c880fef9245fca685c102541e46420893db2e))
* release 1.0.0-rc.3 ([1d3132c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/1d3132c4ddb34db4f4c71613f6360906c4fb8664))
* release 1.2.0-alpha.1 ([5c7ebf5](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/5c7ebf5abd9c0fa407a742e2412df9e064162f1d))
* release 1.2.0-rc.1 ([8ada2a3](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/8ada2a30d68d200b615c3f912d61e0066d7fdcad))

## [1.2.0-rc.1](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.2.0-alpha.1...v1.2.0-rc.1) (2024-10-21)

### Features

* create open api spec on build ([#262](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/262)) ([367db2d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/367db2d3cbfd8a5395a508a31d5db9cd1b8fd975)), closes [#256](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/256)
* **retrigger:** add retrigger process steps ([#242](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/242)) ([f08ddd5](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f08ddd57af74c9a6d292499e6a062202266f29fc)), closes [#209](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/209)
* **testdata:** enable seeding via configMap, remove consortia files ([#241](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/241)) ([e3c92d3](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e3c92d3a270d5aca18784315b0f1628bc92806ab)), closes [#205](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/205)
* check holder equals issuer ([#275](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/275)) ([fd43a27](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/fd43a27abffef920d3d6b65021070754653b42f3)), closes [#250](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/250)

### Bug Fixes

* adjust create credential request structure ([#266](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/266)) ([e466fa3](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e466fa3390b68cc7a51f0aff2be53486a5d6d668)), closes [#265](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/265)

### Miscellaneous Chores

* release 1.2.0-rc.1 ([8ada2a3](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/8ada2a30d68d200b615c3f912d61e0066d7fdcad))

## [1.2.0-alpha.1](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.1.0-rc.2...v1.2.0-alpha.1) (2024-09-23)

### Features

* add imagePullSecrets ([#236](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/236)) ([bed4ff8](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/bed4ff875abdcca06fbdbb14779812a465773e10))
* **config:** make wallet application and paths configurable ([#230](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/230)) ([7232f27](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/7232f271f8748d281d2909e5016e251217e88e39)), closes [#226](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/226)
* enhanced the owned-credentials endpoint ([#240](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/240)) ([e41722a](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e41722a4e1d02ff631d8b9d1c4940b391f7fd500))
* **notification:** adjust notification request parameter ([#233](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/233)) ([37b359d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/37b359d9a289b58e548c6b4935d0e1016872fbff))
* **ssi:** merge create and sign credential into one ([#235](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/235)) ([510de92](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/510de9206f916b7eedbc205ff6d3fe9428b73265)), closes [#232](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/232)

### Bug Fixes

* **document:** adjust validation to allow the issuer to display documents of credentials ([#229](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/229)) ([a1dd326](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/a1dd326141942de3a873514f6508d42a2400b331)), closes [#225](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/225)
* update the template framework pdf link ([#251](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/251)) ([3356250](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/3356250fd09c6e406748298e4fca1f15a59f038e))

### Miscellaneous Chores

* release 1.2.0-alpha.1 ([abbdff1](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/abbdff1d2381ebb722e1fc505ad067565cd7b185))

## [1.1.0-rc.2](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.1.0-rc.1...v1.1.0-rc.2) (2024-07-25)

### Bug Fixes

* **credentialType:** rename membership certificate to membership ([#217](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/217)) ([818a9a3](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/818a9a32090322d83cc7ed47e061922f9a1f3d03)), closes [#216](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/216)
* set companyName for credentialApproval ([#218](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/218)) ([32bb69c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/32bb69ce1364da275cd8538b6fc5b5a75e62961a)), closes [#215](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/215)

### Miscellaneous Chores

* release 1.1.0-rc.2 ([6d3f95c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/6d3f95c3741106373a30ff6b79d98c12f05b14d0))

## [1.1.0-rc.1](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.0.0...v1.1.0-rc.1) (2024-07-17)

### Features

* **credentials:** add Data_Exchange_Governance credential [#198](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/198) ([3702e5c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/3702e5c5f91e67cf1f84f9f03e549968f7e168b0))
* **helm:** consolidate structure in values.yaml  ([#172](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/172)) ([1eceb1f](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/1eceb1fbc659d567fa762d6014f67b8fa08e2eed))
* **seeding:** add test seeding data ([#121](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/121)) ([c8f07b2](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/c8f07b25772f6bc35603439aad594b7a4b474356)), closes [#118](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/118)


### Bug Fixes

* add the correct type for owned-credentials method [#193](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/193) ([796415c](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/796415c05324bcf9d5f48b1cbf9dadda6ec23704))
* adjust exceptionhandling for encryption ([#185](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/185)) ([6dcf2f5](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/6dcf2f5c0eb0937042061e4d0420bddd29d4d26f))
* **credential:** adjust naming for membership credential ([#176](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/176)) ([ea2d55f](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/ea2d55fb27dd4ff057b791ed6941d94af4b8d650)), closes [#175](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/175)
* **cronjob expiry:** add the missing environment value for portal ([#196](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/196)) ([dc6b130](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/dc6b13002797dd733694f046f4ec19bc476ecc4e)), closes [#195](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/195)
* **document:** adjust document name for presentation docs ([#174](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/174)) ([e10cbcb](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/e10cbcbb03d11e03f9ae5219e1a0163dbf88b280)), closes [#166](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/166)
* **encryptionKey:** align credential and wallet config ([#201](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/201)) ([1e1ca59](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/1e1ca59ffcb60ce55c2c68da31c889d8cd490939)), closes [#197](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/197)
* **expiry:** adjust removal of companySsiDetails ([#208](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/208)) ([897a735](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/897a7350f39d378338e411c9b3083218634eac93)), closes [#195](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/195)
* **expiry:** set expiry to max 1 year ([#173](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/173)) ([46d23e8](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/46d23e8cfb192b6cd1aece437d348d42b88d54dd))
* **image-build:** change from emulation to cross-compile ([#181](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/181)) ([aa378a8](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/aa378a8849ce10aee523bd3c998c49ab33e943cc))
* implement Expiry Date for BPNL and MembershipMerge [#192](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/192) ([54dbd0e](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/54dbd0e374ca2e0da41ce63c91ee626c57059888))


### Miscellaneous Chores

* release 1.1.0-rc.1 ([f767676](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/f7676767ef475142bb374935fc13d7b8eadf99a0))

## [1.0.0](https://github.com/eclipse-tractusx/ssi-credential-issuer/compare/v1.0.0...v1.0.0) (2024-05-27)

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
* **revocation:** add endpoints to revoke credentials ([#43](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/43)) ([dc9c70d](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/dc9c70da4c0bcba979c71b5c636526c13041c774))
* **ssi:** adjust framework creation endpoint ([#70](https://github.com/eclipse-tractusx/ssi-credential-issuer/issues/70)) ([2d06fe6](https://github.com/eclipse-tractusx/ssi-credential-issuer/commit/2d06fe65365b644a209900a464c6823cb0db372e))
