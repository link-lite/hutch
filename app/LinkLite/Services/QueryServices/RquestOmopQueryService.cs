using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using LinkLite.Data;
using LinkLite.Data.Entities;
using LinkLite.Dto;
using LinkLite.Helpers;

using LinqKit;

using Microsoft.EntityFrameworkCore;

namespace LinkLite.Services.QueryServices
{
    /// <summary>
    /// A service for running Rquest queries against an OMOP CDM database
    /// </summary>
    public class RquestOmopQueryService
    {
        private readonly OmopContext _db;

        public RquestOmopQueryService(OmopContext db)
        {
            _db = db;
        }

        public async Task<int> Process(RquestQuery query)
        {
            TreeNode<List<int>> results = new();

            List<Exception> exceptions = new();

            // run a db query for each individual rule
            // and store the results hierarchically in a tree
            for (var iGroup = 0;
                iGroup < query.Groups.Count;
                iGroup++)
            {
                var group = query.Groups[iGroup];
                var groupResults = results.Add(new());

                for (var iRule = 0;
                    iRule < group.Rules.Count;
                    iRule++)
                {
                    try
                    {
                        var rule = group.Rules[iRule];
                        var result = rule.Type switch
                        {
                            RuleTypes.Boolean => await BooleanHandler(rule),
                            RuleTypes.Numeric => await NumericHandler(rule),
                            _ => throw new ArgumentException($"Unknown Rule Type: {rule.Type}")
                        };
                        groupResults.Add(new() { Value = result });
                    }
                    catch (Exception e) { exceptions.Add(e); }
                }
            }

            // any errors running db queries?
            // TODO: should we early exit at first error instead?
            if (exceptions.Count > 0)
                throw new AggregateException(
                    "Errors occurred processing the query",
                    exceptions);

            // Combine rule results into group results
            for (var iGroup = 0; iGroup < query.Groups.Count; iGroup++)
            {
                var group = query.Groups[iGroup];
                var groupResults = results.Children[iGroup];

                if (group.Rules.Count > 1)
                {
                    groupResults.Value = Combine(
                            group.Combinator,
                            groupResults.Children
                                .ConvertAll(ruleResults => ruleResults.Value!))
                        .ToList();
                }
                else
                {
                    groupResults.Value = groupResults.Children
                        .SingleOrDefault()?.Value
                        ?? new();
                }
            }

            // Combine group results into query result
            if (query.Groups.Count > 1)
            {
                results.Value = Combine(
                            query.Combinator,
                            results.Children
                                .ConvertAll(groupResults => groupResults.Value!))
                        .ToList();
            }
            else
            {
                results.Value = results.Children
                    .SingleOrDefault()?.Value
                    ?? new();
            }

            //return query results count
            return results.Value.Count;
        }

        public static HashSet<T> Combine<T>(string combinator, List<List<T>> integrants)
             where T : notnull
            => Combine(combinator, integrants, x => x);

        public static HashSet<TEntry> Combine<TEntry, TKey>(string combinator, List<List<TEntry>> integrants,
            Expression<Func<TEntry, TKey>> keySelector)
            where TKey : notnull
        {
            Func<TEntry, TKey> keyAccessor = keySelector.Compile();

            // keys = unique entries
            // values = the entry itself AND indices of lists in which the entry appears
            Dictionary<TKey, (TEntry entry, HashSet<int> integrants)> entries = new();

            // loop one time through all the lists to log which ones a given entry appears in
            for (var i = 0; i < integrants.Count; i++)
                foreach (var entry in integrants[i])
                {
                    var key = keyAccessor(entry);
                    if (!entries.ContainsKey(key))
                        entries[key] = (entry, integrants: new());
                    entries[key].integrants.Add(i);
                }

            return combinator switch
            {
                // filter the entries by those which appear in ALL lists
                QueryCombinators.And =>
                    entries.Keys
                        .Where(key => entries[key].integrants.Count == integrants.Count)
                        .Select(key => entries[key].entry)
                        .ToHashSet(),

                // return the unique set of entries
                QueryCombinators.Or => entries.Keys
                    .Select(key => entries[key].entry)
                    .ToHashSet(),

                _ => throw new ArgumentException($"Unexpected Combinator: {combinator}")
            };
        }

        public async Task<List<int>> BooleanHandler(RquestQueryRule rule)
        {
            // boolean doesn't require operand, it defaults to "="
            // and the bool value can be used to effect inclusion or exclusion
            var value = bool.Parse(rule.Value);
            if (rule.Operand == RuleOperands.Exclude)
                value = !value;

            var conceptId = Helpers.ParseVariableName(rule.VariableName);

            // Inclusion criteria
            var match = PredicateBuilder
                .New<Person>(p => p.GenderConceptId == conceptId)
                .Or(p => p.RaceConceptId == conceptId)
                .Or(p => p.Measurements.Any(x => x.ConceptId == conceptId))
                .Or(p => p.Observations.Any(x => x.ConceptId == conceptId))
                .Or(p => p.ConditionOccurrences.Any(x => x.ConceptId == conceptId));

            // Build the query
            var person = _db.Person.AsNoTracking();
            var query = value
                ? person.Where(match)
                : person.Where(match.Not());

            // Run the query
            return await query
                .Select(p => p.Id)
                .ToListAsync();
        }

        private Expression<Func<T, bool>> NumericMatch<T>(int conceptId, double? min, double? max)
            where T : IConcept, INumberValue
        {
            var p = PredicateBuilder
                .New<T>(x => x.ConceptId == conceptId);

            // no range is valid, and ultimately works the same as BOOLEAN
            if (min is null && max is null) return p;

            if (min is null) p = p.And(x => x.ValueAsNumber < max);
            else if (max is null) p = p.And(x => x.ValueAsNumber > min);
            else p = p.And(x => x.ValueAsNumber >= min && x.ValueAsNumber <= max);

            return p;
        }

        public async Task<List<int>> NumericHandler(RquestQueryRule rule)
        {
            var conceptId = Helpers.ParseVariableName(rule.VariableName);
            var (min, max) = Helpers.ParseNumericRange(rule.Value);

            // Inclusion criteria
            var match = PredicateBuilder
                .New<Person>(p => p.Measurements.AsQueryable()
                    .Any(NumericMatch<Measurement>(conceptId, min, max)))
                .Or(p => p.Observations.AsQueryable()
                    .Any(NumericMatch<Observation>(conceptId, min, max)));

            // Build the query
            var person = _db.Person.AsNoTracking();
            var query = rule.Operand switch
            {
                RuleOperands.Exclude => person.Where(match.Not()),
                _ => person.Where(match) // For now we default empty or invalid operand to "include"
            };

            // Run the query
            return await query
                .Select(p => p.Id)
                .ToListAsync();
        }
    }
}
