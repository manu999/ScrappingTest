using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("FullTemporaryStopRegulationActions")]
    public class FullTemporaryStopRegulationAction
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long fullTemporaryStopRegulation_hjid { get; set; }
        public string stoppedRegulationId { get; set; }
        public string stoppedRegulationRole { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public FullTemporaryStopRegulationAction()
        {
        }

        public FullTemporaryStopRegulationAction(Scraping.ftsRegulationAction obj, long? FullTemporaryStopRegulation_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            fullTemporaryStopRegulation_hjid = FullTemporaryStopRegulation_hjid ?? 0;
            stoppedRegulationId = obj.stoppedRegulationId;
            stoppedRegulationRole = obj.stoppedRegulationRole;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.ftsRegulationAction obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            stoppedRegulationId = obj.stoppedRegulationId;
            stoppedRegulationRole = obj.stoppedRegulationRole;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
