﻿/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CNS.Auth.Web.Models
{
	public class AutoPostFormModel
	{
		public HtmlString SAMLResponse { get; set; }
		public string RelayState { get; set; }
		public string ActionUrl { get; set; }


	}
}
