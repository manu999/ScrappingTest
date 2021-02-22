using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("CertificateDescriptionPeriods")]
    public class CertificateDescriptionPeriod
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long certificate_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public CertificateDescriptionPeriod()
        {
        }

        public CertificateDescriptionPeriod(certificateDescriptionPeriod certDesc, long Certificate_hjid, string fileName = "")
        {
            hjid = certDesc.hjid;
            certificate_hjid = Certificate_hjid;
            opType = certDesc.metainfo?.opType.ToString();
            origin = certDesc.metainfo?.origin.ToString();
            status = certDesc.metainfo?.status.ToString();
            validityStartDate = certDesc.validityStartDate;
            transactionDate = certDesc.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(certificateDescriptionPeriod certDesc, string fileName = "")
        {
            opType = certDesc.metainfo?.opType.ToString();
            origin = certDesc.metainfo?.origin.ToString();
            status = certDesc.metainfo?.status.ToString();
            validityStartDate = certDesc.validityStartDate;
            transactionDate = certDesc.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
