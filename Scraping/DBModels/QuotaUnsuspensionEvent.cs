using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("QuotaUnsuspensionEvents")]
    public class QuotaUnsuspensionEvent
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long quotaDefinition_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime occurrenceTimestamp { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime unsuspensionDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public QuotaUnsuspensionEvent()
        {
        }

        public QuotaUnsuspensionEvent(quotaUnsuspensionEvent obj, long QuotaDefinition_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            quotaDefinition_hjid = QuotaDefinition_hjid;
            occurrenceTimestamp = obj.occurrenceTimestamp;
            unsuspensionDate = obj.unsuspensionDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(quotaUnsuspensionEvent obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            occurrenceTimestamp = obj.occurrenceTimestamp;
            unsuspensionDate = obj.unsuspensionDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
