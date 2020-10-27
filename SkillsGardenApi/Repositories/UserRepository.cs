using Microsoft.EntityFrameworkCore;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Repositories
{
    public class UserRepository : IDatabaseRepository<User>
    {
        private readonly DatabaseContext ctx;

        public UserRepository(DatabaseContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<User> CreateAsync(User user)
        {
            if (await UserExists(user.Id))
                return null;
            if ((await GetUserByEmail(user.Email)) != null)
                return null;
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            User user = await ReadAsync(id);
            if (user == null)
            {
                return false;
            }
            ctx.Users.Remove(user);
            await ctx.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> ListAsync()
        {
            return await ctx.Users.ToListAsync();
        }

        public async Task<User> ReadAsync(int id)
        {
            return await ctx.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> UpdateAsync(User user)
        {
            User userToBeUpdated = await ReadAsync(user.Id);
            if (userToBeUpdated == null || user == null)
            {
                return null;
            }

            // only update if set
            if (user.Name != null) userToBeUpdated.Name = user.Name;
            if (user.Email != null) userToBeUpdated.Email = user.Email;
            if (user.Password != null) userToBeUpdated.Password = user.Password;
            if (user.Salt != null) userToBeUpdated.Salt = user.Salt;
            if (user.Dateofbirth != null) userToBeUpdated.Dateofbirth = user.Dateofbirth;
            if (user.Gender != null) userToBeUpdated.Gender = user.Gender;
            if (user.Type != null) userToBeUpdated.Type = user.Type;

            await ctx.SaveChangesAsync();
            return userToBeUpdated;
        }

        public async Task<bool> UserExists(int id)
        {
            return await ctx.Users.FirstOrDefaultAsync(x => x.Id == id) != null;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await ctx.Users.Where(b => b.Email == email).FirstOrDefaultAsync();
        }
    }
}
