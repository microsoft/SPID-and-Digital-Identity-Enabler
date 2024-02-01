/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Models
{
	public static class LoggingEvents
	{
		//Events
		public const int INCOMING_SAML_REQUEST_DECODED = 5000;
		public const int ASSERTION_CONSUMER_INVOKED = 5001;
		public const int SAML_RESPONSE_SIGNATURE_VALIDATED = 5002;
		public const int REDIRECT_URL_CREATED = 5003;
		public const int OUTGOING_SAML_REQUEST_CREATED = 5004;
		public const int USER_LOGGED_IN = 5005;
		public const int PROXY_INDEX_INVOKED = 5006;
		public const int INCOMING_SAML_RESPONSE_DECODED = 5007;
		public const int ALTERED_DATEOFBIRTH_TYPE = 5008;
		public const int EXTRACTED_AUTHNCONTEXTCLASSREF = 5009;




		//Errors
		public const int ERROR_IDENTITYPROVIDER_EMPTY = 9000;
		public const int ERROR_DECODE_SAML_REQUEST = 9001;
		public const int ERROR_PROCESS_SAML_REQUEST = 9002;
		public const int ERROR_NO_SAML_QUERYSTRING = 9003;
		public const int ERROR_DECODE_SAML_RESPONSE = 9004;
		public const int ERROR_INVALID_SAML_RESPONSE_SIGNATURE = 9005;
		public const int ERROR_SAML_RESPONSE_VALIDATION_FAILED = 9006;
		public const int ERROR_ASSERTION_CONSUMER_GENERIC_ERROR = 9007;
		public const int ERROR_OPTIONAL_RESPONSE_ALTERATION = 9008;
	}
}
