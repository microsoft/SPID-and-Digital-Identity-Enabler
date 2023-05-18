/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models.Options
{
	public class OptionalResponseAlterationOptions
	{
		public bool AlterDateOfBirth { get; set; }
        public string DateOfBirthFormat { get; set; }
    }
}
