using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("GeographicalAreas")]
    public class GeographicalArea
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        //public long oid { get; set; }
        //public long sid { get; set; }
        public string geographicalAreaId { get; set; }
        public string geographicalCode { get; set; }
        public long parentGeographicalAreaGroupSid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }


        public GeographicalArea()
        {
        }

        public GeographicalArea(Scraping.GeographicalArea ga, string fileName = "")
        {
            hjid = ga.hjid;
            opType = ga.metainfo?.opType.ToString();
            origin = ga.metainfo?.origin.ToString();
            status = ga.metainfo?.status.ToString();
            geographicalAreaId = ga.geographicalAreaId;
            geographicalCode = ga.geographicalCode;
            validityStartDate = ga.validityStartDate;
            validityEndDate = ga.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : ga.validityEndDate;
            transactionDate = ga.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.GeographicalArea ga, string fileName = "")
        {
            opType = ga.metainfo?.opType.ToString();
            origin = ga.metainfo?.origin.ToString();
            status = ga.metainfo?.status.ToString();
            geographicalAreaId = ga.geographicalAreaId;
            geographicalCode = ga.geographicalCode;
            validityStartDate = ga.validityStartDate;
            validityEndDate = ga.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : ga.validityEndDate;
            transactionDate = ga.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
