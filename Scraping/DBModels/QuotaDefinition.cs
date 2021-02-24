using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("QuotaDefinitions")]
    public class QuotaDefinition
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string criticalState { get; set; }
        public int criticalThreshold { get; set; }
        public string description { get; set; }
        public decimal initialVolume { get; set; }
        public string maximumPrecision { get; set; }
        public decimal volume { get; set; }
        public long measurementUnit_hjid { get; set; }
        public long measurementUnitQualifier_hjid { get; set; }
        public long monetaryUnit_hjid { get; set; } 

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public QuotaDefinition()
        {
        }

        public QuotaDefinition(Scraping.QuotaDefinition obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            criticalState = obj.criticalState;
            criticalThreshold = obj.criticalThreshold;
            description = obj.description;
            initialVolume = obj.initialVolume;
            maximumPrecision = obj.maximumPrecision;
            volume = obj.volume;
            measurementUnit_hjid = obj.measurementUnit.hjid;
            measurementUnitQualifier_hjid = obj.measurementUnitQualifier.hjid;
            monetaryUnit_hjid = obj.monetaryUnit.hjid;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.QuotaDefinition obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            criticalThreshold = obj.criticalThreshold;
            description = obj.description;
            initialVolume = obj.initialVolume;
            maximumPrecision = obj.maximumPrecision;
            volume = obj.volume;
            measurementUnit_hjid = obj.measurementUnit.hjid;
            measurementUnitQualifier_hjid = obj.measurementUnitQualifier.hjid;
            monetaryUnit_hjid = obj.monetaryUnit.hjid;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
