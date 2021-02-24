﻿using System;
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

        public GeographicalArea(Scraping.GeographicalArea obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            geographicalAreaId = obj.geographicalAreaId;
            geographicalCode = obj.geographicalCode;
            parentGeographicalAreaGroupSid = obj.parentGeographicalAreaGroupSid;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.GeographicalArea obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            geographicalAreaId = obj.geographicalAreaId;
            geographicalCode = obj.geographicalCode;
            parentGeographicalAreaGroupSid = obj.parentGeographicalAreaGroupSid;
            validityStartDate = obj.validityStartDate;
            validityEndDate = obj.validityEndDate == DateTime.MinValue ? DateTime.MaxValue : obj.validityEndDate;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
