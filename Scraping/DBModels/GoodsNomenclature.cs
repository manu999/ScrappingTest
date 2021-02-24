using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("GoodsNomenclatures")]
    public class GoodsNomenclature
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string goodsNomenclatureItemId { get; set; }
        public string produclineSuffix { get; set; }
        public string statisticalIndicator { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public GoodsNomenclature()
        {
        }

        public GoodsNomenclature(Scraping.GoodsNomenclature obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            goodsNomenclatureItemId = obj.goodsNomenclatureItemId;
            produclineSuffix = obj.produclineSuffix;
            statisticalIndicator = obj.statisticalIndicator;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.GoodsNomenclature obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            goodsNomenclatureItemId = obj.goodsNomenclatureItemId;
            produclineSuffix = obj.produclineSuffix;
            statisticalIndicator = obj.statisticalIndicator;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
