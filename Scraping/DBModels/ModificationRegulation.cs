using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("ModificationRegulations")]
    public class ModificationRegulation
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

        public string informationText { get; set; }
        public string modificationRegulationId { get; set; }
        public string officialjournalNumber { get; set; }
        public int officialjournalPage { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime publishedDate { get; set; }

        public string replacementIndicator { get; set; }
        public string stoppedFlag { get; set; }
        public long baseRegulation_hjid { get; set; }
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

        public ModificationRegulation()
        {
        }
        public ModificationRegulation(Scraping.ModificationRegulation obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            approvedFlag = obj.approvedFlag;
            informationText = obj.informationText;
            modificationRegulationId = obj.modificationRegulationId;
            officialjournalNumber = obj.officialjournalNumber;
            officialjournalPage = obj.officialjournalPage;
            replacementIndicator = obj.replacementIndicator;
            stoppedFlag = obj.stoppedFlag;
            baseRegulation_hjid = obj.baseRegulation?.hjid ?? 0;
            explicitAbrogationRegulation_hjid = obj.explicitAbrogationRegulation?.hjid ?? 0;
            regulationRoleType_hjid = obj.regulationRoleType?.hjid ?? 0;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.ModificationRegulation obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            approvedFlag = obj.approvedFlag;
            informationText = obj.informationText;
            modificationRegulationId = obj.modificationRegulationId;
            officialjournalNumber = obj.officialjournalNumber;
            officialjournalPage = obj.officialjournalPage;
            replacementIndicator = obj.replacementIndicator;
            stoppedFlag = obj.stoppedFlag;
            baseRegulation_hjid = obj.baseRegulation?.hjid ?? 0;
            explicitAbrogationRegulation_hjid = obj.explicitAbrogationRegulation?.hjid ?? 0;
            regulationRoleType_hjid = obj.regulationRoleType?.hjid ?? 0;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
