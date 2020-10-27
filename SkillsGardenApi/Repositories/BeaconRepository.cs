using Microsoft.EntityFrameworkCore;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillsGardenApi.Repositories
{
    public class BeaconRepository : IDatabaseRepository<Beacon>
    {
        private readonly DatabaseContext ctx;

        public BeaconRepository(DatabaseContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<Beacon> CreateAsync(Beacon beacon)
        {
            if (await BeaconExists(beacon.Id))
                return null;
            ctx.Beacons.Add(beacon);
            await ctx.SaveChangesAsync();
            return beacon;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Beacon beacon = await ReadAsync(id);
            if (beacon == null)
            {
                return false;
            }
            ctx.Beacons.Remove(beacon);
            await ctx.SaveChangesAsync();
            return true;
        }

        public async Task<List<Beacon>> ListAsync()
        {
            return await ctx.Beacons.ToListAsync();
        }

        public async Task<Beacon> ReadAsync(int id)
        {
            return await ctx.Beacons.Where(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Beacon> UpdateAsync(Beacon beacon)
        {
            Beacon beaconToBeUpdated = await ReadAsync(beacon.Id);
            if (beaconToBeUpdated == null || beacon == null)
            {
                return null;
            }

            // only update if set
            if (beacon.Name != null) beaconToBeUpdated.Name = beacon.Name;
            if (beacon.LocationId != null) beaconToBeUpdated.LocationId = beacon.LocationId;
            if (beacon.Lat != null) beaconToBeUpdated.Lat = beacon.Lat;
            if (beacon.Lng != null) beaconToBeUpdated.Lng = beacon.Lng;

            await ctx.SaveChangesAsync();
            return beaconToBeUpdated;
        }

        public async Task<bool> BeaconExists(int id)
        {
            return await ctx.Beacons.FirstOrDefaultAsync(x => x.Id == id) != null;
        }

        public async Task<bool> LogUser(BeaconLog beaconLog)
        {
            if (!await BeaconExists(beaconLog.BeaconId) || beaconLog == null)
                return false;

            ctx.BeaconLogs.Add(beaconLog);
            await ctx.SaveChangesAsync();
            return true;
        }

        public async Task<List<BeaconLog>> ListAsyncByuserId(int userId)
        {
            return await ctx.BeaconLogs.Where(u => u.UserId == userId).ToListAsync();
        }

        public async Task<bool> DeleteBeaconLogAsync(int userId)
        {
            List<BeaconLog> beaconlogs = await ListAsyncByuserId(userId);

            if (beaconlogs == null) return false;
                
            foreach (BeaconLog beaconlog in beaconlogs)
            {
                ctx.BeaconLogs.Remove(beaconlog);
            }

            await ctx.SaveChangesAsync();
            return true;
        }
    }
}
