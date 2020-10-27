using SkillsGardenApi.Models;
using SkillsGardenApi.Utils;
using SkillsGardenDTO;
using System;

namespace SkillsGardenApiTests.Factory
{
    public static class UserFactory
    {
        public static User CreateNormalUser(int id = 1, string email = "pete@mail.com")
        {
            byte[] salt = EncryptionUtil.CreateSalt();
            return new User
            {
                Id = id,
                Name = "Pete",
                Email = email,
                Password = EncryptionUtil.Hash("pete", salt),
                Salt = salt,
                Dateofbirth = new DateTime(2000, 1, 1),
                Gender = UserGender.Male,
                Type = UserType.User
            };
        }

        public static User CreateOrganiserUser(int id = 2)
        {
            byte[] salt = EncryptionUtil.CreateSalt();
            return new User
            {
                Id = id,
                Name = "Frank",
                Email = "frank@mail.com",
                Password = EncryptionUtil.Hash("frank", salt),
                Salt = salt,
                Dateofbirth = new DateTime(1999, 2, 2),
                Gender = UserGender.Male,
                Type = UserType.Organiser
            };
        }

        public static User CreateAdminUser(int id = 3)
        {
            byte[] salt = EncryptionUtil.CreateSalt();
            return new User
            {
                Id = id,
                Name = "Admin",
                Email = "admin@admin.nl",
                Password = EncryptionUtil.Hash("admin", salt),
                Salt = salt,
                Dateofbirth = new DateTime(1998, 3, 3),
                Gender = UserGender.Male,
                Type = UserType.Admin
            };
        }

        public static User CreateUserByType(UserType userType)
        {
            byte[] salt = EncryptionUtil.CreateSalt();
            return new User
            {
                Id = 4,
                Name = "User",
                Email = "user@user.nl",
                Password = EncryptionUtil.Hash("user", salt),
                Salt = salt,
                Dateofbirth = new DateTime(2000, 1, 1),
                Gender = UserGender.Male,
                Type = userType
            };
        }
    }
}
