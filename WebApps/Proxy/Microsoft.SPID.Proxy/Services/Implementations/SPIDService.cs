/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Specialized;

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class SPIDService : ISPIDService
{
    private readonly ILogger _logger;
    private readonly SPIDOptions _spidOptions;
    private readonly AttributeConsumingServiceOptions _attributeConsumingServiceOptions;
    public SPIDService(ILogger<SPIDService> logger, 
        IOptions<SPIDOptions> spidOptions, 
        IOptions<AttributeConsumingServiceOptions> attributeConsumingServiceOptions)
    {
        _logger = logger;
        _spidOptions = spidOptions.Value;
        _attributeConsumingServiceOptions = attributeConsumingServiceOptions.Value;
    }

    public bool IsSpidLValid(NameValueCollection queryStringCollection, string origin = "")
    {

        if (queryStringCollection == null)
        {
            _logger.LogDebug("Checking {SpidLevelQueryStringParamName} in {origin}: queryString is null", _spidOptions.SpidLevelQueryStringParamName, origin);
            return false;
        }

        if (string.IsNullOrWhiteSpace(queryStringCollection[_spidOptions.SpidLevelQueryStringParamName]))
        {
            _logger.LogDebug("Checking {SpidLevelQueryStringParamName} in {origin}: {SpidLevelQueryStringParamName} key not present", _spidOptions.SpidLevelQueryStringParamName, origin, _spidOptions.SpidLevelQueryStringParamName);
            return false;
        }

        if (!_spidOptions.ValidSPIDL.Contains(int.Parse(queryStringCollection[_spidOptions.SpidLevelQueryStringParamName])))
        {
            _logger.LogDebug("Checking {SpidLevelQueryStringParamName} in {origin}: {SpidLevelQueryStringParamName} value not valid. Found: {foundSpidLevelQueryStringParamName}, Expecting: [{expectedSpidLevelQueryStringParamName}]", 
                _spidOptions.SpidLevelQueryStringParamName, origin, _spidOptions.SpidLevelQueryStringParamName, queryStringCollection[_spidOptions.SpidLevelQueryStringParamName], string.Join(",", _spidOptions.ValidSPIDL));
            return false;
        }

        return true;
    }

    public bool IsPurposeValid(NameValueCollection queryStringCollection, string origin = "")
    {
        if (queryStringCollection == null)
        {
            _logger.LogDebug("Checking {PurposeName} in {origin}: queryString is null", _spidOptions.PurposeName, origin);
            return false;
        }

        if (string.IsNullOrWhiteSpace(queryStringCollection[_spidOptions.PurposeName]))
        {
            _logger.LogDebug("Checking {PurposeName} in {origin}: {PurposeName} key not present", _spidOptions.PurposeName, origin, _spidOptions.PurposeName);
            return false;
        }

        if (!_spidOptions.ValidSPIDPurposeExtension.Contains(queryStringCollection[_spidOptions.PurposeName]))
        {
            _logger.LogDebug("Checking {PurposeName} in {origin}: {PurposeName} value not valid. Found: {foundPurposeName}, Expecting: [{expectedPurposeName}]",
                _spidOptions.PurposeName, origin, _spidOptions.PurposeName, queryStringCollection[_spidOptions.PurposeName], string.Join(",", _spidOptions.ValidSPIDPurposeExtension));
            return false;
        }

        return true;
    }

    public bool IsACSValid(NameValueCollection queryStringCollection, string origin = "")
    {

        if (queryStringCollection == null)
        {
            _logger.LogDebug("Checking {AttrConsServIndexQueryStringParamName} in {origin}: queryString is null", _attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName, origin);
            return false;
        }

        if (string.IsNullOrWhiteSpace(queryStringCollection[_attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName]))
        {
            _logger.LogDebug("Checking {AttrConsServIndexQueryStringParamName} in {origin}: {AttrConsServIndexQueryStringParamName} key not present",
                _attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName, origin, _attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName);
            return false;
        }

        if (!_attributeConsumingServiceOptions.ValidACS.Contains(queryStringCollection[_attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName]))
        {
            _logger.LogDebug("Checking {AttrConsServIndexQueryStringParamName} in {origin}: {AttrConsServIndexQueryStringParamName} value not valid. Found: {foundAttrConsServIndexQueryStringParamName]}, Expecting one of: [{expectedAttrConsServIndexQueryStringParamName}]",
                _attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName, origin, _attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName, queryStringCollection[_attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName], _attributeConsumingServiceOptions.ValidACS);
            return false;
        }

        return true;
    }

    public int GetACSValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString)
    {
        var ACSValue = _attributeConsumingServiceOptions.AttributeConsumingServiceDefaultValue;

        //check for ACS in referer first, then relaystate, then wctx. If none is found, use default
        if (IsACSValid(refererQueryString, "REFERER"))
        {
            _logger.LogDebug("Using AttributeConsumingServiceIndex from Referer: {acsValue}", refererQueryString[_attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName]);
            ACSValue = int.Parse(refererQueryString[_attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName]);
        }
        else if (IsACSValid(relayQueryString, "RELAYSTATE"))
        {
            _logger.LogDebug("Using AttributeConsumingServiceIndex from RelayState: {acsValue}", relayQueryString[_attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName]);
            ACSValue = int.Parse(relayQueryString[_attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName]);
        }
        else if (IsACSValid(wctxQueryString, "WCTX"))
        {
            _logger.LogDebug("Using AttributeConsumingServiceIndex from WCTX: {acsValue}", wctxQueryString[_attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName]);
            ACSValue = int.Parse(wctxQueryString[_attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName]);
        }
        else
        {
            _logger.LogDebug("Using Default AttributeConsumingServiceIndex: {acsValue}", ACSValue);
        }

        return ACSValue;
    }

    public int GetSPIDLValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString)
    {
        var spidL = _spidOptions.DefaultSPIDL;

        if (IsSpidLValid(refererQueryString, "REFERER"))
        {
            _logger.LogDebug("Using spidL from Referer: {spidLValue}", refererQueryString[_spidOptions.SpidLevelQueryStringParamName]);
            spidL = int.Parse(refererQueryString[_spidOptions.SpidLevelQueryStringParamName]);
        }
        else if (IsSpidLValid(relayQueryString, "RELAYSTATE"))
        {
            _logger.LogDebug("Using spidL from RelayState: {spidLValue}", relayQueryString[_spidOptions.SpidLevelQueryStringParamName]);
            spidL = int.Parse(relayQueryString[_spidOptions.SpidLevelQueryStringParamName]);
        }
        else if (IsSpidLValid(wctxQueryString, "WCTX"))
        {
            _logger.LogDebug("Using spidL from WCTX: {spidLValue}", wctxQueryString[_spidOptions.SpidLevelQueryStringParamName]);
            spidL = int.Parse(wctxQueryString[_spidOptions.SpidLevelQueryStringParamName]);
        }
        else
        {
            _logger.LogDebug("Using Default spidL: {spidLValue}", spidL);
        }

        return spidL;
    }

    public string GetPurposeValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString)
    {
        string purposeValue = string.Empty;

        if (IsPurposeValid(refererQueryString, "REFERER"))
        {
            _logger.LogDebug("Using Purpose from Referer: {purposeValue}", refererQueryString[_spidOptions.PurposeName]);
            purposeValue = refererQueryString[_spidOptions.PurposeName];
        }
        else if (IsPurposeValid(relayQueryString, "RELAYSTATE"))
        {
            _logger.LogDebug("Using Purpose from RelayState: {purposeValue}", relayQueryString[_spidOptions.PurposeName]);
            purposeValue = relayQueryString[_spidOptions.PurposeName];
        }
        else if (IsPurposeValid(wctxQueryString, "WCTX"))
        {
            _logger.LogDebug("Using Purpose from WCTX: {purposeValue}", wctxQueryString[_spidOptions.PurposeName]);
            purposeValue = wctxQueryString[_spidOptions.PurposeName];
        }
        else
        {
            _logger.LogDebug("Not adding Purpose extension to the SAMl request");
        }

        return purposeValue;
    }
}