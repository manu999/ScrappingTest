using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("ExplicitAbrogationRegulations")]
    public class ExplicitAbrogationRegulation
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }


        [Column(TypeName = "datetime2")]
        public System.DateTime abrogationDate { get; set; }

        public string approvedFlag { get; set; }
        public string explicitAbrogationRegulationId { get; set; }
        public string informationText { get; set; }
        public string officialjournalNumber { get; set; }
        public int officialjournalPage { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime publishedDate { get; set; }
        public string replacementIndicator { get; set; }
        public long regulationRoleType_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public ExplicitAbrogationRegulation()
        {
        }
        public ExplicitAbrogationRegulation(Scraping.explicitAbrogationRegulation obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            abrogationDate = obj.abrogationDate;
            approvedFlag = obj.approvedFlag;
            explicitAbrogationRegulationId = obj.explicitAbrogationRegulationId;
            informationText = obj.informationText;
            officialjournalNumber = obj.officialjournalNumber;
            officialjournalPage = obj.officialjournalPage;
            publishedDate = obj.publishedDate;
            replacementIndicator = obj.replacementIndicator;
            regulationRoleType_hjid = obj.regulationRoleType?.hjid??0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.explicitAbrogationRegulation obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            abrogationDate = obj.abrogationDate;
            approvedFlag = obj.approvedFlag;
            explicitAbrogationRegulationId = obj.explicitAbrogationRegulationId;
            informationText = obj.informationText;
            officialjournalNumber = obj.officialjournalNumber;
            officialjournalPage = obj.officialjournalPage;
            publishedDate = obj.publishedDate;
            replacementIndicator = obj.replacementIndicator;
            regulationRoleType_hjid = obj.regulationRoleType?.hjid??0;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
