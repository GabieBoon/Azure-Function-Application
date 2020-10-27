using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using System.Threading.Tasks;

namespace SkillsGardenApi.Services
{
    public class AuthService
    {
        private UserRepository userRepository;

        public AuthService(IDatabaseRepository<User> userRepository)
        {
            this.userRepository = (UserRepository)userRepository;
        }

        public async Task<User> VerifyLogin(LoginBody loginBody)
        {   
            // get the user by email
            User user = await userRepository.GetUserByEmail(loginBody.Email);

            // if the user does not exist
            if (user == null)
                return null;

            // verify the password
            if (EncryptionUtil.Verify(loginBody.Password, user.Password, user.Salt))
                return user;

            return null;
        }
    }
}
