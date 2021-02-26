﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("QuotaClosedTransferredEvents")]
    public class QuotaClosedTransferredEvent
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long quotaDefinition_hjid { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime closingDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime occurrenceTimestamp { get; set; }

        public long target_QuotaDefinition_hjid { get; set; }
        public decimal transferredAmount { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public QuotaClosedTransferredEvent()
        {
        }

        public QuotaClosedTransferredEvent(quotaClosedTransEvent obj, long QuotaDefinition_hjid, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            quotaDefinition_hjid = QuotaDefinition_hjid;
            closingDate = obj.closingDate;
            occurrenceTimestamp = obj.occurrenceTimestamp;
            target_QuotaDefinition_hjid = obj.targetQuotaDefinition?.hjid ?? 0;
            transferredAmount = obj.transferredAmount;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(quotaClosedTransEvent obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            closingDate = obj.closingDate;
            occurrenceTimestamp = obj.occurrenceTimestamp;
            target_QuotaDefinition_hjid = obj.targetQuotaDefinition?.hjid ?? 0;
            transferredAmount = obj.transferredAmount;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
