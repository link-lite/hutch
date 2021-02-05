using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using LinkLite.Data;
using LinkLite.Dto;
using LinkLite.Helpers;

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
                            groupResults.Children!
                                .ConvertAll(ruleResults => ruleResults.Value))
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
                            results.Children!
                                .ConvertAll(groupResults => groupResults.Value))
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

            var person = _db.Person.AsNoTracking()
                .Include(p => p.ConditionOccurrences)
                .Include(p => p.Measurements)
                .Include(p => p.Observations);

            // differ the query inclusion criteria
            // based on value.
            // doing it this way allows EF to produce
            // nice SQL either way round?
            var query = value
                ? person.Where(p =>
                    p.ConditionOccurrences.Select(co => co.ConditionConceptId).Contains(conceptId) ||
                    p.Measurements.Select(co => co.MeasurementConceptId).Contains(conceptId) ||
                    p.Observations.Select(co => co.ObservationConceptId).Contains(conceptId) ||
                    p.GenderConceptId == conceptId ||
                    p.RaceConceptId == conceptId)
                : person.Where(p =>
                    !p.ConditionOccurrences.Select(co => co.ConditionConceptId).Contains(conceptId) ||
                    !p.Measurements.Select(co => co.MeasurementConceptId).Contains(conceptId) ||
                    !p.Observations.Select(co => co.ObservationConceptId).Contains(conceptId) ||
                    p.GenderConceptId != conceptId ||
                    p.RaceConceptId != conceptId);

            // Run the query
            return await query
                .Select(p => p.Id)
                .ToListAsync();
        }
    }
}
