/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options
{
	public class OptionalResponseAlterationOptions
	{
		public bool AlterDateOfBirth { get; set; }
		public string DateOfBirthFormat { get; set; } = "xs:date";

		public bool AddSAMLResponseIDAttribute { get; set; }
		public string SAMLResponseIDAttributeName { get; set; } = "OriginalSAMLResponseID";
		public string SAMLResponseIDAttributeXsiType { get; set; } = "xs:string";

		public bool AddAuthnContextClassRefAttribute { get; set; }
		public string AuthnContextClassRefAttributeName { get; set; } = "SPIDLevel";
		public string AuthnContextClassRefAttributeXsiType { get; set; } = "xs:string";
	}
}
