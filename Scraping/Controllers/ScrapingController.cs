using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System.Xml.Serialization;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using Scraping.DBModels;

namespace Scraping.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScrapingController : ControllerBase
    {
        private readonly ILogger<ScrapingController> _logger;
        private readonly DBContext _dbContext;

        public ScrapingController(ILogger<ScrapingController> logger)
        {
            _logger = logger;
            _dbContext = new DBContext();
        }

        [HttpGet]
        public string Get()
        {
            var getFileFromWeb = false;
            var proccesFile = true;

            var aux = _dbContext.Footnotes.FirstOrDefault();

            var elapsedMs = 0L;
            if (getFileFromWeb) { 
                //URL from where we are going to start
                string fullUrl = "https://eservices.minfin.fgov.be/extTariffBrowser/XmlExtractions?date=20210204&lang=EN&page=1&searchMonth=02&searchYear=2021";
            
                //Location of the chromedriver.exe should be C:\Program Files (x86)\Google\Chrome\Application
                string driverPath = "C:\\Program Files (x86)\\Google\\Chrome\\Application";
            
                //Chrome driver Options
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddUserProfilePreference("download.default_directory", "C:\\Users\\malzugaray\\Downloads");
                chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
                chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                var browser = new ChromeDriver(driverPath, chromeOptions);
            
                //Open browser in the index page
                browser.Navigate().GoToUrl(fullUrl);

                //Wait until the table with the links was loaded, timeout if it didn't appears after 10 seconds.
                var wait = new WebDriverWait(browser, TimeSpan.FromSeconds(10));
                wait.Until(x => browser.FindElementsByTagName("a").Where(node => node.GetAttribute("class").Contains("ui-commandlink ui-widget")
                                && (node.Text.EndsWith("xml") || node.Text.EndsWith("zip"))).Any());

                //Find all the links 'a' where class contains 'ui-commandlink ui-widget' and the text finished with xml or zip
                var links = browser.FindElementsByTagName("a").Where(node => node.GetAttribute("class").Contains("ui-commandlink ui-widget")
                                && (node.Text.EndsWith("xml") || node.Text.EndsWith("zip")))
                        .Reverse();

                //For each link we need to download the files and proccess them
                foreach (var link in links)
                {
                    ////Click the link
                    //link.Click();
                    ////Wait until preparing downloading disapears Id = 'downloadStatusDlgID'
                    //wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id("downloadStatusDlgID")));

                    break;
                }
            }

            if (proccesFile)
            {
                //fileName to be proccess
                //var fileName = "C:\\Users\\malzugaray\\Documents\\export-20181201T000000-20181201T000500\\export-20181201T000000-20181201T000500.xml";
                //var fileName = "C:\\Users\\malzugaray\\Documents\\export-20210101T000000-20210101T000500\\export-20210101T000000-20210101T000500.xml";
                var fileName = "C:\\Users\\malzugaray\\source\\repos\\ScrapingTest\\Scraping\\testMessage.xml";
                //C:\Users\malzugaray\Documents\export-20181201T000000-20181201T000500

                //Proccess Model and store in DB
                elapsedMs = ProccessFile(fileName);
            }

            return elapsedMs.ToString();
        }

        /// <summary>
        /// Proccess model, insert/update/delete data in the DB
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private long ProccessFile(string fileName)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                reader.MoveToContent();

                int maxThreadsToOpen = Environment.ProcessorCount * 2 - 1;
                int activeThreadCount = 0;
                while (reader.Read())
                {
                    while (activeThreadCount > maxThreadsToOpen)
                    {
                        //While there is no free spots for threads
                        Thread.Sleep(50);
                    }
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        
                        
                        switch (reader.Name)
                        {
                            case "FootnoteType":
                                if (FindXmlElements.FootnoteTypeBypass) break;
                                var footnoteTypeString = (XNode.ReadFrom(reader) as XElement).ToString();
                                //Open new thread
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(footnoteTypeString))
                                            {
                                                XmlSerializer serializer = new XmlSerializer(typeof(Scraping.FootnoteType));
                                                var xmlObject = (Scraping.FootnoteType)serializer.Deserialize(stringReader);

                                                ProccessFootNotesTypes(xmlObject, dbContext, fileName);
                                            }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "Footnote":
                                if (FindXmlElements.FootnoteBypass) break;
                                var footnoteString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(footnoteString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.Footnote));
                                            var xmlObject = (Scraping.Footnote)serializer.Deserialize(stringReader);

                                            ProccessFootNotes(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "CertificateType":
                                if (FindXmlElements.CertificateTypeBypass) break;
                                var certificateTypeString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(certificateTypeString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.CertificateType));
                                            var xmlObject = (Scraping.CertificateType)serializer.Deserialize(stringReader);

                                            ProccessCertificateTypes(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "Certificate":
                                if (FindXmlElements.CertificateBypass) break;
                                var certificateString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(certificateString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.Certificate));
                                            var xmlObject = (Scraping.Certificate)serializer.Deserialize(stringReader);

                                            ProccessCertificate(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "GeographicalArea":
                                if (FindXmlElements.GeographicalAreaBypass) break;
                                var geographicalAreaString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(geographicalAreaString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.GeographicalArea));
                                            var xmlObject = (Scraping.GeographicalArea)serializer.Deserialize(stringReader);

                                            ProccessGeographicalArea(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "Language":
                                if (FindXmlElements.LanguageBypass) break;
                                var languageString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(languageString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.Language));
                                            var xmlObject = (Scraping.Language)serializer.Deserialize(stringReader);

                                            ProccessLanguage(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "Measure":
                                if (FindXmlElements.MeasureBypass) break;
                                var measureString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(measureString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.Measure));
                                            var xmlObject = (Scraping.Measure)serializer.Deserialize(stringReader);

                                            ProccessMeasure(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "AdditionalCodeType":
                                if (FindXmlElements.AdditionalCodeTypeBypass) break;
                                var additionalCodeTypeString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(additionalCodeTypeString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.AdditionalCodeType));
                                            var xmlObject = (Scraping.AdditionalCodeType)serializer.Deserialize(stringReader);

                                            ProccessAdditionalCodeType(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MeasureTypeSeries":
                                if (FindXmlElements.MeasureTypeSeriesBypass) break;
                                var measureTypeSeriesString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(measureTypeSeriesString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MeasureTypeSeries));
                                            var xmlObject = (Scraping.MeasureTypeSeries)serializer.Deserialize(stringReader);

                                            ProccessMeasureTypeSeries(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MeasurementUnitQualifier":
                                if (FindXmlElements.MeasurementUnitQualifierBypass) break;
                                var measurementUnitQualifierString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(measurementUnitQualifierString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MeasurementUnitQualifier));
                                            var xmlObject = (Scraping.MeasurementUnitQualifier)serializer.Deserialize(stringReader);

                                            ProccessMeasurementUnitQualifier(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "GoodsNomenclatureGroup":
                                if (FindXmlElements.GoodsNomenclatureGroupBypass) break;
                                var goodsNomenclatureGroupString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(goodsNomenclatureGroupString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.GoodsNomenclatureGroup));
                                            var xmlObject = (Scraping.GoodsNomenclatureGroup)serializer.Deserialize(stringReader);

                                            ProccessGoodsNomenclatureGroup(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "RegulationRoleType":
                                if (FindXmlElements.RegulationRoleTypeBypass) break;
                                var regulationRoleTypeString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(regulationRoleTypeString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.RegulationRoleType));
                                            var xmlObject = (Scraping.RegulationRoleType)serializer.Deserialize(stringReader);

                                            ProccessRegulationRoleType(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MeasurementUnit":
                                if (FindXmlElements.MeasurementUnitBypass) break;
                                var measurementUnitString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(measurementUnitString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MeasurementUnit));
                                            var xmlObject = (Scraping.MeasurementUnit)serializer.Deserialize(stringReader);

                                            ProccessMeasurementUnit(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "PublicationSigle":
                                if (FindXmlElements.PublicationSigleBypass) break;
                                var publicationSigleString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(publicationSigleString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.PublicationSigle));
                                            var xmlObject = (Scraping.PublicationSigle)serializer.Deserialize(stringReader);

                                            ProccessPublicationSigle(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "ExportRefundNomenclature":
                                if (FindXmlElements.ExportRefundNomenclatureBypass) break;
                                var exportRefundNomenclatureString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(exportRefundNomenclatureString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.ExportRefundNomenclature));
                                            var xmlObject = (Scraping.ExportRefundNomenclature)serializer.Deserialize(stringReader);

                                            ProccessExportRefundNomenclature(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MeasureConditionCode":
                                if (FindXmlElements.MeasureConditionCodeBypass) break;
                                var measureConditionCodeString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(measureConditionCodeString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MeasureConditionCode));
                                            var xmlObject = (Scraping.MeasureConditionCode)serializer.Deserialize(stringReader);

                                            ProccessMeasureConditionCode(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "DutyExpression":
                                if (FindXmlElements.DutyExpressionBypass) break;
                                var dutyExpressionString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(dutyExpressionString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.DutyExpression));
                                            var xmlObject = (Scraping.DutyExpression)serializer.Deserialize(stringReader);

                                            ProccessDutyExpression(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MeasureAction":
                                if (FindXmlElements.MeasureActionBypass) break;
                                var measureActionString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(measureActionString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MeasureAction));
                                            var xmlObject = (Scraping.MeasureAction)serializer.Deserialize(stringReader);

                                            ProccessMeasureAction(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "RegulationGroup":
                                if (FindXmlElements.RegulationGroupBypass) break;
                                var regulationGroupString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(regulationGroupString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.RegulationGroup));
                                            var xmlObject = (Scraping.RegulationGroup)serializer.Deserialize(stringReader);

                                            ProccessRegulationGroup(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MeasureType":
                                if (FindXmlElements.MeasureTypeBypass) break;
                                var measureTypeString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(measureTypeString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MeasureType));
                                            var xmlObject = (Scraping.MeasureType)serializer.Deserialize(stringReader);

                                            ProccessMeasureType(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "GoodsNomenclature":
                                if (FindXmlElements.GoodsNomenclatureBypass) break;
                                var goodsNomenclatureString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(goodsNomenclatureString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.GoodsNomenclature));
                                            var xmlObject = (Scraping.GoodsNomenclature)serializer.Deserialize(stringReader);

                                            ProccessGoodsNomenclature(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "ProrogationRegulation":
                                if (FindXmlElements.ProrogationRegulationBypass) break;
                                var prorogationRegulationString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(prorogationRegulationString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.ProrogationRegulation));
                                            var xmlObject = (Scraping.ProrogationRegulation)serializer.Deserialize(stringReader);

                                            ProccessProrogationRegulation(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "FullTemporaryStopRegulation":
                                if (FindXmlElements.FullTemporaryStopRegulationBypass) break;
                                var fullTemporaryStopRegulationString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(fullTemporaryStopRegulationString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.FullTemporaryStopRegulation));
                                            var xmlObject = (Scraping.FullTemporaryStopRegulation)serializer.Deserialize(stringReader);

                                            ProccessFullTemporaryStopRegulation(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "RegulationReplacement":
                                if (FindXmlElements.RegulationReplacementBypass) break;
                                var regulationReplacementString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(regulationReplacementString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.RegulationReplacement));
                                            var xmlObject = (Scraping.RegulationReplacement)serializer.Deserialize(stringReader);

                                            ProccessRegulationReplacement(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MeursingAdditionalCode":
                                if (FindXmlElements.MeursingAdditionalCodeBypass) break;
                                var meursingAdditionalCodeString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(meursingAdditionalCodeString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MeursingAdditionalCode));
                                            var xmlObject = (Scraping.MeursingAdditionalCode)serializer.Deserialize(stringReader);

                                            ProccessMeursingAdditionalCode(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "QuotaOrderNumber":
                                if (FindXmlElements.QuotaOrderNumberBypass) break;
                                var quotaOrderNumberString = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(quotaOrderNumberString))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.QuotaOrderNumber));
                                            var xmlObject = (Scraping.QuotaOrderNumber)serializer.Deserialize(stringReader);

                                            ProccessQuotaOrderNumber(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            _logger.LogInformation($"Elapsed time in ms: {elapsedMs}, some threads could be still running");
            //Return
            return elapsedMs;

        }

        #region ProccessFootNotes
        private bool ProccessFootNotes(Scraping.Footnote item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.Footnotes.Add(new DBModels.Footnote(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.Footnotes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.Footnotes.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.Footnotes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.Footnotes.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Footnote Description Periods
                foreach(footnoteDescriptionPeriod fnd in item.footnoteDescriptionPeriod)
                {
                    switch (fnd.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.FootnoteDescriptionPeriods.Add(new DBModels.FootnoteDescriptionPeriod(fnd, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.FootnoteDescriptionPeriods.Where(x => x.hjid == fnd.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(fnd, fileName);
                                dbContext.FootnoteDescriptionPeriods.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.FootnoteDescriptionPeriods.Where(x => x.hjid == fnd.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.FootnoteDescriptionPeriods.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                    //Proccess Footnote Descriptions
                    foreach (Description desc in fnd.footnoteDescription)
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.FootnoteDescriptions.Add(new DBModels.FootnoteDescription(desc, fnd.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.FootnoteDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.FootnoteDescriptions.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.FootnoteDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.FootnoteDescriptions.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }
                            
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch(Exception e)
            {
                _logger.LogError($"Error saving Footnotes");
            }

            return success;
        }
        #endregion ProccessFootNotes

        #region ProccessFootNotesTypes
        private bool ProccessFootNotesTypes(Scraping.FootnoteType item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.FootnoteTypes.Add(new DBModels.FootnoteType(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.FootnoteTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.FootnoteTypes.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.FootnoteTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.FootnoteTypes.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Footnote Type Descriptions
                foreach (Description d in item.footnoteTypeDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.FootnoteTypeDescriptions.Add(new DBModels.FootnoteTypeDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.FootnoteTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.FootnoteTypeDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.FootnoteTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.FootnoteTypeDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Footnote Types");
            }

            return success;
        }
        #endregion ProccessFootNotesTypes

        #region ProccessCertificate
        private bool ProccessCertificate(Scraping.Certificate item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.Certificates.Add(new DBModels.Certificate(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.Certificates.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.Certificates.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.Certificates.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.Certificates.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Certificate Description Periods
                foreach (certificateDescriptionPeriod certificateDP in item.certificateDescriptionPeriod)
                {
                    switch (certificateDP.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.CertificateDescriptionPeriods.Add(new DBModels.CertificateDescriptionPeriod(certificateDP, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.CertificateDescriptionPeriods.Where(x => x.hjid == certificateDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(certificateDP, fileName);
                                dbContext.CertificateDescriptionPeriods.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.CertificateDescriptionPeriods.Where(x => x.hjid == certificateDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.CertificateDescriptionPeriods.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                    //Proccess Footnote Descriptions
                    foreach (Description desc in certificateDP.certificateDescription)
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.CertificateDescriptions.Add(new DBModels.CertificateDescription(desc, certificateDP.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.CertificateDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.CertificateDescriptions.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.CertificateDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.CertificateDescriptions.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Certificates");
            }

            return success;
        }
        #endregion ProccessCertificate

        #region ProccessCertificateTypes
        private bool ProccessCertificateTypes(Scraping.CertificateType item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.CertificateTypes.Add(new DBModels.CertificateType(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.CertificateTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.CertificateTypes.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.CertificateTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.CertificateTypes.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Footnote Type Descriptions
                foreach (Description d in item.certificateTypeDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.CertificateTypeDescriptions.Add(new DBModels.CertificateTypeDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.CertificateTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.CertificateTypeDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.CertificateTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.CertificateTypeDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Certificate Types");
            }

            return success;
        }
        #endregion ProccessCertificateTypes

        #region ProccessGeographicalArea
        private bool ProccessGeographicalArea(Scraping.GeographicalArea item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.GeographicalAreas.Add(new DBModels.GeographicalArea(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.GeographicalAreas.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.GeographicalAreas.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.GeographicalAreas.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.GeographicalAreas.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Geographical Area Description Periods
                foreach (GeographicalAreaDescriptionPeriod geoAreaDP in item.geographicalAreaDescriptionPeriod)
                {
                    switch (geoAreaDP.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.GeographicalAreaDescriptionPeriods.Add(new DBModels.GeographicalAreaDescriptionPeriod(geoAreaDP, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.GeographicalAreaDescriptionPeriods.Where(x => x.hjid == geoAreaDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(geoAreaDP, fileName);
                                dbContext.GeographicalAreaDescriptionPeriods.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.GeographicalAreaDescriptionPeriods.Where(x => x.hjid == geoAreaDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.GeographicalAreaDescriptionPeriods.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                    //Proccess Geographical Area Descriptions
                    foreach (Description desc in geoAreaDP.geographicalAreaDescription)
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.GeographicalAreaDescriptions.Add(new DBModels.GeographicalAreaDescription(desc, geoAreaDP.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.GeographicalAreaDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.GeographicalAreaDescriptions.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.GeographicalAreaDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.GeographicalAreaDescriptions.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }

                if(item.geographicalAreaMembership != null)
                {
                    //Proccess Geographical Area Membership
                    foreach (geographicalAreaMembership gam in item.geographicalAreaMembership)
                    {
                        switch (gam.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.GeographicalAreaMemberships.Add(new DBModels.GeographicalAreaMembership(gam, item.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.GeographicalAreaMemberships.Where(x => x.hjid == gam.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(gam, fileName);
                                    dbContext.GeographicalAreaMemberships.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.GeographicalAreaMemberships.Where(x => x.hjid == gam.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.GeographicalAreaMemberships.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving ProccessGeographicalArea");
            }

            return success;
        }
        #endregion ProccessGeographicalArea

        #region ProccessLanguage
        private bool ProccessLanguage(Scraping.Language item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.Languages.Add(new DBModels.Language(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.Languages.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.Languages.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.Languages.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.Languages.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Footnote Type Descriptions
                foreach (DescriptionLang d in item.languageDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.LanguageDescriptions.Add(new DBModels.LanguageDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.LanguageDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.LanguageDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.LanguageDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.LanguageDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessLanguage

        #region ProccessMeasure
        private bool ProccessMeasure(Scraping.Measure item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.Measures.Add(new DBModels.Measure(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.Measures.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.Measures.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.Measures.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.Measures.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess footnoteAssociationMeasure
                foreach (var d in item.footnoteAssociationMeasure)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasureFootnoteAssociations.Add(new DBModels.MeasureFootnoteAssociation(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasureFootnoteAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasureFootnoteAssociations.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasureFootnoteAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasureFootnoteAssociations.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Proccess measureComponent
                foreach (var d in item.measureComponent)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasureComponents.Add(new DBModels.MeasureComponent(d, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasureComponents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasureComponents.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasureComponents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasureComponents.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Proccess measureCondition
                foreach (var d in item.measureCondition)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasureConditions.Add(new DBModels.MeasureCondition(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasureConditions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasureConditions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasureConditions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasureConditions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                    //Proccess measureConditionComponent
                    foreach (var c in d.measureConditionComponent)
                    {
                        switch (c.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.MeasureConditionComponents.Add(new DBModels.MeasureConditionComponent(c, d.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.MeasureConditionComponents.Where(x => x.hjid == c.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(c, fileName);
                                    dbContext.MeasureConditionComponents.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.MeasureConditionComponents.Where(x => x.hjid == c.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.MeasureConditionComponents.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                //Proccess measureExcludedGeographicalArea
                foreach (var d in item.measureExcludedGeographicalArea)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasureExcludedGeographicalAreas.Add(new DBModels.MeasureExcludedGeographicalArea(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasureExcludedGeographicalAreas.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasureExcludedGeographicalAreas.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasureExcludedGeographicalAreas.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasureExcludedGeographicalAreas.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Proccess measurePartialTemporaryStop
                foreach (var d in item.measurePartialTemporaryStop)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasurePartialTemporaryStops.Add(new DBModels.MeasurePartialTemporaryStop(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasurePartialTemporaryStops.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasurePartialTemporaryStops.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasurePartialTemporaryStops.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasurePartialTemporaryStops.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessMeasure

        #region ProccessAdditionalCodeType
        private bool ProccessAdditionalCodeType(Scraping.AdditionalCodeType item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.AdditionalCodeTypes.Add(new DBModels.AdditionalCodeType(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.AdditionalCodeTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.AdditionalCodeTypes.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.AdditionalCodeTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.AdditionalCodeTypes.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess additional Code Type Description
                foreach (var d in item.additionalCodeTypeDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.AdditionalCodeTypeDescriptions.Add(new DBModels.AdditionalCodeTypeDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.AdditionalCodeTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.AdditionalCodeTypeDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.AdditionalCodeTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.AdditionalCodeTypeDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Proccess additional Code Type Measure Type
                foreach (var d in item.additionalCodeTypeMeasureType)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.AdditionalCodeTypeMeasureTypes.Add(new DBModels.AdditionalCodeTypeMeasureType(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.AdditionalCodeTypeMeasureTypes.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.AdditionalCodeTypeMeasureTypes.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.AdditionalCodeTypeMeasureTypes.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.AdditionalCodeTypeMeasureTypes.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessAdditionalCodeType

        #region ProccessMeasureTypeSeries
        private bool ProccessMeasureTypeSeries(Scraping.MeasureTypeSeries item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MeasureTypeSeries.Add(new DBModels.MeasureTypeSeries(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MeasureTypeSeries.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MeasureTypeSeries.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MeasureTypeSeries.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MeasureTypeSeries.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess measure Type Series Description
                foreach (var d in item.measureTypeSeriesDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasureTypeSeriesDescriptions.Add(new DBModels.MeasureTypeSeriesDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasureTypeSeriesDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasureTypeSeriesDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasureTypeSeriesDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasureTypeSeriesDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessMeasureTypeSeries

        #region ProccessMeasurementUnitQualifier
        private bool ProccessMeasurementUnitQualifier(Scraping.MeasurementUnitQualifier item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MeasurementUnitQualifiers.Add(new DBModels.MeasurementUnitQualifier(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MeasurementUnitQualifiers.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MeasurementUnitQualifiers.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MeasurementUnitQualifiers.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MeasurementUnitQualifiers.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess measurement Unit Qualifier Description
                foreach (var d in item.measurementUnitQualifierDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasurementUnitQualifierDescriptions.Add(new DBModels.MeasurementUnitQualifierDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasurementUnitQualifierDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasurementUnitQualifierDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasurementUnitQualifierDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasurementUnitQualifierDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessMeasurementUnitQualifier

        #region ProccessGoodsNomenclatureGroup
        private bool ProccessGoodsNomenclatureGroup(Scraping.GoodsNomenclatureGroup item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.GoodsNomenclatureGroups.Add(new DBModels.GoodsNomenclatureGroup(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.GoodsNomenclatureGroups.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.GoodsNomenclatureGroups.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.GoodsNomenclatureGroups.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.GoodsNomenclatureGroups.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess goods Nomenclature Group Description
                foreach (var d in item.goodsNomenclatureGroupDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.GoodsNomenclatureGroupDescriptions.Add(new DBModels.GoodsNomenclatureGroupDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.GoodsNomenclatureGroupDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.GoodsNomenclatureGroupDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.GoodsNomenclatureGroupDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.GoodsNomenclatureGroupDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessGoodsNomenclatureGroup

        #region ProccessRegulationRoleType
        private bool ProccessRegulationRoleType(Scraping.RegulationRoleType item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.RegulationRoleTypes.Add(new DBModels.RegulationRoleType(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.RegulationRoleTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.RegulationRoleTypes.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.RegulationRoleTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.RegulationRoleTypes.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess regulation Role Type Description
                foreach (var d in item.regulationRoleTypeDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.RegulationRoleTypeDescriptions.Add(new DBModels.RegulationRoleTypeDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.RegulationRoleTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.RegulationRoleTypeDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.RegulationRoleTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.RegulationRoleTypeDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Proccess regulation Role Combinations
                foreach (var d in item.regulationRoleCombinations)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.RegulationRoleCombinationss.Add(new DBModels.RegulationRoleCombinations(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.RegulationRoleCombinationss.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.RegulationRoleCombinationss.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.RegulationRoleCombinationss.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.RegulationRoleCombinationss.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessRegulationRoleType

        #region ProccessMeasurementUnit
        private bool ProccessMeasurementUnit(Scraping.MeasurementUnit item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MeasurementUnits.Add(new DBModels.MeasurementUnit(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MeasurementUnits.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MeasurementUnits.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MeasurementUnits.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MeasurementUnits.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess measurement Unit Description
                foreach (var d in item.measurementUnitDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasurementUnitDescriptions.Add(new DBModels.MeasurementUnitDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasurementUnitDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasurementUnitDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasurementUnitDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasurementUnitDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessMeasurementUnit

        #region ProccessPublicationSigle
        private bool ProccessPublicationSigle(Scraping.PublicationSigle item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.PublicationSigles.Add(new DBModels.PublicationSigle(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.PublicationSigles.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.PublicationSigles.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.PublicationSigles.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.PublicationSigles.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessMeasurementUnit

        #region ProccessExportRefundNomenclature
        private bool ProccessExportRefundNomenclature(Scraping.ExportRefundNomenclature item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.ExportRefundNomenclatures.Add(new DBModels.ExportRefundNomenclature(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.ExportRefundNomenclatures.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.ExportRefundNomenclatures.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.ExportRefundNomenclatures.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.ExportRefundNomenclatures.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Export Refund Nomenclature Description Periods
                foreach (exportRefundNomenDescriptionPeriod certificateDP in item.exportRefundNomenclatureDescriptionPeriod)
                {
                    switch (certificateDP.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.ExportRefundNomenclatureDescriptionPeriods.Add(new DBModels.ExportRefundNomenclatureDescriptionPeriod(certificateDP, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.ExportRefundNomenclatureDescriptionPeriods.Where(x => x.hjid == certificateDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(certificateDP, fileName);
                                dbContext.ExportRefundNomenclatureDescriptionPeriods.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.ExportRefundNomenclatureDescriptionPeriods.Where(x => x.hjid == certificateDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.ExportRefundNomenclatureDescriptionPeriods.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                    //Proccess export Refund Nomenclature Description
                    foreach (Description desc in certificateDP.exportRefundNomenclatureDescription)
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.ExportRefundNomenclatureDescriptions.Add(new DBModels.ExportRefundNomenclatureDescription(desc, certificateDP.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.ExportRefundNomenclatureDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.ExportRefundNomenclatureDescriptions.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.ExportRefundNomenclatureDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.ExportRefundNomenclatureDescriptions.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }

                //Proccess Export Refund Nomen Footnotes Association
                foreach (ExportRefundNomenFootnotesAssoc d in item.footnoteAssociationErn)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.ExportRefundNomenFootnotesAssociations.Add(new DBModels.ExportRefundNomenFootnotesAssociation(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.ExportRefundNomenFootnotesAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.ExportRefundNomenFootnotesAssociations.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.ExportRefundNomenFootnotesAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.ExportRefundNomenFootnotesAssociations.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Proccess Export Refund Nomen Indents
                foreach (ExportRefundNomenIndents d in item.exportRefundNomenclatureIndents)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.ExportRefundNomenIndentss.Add(new DBModels.ExportRefundNomenIndents(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.ExportRefundNomenIndentss.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.ExportRefundNomenIndentss.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.ExportRefundNomenIndentss.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.ExportRefundNomenIndentss.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Certificates");
            }

            return success;
        }

        #endregion ProccessExportRefundNomenclature

        #region ProccessMeasureConditionCode
        private bool ProccessMeasureConditionCode(Scraping.MeasureConditionCode item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MeasureConditionCodes.Add(new DBModels.MeasureConditionCode(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MeasureConditionCodes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MeasureConditionCodes.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MeasureConditionCodes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MeasureConditionCodes.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess measure Condition Code Description
                foreach (var d in item.measureConditionCodeDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasureConditionCodeDescriptions.Add(new DBModels.MeasureConditionCodeDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasureConditionCodeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasureConditionCodeDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasureConditionCodeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasureConditionCodeDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessMeasureConditionCode

        #region ProccessDutyExpression
        private bool ProccessDutyExpression(Scraping.DutyExpression item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.DutyExpressions.Add(new DBModels.DutyExpression(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.DutyExpressions.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.DutyExpressions.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.DutyExpressions.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.DutyExpressions.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess duty Expression Description
                foreach (var d in item.dutyExpressionDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.DutyExpressionDescriptions.Add(new DBModels.DutyExpressionDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.DutyExpressionDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.DutyExpressionDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.DutyExpressionDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.DutyExpressionDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessDutyExpression

        #region ProccessMeasureAction
        private bool ProccessMeasureAction(Scraping.MeasureAction item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MeasureActions.Add(new DBModels.MeasureAction(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MeasureActions.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MeasureActions.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MeasureActions.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MeasureActions.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess measure Action Description
                foreach (var d in item.measureActionDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasureActionDescriptions.Add(new DBModels.MeasureActionDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasureActionDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasureActionDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasureActionDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasureActionDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessMeasureAction

        #region ProccessRegulationGroup
        private bool ProccessRegulationGroup(Scraping.RegulationGroup item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.RegulationGroups.Add(new DBModels.RegulationGroup(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.RegulationGroups.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.RegulationGroups.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.RegulationGroups.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.RegulationGroups.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess regulation Group Description
                foreach (var d in item.regulationGroupDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.RegulationGroupDescriptions.Add(new DBModels.RegulationGroupDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.RegulationGroupDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.RegulationGroupDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.RegulationGroupDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.RegulationGroupDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessRegulationGroup

        #region ProccessMeasureType
        private bool ProccessMeasureType(Scraping.MeasureType item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MeasureTypes.Add(new DBModels.MeasureType(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MeasureTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MeasureTypes.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MeasureTypes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MeasureTypes.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess measure Type Description
                foreach (var d in item.measureTypeDescription)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeasureTypeDescriptions.Add(new DBModels.MeasureTypeDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeasureTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeasureTypeDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeasureTypeDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeasureTypeDescriptions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessMeasureType

        #region ProccessGoodsNomenclature
        private bool ProccessGoodsNomenclature(Scraping.GoodsNomenclature item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.GoodsNomenclatures.Add(new DBModels.GoodsNomenclature(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.GoodsNomenclatures.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.GoodsNomenclatures.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.GoodsNomenclatures.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.GoodsNomenclatures.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess footnote Association Goods Nomenclature
                foreach (var d in item.footnoteAssociationGoodsNomenclature)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.GoodsNomenclatureFootnoteAssociations.Add(new DBModels.GoodsNomenclatureFootnoteAssociation(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.GoodsNomenclatureFootnoteAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.GoodsNomenclatureFootnoteAssociations.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.GoodsNomenclatureFootnoteAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.GoodsNomenclatureFootnoteAssociations.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Proccess goods Nomenclature Description Period
                foreach (goodsNomenDescriptionPeriod certificateDP in item.goodsNomenclatureDescriptionPeriod)
                {
                    switch (certificateDP.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.GoodsNomenclatureDescriptionPeriods.Add(new DBModels.GoodsNomenclatureDescriptionPeriod(certificateDP, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.GoodsNomenclatureDescriptionPeriods.Where(x => x.hjid == certificateDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(certificateDP, fileName);
                                dbContext.GoodsNomenclatureDescriptionPeriods.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.GoodsNomenclatureDescriptionPeriods.Where(x => x.hjid == certificateDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.GoodsNomenclatureDescriptionPeriods.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                    //Proccess goods Nomenclature Description
                    foreach (Description desc in certificateDP.goodsNomenclatureDescription)
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.GoodsNomenclatureDescriptions.Add(new DBModels.GoodsNomenclatureDescription(desc, certificateDP.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.GoodsNomenclatureDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.GoodsNomenclatureDescriptions.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.GoodsNomenclatureDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.GoodsNomenclatureDescriptions.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

                //Proccess Goods Nomenclature Indents
                foreach (var d in item.goodsNomenclatureIndents)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.GoodsNomenclatureIndentss.Add(new DBModels.GoodsNomenclatureIndents(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.GoodsNomenclatureIndentss.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.GoodsNomenclatureIndentss.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.GoodsNomenclatureIndentss.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.GoodsNomenclatureIndentss.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Proccess Goods Nomenclature Origin
                foreach (var d in item.goodsNomenclatureOrigin)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.GoodsNomenclatureOrigins.Add(new DBModels.GoodsNomenclatureOrigin(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.GoodsNomenclatureOrigins.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.GoodsNomenclatureOrigins.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.GoodsNomenclatureOrigins.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.GoodsNomenclatureOrigins.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                //Proccess Goods Nomenclature Successor
                foreach (var d in item.goodsNomenclatureSuccessor)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.GoodsNomenclatureSuccessors.Add(new DBModels.GoodsNomenclatureSuccessor(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.GoodsNomenclatureSuccessors.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.GoodsNomenclatureSuccessors.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.GoodsNomenclatureSuccessors.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.GoodsNomenclatureSuccessors.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessGoodsNomenclature

        #region ProccessProrogationRegulation
        private bool ProccessProrogationRegulation(Scraping.ProrogationRegulation item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.ProrogationRegulations.Add(new DBModels.ProrogationRegulation(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.ProrogationRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.ProrogationRegulations.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.ProrogationRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.ProrogationRegulations.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess prorogation Regulation Action
                foreach (var d in item.prorogationRegulationAction)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.ProrogationRegulationActions.Add(new DBModels.ProrogationRegulationAction(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.ProrogationRegulationActions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.ProrogationRegulationActions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.ProrogationRegulationActions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.ProrogationRegulationActions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessProrogationRegulation

        #region ProccessFullTemporaryStopRegulation
        private bool ProccessFullTemporaryStopRegulation(Scraping.FullTemporaryStopRegulation item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.FullTemporaryStopRegulations.Add(new DBModels.FullTemporaryStopRegulation(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.FullTemporaryStopRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.FullTemporaryStopRegulations.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.FullTemporaryStopRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.FullTemporaryStopRegulations.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }
                
                //Proccess Full Temporary Stop Regulation Action
                foreach (var d in item.ftsRegulationAction)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.FullTemporaryStopRegulationActions.Add(new DBModels.FullTemporaryStopRegulationAction(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.FullTemporaryStopRegulationActions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.FullTemporaryStopRegulationActions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.FullTemporaryStopRegulationActions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.FullTemporaryStopRegulationActions.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessFullTemporaryStopRegulation

        #region ProccessRegulationReplacement
        private bool ProccessRegulationReplacement(Scraping.RegulationReplacement item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.RegulationReplacements.Add(new DBModels.RegulationReplacement(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.RegulationReplacements.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.RegulationReplacements.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.RegulationReplacements.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.RegulationReplacements.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessRegulationReplacement

        #region ProccessMeursingAdditionalCode
        private bool ProccessMeursingAdditionalCode(Scraping.MeursingAdditionalCode item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MeursingAdditionalCodes.Add(new DBModels.MeursingAdditionalCode(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MeursingAdditionalCodes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MeursingAdditionalCodes.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MeursingAdditionalCodes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MeursingAdditionalCodes.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Meursing Cell Component
                foreach (var d in item.meursingCellComponent)
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeursingCellComponents.Add(new DBModels.MeursingCellComponent(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeursingCellComponents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeursingCellComponents.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeursingCellComponents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeursingCellComponents.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Proccess Language");
            }

            return success;
        }
        #endregion ProccessMeursingAdditionalCode

        #region ProccessQuotaOrderNumber
        private bool ProccessQuotaOrderNumber(Scraping.QuotaOrderNumber item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.QuotaOrderNumbers.Add(new DBModels.QuotaOrderNumber(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.QuotaOrderNumbers.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.QuotaOrderNumbers.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.QuotaOrderNumbers.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.QuotaOrderNumbers.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Quota Order Number Origin
                foreach (QuotaOrderNumberOrigin certificateDP in item.quotaOrderNumberOrigin)
                {
                    switch (certificateDP.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaOrderNumberOrigins.Add(new DBModels.QuotaOrderNumberOrigin(certificateDP, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaOrderNumberOrigins.Where(x => x.hjid == certificateDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(certificateDP, fileName);
                                dbContext.QuotaOrderNumberOrigins.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaOrderNumberOrigins.Where(x => x.hjid == certificateDP.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaOrderNumberOrigins.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                    //Proccess Quota Order Number Origin Exclusions
                    foreach (QuotaOrderNumberOriginExcl desc in certificateDP.quotaOrderNumberOriginExclusions)
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.QuotaOrderNumberOriginExclusionss.Add(new DBModels.QuotaOrderNumberOriginExclusions(desc, certificateDP.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.QuotaOrderNumberOriginExclusionss.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.QuotaOrderNumberOriginExclusionss.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.QuotaOrderNumberOriginExclusionss.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.QuotaOrderNumberOriginExclusionss.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                dbContext.SaveChanges();
                success = true;
            }
            catch (Exception e)
            {
                _logger.LogError($"Error saving Certificates");
            }

            return success;
        }

        #endregion ProccessQuotaOrderNumber
    }

    internal static class FindXmlElements
    {
        internal static bool IsActive { get; set; } = true;
        internal static StringBuilder XmlString { get; set; } = new StringBuilder();
        internal static bool GoodNominclatureBypass { get; set; } = true;
        internal static bool FootnoteBypass { get; set; } = false;
        internal static bool AdditionalCodeBypass { get; set; } = true;
        internal static bool FootnoteTypeBypass { get; set; } = true;
        internal static bool MeasureBypass { get; set; } = true;
        internal static bool MonetaryPlaceOfPublicationBypass { get; set; } = true;
        internal static bool MonetaryExchangePeriodBypass { get; set; } = true;
        internal static bool ExportRefundNomenclatureBypass { get; set; } = true;
        internal static bool QuotaOrderNumberBypass { get; set; } = true;
        internal static bool MeasureActionBypass { get; set; } = true;
        internal static bool MeasureConditionCodeBypass { get; set; } = true;
        internal static bool MeursingAdditionalCodeBypass { get; set; } = true;
        internal static bool MeursingTablePlanBypass { get; set; } = true;
        internal static bool RegulationReplacementBypass { get; set; } = true;
        internal static bool FullTemporaryStopRegulationBypass { get; set; } = true;
        internal static bool ProrogationRegulationBypass { get; set; } = true;
        internal static bool CompleteAbrogationRegulationBypass { get; set; } = true;
        internal static bool PublicationSigleBypass { get; set; } = true;
        internal static bool MonetaryUnitBypass { get; set; } = true;
        internal static bool GoodsNomenclatureBypass { get; set; } = true;
        internal static bool ModificationRegulationBypass { get; set; } = true;
        internal static bool BaseRegulationBypass { get; set; } = true;
        internal static bool DutyExpressionBypass { get; set; } = true;
        internal static bool GoodsNomenclatureGroupBypass { get; set; } = true;
        internal static bool GeographicalAreaBypass { get; set; } = true;
        internal static bool MeasureTypeBypass { get; set; } = true;
        internal static bool MeasurementUnitQualifierBypass { get; set; } = true;
        internal static bool MeasurementUnitBypass { get; set; } = true;
        internal static bool CertificateBypass { get; set; } = true;
        internal static bool RegulationGroupBypass { get; set; } = true;
        internal static bool MeasureTypeSeriesBypass { get; set; } = true;
        internal static bool LanguageBypass { get; set; } = true;
        internal static bool AdditionalCodeTypeBypass { get; set; } = true;
        internal static bool CertificateTypeBypass { get; set; } = true;
        internal static bool RegulationRoleTypeBypass { get; set; } = true;
        internal static bool QuotaDefinitionBypass { get; set; } = true;
        internal static bool ExplicitAbrogationRegulation { get; set; } = true;
    }
}
