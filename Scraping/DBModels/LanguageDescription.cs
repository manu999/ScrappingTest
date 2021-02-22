using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scraping.DBModels
{
    [Table("LanguageDescriptions")]
    public class LanguageDescription
    {
        [Key]
        public long key { get; set; }
        
		public long hjid { get; set; }
        public string opType { get; set; }
        public string origin { get; set; }
        public string status { get; set; }
        public long language_hjid { get; set; }
        public string description { get; set; }
        public string language { get; set; }

        [Column(TypeName = "datetime2")]
        public System.DateTime transactionDate { get; set; }

        public string dataFileType { get; set; }
		public int dataFileTypeValue { get; set; }
        public string dataFileName { get; set; }

        public LanguageDescription()
        {
        }

        public LanguageDescription(DescriptionLang langDesc, long Language_hjid, string fileName = "")
        {
            hjid = langDesc.hjid;
            language_hjid = Language_hjid;
            opType = langDesc.metainfo?.opType.ToString();
            origin = langDesc.metainfo?.origin.ToString();
            status = langDesc.metainfo?.status.ToString();
            description = langDesc.description;
            language = langDesc.language;
            transactionDate = langDesc.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }

        public void UpdateFields(DescriptionLang langDesc, string fileName = "")
        {
            language_hjid = langDesc.hjid;
            opType = langDesc.metainfo?.opType.ToString();
            origin = langDesc.metainfo?.origin.ToString();
            status = langDesc.metainfo?.status.ToString();
            description = langDesc.description;
            language = langDesc.language;
            transactionDate = langDesc.metainfo?.transactionDate ?? new DateTime();
            dataFileName = fileName;
            dataFileType = "";
            dataFileTypeValue = 0;
        }
    }
}
