using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("GeographicalAreaMemberships")]
    public class GeographicalAreaMembership
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long geographicalArea_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public GeographicalAreaMembership()
        {
        }

        public GeographicalAreaMembership(geographicalAreaMembership gam, long GeographicalArea_hjid, string fileName = "")
        {
            hjid = gam.hjid;
            geographicalArea_hjid = GeographicalArea_hjid;
            opType = gam.metainfo?.opType.ToString();
            origin = gam.metainfo?.origin.ToString();
            status = gam.metainfo?.status.ToString();
            validityStartDate = gam.validityStartDate;
            transactionDate = gam.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(geographicalAreaMembership gam, string fileName = "")
        {
            opType = gam.metainfo?.opType.ToString();
            origin = gam.metainfo?.origin.ToString();
            status = gam.metainfo?.status.ToString();
            validityStartDate = gam.validityStartDate;
            transactionDate = gam.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

    }
}
