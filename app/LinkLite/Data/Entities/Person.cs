using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkLite.Data.Entities
{
    [Table("person")]
    public class Person
    {
        [Column("person_id")]
        public int Id { get; set; }

        [Column("gender_concept_id")]
        public int GenderConceptId { get; set; }

        [Column("race_concept_id")]
        public int RaceConceptId { get; set; }

        public List<ConditionOccurrence> ConditionOccurrences { get; set; } = new();
        public List<Measurement> Measurements { get; set; } = new();
        public List<Observation> Observations { get; set; } = new();
    }
}
