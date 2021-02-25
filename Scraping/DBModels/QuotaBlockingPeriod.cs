using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("QuotaBlockingPeriods")]
    public class QuotaBlockingPeriod
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long quotaDefinition_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime blockingEndDate { get; set; }

        public string blockingPeriodType { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime blockingStartDate { get; set; }

        public string description { get; set; }
        public int quotaBlockingPeriodSid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public QuotaBlockingPeriod()
        {
        }

        public QuotaBlockingPeriod(quotaBlockingPeriod obj, long QuotaDefinition_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            quotaDefinition_hjid = QuotaDefinition_hjid;
            blockingEndDate = obj.blockingEndDate;
            blockingPeriodType = obj.blockingPeriodType;
            blockingStartDate = obj.blockingStartDate;
            description = obj.description;
            quotaBlockingPeriodSid = obj.quotaBlockingPeriodSid;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(quotaBlockingPeriod obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            blockingEndDate = obj.blockingEndDate;
            blockingPeriodType = obj.blockingPeriodType;
            blockingStartDate = obj.blockingStartDate;
            description = obj.description;
            quotaBlockingPeriodSid = obj.quotaBlockingPeriodSid;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
