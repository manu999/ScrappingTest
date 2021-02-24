using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("FullTemporaryStopRegulations")]
    public class FullTemporaryStopRegulation
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string approvedFlag { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime effectiveEndDate { get; set; }

        public string fullTemporaryStopRegulationId { get; set; }
        public string informationText { get; set; }
        public string officialjournalNumber { get; set; }
        public int officialjournalPage { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime publishedDate { get; set; }

        public string replacementIndicator { get; set; }
        public long completeAbrogationRegulation_hjid { get; set; }
        public long explicitAbrogationRegulation_hjid { get; set; }
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

        public FullTemporaryStopRegulation()
        {
        }

        public FullTemporaryStopRegulation(Scraping.FullTemporaryStopRegulation obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            fullTemporaryStopRegulationId = obj.fullTemporaryStopRegulationId;
            replacementIndicator = obj.replacementIndicator;
            completeAbrogationRegulation_hjid = obj.completeAbrogationRegulation.hjid;
            explicitAbrogationRegulation_hjid = obj.explicitAbrogationRegulation.hjid;
            regulationRoleType_hjid = obj.regulationRoleType.hjid;
            approvedFlag = obj.approvedFlag;
            informationText = obj.informationText;
            officialjournalNumber = obj.officialjournalNumber;
            officialjournalPage = obj.officialjournalPage;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.FullTemporaryStopRegulation obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            fullTemporaryStopRegulationId = obj.fullTemporaryStopRegulationId;
            replacementIndicator = obj.replacementIndicator;
            completeAbrogationRegulation_hjid = obj.completeAbrogationRegulation.hjid;
            explicitAbrogationRegulation_hjid = obj.explicitAbrogationRegulation.hjid;
            regulationRoleType_hjid = obj.regulationRoleType.hjid;
            approvedFlag = obj.approvedFlag;
            informationText = obj.informationText;
            officialjournalNumber = obj.officialjournalNumber;
            officialjournalPage = obj.officialjournalPage;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
