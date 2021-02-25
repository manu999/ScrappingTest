using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("QuotaExtendedInformations")]
    public class QuotaExtendedInformation
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long quotaDefinition_hjid { get; set; }
        public decimal allocatedPercentage { get; set; }
        public decimal balance { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime exhaustionDate { get; set; }

        public decimal importedAmount { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime lastAllocationDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime lastImportDate { get; set; }

        public string quotaBlockedState { get; set; }
        public string quotaExhaustionState { get; set; }
        public string quotaSuspendedState { get; set; }
        public decimal totalAwaitingAllocation { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public QuotaExtendedInformation()
        {
        }

        public QuotaExtendedInformation(quotaExtendedInformation obj, long QuotaDefinition_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            quotaDefinition_hjid = QuotaDefinition_hjid;
            allocatedPercentage = obj.allocatedPercentage;
            balance = obj.balance;
            exhaustionDate = obj.exhaustionDate;
            importedAmount = obj.importedAmount;
            lastAllocationDate = obj.lastAllocationDate;
            lastImportDate = obj.lastImportDate;
            quotaBlockedState = obj.quotaBlockedState;
            quotaExhaustionState = obj.quotaExhaustionState;
            quotaSuspendedState = obj.quotaSuspendedState;
            totalAwaitingAllocation = obj.totalAwaitingAllocation;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(quotaExtendedInformation obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            allocatedPercentage = obj.allocatedPercentage;
            balance = obj.balance;
            exhaustionDate = obj.exhaustionDate;
            importedAmount = obj.importedAmount;
            lastAllocationDate = obj.lastAllocationDate;
            lastImportDate = obj.lastImportDate;
            quotaBlockedState = obj.quotaBlockedState;
            quotaExhaustionState = obj.quotaExhaustionState;
            quotaSuspendedState = obj.quotaSuspendedState;
            totalAwaitingAllocation = obj.totalAwaitingAllocation;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

    }
}
