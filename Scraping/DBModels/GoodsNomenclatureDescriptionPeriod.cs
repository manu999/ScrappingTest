using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("GoodsNomenclatureDescriptionPeriods")]
    public class GoodsNomenclatureDescriptionPeriod
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long goodsNomenclature_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public GoodsNomenclatureDescriptionPeriod()
        {
        }

        public GoodsNomenclatureDescriptionPeriod(goodsNomenDescriptionPeriod obj, long GoodsNomenclature_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            goodsNomenclature_hjid = GoodsNomenclature_hjid;
            opType = obj.metainfo.opType.ToString();
            origin = obj.metainfo.origin.ToString();
            status = obj.metainfo.status.ToString();
            validityStartDate = obj.validityStartDate;
            transactionDate = obj.metainfo.transactionDate;
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(goodsNomenDescriptionPeriod obj, string fileName = "")
        {
            opType = obj.metainfo.opType.ToString();
            origin = obj.metainfo.origin.ToString();
            status = obj.metainfo.status.ToString();
            validityStartDate = obj.validityStartDate;
            transactionDate = obj.metainfo.transactionDate;
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
