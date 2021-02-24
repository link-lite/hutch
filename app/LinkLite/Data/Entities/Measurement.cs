using System.ComponentModel.DataAnnotations.Schema;

namespace LinkLite.Data.Entities
{
    [Table("measurement")]
    public class Measurement : IConcept, INumberValue
    {
        [Column("measurement_id")]
        public int Id { get; set; }

        [Column("person_id")]
        public int PersonId { get; set; }

        [Column("measurement_concept_id")]
        public int ConceptId { get; set; }

        [Column("value_as_number")]
        public double? ValueAsNumber { get; set; }

        public Person Person { get; set; } = new();
    }
}
