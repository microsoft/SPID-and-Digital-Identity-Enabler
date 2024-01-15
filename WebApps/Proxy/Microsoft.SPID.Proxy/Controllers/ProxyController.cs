/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.SPID.Proxy.Controllers;

public class ProxyController : Controller
{
	private readonly ILogger _logger;
	private readonly ILogAccessService _logAccessService;
	private readonly IFederatorResponseService _federatorResponseService;
	private readonly IFederatorRequestService _federatorRequestService;
	private readonly ISAMLService _samlService;
	private readonly FederatorOptions _federatorOptions;
	private readonly TechnicalChecksOptions _technicalChecksOptions;
	private readonly LoggingOptions _loggingOptions;

	public ProxyController(ILogger<ProxyController> logger,
		ILogAccessService logAccessService,
		IFederatorResponseService federatorResponseService,
		IFederatorRequestService federatorRequestService,
		ISAMLService samlService,
		IOptions<FederatorOptions> federatorOptions,
		IOptions<TechnicalChecksOptions> technicalChecksOptions,
		IOptions<LoggingOptions> loggingOptions)
	{
		_logger = logger;
		_logAccessService = logAccessService;
		_federatorResponseService = federatorResponseService;
		_federatorRequestService = federatorRequestService;
		_samlService = samlService;
		_federatorOptions = federatorOptions.Value;
		_technicalChecksOptions = technicalChecksOptions.Value;
		_loggingOptions = loggingOptions.Value;
	}

	[HttpGet]
	[Route("proxy/index/{identityProvider}")]
	public async Task<IActionResult> Index(string identityProvider)
	{
		_logger.LogInformation(LoggingEvents.PROXY_INDEX_INVOKED, "Proxy/Index endpoint invoked. IdentityProvider = {identityProvider}, RequestUrl = {requestUrl}", identityProvider, HttpContext.Request.GetDisplayUrl());

		if (string.IsNullOrWhiteSpace(identityProvider))
		{
			_logger.LogError(LoggingEvents.ERROR_IDENTITYPROVIDER_EMPTY, $"{nameof(identityProvider)} is null or empty. Impossible to continue processing");
			throw new ArgumentException("Parameter cannot be null or empty", nameof(identityProvider));
		}

		if (HttpContext.Request.Query != null && HttpContext.Request.Query.Count > 0)
		{
			var samlRequest = HttpContext.Request.Query["SAMLRequest"];
			var relayState = HttpContext.Request.Query["RelayState"];
			var sigAlg = HttpContext.Request.Query["SigAlg"];
			var signature = HttpContext.Request.Query["Signature"];
			var request = new FederatorRequest(identityProvider, samlRequest, relayState, sigAlg, signature);

			XmlDocument requestAsXml;
			try
			{
				var decodedSAMLRequest = request.SAMLRequest.DecodeSamlRequest();
				_logger.LogInformation(LoggingEvents.INCOMING_SAML_REQUEST_DECODED, "Decoded SAML Request: {decodedSAMLRequest}", decodedSAMLRequest);
				requestAsXml = decodedSAMLRequest.ToXmlDocument();
				_logger.LogDebug("SAMLRequest converted to XMLDocument");
			}
			catch (Exception e)
			{
				_logger.LogError(LoggingEvents.ERROR_DECODE_SAML_REQUEST, e, "Unable to decode SAMLRequest.");
				return BadRequest();
			}

			var ID = requestAsXml.GetRequestID();
			var loggerState = new Dictionary<string, object>() { ["SAMLRequestID"] = ID };

			using (_logger.BeginScope(loggerState))
			{
				try
				{
					return new RedirectResult(await _federatorRequestService.GetRedirectUriAsync(request, requestAsXml), false);
				}

				catch (Exception e)
				{
					_logger.LogError(LoggingEvents.ERROR_PROCESS_SAML_REQUEST, e, "The proxy wasn't able to successfully process the incoming SAML Request. Pass-through redirection will occur.");
					return new RedirectResult(_federatorRequestService.GetPassThrowRedirectUri(request), false);
				}
			}

		}
		else
		{
			_logger.LogError(LoggingEvents.ERROR_NO_SAML_QUERYSTRING, "The request doesn't have SAML mandatory parameters such as SAMLRequest. Returning BadRequest");
			return BadRequest();
		}
	}

	// GET: Proxy/Create
	public ActionResult CourtesyPage()
	{
		return View();
	}

	[HttpPost]
	public async Task<ActionResult> AssertionConsumer(string SAMLResponse, string RelayState)
	{
		_logger.LogInformation(LoggingEvents.ASSERTION_CONSUMER_INVOKED, "AssertionConsumer endpoint invoked. SAMLResponse = {samlResponse}, RelayState = {relayState}", SAMLResponse, RelayState);

		string inResponseTo, id, issuer, decodedSamlResponse;
		XmlDocument responseXml;

		try
		{
			decodedSamlResponse = SAMLResponse.DecodeSamlResponse();
			responseXml = decodedSamlResponse.ToXmlDocument();

			if (_loggingOptions.LogDecodedSamlResponse)
				_logger.LogInformation(LoggingEvents.INCOMING_SAML_RESPONSE_DECODED, "SAMLResponse decoded = {samlResponseDecoded}", decodedSamlResponse);
			else
				_logger.LogInformation(LoggingEvents.INCOMING_SAML_RESPONSE_DECODED, "SAMLResponse decoded.");
		}
		catch (Exception e)
		{
			_logger.LogError(LoggingEvents.ERROR_DECODE_SAML_RESPONSE, e, "An error occurred decoding the SAMLResponse");
			throw;
		}

		inResponseTo = responseXml.GetInResponseTo();
		id = responseXml.GetResponseID();
		issuer = responseXml.GetIssuer();

		_logger.LogDebug("InResponseTo, ID and Issuer retrieved.");

		var scopeState = new Dictionary<string, object>() { ["SAMLResponseInResponseTo"] = inResponseTo, ["SAMLResponseID"] = id, ["SAMLResponseIssuer"] = issuer };

		using (_logger.BeginScope(scopeState))
		{
			try
			{
				SPIDErrorModel errorModel;
				if (!_technicalChecksOptions.SkipSignaturesValidation)
				{
					if (!await _federatorResponseService.CheckSignature(responseXml))
					{
						errorModel = new SPIDErrorModel()
						{
							UserFriendlyMessage = new HtmlString("Signature della risposta non valida o non presente, impossibile proseguire con l'autenticazione.")
						};
						_logger.LogError(LoggingEvents.ERROR_INVALID_SAML_RESPONSE_SIGNATURE, "Invalid SAMLResponse Signature");
						return View("CourtesyPage", errorModel);
					}
					_logger.LogInformation(LoggingEvents.SAML_RESPONSE_SIGNATURE_VALIDATED, "SAMLResponse signature validated");
				}

				responseXml.RemoveSignatures();
				_logger.LogDebug("Removed original Signature(s)");

				if (_federatorResponseService.ResponseHasBlockingStatusCode(responseXml, out errorModel))
				{
					_logger.LogWarning("Blocking statusCode found: {StatusCode}. statusMessage : {StatusMessage}", errorModel.StatusCode, errorModel.StatusMessage);
					return View("CourtesyPage", errorModel);
				}

				_logger.LogDebug("Checked SAMLResponse Status");
				_federatorResponseService.RunTechnicalChecks(responseXml);
				_logger.LogDebug("Ran Technical Checks");

				_logger.LogDebug("Setting audience to = {newAudience}", _federatorOptions.EntityId);
				_logger.LogDebug("Setting destination to = {newDestination}", _federatorOptions.FederatorAttributeConsumerServiceUrl);
				_logger.LogDebug("Removing NameQualifier from Issuer");
				_logger.LogDebug("Setting new SubjectConfirmationData.Recipient = {newRecipient}", _federatorOptions.FederatorAttributeConsumerServiceUrl);
				responseXml.AlterAudience(_federatorOptions.EntityId)
					.AlterDestination(_federatorOptions.FederatorAttributeConsumerServiceUrl, _samlService.GetAttributeConsumerService(), _technicalChecksOptions.SkipTechnicalChecks)
					.AlterSubjectConfirmation(_federatorOptions.FederatorAttributeConsumerServiceUrl)
					.RemoveNameQualifierIfFormatEntity();

				_federatorResponseService.ApplyOptionalResponseAlteration(responseXml);

				await _federatorResponseService.SignAssertionAsync(responseXml);
				_logger.LogDebug("Assertions Signed");

				await _federatorResponseService.SignWholeResponseMessageAsync(responseXml);
				_logger.LogDebug("SAMLResponse signed");

				var model = _federatorResponseService.GetFederatorResponse(responseXml, RelayState);
				_logAccessService.LogAccess(responseXml);
				return View(model);
			}
			catch (SPIDValidationException spidEx)
			{
				_logger.LogError(LoggingEvents.ERROR_SAML_RESPONSE_VALIDATION_FAILED, spidEx, "SPID SAMLResponse validation failed");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(LoggingEvents.ERROR_ASSERTION_CONSUMER_GENERIC_ERROR, ex, "An error occurred on the AssertionConsumer endpoint.");
				throw;
			}
		}


	}

}