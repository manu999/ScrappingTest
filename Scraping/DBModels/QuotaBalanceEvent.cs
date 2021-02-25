using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("QuotaBalanceEvents")]
    public class QuotaBalanceEvent
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long quotaDefinition_hjid { get; set; }
        public decimal importedAmount { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime lastImportDateInAllocation { get; set; }

        public decimal newBalance { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime occurrenceTimestamp { get; set; }

        public decimal oldBalance { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public QuotaBalanceEvent()
        {
        }

        public QuotaBalanceEvent(quotaBalanceEvent obj, long QuotaDefinition_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            quotaDefinition_hjid = QuotaDefinition_hjid;
            importedAmount = obj.importedAmount;
            lastImportDateInAllocation = obj.lastImportDateInAllocation;
            newBalance = obj.newBalance;
            occurrenceTimestamp = obj.occurrenceTimestamp;
            oldBalance = obj.oldBalance;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(quotaBalanceEvent obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            importedAmount = obj.importedAmount;
            lastImportDateInAllocation = obj.lastImportDateInAllocation;
            newBalance = obj.newBalance;
            occurrenceTimestamp = obj.occurrenceTimestamp;
            oldBalance = obj.oldBalance;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
