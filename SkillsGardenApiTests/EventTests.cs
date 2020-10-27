using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillsGardenApi.Controllers;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories;
using SkillsGardenApi.Repositories.Context;
using SkillsGardenApi.Services;
using SkillsGardenApiTests.Factory;
using SkillsGardenApiTests.Mock;
using SkillsGardenDTO.Response;
using System.Collections.Generic;
using Xunit;

namespace SkillsGardenApiTests
{
    public class EventTests : UnitTest
    {
        private EventRepository eventRepository;
        private EventService eventService;
        private EventController eventController;

        private LocationService locationService;
        private LocationRepository locationRepository;

        private UserRepository userRepository;
        
        private IAzureService azureService;

        public EventTests()
        {
            DatabaseContext dbContext = CreateEmptyDatabase();

            this.eventRepository = new EventRepository(dbContext);
            this.locationRepository = new LocationRepository(dbContext);
            this.userRepository = new UserRepository(dbContext);

            this.azureService = new AzureServiceMock();
            this.locationService = new LocationService(this.locationRepository, this.azureService);
            this.eventService = new EventService(this.eventRepository, this.userRepository, this.azureService);

            this.eventController = new EventController(this.eventService, this.locationService);
        }

        //Get all events for a specific location
        [Fact]
        public async void GetAllEventsTest()
        {
            //Create 1 location using the locationFactory
            Location locationTest = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            //Create 2 Events using the eventFactory
            Event event1 = await this.eventRepository.CreateAsync(EventFactory.CreateEvent(1, 1, 1));
            Event event2 = await this.eventRepository.CreateAsync(EventFactory.CreateEvent(2, 1, 1));

            var test = await this.eventRepository.ListAsync();

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await eventController.LocationGetEvents(request, locationTest.Id);

            List<EventResponse> events = (List<EventResponse>)result.Value;

            // Status code should return 200 OK
            Assert.Equal(200, result.StatusCode);
            // Count of events should equal 2
            Assert.Equal(2, events.Count);

            //Check if organisorId is the same for both
            Assert.Equal(1, event1.OrganisorId);
            Assert.Equal(1, event2.OrganisorId);
        }

        //Get all events for a specific location
        [Fact]
        public async void GetLocationEvent()
        {
            //Create 1 location using the locationFactory
            Location locationTest = await this.locationRepository.CreateAsync(LocationFactory.CreateLocation(1));

            //Create 2 Events using the eventFactory
            Event event1 = await this.eventRepository.CreateAsync(EventFactory.CreateEvent(1, 1, 1));

            HttpRequest request = HttpRequestFactory.CreateGetRequest();

            ObjectResult result = (ObjectResult)await eventController.LocationGetEvent(request, locationTest.Id, event1.Id);

            EventResponse eventResult = (EventResponse)result.Value;

            // Status code should return 200 OK
            Assert.Equal(200, result.StatusCode);

            //Check if organisorId is the same for both
            //Assert.Equal(1, eventResult.Organisor);
        }
    }
}
