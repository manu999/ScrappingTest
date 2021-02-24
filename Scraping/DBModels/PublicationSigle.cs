﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("PublicationSigles")]
    public class PublicationSigle
    {
        [Key]
        public long key { get; set; }

        public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public string code { get; set; }
        public string codeTypeId { get; set; }
        public string publicationCode { get; set; }
        public string publicationSigle1 { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityStartDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime validityEndDate { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public PublicationSigle()
        {
        }

        public PublicationSigle(Scraping.PublicationSigle obj, string fileName = "")
        {
            hjid = obj.hjid;
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            codeTypeId = obj.codeTypeId;
            code = obj.code;
            publicationCode = obj.publicationCode;
            publicationSigle1 = obj.publicationSigle1;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(Scraping.PublicationSigle obj, string fileName = "")
        {
            opType = obj.metainfo?.opType.ToString();
            origin = obj.metainfo?.origin.ToString();
            status = obj.metainfo?.status.ToString();
            codeTypeId = obj.codeTypeId;
            code = obj.code;
            publicationCode = obj.publicationCode;
            publicationSigle1 = obj.publicationSigle1;
            transactionDate = obj.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}