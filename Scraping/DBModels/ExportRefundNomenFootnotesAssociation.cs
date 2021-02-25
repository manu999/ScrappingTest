using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("ExportRefundNomenFootnotesAssociations")]
    public class ExportRefundNomenFootnotesAssociation
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long exportRefundNomenclature_hjid { get; set; }
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

        public ExportRefundNomenFootnotesAssociation()
        {
        }

        public ExportRefundNomenFootnotesAssociation(Scraping.ExportRefundNomenFootnotesAssoc obj, long? ExportRefundNomenclature_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            exportRefundNomenclature_hjid = ExportRefundNomenclature_hjid ?? 0;
            footnote_hjid = obj.footnote?.hjid ?? 0;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.ExportRefundNomenFootnotesAssoc obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            footnote_hjid = obj.footnote?.hjid ?? 0;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
