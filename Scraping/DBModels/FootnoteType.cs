using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("FootnoteTypes")]
    public class FootnoteType
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string applicationCode { get; set; }
        public string footnoteTypeId { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }
        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public FootnoteType()
        {
        }

        public FootnoteType(Scraping.FootnoteType fn, string fileName = "")
        {
            hjid = fn.hjid;
            opType = fn.metainfo?.opType.ToString();
            origin = fn.metainfo?.origin.ToString();
            status = fn.metainfo?.status.ToString();
            applicationCode = fn.applicationCode;
            footnoteTypeId = fn.footnoteTypeId;
            validityStartDate = fn.validityStartDate;
            validityEndDate = fn.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : fn.validityEndDate;
            transactionDate = fn.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.FootnoteType fn, string fileName = "")
        {
            opType = fn.metainfo?.opType.ToString();
            origin = fn.metainfo?.origin.ToString();
            status = fn.metainfo?.status.ToString();
            applicationCode = fn.applicationCode;
            footnoteTypeId = fn.footnoteTypeId;
            validityStartDate = fn.validityStartDate;
            validityEndDate = fn.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : fn.validityEndDate;
            transactionDate = fn.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
