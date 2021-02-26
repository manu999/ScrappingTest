using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("ExportRefundNomenclatures")]
    public class ExportRefundNomenclature
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string exportRefundCode { get; set; }
        public string productLine { get; set; }
        public long additionalCodeType_hjid { get; set; }
        public long exportRefundNomenclatureIndents_hjid { get; set; }
        public long goodsNomenclature_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public ExportRefundNomenclature()
        {
        }

        public ExportRefundNomenclature(Scraping.ExportRefundNomenclature obj, string fileName = "")
        {
            hjid = obj.hjid;
            additionalCodeType_hjid = obj.additionalCodeType?.hjid??0;
            exportRefundCode = obj.exportRefundCode;
            productLine = obj.productLine;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.ExportRefundNomenclature obj, string fileName = "")
        {
            additionalCodeType_hjid = obj.additionalCodeType?.hjid??0;
            exportRefundCode = obj.exportRefundCode;
            productLine = obj.productLine;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
