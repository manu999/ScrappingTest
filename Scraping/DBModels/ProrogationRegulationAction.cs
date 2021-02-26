using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("ProrogationRegulationActions")]
    public class ProrogationRegulationAction
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long prorogationRegulation_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime prorogatedDate { get; set; }

        public string prorogatedRegulationId { get; set; }
        public string prorogatedRegulationRole { get; set; }
        public long baseRegulation_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public ProrogationRegulationAction()
        {
        }

        public ProrogationRegulationAction(Scraping.ProrogationRegulationAction obj, long? ProrogationRegulation_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            prorogatedRegulationId = obj.prorogatedRegulationId;
            prorogatedRegulationRole = obj.prorogatedRegulationRole;
            prorogationRegulation_hjid = ProrogationRegulation_hjid ?? 0;
            baseRegulation_hjid = obj.baseRegulation?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.ProrogationRegulationAction obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            prorogatedRegulationId = obj.prorogatedRegulationId;
            prorogatedRegulationRole = obj.prorogatedRegulationRole;
            baseRegulation_hjid = obj.baseRegulation?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
