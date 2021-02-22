using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("Measures")]
    public class Measure
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string justificationRegulationId { get; set; }
        public long justification_RegulationRoleType_hjid { get; set; }
        public string measureGeneratingRegulationId { get; set; }
        public long measureGenerating_RegulationRoleType_hjid { get; set; }
        public long measureGeneratingRegulationSID { get; set; }
        public string ordernumberField { get; set; }
        public string reductionIndicator { get; set; }
        public string stoppedFlag { get; set; }
        public long additionalCode_hjid { get; set; }
        public long exportRefundNomenclature_hjid { get; set; }
        public long geographicalArea_hjid { get; set; }
        public long goodsNomenclature_hjid { get; set; }
        public long measureType_hjid { get; set; }
        public long meursingAdditionalCode_hjid { get; set; }
        public long regulationRoleType_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public Measure()
        {
        }

        public Measure(Scraping.Measure measure, string fileName = "")
        {
            hjid = measure.hjid;
            opType = measure.metainfo?.opType.ToString();
            origin = measure.metainfo?.origin.ToString();
            status = measure.metainfo?.status.ToString();
            justificationRegulationId = measure.justificationRegulationId;
            justification_RegulationRoleType_hjid = measure.regulationRoleType?.hjid ?? 0;
            measureGeneratingRegulationId = measure.measureGeneratingRegulationId;
            measureGenerating_RegulationRoleType_hjid = measure.measureGeneratingRegulationRole?.hjid ?? 0;
            measureGeneratingRegulationSID = measure.measureGeneratingRegulationSID;
            ordernumberField = measure.ordernumber;
            reductionIndicator = measure.reductionIndicator;
            stoppedFlag = measure.stoppedFlag;
            additionalCode_hjid = measure.additionalCode?.hjid ?? 0;
            exportRefundNomenclature_hjid = measure.exportRefundNomenclature?.hjid ?? 0;
            geographicalArea_hjid = measure.geographicalArea?.hjid ?? 0;
            goodsNomenclature_hjid = measure.goodsNomenclature?.hjid ?? 0;
            measureType_hjid = measure.measureType?.hjid ?? 0;
            meursingAdditionalCode_hjid = measure.meursingAdditionalCode?.hjid ?? 0;
            regulationRoleType_hjid = measure.regulationRoleType?.hjid ?? 0;
            validityStartDate = measure.validityStartDate;
            validityEndDate = measure.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : measure.validityEndDate;
            transactionDate = measure.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.Measure measure, string fileName = "")
        {
            opType = measure.metainfo?.opType.ToString();
            origin = measure.metainfo?.origin.ToString();
            status = measure.metainfo?.status.ToString();
            justificationRegulationId = measure.justificationRegulationId;
            justification_RegulationRoleType_hjid = measure.regulationRoleType?.hjid ?? 0;
            measureGeneratingRegulationId = measure.measureGeneratingRegulationId;
            measureGenerating_RegulationRoleType_hjid = measure.measureGeneratingRegulationRole?.hjid ?? 0;
            measureGeneratingRegulationSID = measure.measureGeneratingRegulationSID;
            ordernumberField = measure.ordernumber;
            reductionIndicator = measure.reductionIndicator;
            stoppedFlag = measure.stoppedFlag;
            additionalCode_hjid = measure.additionalCode?.hjid ?? 0;
            exportRefundNomenclature_hjid = measure.exportRefundNomenclature?.hjid ?? 0;
            geographicalArea_hjid = measure.geographicalArea?.hjid ?? 0;
            goodsNomenclature_hjid = measure.goodsNomenclature?.hjid ?? 0;
            measureType_hjid = measure.measureType?.hjid ?? 0;
            meursingAdditionalCode_hjid = measure.meursingAdditionalCode?.hjid ?? 0;
            regulationRoleType_hjid = measure.regulationRoleType?.hjid ?? 0;
            validityStartDate = measure.validityStartDate;
            validityEndDate = measure.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : measure.validityEndDate;
            transactionDate = measure.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
