using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("ExportRefundNomenclatureDescriptionPeriods")]
    public class ExportRefundNomenclatureDescriptionPeriod
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long exportRefundNomenclature_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public ExportRefundNomenclatureDescriptionPeriod()
        {
        }

        public ExportRefundNomenclatureDescriptionPeriod(exportRefundNomenDescriptionPeriod obj, long ExportRefundNomenclature_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            exportRefundNomenclature_hjid = ExportRefundNomenclature_hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            validityStartDate = obj.validityStartDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(exportRefundNomenDescriptionPeriod obj, string fileName = "")
        {
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
