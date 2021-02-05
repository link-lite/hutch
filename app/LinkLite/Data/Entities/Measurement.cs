using System.ComponentModel.DataAnnotations.Schema;

namespace LinkLite.Data.Entities
{
    [Table("measurement")]
    public class Measurement
    {
        [Column("measurement_id")]
        public int Id { get; set; }

        [Column("person_id")]
        public int PersonId { get; set; }

        [Column("measurement_concept_id")]
        public int MeasurementConceptId { get; set; }

        public virtual Person Person { get; set; } = new();
    }
}
