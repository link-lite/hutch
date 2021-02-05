using LinkLite.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace LinkLite.Data
{
    public class OmopContext : DbContext
    {
        public OmopContext(DbContextOptions<OmopContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Person { get; set; }

        public DbSet<ConditionOccurrence> ConditionOccurrence { get; set; }
        public DbSet<Measurement> Measurement { get; set; }
        public DbSet<Observation> Observation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>()
                .HasMany(p => p.ConditionOccurrences)
                .WithOne(co => co.Person)
                .HasForeignKey("PersonId");
            modelBuilder.Entity<Person>()
                .HasMany(p => p.Observations)
                .WithOne(o => o.Person)
                .HasForeignKey("PersonId");
            modelBuilder.Entity<Person>()
                .HasMany(p => p.Measurements)
                .WithOne(m => m.Person)
                .HasForeignKey("PersonId");

        }
    }
}
