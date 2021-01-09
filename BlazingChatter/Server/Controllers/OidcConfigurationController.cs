using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlazingChatter.Server.Controllers
{
    public class OidcConfigurationController : Controller
    {
        private readonly IClientRequestParametersProvider _clientRequestParametersProvider;
        private readonly ILogger<OidcConfigurationController> _logger;

        public OidcConfigurationController(IClientRequestParametersProvider clientRequestParametersProvider, ILogger<OidcConfigurationController> logger)
        {
            _clientRequestParametersProvider = clientRequestParametersProvider;
            _logger = logger;
        }

        [HttpGet("_configuration/{clientId}")]
        public IActionResult GetClientRequestParameters([FromRoute] string clientId)
        {
            var parameters = _clientRequestParametersProvider.GetClientParameters(HttpContext, clientId);
            return Ok(parameters);
        }
    }
}
