namespace LinkLite.Services.QueryServices
{
    public abstract class Helpers
    {
        /// <summary>
        /// Parse a Rule's Variable Name into an OMOP Concept ID.
        /// TODO: Assumes always OMOP for now
        /// </summary>
        /// <param name="variableName">The VariableName property of Query Rule, e.g. "OMOP:123456"</param>
        /// <returns>The OMOP Concept Integer ID</returns>
        public static int ParseVariableName(string variableName)
            => int.Parse(variableName.Replace("OMOP:", ""));
    }
}
