using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("MeasureConditions")]
    public class MeasureCondition
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long measure_hjid { get; set; }
        public int conditionSequenceNumber { get; set; }
        public decimal conditionDutyAmount { get; set; }
        public long certificate_hjid { get; set; }
        public long measureAction_hjid { get; set; }
        public long measureConditionCode_hjid { get; set; }
        public long measurementUnit_hjid { get; set; }
        public long measurementUnitQualifier_hjid { get; set; }
        public long monetaryUnit_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public MeasureCondition()
        {
        }

        public MeasureCondition(Scraping.measureCondition obj, long? measure_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            measure_hjid = measure_hjid ?? 0;
            conditionDutyAmount = obj.conditionDutyAmount;
            conditionSequenceNumber = obj.conditionSequenceNumber;
            certificate_hjid = obj.certificate?.hjid ?? 0;
            measureAction_hjid = obj.measureAction?.hjid ?? 0;
            measureConditionCode_hjid = obj.measureConditionCode?.hjid ?? 0;
            measurementUnitQualifier_hjid = obj.measurementUnitQualifier?.hjid ?? 0;
            measurementUnit_hjid = obj.measurementUnit?.hjid ?? 0;
            monetaryUnit_hjid = obj.measurementUnit?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.measureCondition obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            conditionDutyAmount = obj.conditionDutyAmount;
            conditionSequenceNumber = obj.conditionSequenceNumber;
            certificate_hjid = obj.certificate?.hjid ?? 0;
            measureAction_hjid = obj.measureAction?.hjid ?? 0;
            measureConditionCode_hjid = obj.measureConditionCode?.hjid ?? 0;
            measurementUnitQualifier_hjid = obj.measurementUnitQualifier?.hjid ?? 0;
            measurementUnit_hjid = obj.measurementUnit?.hjid ?? 0;
            monetaryUnit_hjid = obj.measurementUnit?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
