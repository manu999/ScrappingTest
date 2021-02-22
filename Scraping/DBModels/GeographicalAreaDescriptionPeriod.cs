using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("GeographicalAreaDescriptionPeriods")]
    public class GeographicalAreaDescriptionPeriod
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
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public GeographicalAreaDescriptionPeriod()
        {
        }

        public GeographicalAreaDescriptionPeriod(geographicalAreaDescriptionPeriod gad, long GeographicalArea_hjid, string fileName = "")
        {
            hjid = gad.hjid;
            geographicalArea_hjid = GeographicalArea_hjid;
            opType = gad.metainfo?.opType.ToString();
            origin = gad.metainfo?.origin.ToString();
            status = gad.metainfo?.status.ToString();
            validityStartDate = gad.validityStartDate;
            transactionDate = gad.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(geographicalAreaDescriptionPeriod gad, string fileName = "")
        {
            opType = gad.metainfo?.opType.ToString();
            origin = gad.metainfo?.origin.ToString();
            status = gad.metainfo?.status.ToString();
            validityStartDate = gad.validityStartDate;
            transactionDate = gad.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
