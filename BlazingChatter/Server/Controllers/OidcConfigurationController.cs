using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace BlazingChatter.Server.Controllers
{
    public class OidcConfigurationController : Controller
    {
        private readonly SymmetricSecurityKey _securityKey = new(Guid.NewGuid().ToByteArray());
        private readonly JwtSecurityTokenHandler _jwtTokenHandler = new();

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

        [HttpGet("genaratetoken")]
        public string GetToken(
            [FromServices] IHttpContextAccessor context)
        {
            var audience = context.HttpContext.Request.Host.Value;
            SigningCredentials credentials = new(_securityKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken token = new(
                "BlazoR.Chat", audience,
                claims: context.HttpContext.User.Claims,
                signingCredentials: credentials);

            _logger.LogInformation("Generated token for SignalR connection.");

            return _jwtTokenHandler.WriteToken(token);
        }
    }
}
