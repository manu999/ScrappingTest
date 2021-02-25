using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("AdditionalCodeTypes")]
    public class AdditionalCodeType
    {
        [Key]
        public long key { get; set; }

		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string additionalCodeTypeId { get; set; }
        public string applicationCode { get; set; }
        public long meursingTablePlan_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public AdditionalCodeType()
        {
        }

        public AdditionalCodeType(Scraping.AdditionalCodeType obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            meursingTablePlan_hjid = obj.meursingTablePlan.hjid;
            additionalCodeTypeId = obj.additionalCodeTypeId;
            meursingTablePlan_hjid = obj.meursingTablePlan.hjid;
            applicationCode = obj.applicationCode;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.AdditionalCodeType obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            meursingTablePlan_hjid = obj.meursingTablePlan.hjid;
            additionalCodeTypeId = obj.additionalCodeTypeId;
            meursingTablePlan_hjid = obj.meursingTablePlan.hjid;
            applicationCode = obj.applicationCode;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
