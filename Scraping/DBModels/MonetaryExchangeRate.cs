using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("MonetaryExchangeRates")]
    public class MonetaryExchangeRates
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long monetaryExchangePeriod_hjid { get; set; }
        public string childMonetaryUnitCode { get; set; }
        public string childMonetaryUnitHJID { get; set; }
        public decimal exchangeRate { get; set; }
        public long monetaryUnit_hjid { get; set; }

       [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }
        public MonetaryExchangeRate()
        {
        }

        public MonetaryExchangeRate(monetaryExchangeRate obj, long MonetaryExchangePeriod_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            monetaryExchangePeriod_hjid = MonetaryExchangePeriod_hjid;
            childMonetaryUnitCode = obj.childMonetaryUnitCode;
            childMonetaryUnitHJID = obj.childMonetaryUnitHJID;
            exchangeRate = obj.exchangeRate;
            monetaryUnit_hjid = obj.monetaryUnit.hjid;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(monetaryExchangeRate obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            childMonetaryUnitCode = obj.childMonetaryUnitCode;
            childMonetaryUnitHJID = obj.childMonetaryUnitHJID;
            exchangeRate = obj.exchangeRate;
            monetaryUnit_hjid = obj.monetaryUnit.hjid;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
