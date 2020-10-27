using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillsGardenApi.Security
{
    public class Token {
        protected JwtSecurityToken SecurityToken { get; }

        public string Value {
            get {
                JwtSecurityTokenHandler Handler = new JwtSecurityTokenHandler();

                return Handler.WriteToken(SecurityToken);
            }
        }

        public IEnumerable<Claim> Claims {
            get {
                return SecurityToken.Claims;
            }
        }

        public Token(JwtSecurityToken SecurityToken) {
            this.SecurityToken = SecurityToken;
        }
    }
}
