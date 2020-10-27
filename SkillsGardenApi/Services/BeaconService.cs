using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenDTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkillsGardenApi.Services
{
    public class BeaconService
    {
        private BeaconRepository beaconRepository;

        public BeaconService(IDatabaseRepository<Beacon> beaconRepository)
        {
            this.beaconRepository = (BeaconRepository)beaconRepository;
        }

        public async Task<List<Beacon>> GetAllBeacons()
        {
            List<Beacon> beacons = await beaconRepository.ListAsync();
            return beacons;
        }

        public async Task<Beacon> GetBeaconById(int beaconId)
        {
            Beacon beacon = await beaconRepository.ReadAsync(beaconId);
            return beacon;
        }

        public async Task<Beacon> CreateBeacon(BeaconBody beaconBody)
        {
            // create beacon
            Beacon newBeacon = new Beacon
            {
                Name = beaconBody.Name,
                LocationId = beaconBody.LocationId,
                Lat = beaconBody.Lat,
                Lng = beaconBody.Lng
            };

            // save to database
            return await beaconRepository.CreateAsync(newBeacon);
        }

        public async Task<Beacon> UpdateBeacon(int beaconId, BeaconBody beaconBody)
        {
            // create beacon
            Beacon updatedBeacon = new Beacon
            {
                Id = beaconId,
                Name = beaconBody.Name,
                LocationId = beaconBody.LocationId,
                Lat = beaconBody.Lat,
                Lng = beaconBody.Lng
            };

            // save to database
            return await beaconRepository.UpdateAsync(updatedBeacon);
        }

        public async Task<bool> DeleteBeacon(int beaconId)
        {
            // delete beacon
            return await beaconRepository.DeleteAsync(beaconId);
        }

        public async Task<bool> LogUser(int userId, int beaconId)
        {
            BeaconLog newBeaconLog = new BeaconLog
            {
                UserId = userId,
                BeaconId = beaconId,
                Timestamp = DateTime.Now
        };

            return await beaconRepository.LogUser(newBeaconLog);
        }

        public async Task<List<BeaconLog>> GetBeaconLogsByUserId(int userId)
        {
            return await beaconRepository.ListAsyncByuserId(userId);
        }

        public async Task<bool> DeleteBeaconLog(int userId)
        {
            // delete beacon
            return await beaconRepository.DeleteBeaconLogAsync(userId);
        }
    }
}
