using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Descartes.CDS.TariffFileProcessor.DBobjects
{
    [Table("MeasureTypeDescriptions")]
    public class MeasureTypeDescription
    {
        [Key]
        public long key { get; set; }

        [Index]
        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long oid { get; set; }
        public long sid { get; set; }
        public long measureType_hjid { get; set; }
        public string description { get; set; }
        public long languages_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }
    }
}
