using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkillsGardenApi.Services
{
    public class LocationService
    {
        private LocationRepository locationRepository;
        private IAzureService azureService;

        public LocationService(IDatabaseRepository<Location> locationRepository, IAzureService azureService)
        {
            this.locationRepository = (LocationRepository)locationRepository;
            this.azureService = azureService;
        }

        public async Task<List<Location>> GetLocations()
        {
            // get all locations
            List<Location> locations = await locationRepository.ListAsync();

            // get a SAS url for each location image
            foreach (Location location in locations)
            {
                string SASurl = azureService.getBlobSas(location.Image);
                location.Image = SASurl;
            }

            return locations;
        }

        public async Task<Location> GetLocation(int locationId)
        {
            // get location
            Location location = await locationRepository.ReadAsync(locationId);

            // if the location does not exist
            if (location == null)
                return null;

            // get a SAS token for the image
            string SASurl = azureService.getBlobSas(location.Image);
            location.Image = SASurl;

            return location;
        }

        public async Task<int> CreateLocation(LocationBody locationBody)
        {
            // save image to blob
            string imageName = await azureService.saveImageToBlobStorage(locationBody.Image);

            // create new location
            Location newLocation = new Location
            {
                Name = locationBody.Name,
                City = locationBody.City,
                Lat = locationBody.Lat,
                Lng = locationBody.Lng,
                Image = imageName
            };

            // save location to database
            await locationRepository.CreateAsync(newLocation);

            return newLocation.Id;
        }

        public async Task<Location> UpdateLocation(LocationBody locationBody, int locationId)
        {
            Location oldLocation = await locationRepository.ReadAsync(locationId);

            // create location
            Location location = new Location
            {
                Id = locationId,
                Name = locationBody.Name,
                City = locationBody.City,
                Lat = locationBody.Lat,
                Lng = locationBody.Lng
            };

            if (locationBody.Image != null)
            {
                // save image to blob
                string imageName = await azureService.saveImageToBlobStorage(locationBody.Image);
                location.Image = imageName;

                // delete old image
                azureService.deleteImageFromBlobStorage(oldLocation.Image);
            }

            // save location to database
            Location updatedLocation = await locationRepository.UpdateAsync(location);

            return updatedLocation;
        }

        public async Task<bool> DeleteLocation(int locationId)
        {
            // get the location
            Location location = await locationRepository.ReadAsync(locationId);

            // if location was not found
            if (location == null)
                return false;

            // delete image from blob storage
            azureService.deleteImageFromBlobStorage(location.Image);

            // delete location from database
            return await locationRepository.DeleteAsync(locationId);
        }

        public async Task<bool> Exists(int locationId)
        {
            return await locationRepository.LocationExists(locationId);
        }
    }
}
