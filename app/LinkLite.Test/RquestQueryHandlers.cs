using LinkLite.Data;
using LinkLite.Data.Entities;
using LinkLite.Services.QueryServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Xunit;

namespace LinkLite.Test
{
    public class RquestQueryHandlers : IDisposable
    {
        private readonly RquestOmopQueryService _sut;
        private readonly OmopContext _testContext;

        protected DbContextOptions<OmopContext> ContextOptions { get; }

        public RquestQueryHandlers()
        {
            ContextOptions = new DbContextOptionsBuilder<OmopContext>()
                .UseInMemoryDatabase("db")
                .Options;

            Seed();

            _testContext = new OmopContext(ContextOptions);
            _sut = new(_testContext);
        }

        private void Seed()
        {
            using var db = new OmopContext(ContextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // TODO: Seed for tests
            List<Person> people = new()
            {
                new() { Id = 1 },
                new() { Id = 2 },
                new() { Id = 3 },
                new() { Id = 4 },
                new() { Id = 5 },
            };
            people[0].ConditionOccurrences.AddRange(new List<ConditionOccurrence>
            {
                new() { Id = 1, ConditionConceptId = 12345 }
            });
            people[1].Measurements.AddRange(new List<Measurement>
            {
                new() { Id = 1, MeasurementConceptId = 12345}
            });
            people[2].Observations.AddRange(new List<Observation>
            {
                new() { Id = 1, ObservationConceptId = 12345}
            });
            db.Person.AddRange(people);

            db.SaveChanges();
        }

        [Fact]
        public async void BooleanRule()
        {
            var expected = new[] { 1, 2, 3 };

            var rule = new Dto.RquestQueryRule
            {
                Type = RuleTypes.Boolean,
                Value = true.ToString(),
                VariableName = "OMOP:12345"
            };
            var result = await _sut.BooleanHandler(rule);

            Assert.Collection(result,
                x => Assert.StrictEqual(expected[0], x),
                x => Assert.StrictEqual(expected[1], x),
                x => Assert.StrictEqual(expected[2], x));
        }

        public void Dispose()
        {
            _testContext.Dispose();
        }
    }
}
