using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("GoodsNomenclatureFootnoteAssociations")]
    public class GoodsNomenclatureFootnoteAssociation
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long goodsNomenclature_hjid { get; set; }
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

        public GoodsNomenclatureFootnoteAssociation()
        {
        }

        public GoodsNomenclatureFootnoteAssociation(Scraping.goodsNomenFootnotesAssoc obj, long? GoodsNomenclature_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            goodsNomenclature_hjid = GoodsNomenclature_hjid ?? 0;
            footnote_hjid = obj.footnote?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.goodsNomenFootnotesAssoc obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            footnote_hjid = obj.footnote?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
