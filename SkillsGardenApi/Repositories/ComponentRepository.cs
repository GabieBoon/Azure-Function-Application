using Microsoft.EntityFrameworkCore;
using SkillsGardenApi.Models;
using SkillsGardenApi.Repositories.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillsGardenApi.Repositories
{
    public class ComponentRepository : IDatabaseRepository<Component>
    {
        private readonly DatabaseContext ctx;

        public ComponentRepository(DatabaseContext ctx)
        {
            this.ctx = ctx;
        }

        /**
         * Create
         */
        public async Task<Component> CreateAsync(Component component)
        {
            ctx.Components.Add(component);
            await ctx.SaveChangesAsync();
            return component;
        }

        /**
         * Delete
         */
        public async Task<bool> DeleteAsync(int id)
        {
            Component component = await ReadAsync(id);
            if (component == null)
            {
                return false;
            }
            ctx.Components.Remove(component);
            await ctx.SaveChangesAsync();
            return true;
        }

        /**
         * List
         */
        public async Task<List<Component>> ListAsync()
        {
            return await ctx.Components.ToListAsync();
        }

        /**
         * Read
         */
        public async Task<Component> ReadAsync(int componentId)
        {
            return await ctx.Components
                .Include(c => c.ComponentExercises)
                .Where(c => c.Id == componentId)
                .FirstOrDefaultAsync();
        }

        /**
         * Update
         */
        public async Task<Component> UpdateAsync(Component component)
        {
            Component componentToBeUpdated = await ctx.Components.Where(x => x.Id == component.Id).FirstOrDefaultAsync();
            if (componentToBeUpdated == null || component == null)
            {
                return null;
            }

            if (component.Name != null) componentToBeUpdated.Name = component.Name;
            if (component.Description != null) componentToBeUpdated.Description = component.Description;
            if (component.Image != null) componentToBeUpdated.Image = component.Image;
            if (component.ComponentExercises != null) componentToBeUpdated.ComponentExercises = component.ComponentExercises;

            await ctx.SaveChangesAsync();
            return componentToBeUpdated;
        }

        /**
         * Exists
         */
        public async Task<bool> ComponentExists(int componentId)
        {
            return await ctx.Components.Where(x => x.Id == componentId).FirstOrDefaultAsync() != null;
        }
    }
}
