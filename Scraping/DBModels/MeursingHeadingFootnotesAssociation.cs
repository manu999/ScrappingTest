using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("MeursingHeadingFootnotesAssociations")]
    public class MeursingHeadingFootnotesAssociation
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long meursingHeading_hjid { get; set; }
        public long footnote_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public MeursingHeadingFootnotesAssociation()
        {
        }

        public MeursingHeadingFootnotesAssociation(meursingHeadingFootnotesAssoc obj, long MeursingHeading_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            meursingHeading_hjid = MeursingHeading_hjid;
            footnote_hjid = obj.footnote?.hjid ?? 0;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(meursingHeadingFootnotesAssoc obj, string fileName = "")
        {
            footnote_hjid = obj.footnote?.hjid ?? 0;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
