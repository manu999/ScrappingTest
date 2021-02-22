using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("FootnoteDescriptionPeriods")]
    public class FootnoteDescriptionPeriod
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        //public long oid { get; set; }
        //public long sid { get; set; }
        public long Footnote_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public FootnoteDescriptionPeriod()
        {
        }

        public FootnoteDescriptionPeriod(footnoteDescriptionPeriod fnd, long footNote_hjid, string fileName = "")
        {
            hjid = fnd.hjid;
            Footnote_hjid = footNote_hjid;
            opType = fnd.metainfo?.opType.ToString();
            origin = fnd.metainfo?.origin.ToString();
            status = fnd.metainfo?.status.ToString();
            validityStartDate = fnd.validityStartDate;
            transactionDate = fnd.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(footnoteDescriptionPeriod fnd, string fileName = "")
        {
            opType = fnd.metainfo?.opType.ToString();
            origin = fnd.metainfo?.origin.ToString();
            status = fnd.metainfo?.status.ToString();
            validityStartDate = fnd.validityStartDate;
            transactionDate = fnd.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
