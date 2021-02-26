using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("RegulationReplacements")]
    public class RegulationReplacement
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string chapterHeading { get; set; }
        public string replacedRegulationId { get; set; }
        public string replacedRegulationRole { get; set; }
        public string replacingRegulationId { get; set; }
        public string replacingRegulationRole { get; set; }
        public long geographicalArea_hjid { get; set; }
        public long measureType_hjid { get; set; }

       [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public RegulationReplacement()
        {
        }

        public RegulationReplacement(Scraping.RegulationReplacement obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            chapterHeading = obj.chapterHeading;
            replacedRegulationId = obj.replacedRegulationId;
            replacedRegulationRole = obj.replacedRegulationRole;
            replacingRegulationId = obj.replacingRegulationId;
            replacingRegulationRole = obj.replacingRegulationRole;
            geographicalArea_hjid = obj.geographicalArea?.hjid ?? 0;
            measureType_hjid = obj.measureType?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.RegulationReplacement obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            chapterHeading = obj.chapterHeading;
            replacedRegulationId = obj.replacedRegulationId;
            replacedRegulationRole = obj.replacedRegulationRole;
            replacingRegulationId = obj.replacingRegulationId;
            replacingRegulationRole = obj.replacingRegulationRole;
            geographicalArea_hjid = obj.geographicalArea?.hjid ?? 0;
            measureType_hjid = obj.measureType?.hjid ?? 0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
