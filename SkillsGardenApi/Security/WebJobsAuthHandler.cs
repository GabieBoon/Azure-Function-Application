using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SkillsGardenApi.Security
{
	// This code will serve as a template for the auth results that we will need to produce with bearer auth
	public class WebJobsAuthLevelHandler : AuthenticationHandler<AuthenticationSchemeOptions> {
		public WebJobsAuthLevelHandler(IOptionsMonitor<AuthenticationSchemeOptions> Options,
									   ILoggerFactory LoggerFactory, UrlEncoder Encoder,
									   ISystemClock Clock) : base(Options, LoggerFactory, Encoder, Clock) {
			//
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
			return await Task.FromResult(AuthenticateResult.NoResult());
		}
	}
}