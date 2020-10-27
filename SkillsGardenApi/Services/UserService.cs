using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using SkillsGardenDTO.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkillsGardenApi.Services
{
    public class UserService
    {
        private UserRepository userRepository;

        public UserService(IDatabaseRepository<User> userRepository)
        {
            this.userRepository = (UserRepository)userRepository;
        }

        public async Task<List<UserResponse>> GetAllUsers()
        {
            List<User> users = await userRepository.ListAsync();

            List<UserResponse> response = new List<UserResponse>();
            foreach (User user in users)
            {
                response.Add(new UserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Dateofbirth = user.Dateofbirth,
                    Gender = user.Gender,
                    Type = user.Type
                });
            }

            return response;
        }

        public async Task<UserResponse> GetUser(int id)
        {
            User user = await userRepository.ReadAsync(id);

            // if the user was not found
            if (user == null)
                return null;

            UserResponse response = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Dateofbirth = user.Dateofbirth,
                Gender = user.Gender,
                Type = user.Type
            };

            return response;
        }

        public async Task<User> CreateUser(UserBody userBody)
        {
            // create salt
            byte[] salt = EncryptionUtil.CreateSalt();

            // create user
            User newUser = new User
            {
                Name = userBody.Name,
                Email = userBody.Email,
                Password = EncryptionUtil.Hash(userBody.Password, salt),
                Salt = salt,
                Dateofbirth = userBody.Dateofbirth,
                Gender = userBody.Gender,
                Type = userBody.Type
            };

            // save to database
            newUser = await userRepository.CreateAsync(newUser);

            return newUser;
        }

        public async Task<User> UpdateUser(int userId, UserBody userBody)
        {
            // create user
            User updatedUser = new User
            {
                Id = userId,
                Name = userBody.Name,
                Email = userBody.Email,
                Dateofbirth = userBody.Dateofbirth,
                Gender = userBody.Gender,
                Type = userBody.Type
            };

            if (userBody.Password != null)
            {
                // create salt
                byte[] salt = EncryptionUtil.CreateSalt();

                updatedUser.Password = EncryptionUtil.Hash(userBody.Password, salt);
                updatedUser.Salt = salt;
            }

            // save to database
            updatedUser = await userRepository.UpdateAsync(updatedUser);

            return updatedUser;
        }

        public async Task<bool> DeleteUser(int userId)
        {
            // delete user
            return await userRepository.DeleteAsync(userId);
        }

        public async Task<bool> EmailExists(string email)
        {
            User user = await userRepository.GetUserByEmail(email);
            if (user == null)
                return true;
            return false;
        }
    }
}
