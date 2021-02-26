using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("MeasureTypes")]
    public class MeasureType
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string measureComponentApplicableCode { get; set; }
        public int measureExplosionLevel { get; set; }
        public string measureTypeId { get; set; }
        public string orderNumberCaptureCode { get; set; }
        public string originDestCode { get; set; }
        public string priorityCode { get; set; }
        public string tradeMovementCode { get; set; }
        public long measureTypeSeries_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public MeasureType()
        {
        }

        public MeasureType(Scraping.MeasureType obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            measureComponentApplicableCode = obj.measureComponentApplicableCode;
            measureExplosionLevel = obj.measureExplosionLevel;
            validityStartDate = obj.validityStartDate;
            measureTypeId = obj.measureTypeId;
            orderNumberCaptureCode = obj.orderNumberCaptureCode;
            originDestCode = obj.originDestCode;
            priorityCode = obj.priorityCode;
            tradeMovementCode = obj.tradeMovementCode;
            measureTypeSeries_hjid = obj.measureTypeSeries?.hjid??0;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.MeasureType obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            measureComponentApplicableCode = obj.measureComponentApplicableCode;
            measureExplosionLevel = obj.measureExplosionLevel;
            validityStartDate = obj.validityStartDate;
            measureTypeId = obj.measureTypeId;
            orderNumberCaptureCode = obj.orderNumberCaptureCode;
            originDestCode = obj.originDestCode;
            priorityCode = obj.priorityCode;
            tradeMovementCode = obj.tradeMovementCode;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
