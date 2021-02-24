using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("RegulationGroupDescriptions")]
    public class RegulationGroupDescription
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long regulationGroup_hjid { get; set; }
        public string description { get; set; }
        public long languages_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public RegulationGroupDescription()
        {
        }

        public RegulationGroupDescription(Description desc, long RegulationGroup_hjid, string fileName = "")
        {
            hjid = desc.hjid;
            regulationGroup_hjid = RegulationGroup_hjid;
            languages_hjid = desc.language.hjid;
            opType = desc.metainfo?.opType.ToString();
            origin = desc.metainfo?.origin.ToString();
            status = desc.metainfo?.status.ToString();
            description = desc.description;
            transactionDate = desc.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Description desc, string fileName = "")
        {
            languages_hjid = desc.language.hjid;
            opType = desc.metainfo?.opType.ToString();
            origin = desc.metainfo?.origin.ToString();
            status = desc.metainfo?.status.ToString();
            description = desc.description;
            transactionDate = desc.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
