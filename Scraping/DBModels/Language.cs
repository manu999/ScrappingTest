using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("Languages")]
    public class Language
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string languageId { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public Language()
        {
        }

        public Language(Scraping.Language fn, string fileName = "")
        {
            hjid = fn.hjid;
            languageId = fn.languageId;
            validityStartDate = fn.validityStartDate;
            validityEndDate = fn.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : fn.validityEndDate;
            transactionDate = fn.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.Language fn, string fileName = "")
        {
            languageId = fn.languageId;
            validityStartDate = fn.validityStartDate;
            validityEndDate = fn.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : fn.validityEndDate;
            transactionDate = fn.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
