using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkillsGardenApi.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SkillsGardenApi.Security
{
    public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
        private ITokenService TokenService { get; }

        public BearerAuthenticationHandler(ITokenService TokenService, IOptionsMonitor<AuthenticationSchemeOptions> Options,
                                           ILoggerFactory LoggerFactory, UrlEncoder Encoder,
                                           ISystemClock Clock) : base(Options, LoggerFactory, Encoder, Clock) {
            this.TokenService = TokenService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            string TokenString = GetTokenString();

            try {
                ClaimsPrincipal Principal = await TokenService.GetByValue(TokenString);

                AuthenticationTicket Ticket = new AuthenticationTicket(Principal, Scheme.Name);

                return AuthenticateResult.Success(Ticket);
            }
            catch {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }
        }

        private string GetTokenString() {
            string AuthorizationHeader = Request.Headers["Authorization"];

            if (AuthorizationHeader != null) {
                if (AuthorizationHeader.StartsWith("Bearer ")) {
                    return AuthorizationHeader.Substring(7);
                }
                else return null;
            }
            else return null;
        }
    }
}