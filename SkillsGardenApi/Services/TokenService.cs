using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SkillsGardenApi.Models;
using SkillsGardenApi.Security;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SkillsGardenApi.Utils;

namespace SkillsGardenApi.Services {
	public interface ITokenService {
		Task<Token> CreateToken(User user);
		Task<ClaimsPrincipal> GetByValue(string	 Value);
	}

	public class TokenService : ITokenService {
		private ILogger Logger { get; }

		private string Issuer { get; }
		private string Audience { get; }
		private TimeSpan ValidityDuration { get; }

		private SigningCredentials Credentials { get; }
		private TokenIdentityValidationParameters ValidationParameters { get; }

		public TokenService(IConfiguration Configuration, ILogger<TokenService> Logger) {
			this.Logger = Logger;

			Issuer = Configuration.GetClassValueChecked("JWTIssuer", "DebugIssuer", Logger);
			Audience = Configuration.GetClassValueChecked("JWTAudience", "DebugAudience", Logger);
			ValidityDuration = TimeSpan.FromHours(1);
			string Key = Configuration.GetClassValueChecked("JWTKey", "DebugKey DebugKey", Logger);

			SymmetricSecurityKey SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));

			Credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256Signature);

			ValidationParameters = new TokenIdentityValidationParameters(Issuer, Audience, SecurityKey);
		}

		public async Task<Token> CreateToken(User user) {
			return await CreateToken(new Claim[] {
				new Claim(ClaimTypes.Role, user.Type.ToString()),
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			});
		}

		private async Task<Token> CreateToken(Claim[] Claims) {
			JwtHeader Header = new JwtHeader(Credentials);

			JwtPayload Payload = new JwtPayload(Issuer, Audience, Claims, DateTime.UtcNow,
												DateTime.UtcNow.Add(ValidityDuration), DateTime.UtcNow);

			JwtSecurityToken SecurityToken = new JwtSecurityToken(Header, Payload);

			return await Task.FromResult(CreateToken(SecurityToken));
		}

		private Token CreateToken(JwtSecurityToken SecurityToken) {
			return new Token(SecurityToken);
		}

		public async Task<ClaimsPrincipal> GetByValue(string Value) {
			if (Value == null) {
				throw new Exception("No Token supplied");
			}

			JwtSecurityTokenHandler Handler = new JwtSecurityTokenHandler();

			try {
				SecurityToken ValidatedToken;
				ClaimsPrincipal Principal = Handler.ValidateToken(Value, ValidationParameters, out ValidatedToken);

				return await Task.FromResult(Principal);
			}
			catch (Exception e) {
				throw e;
			}
		}

	}
}
