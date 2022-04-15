/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text;

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class LogAccessService : ILogAccessService
{
    private readonly ILogger _logger;
    private readonly LogAccessOptions _logAccessOptions;

    public LogAccessService(ILogger<LogAccessService> logger,
        IOptions<LogAccessOptions> logAccessOptions)
    {
        _logger = logger;
        _logAccessOptions = logAccessOptions.Value;
    }

    public void LogAccess(XmlDocument samlResponse)
    {
		if (_logAccessOptions.Enabled)
        {
            var userInfos = new Dictionary<string, string>();
            foreach (var field in _logAccessOptions.FieldsToLog)
            {
                var value = samlResponse.GetAttribute(field);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    userInfos.Add(field, value);
                }
            }

            var issuer = samlResponse.GetIssuer();
            var inResponseTo = samlResponse.GetInResponseTo();
            var id = samlResponse.GetResponseID();

            _logger.LogInformation(LoggingEvents.USER_LOGGED_IN,"A user logged-in via SPIDProxy. User=[{userInfos}];Issuer=[{issuer}];Timestamp=[{date}];InResponseTo=[{inResponseTo}];ID=[{id}];",
                FormatUserInfo(userInfos), issuer, DateTime.UtcNow.ToString("o"), inResponseTo, id);
        }
    }

    private string FormatUserInfo(Dictionary<string, string> userInfos)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var userInfo in userInfos)
        {
            sb.Append($"{userInfo.Key}={userInfo.Value};");
        }
        return sb.ToString();
    }
}