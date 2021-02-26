using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Scraping.DBModels
{
    [Table("Footnotes")]
    public class Footnote
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string footnoteId { get; set; }
       
        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        public long footnoteTypes_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
        public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public Footnote()
        {
        }

        public Footnote(Scraping.Footnote fn, string fileName = "")
        {
            hjid = fn.hjid;
            footnoteTypes_hjid = fn.footnoteType?.hjid??0;
            opType = fn.metainfo?.opType.ToString();
            origin = fn.metainfo?.origin.ToString();
            status = fn.metainfo?.status.ToString();
            footnoteId = fn.footnoteId;
            validityStartDate = fn.validityStartDate;
            validityEndDate = fn.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : fn.validityEndDate;
            transactionDate = fn.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.Footnote fn, string fileName = "")
        {
            footnoteTypes_hjid = fn.footnoteType?.hjid??0;
            opType = fn.metainfo?.opType.ToString();
            origin = fn.metainfo?.origin.ToString();
            status = fn.metainfo?.status.ToString();
            footnoteId = fn.footnoteId;
            validityStartDate = fn.validityStartDate;
            validityEndDate = fn.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : fn.validityEndDate;
            transactionDate = fn.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }

}
