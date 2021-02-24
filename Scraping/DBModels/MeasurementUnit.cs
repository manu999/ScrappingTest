using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("MeasurementUnits")]
    public class MeasurementUnit
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string measurementUnitCode { get; set; }
        public long measurementUnitQualifier_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public MeasurementUnit()
        {
        }

        public MeasurementUnit(Scraping.MeasurementUnit obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            measurementUnitCode = obj.measurementUnitCode;
            //measurementUnitQualifier_hjid = obj.measurementUnitQualifier.hjid;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.MeasurementUnit obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            measurementUnitCode = obj.measurementUnitCode;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
