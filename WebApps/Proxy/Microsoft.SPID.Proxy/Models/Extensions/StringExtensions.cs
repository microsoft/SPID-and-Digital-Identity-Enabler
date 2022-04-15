/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Web;

namespace Microsoft.SPID.Proxy.Models.Extensions;

public static class StringExtensions
{
	public static string UpperCaseUrlEncode(this string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		else
		{
			char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
			for (int i = 0; i < temp.Length - 2; i++)
			{
				if (temp[i] == '%')
				{
					temp[i + 1] = char.ToUpper(temp[i + 1]);
					temp[i + 2] = char.ToUpper(temp[i + 2]);
				}
			}
			return new string(temp);
		}
	}
}
