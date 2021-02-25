using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("MeursingHeadings")]
    public class MeursingHeading
    {
        [Key]
        public long key { get; set; }

		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long meursingTablePlan_hjid { get; set; }
        public string meursingHeadingNumber { get; set; }
        public string rowColumnCode { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public MeursingHeading()
        {
        }

        public MeursingHeading(meursingHeading obj, long MeursingTablePlan_hjid,  string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            meursingTablePlan_hjid = MeursingTablePlan_hjid;
            meursingHeadingNumber = obj.meursingHeadingNumber;
            rowColumnCode = obj.rowColumnCode;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(meursingHeading obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            meursingHeadingNumber = obj.meursingHeadingNumber;
            rowColumnCode = obj.rowColumnCode;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
