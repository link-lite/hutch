namespace LinkLite.Services.QueryServices
{
    /// <summary>
    /// Reusable parameterised error messages
    /// </summary>
    public static class ErrorMessages
    {
        public static string RuleTypeMismatch(string expected, string actual)
            => "Rule processing type mismatch: " +
               $"Expected {expected} but got {actual}.";
    }

    // Keep reference constants for Rule property values
    public static class RuleTypes
    {
        public const string Boolean = "BOOLEAN";
        public const string Numeric = "NUMERIC";
        public const string Alternative = "ALTERNATIVE";
        public const string Text = "TEXT";
    }

    public static class RuleOperands
    {
        public const string Include = "=";
        public const string Exclude = "!=";
    }

    public static class QueryCombinators
    {
        public const string And = "AND";
        public const string Or = "OR";
    }
}
