using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("GoodsNomenclatureOrigins")]
    public class GoodsNomenclatureOrigin
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string derivedGoodsNomenclatureItemId { get; set; }
        public string derivedProductlineSuffix { get; set; }
        public long goodsNomenclatureSid { get; set; }
        public long goodsNomenclature_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public GoodsNomenclatureOrigin()
        {
        }

        public GoodsNomenclatureOrigin(Scraping.GoodsNomenOrigin obj, long GoodsNomenclature_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            derivedGoodsNomenclatureItemId = obj.derivedGoodsNomenclatureItemId;
            goodsNomenclature_hjid = GoodsNomenclature_hjid;
            derivedProductlineSuffix = obj.derivedProductlineSuffix;
            goodsNomenclatureSid = obj.goodsNomenclatureSid;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.GoodsNomenOrigin obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            derivedGoodsNomenclatureItemId = obj.derivedGoodsNomenclatureItemId;
            derivedProductlineSuffix = obj.derivedProductlineSuffix;
            goodsNomenclatureSid = obj.goodsNomenclatureSid;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
