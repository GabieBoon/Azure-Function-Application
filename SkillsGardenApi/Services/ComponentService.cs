 using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenDTO;
using SkillsGardenDTO.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Services
{
    public class ComponentService
    {
        private ComponentRepository componentRepository;
        private LocationRepository locationRepository;
        private IAzureService azureService;

        public ComponentService(IDatabaseRepository<Component> componentRepository, IDatabaseRepository<Location> locationRepository, IAzureService azureService)
        {
            this.componentRepository = (ComponentRepository)componentRepository;
            this.locationRepository = (LocationRepository)locationRepository;
            this.azureService = azureService;
        }

        public async Task<List<Component>> GetComponentsByLocation(int locationId)
        {
            // get all components within location
            Location location = await locationRepository.ReadAsync(locationId);
            List<Component> components = location.Components.ToList();

            // get SAS token
            foreach (Component component in components)
            {
                string SASurl = azureService.getBlobSas(component.Image);
                component.Image = SASurl;
            }

            return components;
        }

        public async Task<ComponentResponse> GetComponent(int locationId, int componentId)
        {
            // get the component
            Component component = await componentRepository.ReadAsync(componentId);

            // if the component was not found within the location
            if (component == null || component.LocationId != locationId)
                return null;

            // get the SAS token
            string SASurl = azureService.getBlobSas(component.Image);
            component.Image = SASurl;

            // create response
            ComponentResponse response = new ComponentResponse
            {
                Id = component.Id,
                Name = component.Name,
                Description = component.Description,
                Image = component.Image,
                Exercises = component.ComponentExercises.Select(e => e.ExerciseId).ToList()
            };

            return response;
        }

        public async Task<int> CreateComponent(ComponentBody componentBody, int locationId)
        {
            // save image to blob
            string url = await azureService.saveImageToBlobStorage(componentBody.Image);

            // create the component exercises
            List<ComponentExercise> componentExercises = new List<ComponentExercise>();
            foreach (int exerciseId in componentBody.Exercises)
            {
                componentExercises.Add(new ComponentExercise
                {
                    ExerciseId = exerciseId
                });
            }

            // create new component
            Component newComponent = new Component
            {
                Name = componentBody.Name,
                Description = componentBody.Description,
                Image = url,
                LocationId = locationId,
                ComponentExercises = componentExercises
            };

            // save component to database
            newComponent = await componentRepository.CreateAsync(newComponent);

            return newComponent.Id;
        }

        public async Task<Component> UpdateComponent(ComponentBody componentBody, int locationId, int componentId)
        {
            // get the current component
            Component oldComponent = await componentRepository.ReadAsync(componentId);

            // if component is not in location
            if (oldComponent == null || oldComponent.LocationId != locationId)
                return null;

            // create updated component
            Component updateComponent = new Component
            {
                Id = componentId,
                Name = componentBody.Name,
                Description = componentBody.Description,
            };

            if (componentBody.Exercises != null)
            {
                // create the component exercises
                List<ComponentExercise> componentExercises = new List<ComponentExercise>();
                foreach (int exerciseId in componentBody.Exercises)
                {
                    componentExercises.Add(new ComponentExercise
                    {
                        ExerciseId = exerciseId
                    });
                }
                updateComponent.ComponentExercises = componentExercises;
            }

            if (componentBody.Image != null)
            {
                // save image to blob
                string url = await azureService.saveImageToBlobStorage(componentBody.Image);
                updateComponent.Image = url;

                // delete old image
                azureService.deleteImageFromBlobStorage(oldComponent.Image);
            }

            // save component to database
            Component updatedComponent = await componentRepository.UpdateAsync(updateComponent);

            return updatedComponent;
        }

        public async Task<bool> DeleteComponent(int componentId)
        {
            // get the component
            Component component = await componentRepository.ReadAsync(componentId);

            // if component was not found
            if (component == null)
                return false;

            // delete the image from the blob storage
            azureService.deleteImageFromBlobStorage(component.Image);

            // delete the component
            return await componentRepository.DeleteAsync(componentId);
        }

        public async Task<bool> Exists(int locationId, int componentId)
        {
            Component component = await componentRepository.ReadAsync(componentId);

            if (component == null || component.LocationId != locationId)
                return false;

            return true;
        }
    }
}
