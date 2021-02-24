using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("RegulationRoleCombinations")]
    public class RegulationRoleCombinations
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long regulationRoleType_hjid { get; set; }
        public long regulationRoleType_master_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public RegulationRoleCombinations()
        {
        }

        public RegulationRoleCombinations(Scraping.regulationRoleCombinations obj, long? RegulationRoleType_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            regulationRoleType_hjid = RegulationRoleType_hjid ?? 0;
            regulationRoleType_master_hjid = obj.regulationRoleTypeMaster.hjid;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.regulationRoleCombinations obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            regulationRoleType_master_hjid = obj.regulationRoleTypeMaster.hjid;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
