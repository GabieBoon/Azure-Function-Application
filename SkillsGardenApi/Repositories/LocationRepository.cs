using Microsoft.EntityFrameworkCore;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Repositories
{
    public class LocationRepository : IDatabaseRepository<Location>
    {
        private readonly DatabaseContext ctx;

        public LocationRepository(DatabaseContext ctx)
        {
            this.ctx = ctx;
        }

        /**
         * Create
         */
        public async Task<Location> CreateAsync(Location location)
        {
            /*if (await LocatieExists(location.Id))
                return null;*/
            ctx.Locations.Add(location);
            await ctx.SaveChangesAsync();
            return location;
        }

        /**
         * Delete
         */
        public async Task<bool> DeleteAsync(int locationId)
        {
            Location location = await ReadAsync(locationId);
            if (location == null)
                return false;
            ctx.Locations.Remove(location);
            await ctx.SaveChangesAsync();
            return true;
        }

        /**
         * List
         */
        public async Task<List<Location>> ListAsync()
        {
            return await ctx.Locations
                .Include(l => l.Components)
                .ToListAsync();
        }

        /**
         * Read
         */
        public async Task<Location> ReadAsync(int locationId)
        {
            return await ctx.Locations
                .Include(e => e.Events)
                .Include(c => c.Components)
                .Where(l => l.Id == locationId)
                .FirstOrDefaultAsync();
        }

        /**
         * Update
         */
        public async Task<Location> UpdateAsync(Location location)
        {
            var locatieToBeUpdated = await ReadAsync(location.Id);
            if (locatieToBeUpdated == null || location == null)
            {
                return null;
            }

            // update only if set
            if (location.Name != null) locatieToBeUpdated.Name = location.Name;
            if (location.City != null) locatieToBeUpdated.City = location.City;
            if (location.Lat != null) locatieToBeUpdated.Lat = location.Lat;
            if (location.Lng != null) locatieToBeUpdated.Lng = location.Lng;
            if (location.Image != null) locatieToBeUpdated.Image = location.Image;

            await ctx.SaveChangesAsync();
            return locatieToBeUpdated;
        }

        /**
         * Exists
         */
        public async Task<bool> LocationExists(int id)
        {
            return await ctx.Locations.FirstOrDefaultAsync(x => x.Id == id) != null;
        }
    }
}
