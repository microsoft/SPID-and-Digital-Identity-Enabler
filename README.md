# SPID Enablement Solution

This solution aims to simplify the implementation of login using SPID or similar services, such as CIE and eIDAS. It supports both the use of ADFS and B2C as identity federators.

## What's SPID
SPID is the Digital Identity Public System which allow all the italian citizens to access online public services (such as INPS, INAIL, AdE, etc) with only one set of credentials.

## Why this repo
Besides [AgID](https://www.agid.gov.it/) asserts that the SPID authentication system is SAML2 compliant, it isn't. Moreover, in the SPID Technical Regulations (https://docs.italia.it/italia/spid/spid-regole-tecniche/it/stabile/index.html), there are a few mandatory requirements which break the SAML2 protocol itself. For this reason, all the SAML2 solutions/products/libraries can't be used out of the box with SPID. So we developed the such called SPIDProxy which proxies the SAMLRequests/SAMLResponses between a Federator (ADFS or AAD B2C in our case) and the SPID IdPs.

## Not only SPID
We started the project with the objective of supporting SPID, and we reached that goal different years ago.
Throughout the years, [AgID](https://www.agid.gov.it/) announced different authentication systems such as CNS (Carta Nazionale dei Servizi), CIE (Carta Identità Elettronica) and eIDAS (electronic IDentification Authentication and Signature).
SPID, CIE and CNS are mandatory for every italian public accessible online service, while eIDAS should be used if you want to make your online service accessible to EU citizens, using their SPID corrispective identities.
To support such authentication systems we extended the SPIDProxy to support CIE and eIDAS, while we developed another component (CNS.Auth.Web) to enable CNS authentication.

## What's inside the repo
Inside this repo you'll find:
 - The SPIDProxy source code
 - The CNS.Auth.Web source code
 - Powershell scripts to configure ADFS to support SPID/CIE/eIDAS authentication
 - Pre-configured AAD B2C Custom Policies for supporting SPID/CIE/eIDAS/CNS authentication
 - [AgID](https://www.agid.gov.it/) compliant Custom UIs for ADFS and AAD B2C

## Contributing
All the contributions are welcome. Check the open issues, create a branch and open a PR. If you notice bugs or potential improvements, don't be shy and open a new issue!

## Main Contributors
The main contributors for this repo are:
 - Fulvio Mercoliano ([fume](https://github.com/fume))
 - Tommaso Stocchi ([tommasodotnet](https://github.com/tommasodotNET))
 - Paolo Castaldi ([paolocastaway](https://github.com/paolocastaway))
 - Pierluigi Pesce ([pierfish](https://github.com/pierfish))
 - Alessandro Ferrillo ([aferrillo](https://github.com/aferrillo))

## Trademark Notice

>This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft’s Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party’s policies.

## License

Copyright (c) Microsoft Corporation. All rights reserved.

Licensed under the [MIT](LICENSE.txt) license.
