﻿/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options;

public class TechnicalChecksOptions
{
    public bool SkipTechnicalChecks { get; set; }
    public bool SkipAssertionSignatureValidation { get; set; }
	public bool SkipSignaturesValidation { get; set; }

}