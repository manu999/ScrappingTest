using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("MeursingSubheadings")]
    public class MeursingSubheading
    {
        [Key]
        public long key { get; set; }

		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long meursingHeading_hjid { get; set; }
        public string description { get; set; }
        public string subheadingSequenceNumber { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }
        public MeursingSubheading()
        {
        }

        public MeursingSubheading(meursingSubheading obj, long MeursingHeading_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            meursingHeading_hjid = MeursingHeading_hjid;
            subheadingSequenceNumber = obj.subheadingSequenceNumber;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate;
            description = obj.description;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(meursingSubheading obj, string fileName = "")
        {
            hjid = obj.hjid;
            subheadingSequenceNumber = obj.subheadingSequenceNumber;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate;
            description = obj.description;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
