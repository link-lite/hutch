using LinkLite.Data;
using LinkLite.Data.Entities;
using LinkLite.Dto;
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
                new() { Id = 1, ConceptId = 1 }
            });
            people[1].Measurements.AddRange(new List<Measurement>
            {
                new() { Id = 1, ConceptId = 1, ValueAsNumber = 1.5 }
            });
            people[2].Observations.AddRange(new List<Observation>
            {
                new() { Id = 1, ConceptId = 1, ValueAsNumber = 2.5 }
            });
            db.Person.AddRange(people);

            db.SaveChanges();
        }

        [Fact]
        public async void BooleanRule()
        {
            var expected = new[] { 1, 2, 3 };

            var rule = new RquestQueryRule
            {
                Type = RuleTypes.Boolean,
                Value = true.ToString(),
                VariableName = "OMOP:1"
            };
            var actual = await _sut.BooleanHandler(rule);

            Assert.Collection(actual,
                x => Assert.StrictEqual(expected[0], x),
                x => Assert.StrictEqual(expected[1], x),
                x => Assert.StrictEqual(expected[2], x));
        }

        [Theory]
        [MemberData(nameof(NumericRuleTestData))]
        public async void NumericRule(string ruleValue, List<int> expected)
        {
            var rule = new RquestQueryRule
            {
                Type = RuleTypes.Numeric,
                Value = ruleValue,
                VariableName = "OMOP:1"
            };

            var actual = await _sut.NumericHandler(rule);
            Assert.StrictEqual(expected.Count, actual.Count);
            Assert.All(actual, item => expected.Contains(item));
        }

        public static List<object[]> NumericRuleTestData =>
        new()
        {
            new object[]
            {
                "|",
                new List<int> { 2, 3 }
            },
            new object[]
            {
                "|2.0",
                new List<int> { 2 }
            },
            new object[]
            {
                "2.0|",
                new List<int> { 3 }
            },
            new object[]
            {
                "1.2|2.7",
                new List<int> { 2, 3 }
            },
            new object[]
            {
                "1.8|2.3",
                new List<int>()
            }
        };

        public void Dispose()
        {
            _testContext.Dispose();
        }
    }
}
