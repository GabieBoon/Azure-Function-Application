using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenDTO;
using SkillsGardenDTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Services
{
    public class EventService
    {
        private EventRepository eventRepository;
        private UserRepository userRepository;
        private IAzureService azureService;

        public EventService(IDatabaseRepository<Event> eventRepository, IDatabaseRepository<User> userRepository, IAzureService azureService)
        {
            this.eventRepository = (EventRepository)eventRepository;
            this.userRepository = (UserRepository)userRepository;
            this.azureService = azureService;
        }

        public async Task<List<EventResponse>> GetEventsByLocation(int locationId, bool all = false)
        {
            // get all events within location
            List<Event> events = await eventRepository.ListEventsByLocation(locationId);

            List<EventResponse> responses = new List<EventResponse>();
            foreach (Event item in events)
            {
                // if event is in past
                if (item.StartTime < DateTime.Now && !all)
                    continue;

                // get the organiser
                User user = await userRepository.ReadAsync(item.OrganisorId);
                // get the organiser name
                string organiserName = user != null ? user.Name : "";

                // get SAS token for the image
                string SASurl = azureService.getBlobSas(item.Image);

                // create response
                responses.Add(new EventResponse
                {
                    Id = item.Id,
                    Title = item.Title,
                    Organisor = organiserName,
                    Description = item.Description,
                    StartTime = item.StartTime,
                    MaxRegistrations = item.MaxRegistrations,
                    Image = SASurl,
                    Registrations = item.EventRegistrations.Select(r => r.UserId.ToString()).Count()
                });
            }

            return responses;
        }

        public async Task<EventResponse> GetEvent(int locationId, int eventId)
        {
            // get event
            Event item = await eventRepository.ReadAsync(eventId);

            // if the event was not found within location
            if (item == null || item.LocationId != locationId)
                return null;

            // get the name of the organiser
            User user = await userRepository.ReadAsync(item.OrganisorId);
            string organiserName = user != null ? user.Name : "";

            // create SAS token for image
            string SASurl = azureService.getBlobSas(item.Image);

            // create event response
            EventResponse response = new EventResponse
            {
                Id = item.Id,
                Title = item.Title,
                Organisor = organiserName,
                Description = item.Description,
                StartTime = item.StartTime,
                MaxRegistrations = item.MaxRegistrations,
                Image = SASurl,
                Registrations = item.EventRegistrations.Select(r => r.UserId.ToString()).Count()
            };

            return response;
        }

        public async Task<int> GetOrganiser(int eventId)
        {
            Event item = await eventRepository.ReadAsync(eventId);
            return item.OrganisorId;
        }

        public async Task<List<EventResponse>> GetUserRegisteredEvents(int userId)
        {
            // get the registrations
            List<Registration> registrations = await eventRepository.ReadAsyncByUserId(userId);

            // get the events the user registered for
            List<EventResponse> registeredEvents = new List<EventResponse>();
            foreach (Registration registration in registrations)
            {
                // get the event
                Event item = registration.Event;

                // get the name of the organiser
                User user = await userRepository.ReadAsync(item.OrganisorId);
                string organiserName = user != null ? user.Name : "";

                // create SAS token for image
                string SASurl = azureService.getBlobSas(item.Image);

                // create the response
                EventResponse response = new EventResponse
                {
                    Id = item.Id,
                    Title = item.Title,
                    Organisor = organiserName,
                    Description = item.Description,
                    StartTime = item.StartTime,
                    MaxRegistrations = item.MaxRegistrations,
                    Image = SASurl,
                    Registrations = item.EventRegistrations.Select(r => r.UserId.ToString()).Count()
                };

                registeredEvents.Add(response);
            }

            return registeredEvents;
        }

        public async Task<int> CreateEvent(EventBody eventBody, int locationId, int organiserId)
        {
            // save image to blob
            string imageName = await azureService.saveImageToBlobStorage(eventBody.Image);

            // create new event
            Event newEvent = new Event
            {
                Title = eventBody.Title,
                Description = eventBody.Description,
                StartTime = eventBody.StartTime,
                OrganisorId = organiserId,
                Image = imageName,
                MaxRegistrations = eventBody.MaxRegistrations,
                LocationId = locationId
            };

            // save event
            await eventRepository.CreateAsync(newEvent);

            return newEvent.Id;
        }

        public async Task<bool> CreateRegistrationForEvent(int eventId, int userId)
        {
            Event item = await eventRepository.ReadAsync(eventId);

            // if max number of registrations has been reached
            if (item.EventRegistrations.Count() >= item.MaxRegistrations)
                return false;

            // create new registration
            Registration newRegistration = new Registration
            {
                UserId = userId,
                EventId = eventId
            };

            // save registration
            await eventRepository.CreateRegistrationForEvent(newRegistration);

            return true;
        }

        public async Task<Event> UpdateEvent(EventBody eventBody, int locationId, int eventId)
        {
            // get the current event
            Event oldEvent = await eventRepository.ReadAsync(eventId);

            // if the event was not found within location
            if (oldEvent.LocationId != locationId)
                return null;

            // create updated event
            Event updateEvent = new Event
            {
                Id = eventId,
                Title = eventBody.Title,
                Description = eventBody.Description,
                MaxRegistrations = eventBody.MaxRegistrations,
                StartTime = eventBody.StartTime
            };

            if (eventBody.Image != null)
            {
                // save image to blob
                string url = await azureService.saveImageToBlobStorage(eventBody.Image);
                updateEvent.Image = url;

                // delete old image
                azureService.deleteImageFromBlobStorage(oldEvent.Image);
            }

            // save event to database
            Event updatedEvent = await eventRepository.UpdateAsync(updateEvent);

            return updatedEvent;
        }

        public async Task<bool> DeleteEvent(int eventId)
        {
            // get the event
            Event item = await eventRepository.ReadAsync(eventId);

            // if event was not found
            if (item == null)
                return false;

            // delete image from blob storage
            azureService.deleteImageFromBlobStorage(item.Image);

            // delete event from database
            bool isDeleted = await eventRepository.DeleteAsync(item.Id);

            return isDeleted;
        }

        public async Task<bool> DeleteRegistration(int registrationId, int userId)
        {
            // delete registration from database
            return await eventRepository.DeleteRegistrationForEvent(registrationId, userId);
        }

        public async Task<bool> Exists(int locationId, int eventId)
        {
            return await eventRepository.EventExists(locationId, eventId);
        }

        public async Task<bool> RegistrationExists(int eventId, int userId)
        {
            return await eventRepository.RegistrationExists(eventId, userId);
        }
    }
}
