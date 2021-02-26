using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("QuotaOrderNumberOriginExclusions")]
    public class QuotaOrderNumberOriginExclusions
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long quotaOrderNumberOrigin_hjid { get; set; }
        public long geographicalArea_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public QuotaOrderNumberOriginExclusions()
        {
        }

        public QuotaOrderNumberOriginExclusions(Scraping.QuotaOrderNumberOriginExcl obj, long QuotaOrderNumberOrigin_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            quotaOrderNumberOrigin_hjid = QuotaOrderNumberOrigin_hjid;
            geographicalArea_hjid = obj.geographicalArea?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.QuotaOrderNumberOriginExcl obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            geographicalArea_hjid = obj.geographicalArea?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
