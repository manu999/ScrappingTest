using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("AdditionalCodeDescriptionPeriods")]
    public class AdditionalCodeDescriptionPeriod
    {
        [Key]
        public long key { get; set; }

		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long additionalCode_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public AdditionalCodeDescriptionPeriod()
        {
        }

        public AdditionalCodeDescriptionPeriod(additionalCodeDescriptionPeriod obj, long AdditionalCode_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            additionalCode_hjid = AdditionalCode_hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            validityStartDate = obj.validityStartDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(additionalCodeDescriptionPeriod obj, string fileName = "")
        {
            additionalCode_hjid = additionalCode_hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            validityStartDate = obj.validityStartDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
