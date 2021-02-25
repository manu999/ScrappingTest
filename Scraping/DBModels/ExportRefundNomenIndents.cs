using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("ExportRefundNomenIndents")]
    public class ExportRefundNomenIndents
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string numberExportRefundNomenclatureIndents { get; set; }
        public long exportRefundNomenclature_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public ExportRefundNomenIndents()
        {
        }

        public ExportRefundNomenIndents(Scraping.ExportRefundNomenIndents obj, long ExportRefundNomenclature_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            numberExportRefundNomenclatureIndents = obj.numberExportRefundNomenclatureIndents;
            exportRefundNomenclature_hjid = ExportRefundNomenclature_hjid;
            validityStartDate = obj.validityStartDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.ExportRefundNomenIndents obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            numberExportRefundNomenclatureIndents = obj.numberExportRefundNomenclatureIndents;
            validityStartDate = obj.validityStartDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
