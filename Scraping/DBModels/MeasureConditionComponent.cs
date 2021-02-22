using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("MeasureConditionComps")]
    public class MeasureConditionComponent
    {

        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long measureCondition_hjid { get; set; }
        public decimal dutyAmount { get; set; }
        public long dutyExpression_hjid { get; set; }
        public long measurementUnit_hjid { get; set; }
        public long measurementUnitQualifier_hjid { get; set; }
        public long monetaryUnit_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public MeasureConditionComponent()
        {
        }

        public MeasureConditionComponent(Scraping.measureConditionComp obj, long? MeasureCondition_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            measureCondition_hjid = MeasureCondition_hjid ?? 0;
            dutyAmount = obj.dutyAmount;
            dutyExpression_hjid = obj.dutyExpression?.hjid ?? 0;
            measurementUnitQualifier_hjid = obj.measurementUnitQualifier?.hjid ?? 0;
            monetaryUnit_hjid = obj.measurementUnit?.hjid ?? 0;
            measurementUnit_hjid = obj.measurementUnit?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.measureConditionComp obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            dutyAmount = obj.dutyAmount;
            dutyExpression_hjid = obj.dutyExpression?.hjid ?? 0;
            measurementUnitQualifier_hjid = obj.measurementUnitQualifier?.hjid ?? 0;
            monetaryUnit_hjid = obj.measurementUnit?.hjid ?? 0;
            measurementUnit_hjid = obj.measurementUnit?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

    }
}
