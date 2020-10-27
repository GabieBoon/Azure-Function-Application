using Microsoft.EntityFrameworkCore;
using SkillsGardenApi.Models;

namespace SkillsGardenApi.Repositories.Context
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<Location> Locations { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Registration> EventRegistrations { get; set; }

        public DbSet<Component> Components { get; set; }

        public DbSet<ComponentExercise> ComponentExercises { get; set; }

        public DbSet<Exercise> Exercises { get; set; }

        public DbSet<ExerciseRequirement> ExerciseRequirements { get; set; }

        public DbSet<ExerciseStep> ExerciseSteps { get; set; }

        public DbSet<ExerciseForm> ExerciseForms { get; set; }

        public DbSet<Workout> Workouts { get; set; }

        public DbSet<WorkoutExercise> WorkoutExercises { get; set; }

        public DbSet<Beacon> Beacons { get; set; }

        public DbSet<BeaconLog> BeaconLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Locations
            modelBuilder.Entity<Location>()
                .HasMany(e => e.Events)
                .WithOne(r => r.Location);

            // Events
            modelBuilder.Entity<Event>()
                .HasMany(e => e.EventRegistrations)
                .WithOne(r => r.Event);

            // Components
            modelBuilder.Entity<Location>()
                .HasMany(l => l.Components)
                .WithOne(c => c.Location);

            // Exercises
            modelBuilder.Entity<ExerciseStep>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Exercise>()
                .HasMany(e => e.ExerciseRequirements)
                .WithOne(r => r.Exercise);

            modelBuilder.Entity<Exercise>()
                .HasMany(e => e.ExerciseSteps)
                .WithOne(s => s.Exercise);

            modelBuilder.Entity<Exercise>()
                .HasMany(e => e.ExerciseForms)
                .WithOne(f => f.Exercise);

            // Workouts
            modelBuilder.Entity<Workout>()
                .HasMany(w => w.Exercises)
                .WithOne(e => e.Workout);

            // Beacons
            modelBuilder.Entity<Beacon>()
                .HasMany(b => b.BeaconLogs)
                .WithOne(l => l.Beacon);
        }
    }
}