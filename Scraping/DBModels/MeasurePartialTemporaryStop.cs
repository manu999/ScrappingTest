using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("MeasurePartialTemporaryStops")]
    public class MeasurePartialTemporaryStop
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long measure_hjid { get; set; }
        public string abrogationRegulationId { get; set; }
        public string abrogationRegulationOfficialjournalNumber { get; set; }
        public int abrogationRegulationOfficialjournalPage { get; set; }
        public string partialTemporaryStopRegulationId { get; set; }
        public string partialTemporaryStopRegulationOfficialjournalNumber { get; set; }
        public int partialTemporaryStopRegulationOfficialjournalPage { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public MeasurePartialTemporaryStop()
        {
        }

        public MeasurePartialTemporaryStop(Scraping.measurePartTempStop obj, long? Measure_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            measure_hjid = Measure_hjid ?? 0;
            abrogationRegulationId = obj.abrogationRegulationId;
            abrogationRegulationOfficialjournalNumber = obj.abrogationRegulationOfficialjournalNumber;
            abrogationRegulationOfficialjournalPage = obj.abrogationRegulationOfficialjournalPage;
            partialTemporaryStopRegulationId = obj.partialTemporaryStopRegulationId;
            partialTemporaryStopRegulationOfficialjournalNumber = obj.partialTemporaryStopRegulationOfficialjournalNumber;
            partialTemporaryStopRegulationOfficialjournalPage = obj.partialTemporaryStopRegulationOfficialjournalPage;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj?.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.measurePartTempStop obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            abrogationRegulationId = obj.abrogationRegulationId;
            abrogationRegulationOfficialjournalNumber = obj.abrogationRegulationOfficialjournalNumber;
            abrogationRegulationOfficialjournalPage = obj.abrogationRegulationOfficialjournalPage;
            partialTemporaryStopRegulationId = obj.partialTemporaryStopRegulationId;
            partialTemporaryStopRegulationOfficialjournalNumber = obj.partialTemporaryStopRegulationOfficialjournalNumber;
            partialTemporaryStopRegulationOfficialjournalPage = obj.partialTemporaryStopRegulationOfficialjournalPage;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj?.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
