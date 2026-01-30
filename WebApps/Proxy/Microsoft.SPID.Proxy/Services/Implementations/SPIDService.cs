/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Specialized;
using System.Xml;

namespace Microsoft.SPID.Proxy.Services.Implementations;

public class SPIDService : ISPIDService
{
	private readonly ILogger _logger;
	private readonly CIEOptions _cieOptions;
	private readonly SPIDOptions _spidOptions;
	private readonly AttributeConsumingServiceOptions _attributeConsumingServiceOptions;

	public SPIDService(ILogger<SPIDService> logger,
		IOptions<SPIDOptions> spidOptions,
		IOptions<AttributeConsumingServiceOptions> attributeConsumingServiceOptions,
		IOptions<CIEOptions> cieOptions)
	{
		_logger = logger;
		_cieOptions = cieOptions.Value;
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

	public bool IsSPIDACSValid(NameValueCollection queryStringCollection, string origin = "")
	{
		string paramName = _attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName;

		if (queryStringCollection == null)
		{
			_logger.LogDebug("Checking {AttrConsServIndexQueryStringParamName} in {origin}: queryString is null", paramName, origin);
			return false;
		}

		if (string.IsNullOrWhiteSpace(queryStringCollection[paramName]))
		{
			_logger.LogDebug("Checking {AttrConsServIndexQueryStringParamName} in {origin}: {AttrConsServIndexQueryStringParamName} key not present",
				paramName, origin, paramName);
			return false;
		}

		if (!_attributeConsumingServiceOptions.ValidACS.Contains(queryStringCollection[paramName]))
		{
			_logger.LogDebug("Checking {AttrConsServIndexQueryStringParamName} in {origin}: {AttrConsServIndexQueryStringParamName} value not valid. Found: {foundAttrConsServIndexQueryStringParamName]}, Expecting one of: [{expectedAttrConsServIndexQueryStringParamName}]",
				paramName, origin, paramName, queryStringCollection[paramName], _attributeConsumingServiceOptions.ValidACS);
			return false;
		}

		return true;
	}

	public bool IsCIEACSValid(NameValueCollection queryStringCollection, string origin = "")
	{
		string paramName = _attributeConsumingServiceOptions.CIEAttrConsServIndexQueryStringParamName;

		if (queryStringCollection == null)
		{
			_logger.LogDebug("Checking {CIEAttrConsServIndexQueryStringParamName} in {origin}: queryString is null", paramName, origin);
			return false;
		}

		if (string.IsNullOrWhiteSpace(queryStringCollection[paramName]))
		{
			_logger.LogDebug("Checking {CIEAttrConsServIndexQueryStringParamName} in {origin}: {CIEAttrConsServIndexQueryStringParamName} key not present",
				paramName, origin, paramName);
			return false;
		}

		if (!_attributeConsumingServiceOptions.CIEValidACS.Contains(queryStringCollection[paramName]))
		{
			_logger.LogDebug("Checking {CIEAttrConsServIndexQueryStringParamName} in {origin}: {CIEAttrConsServIndexQueryStringParamName} value not valid. Found: {foundCIEAttrConsServIndexQueryStringParamName]}, Expecting one of: [{expectedCIEAttrConsServIndexQueryStringParamName}]",
				paramName, origin, paramName, queryStringCollection[paramName], _attributeConsumingServiceOptions.CIEValidACS);
			return false;
		}

		return true;
	}


	public bool IsComparisonValid(NameValueCollection queryStringCollection, string origin = "")
	{

		if (queryStringCollection == null)
		{
			_logger.LogDebug("Checking {ComparisonQueryStringParamName} in {origin}: queryString is null", _spidOptions.ComparisonQueryStringParamName, origin);
			return false;
		}

		if (string.IsNullOrWhiteSpace(queryStringCollection[_spidOptions.ComparisonQueryStringParamName]))
		{
			_logger.LogDebug("Checking {ComparisonQueryStringParamName} in {origin}: {ComparisonQueryStringParamName} key not present",
				_spidOptions.ComparisonQueryStringParamName, origin, _spidOptions.ComparisonQueryStringParamName);
			return false;
		}

		return true;
	}

	private bool IsValidSpidACSFromExtensions(string spidACSValue, out int parsedValue)
	{
		parsedValue = 0;
		
		if (string.IsNullOrWhiteSpace(spidACSValue))
		{
			return false;
		}

		if (!_attributeConsumingServiceOptions.ValidACS.Contains(spidACSValue))
		{
			_logger.LogDebug("AttributeConsumingServiceIndex from SAMLRequest Extensions is not valid. Found: {foundValue}, Expecting one of: [{expectedValues}]",
				spidACSValue, string.Join(",", _attributeConsumingServiceOptions.ValidACS));
			return false;
		}

		if (!int.TryParse(spidACSValue, out parsedValue))
		{
			_logger.LogDebug("AttributeConsumingServiceIndex from SAMLRequest Extensions could not be parsed as integer: {foundValue}", spidACSValue);
			return false;
		}

		return true;
	}

	public int GetSPIDAttributeConsumigServiceValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString, XmlDocument requestAsXml)
	{
		var ACSValue = _attributeConsumingServiceOptions.AttributeConsumingServiceDefaultValue;
		
		string paramName = _attributeConsumingServiceOptions.AttrConsServIndexQueryStringParamName;

		// First, try to get spidACS from SAMLRequest Extensions (highest priority)
		if (requestAsXml != null)
		{
			try
			{
				var spidACSFromExtensions = requestAsXml.GetSpidACSFromExtensions(
					_spidOptions.ExtensionsElementName,
					_attributeConsumingServiceOptions.SpidACSElementName);

				if (IsValidSpidACSFromExtensions(spidACSFromExtensions, out int parsedValue))
				{
					_logger.LogDebug("Using AttributeConsumingServiceIndex from SAMLRequest Extensions: {acsValue}", spidACSFromExtensions);
					return parsedValue;
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Error extracting spidACS from SAMLRequest Extensions, falling back to other methods");
			}
		}

		// Check if referer-based extraction is disabled
		if (_attributeConsumingServiceOptions.DisableACSFromReferer)
		{
			return ACSValue;
		}

		//check for ACS in referer first, then relaystate, then wctx. If none is found, use default
		if (IsSPIDACSValid(refererQueryString, "REFERER"))
		{
			_logger.LogDebug("Using AttributeConsumingServiceIndex from Referer: {acsValue}", refererQueryString[paramName]);
			ACSValue = int.Parse(refererQueryString[paramName]);
		}
		else if (IsSPIDACSValid(relayQueryString, "RELAYSTATE"))
		{
			_logger.LogDebug("Using AttributeConsumingServiceIndex from RelayState: {acsValue}", relayQueryString[paramName]);
			ACSValue = int.Parse(relayQueryString[paramName]);
		}
		else if (IsSPIDACSValid(wctxQueryString, "WCTX"))
		{
			_logger.LogDebug("Using AttributeConsumingServiceIndex from WCTX: {acsValue}", wctxQueryString[paramName]);
			ACSValue = int.Parse(wctxQueryString[paramName]);
		}
		else
		{
			_logger.LogDebug("Using Default AttributeConsumingServiceIndex: {acsValue}", ACSValue);
		}

		return ACSValue;
	}

	public int GetCIEAttributeConsumigServiceValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString)
	{
		var ACSValue = _attributeConsumingServiceOptions.CIEAttributeConsumingService;

		if (_attributeConsumingServiceOptions.DisableACSFromReferer)
		{
			return ACSValue;
		}

		string paramName = _attributeConsumingServiceOptions.CIEAttrConsServIndexQueryStringParamName;

		//check for ACS in referer first, then relaystate, then wctx. If none is found, use default
		if (IsCIEACSValid(refererQueryString, "REFERER"))
		{
			_logger.LogDebug("Using AttributeConsumingServiceIndex from Referer: {acsValue}", refererQueryString[paramName]);
			ACSValue = int.Parse(refererQueryString[paramName]);
		}
		else if (IsCIEACSValid(relayQueryString, "RELAYSTATE"))
		{
			_logger.LogDebug("Using AttributeConsumingServiceIndex from RelayState: {acsValue}", relayQueryString[paramName]);
			ACSValue = int.Parse(relayQueryString[paramName]);
		}
		else if (IsCIEACSValid(wctxQueryString, "WCTX"))
		{
			_logger.LogDebug("Using AttributeConsumingServiceIndex from WCTX: {acsValue}", wctxQueryString[paramName]);
			ACSValue = int.Parse(wctxQueryString[paramName]);
		}
		else
		{
			_logger.LogDebug("Using Default AttributeConsumingServiceIndex: {acsValue}", ACSValue);
		}

		return ACSValue;
	}


	public int GetSPIDLValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString, bool isCie)
	{
		var spidL = isCie ? _cieOptions.DefaultSPIDL : _spidOptions.DefaultSPIDL;

		if(_spidOptions.DisableSpidLevelFromReferer)
		{
			return spidL;
		}

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
			_logger.LogDebug("Using Default spidL: {spidLValue}, isCIE: {isCie}", spidL, isCie);
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

	public string GetComparisonValue(NameValueCollection refererQueryString, NameValueCollection relayQueryString, NameValueCollection wctxQueryString, bool isCie)
	{
		var comparisonValue = isCie ? _cieOptions.DefaultComparison : _spidOptions.DefaultComparison;

		//check for comparison in referer first, then relaystate, then wctx. If none is found, use default
		if (IsComparisonValid(refererQueryString, "REFERER"))
		{
			_logger.LogDebug("Using Comparison from Referer: {comparisonValue}", refererQueryString[_spidOptions.ComparisonQueryStringParamName]);
			comparisonValue = refererQueryString[_spidOptions.ComparisonQueryStringParamName];
		}
		else if (IsComparisonValid(relayQueryString, "RELAYSTATE"))
		{
			_logger.LogDebug("Using Comparison from RelayState: {comparisonValue}", relayQueryString[_spidOptions.ComparisonQueryStringParamName]);
			comparisonValue = relayQueryString[_spidOptions.ComparisonQueryStringParamName];
		}
		else if (IsComparisonValid(wctxQueryString, "WCTX"))
		{
			_logger.LogDebug("Using Comparison from WCTX: {comparisonValue}", wctxQueryString[_spidOptions.ComparisonQueryStringParamName]);
			comparisonValue = wctxQueryString[_spidOptions.ComparisonQueryStringParamName];
		}
		else
		{
			_logger.LogDebug("Using Default Comparison: {comparisonValue}, isCie: {isCie}", comparisonValue, isCie);
		}

		return comparisonValue;
	}
}