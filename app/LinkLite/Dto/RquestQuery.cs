using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LinkLite.Dto
{
    public class RquestQuery
    {
        [JsonPropertyName("groups_oper")]
        public string Combinator { get; set; } = string.Empty;

        [JsonPropertyName("groups")]
        public List<RquestQueryGroup> Groups { get; set; } = new ();
    }

    public class RquestQueryGroup
    {
        [JsonPropertyName("rules_oper")]
        public string Combinator { get; set; } = string.Empty;

        [JsonPropertyName("rules")]
        public List<RquestQueryRule> Rules { get; set; } = new ();

        /// <summary>
        /// TODO: Not sure what this is for, since Rules Operand `!=` should have the same effect?
        /// Unless it's the group equivalent?
        /// </summary>
        [JsonPropertyName("exclude")]
        public bool Exclude { get; set; }
    }

    public class RquestQueryRule
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("varname")]
        public string VariableName { get; set; } = string.Empty;

        /// <summary>
        /// Note that this isn't really an operand on the value
        /// it's pure inclusion or exclusion criteria: `=` or `!=`
        /// </summary>
        [JsonPropertyName("oper")]
        public string Operand { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("ext")]
        public string ExternalAttribute { get; set; } = string.Empty;

        /// <summary>
        /// Reserved for Future Use with some types (e.g. NUMERIC)
        /// 
        /// </summary>
        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// TEXT type only; the string to be uased for matching
        /// in a SQL `LIKE` expression (not, confusingly, a RegEx)
        /// </summary>
        [JsonPropertyName("regex")]
        public string RegEx { get; set; } = string.Empty;
    }
}
