using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IEvangelist.SignalR.Chat.Pages
{
    public class LoginModel : PageModel
    {
        readonly IAuthenticationSchemeProvider _authSchemeProvider;

        public IEnumerable<AuthenticationScheme> AuthSchemes { get; private set; }

        public LoginModel(IAuthenticationSchemeProvider authSchemeProvider)
            => _authSchemeProvider = authSchemeProvider;

        public async Task<IActionResult> OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }

            AuthSchemes = await _authSchemeProvider.GetRequestHandlerSchemesAsync();

            return Page();
        }

        public IActionResult OnPost(string scheme)
            => Challenge(new AuthenticationProperties { RedirectUri = Url.Page("/Index") }, scheme);
    }
}