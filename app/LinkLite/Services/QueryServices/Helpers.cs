using System;
using System.Linq;

namespace LinkLite.Services.QueryServices
{
    public abstract class Helpers
    {
        /// <summary>
        /// Parse a Rule's Variable Name into an OMOP Concept ID.
        /// Assumes always OMOP for now
        /// </summary>
        /// <param name="variableName">The VariableName property of Query Rule, e.g. "OMOP:123456"</param>
        /// <returns>The OMOP Concept Integer ID</returns>
        public static int ParseVariableName(string variableName)
            => int.Parse(variableName.Replace("OMOP:", ""));

        /// <summary>
        /// Parse a NUMERIC Rule's value into the range bounds it describes.
        /// </summary>
        /// <param name="value">The Rule value in the form "double?|double?"</param>
        /// <returns>A Tuple of (min, max) where each value is optional</returns>
        public static (double? min, double? max) ParseNumericRange(string value)
        {
            var values = value.Split("|");

            if (values.Length != 2)
                throw new ArgumentException(
                    $"Invalid value: {value}", nameof(value));

            var results = values
                .Select<string, double?>(v =>
                     string.IsNullOrWhiteSpace(v)
                     ? null
                     : double.Parse(v))
                .ToList();

            return (results[0], results[1]);
        }
    }
}
