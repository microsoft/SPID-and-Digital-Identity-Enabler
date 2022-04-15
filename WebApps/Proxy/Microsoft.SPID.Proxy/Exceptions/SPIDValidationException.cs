/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Microsoft.SPID.Proxy.Exceptions;

[Serializable]
public class SPIDValidationException : Exception
{
    public SPIDValidationException() { }
    public SPIDValidationException(string message) : base(message) { }
    public SPIDValidationException(string message, Exception inner) : base(message, inner) { }
    protected SPIDValidationException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}