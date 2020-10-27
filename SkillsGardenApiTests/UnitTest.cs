using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories.Context;
using SkillsGardenApi.Security;
using SkillsGardenApi.Services;
using SkillsGardenApiTests.Database;
using SkillsGardenApiTests.Factory;
using SkillsGardenDTO;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SkillsGardenApiTests
{
    public abstract class UnitTest
	{
		private TokenService tokenService;

		protected ClaimsPrincipal userClaim;
		protected ClaimsPrincipal organiserClaim;
		protected ClaimsPrincipal adminClaim;

		public UnitTest()
		{
			// create configuration for token service
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string>())
				.Build();

			// create logger for token service
			ILogger<TokenService> logger = LoggerFactoryExtensions.CreateLogger<TokenService>(new LoggerFactory());

			// create token service
			this.tokenService = new TokenService(configuration, logger);

			CreateClaimPrincipals();
		}

		protected DatabaseContext CreateEmptyDatabase()
        {
			TestDbService dbService = new TestDbService();
			return dbService.GetDatabaseContext();
		}

		protected async Task<ClaimsPrincipal> getClaimsPrincipal(UserType userType)
		{
			User user = UserFactory.CreateUserByType(userType);

			Token token = await tokenService.CreateToken(user);
			ClaimsPrincipal claimsPrincipal = await tokenService.GetByValue(token.Value);
			return claimsPrincipal;
		}

		protected async void CreateClaimPrincipals()
		{
			this.userClaim = await getClaimsPrincipal(UserType.User);
			this.organiserClaim = await getClaimsPrincipal(UserType.Organiser);
			this.adminClaim = await getClaimsPrincipal(UserType.Admin);
		}
	}
}
