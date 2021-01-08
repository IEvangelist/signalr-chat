using BlazoR.Chat.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace BlazoR.Chat.Server.Controllers
{
    [ApiController]
    public class UserController : Controller
    {
        private static readonly UserInfo _loggedOutUser = new(false);

        [HttpGet("user")]
        public UserInfo GetUser() =>
            User.Identity.IsAuthenticated
                ? new(true, User.Identity.Name)
                : _loggedOutUser;

        [EnableCors, HttpGet("user/signin")]
        public async Task SignIn(
            [FromQuery] string redirectUri,
            [FromQuery] string scheme)
        {
            if (string.IsNullOrEmpty(redirectUri) || !Url.IsLocalUrl(redirectUri))
            {
                redirectUri = "/";
            }

            await HttpContext.ChallengeAsync(
                scheme ?? TwitterDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = redirectUri });
        }

        [HttpGet("user/signout")]
        public async Task<IActionResult> SingOutUser()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/");
        }

        [HttpGet("user/schemes")]
        public async Task<IActionResult> GetSchemes(
            [FromServices] IAuthenticationSchemeProvider schemeProvider)
        {
            var schemes = await schemeProvider.GetRequestHandlerSchemesAsync();
            return Json(schemes.Select(scheme => new AuthScheme(scheme.DisplayName, scheme.Name)));
        }
    }
}
