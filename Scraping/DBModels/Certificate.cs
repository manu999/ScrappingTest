using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("Certificates")]
    public class Certificate
    {
        [Key]
        public long key { get; set; }

		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string certificateCode { get; set; }
        public long certificateType_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public Certificate()
        {
        }

        public Certificate(Scraping.Certificate cert, string fileName = "")
        {
            hjid = cert.hjid;
            certificateType_hjid = cert.certificateType?.hjid ?? 0;
            opType = cert.metainfo?.opType.ToString();
            origin = cert.metainfo?.origin.ToString();
            status = cert.metainfo?.status.ToString();
            certificateCode = cert.certificateCode;
            validityStartDate = cert.validityStartDate;
            validityEndDate = cert.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : cert.validityEndDate;
            transactionDate = cert.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.Certificate cert, string fileName = "")
        {
            certificateType_hjid = cert.certificateType?.hjid??0;
            opType = cert.metainfo?.opType.ToString();
            origin = cert.metainfo?.origin.ToString();
            status = cert.metainfo?.status.ToString();
            certificateCode = cert.certificateCode;
            validityStartDate = cert.validityStartDate;
            validityEndDate = cert.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : cert.validityEndDate;
            transactionDate = cert.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
