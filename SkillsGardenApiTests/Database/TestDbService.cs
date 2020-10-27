using Microsoft.EntityFrameworkCore;
using SkillsGardenApi.Repositories.Context;
using System;

namespace SkillsGardenApiTests.Database
{
    public class TestDbService
    {
        private DatabaseContext context;

        public TestDbService()
        {
            // create in memory database
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            this.context = new DatabaseContext(options);
        }

        public DatabaseContext GetDatabaseContext()
        {
            return this.context;
        }
    }
}
