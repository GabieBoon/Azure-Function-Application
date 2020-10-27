using Microsoft.EntityFrameworkCore;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Repositories
{
    public class EventRepository : IDatabaseRepository<Event>
    {
        private readonly DatabaseContext ctx;

        public EventRepository(DatabaseContext ctx)
        {
            this.ctx = ctx;
        }

        /**
         * Create
         */
        // create new event
        public async Task<Event> CreateAsync(Event item)
        {
            ctx.Events.Add(item);
            await ctx.SaveChangesAsync();
            return item;
        }

        // create new registration
        public async Task<Registration> CreateRegistrationForEvent(Registration newRegistration)
        {
            ctx.EventRegistrations.Add(newRegistration);
            
            await ctx.SaveChangesAsync();
            return newRegistration;
        }

        /**
         * Delete
         */
        // delete event
        public async Task<bool> DeleteAsync(int eventId)
        {
            Event item = await ReadAsync(eventId);

            // if event does not exist
            if (item == null)
                return false;

            // delete all registrations for this event
            ctx.EventRegistrations.RemoveRange(ctx.EventRegistrations.Where(x => x.Event == item));

            // delete event
            ctx.Events.Remove(item);

            await ctx.SaveChangesAsync();
            return true;
        }

        // delete registration
        public async Task<bool> DeleteRegistrationForEvent(int eventId, int userId)
        {
            // get the registration
            Registration registrationCheck = await ctx.EventRegistrations
                .Where(x => x.UserId == userId)
                .Where(x => x.Event.Id == eventId)
                .FirstOrDefaultAsync();

            // check if the registration exists
            if (registrationCheck == null)
                return false;

            // delete the event registration
            ctx.EventRegistrations.Remove(registrationCheck);
            
            await ctx.SaveChangesAsync();
            return true;
        }

        /**
         * List
         */
        // get all events
        public async Task<List<Event>> ListAsync()
        {
            return await ctx.Events.ToListAsync();
        }

        // get events by location
        public async Task<List<Event>> ListEventsByLocation(int locationId)
        {
            return await ctx.Events
                .Include(e => e.EventRegistrations)
                .Where(e => e.Location.Id == locationId)
                .ToListAsync();
        }

        /**
         * Read
         */
        // get one event by id
        public async Task<Event> ReadAsync(int eventId)
        {
            return await ctx.Events
                .Include(e => e.EventRegistrations)
                .Where(e => e.Id == eventId)
                .FirstOrDefaultAsync();
        }

        // get events by user id
        public async Task<List<Registration>> ReadAsyncByUserId(int userId)
        {
            return await ctx.EventRegistrations
                .Include(e => e.Event)
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        /*
         * Update
         */
        // update event
        public async Task<Event> UpdateAsync(Event item)
        {
            // get the event
            Event eventToBeUpdated = await ctx.Events
                .Where(x => x.Id == item.Id)
                .FirstOrDefaultAsync();

            // check if the event exists
            if (eventToBeUpdated == null || item == null)
                return null;

            // update only if given
            if (item.Title != null) eventToBeUpdated.Title = item.Title;
            if (item.Description != null) eventToBeUpdated.Description = item.Description;
            if (item.StartTime != null) eventToBeUpdated.StartTime = item.StartTime;
            if (item.MaxRegistrations != null) eventToBeUpdated.MaxRegistrations = item.MaxRegistrations;
            if (item.EventRegistrations != null) eventToBeUpdated.EventRegistrations = item.EventRegistrations;
            if (item.Image != null) eventToBeUpdated.Image = item.Image;

            await ctx.SaveChangesAsync();
            return eventToBeUpdated;
        }

        /**
         * Exists
         */
        // check if event exists
        public async Task<bool> EventExists(int eventId)
        {
            return await ctx.Events.FirstOrDefaultAsync(x => x.Id == eventId) != null;
        }

        // check if event exists within location
        public async Task<bool> EventExists(int locationId, int eventId)
        {
            return await ctx.Events
                .Where(e => e.Location.Id == locationId)
                .Where(e => e.Id == eventId)
                .FirstOrDefaultAsync() != null;
        }

        // check if registration exists
        public async Task<bool> RegistrationExists(int eventId, int userId)
        {
            return await ctx.EventRegistrations
                .Where(x => x.UserId == userId)
                .Where(x => x.Event.Id == eventId)
                .FirstOrDefaultAsync() != null;
        }
    }
}
