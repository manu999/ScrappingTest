﻿using System;
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
                var fileName = "C:\\Users\\malzugaray\\Documents\\export-20181201T000000-20181201T000500\\export-20181201T000000-20181201T000500.xml";
                //var fileName = "C:\\Users\\malzugaray\\Documents\\export-20210101T000000-20210101T000500\\export-20210101T000000-20210101T000500.xml";
                //var fileName = "C:\\Users\\malzugaray\\source\\repos\\ScrapingTest\\Scraping\\testMessage.xml";
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
                            case "DutyExpressions":
                                if (FindXmlElements.DutyExpressionBypass) break;
                                var dutyExpressionsstring = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(dutyExpressionsstring))
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
                            case "MonetaryUnit":
                                if (FindXmlElements.MonetaryUnitBypass) break;
                                var monetaryUnitsstring = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(monetaryUnitsstring))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MonetaryUnit));
                                            var xmlObject = (Scraping.MonetaryUnit)serializer.Deserialize(stringReader);

                                            ProccessMonetaryUnit(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "AdditionalCode":
                                if (FindXmlElements.AdditionalCodeBypass) break;
                                var additionalCode = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(additionalCode))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.AdditionalCode));
                                            var xmlObject = (Scraping.AdditionalCode)serializer.Deserialize(stringReader);

                                            ProcessAditionalCode(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;

                            case "BaseRegulation":
                                if (FindXmlElements.BaseRegulationBypass) break;
                                var baseRegulations = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(baseRegulations))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.BaseRegulation));
                                            var xmlObject = (Scraping.BaseRegulation)serializer.Deserialize(stringReader);

                                            ProccessBaseRegulations(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;

                            case "ModificationRegulation":
                                if (FindXmlElements.ModificationRegulationBypass) break;
                                var modificationRegulation = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(modificationRegulation))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.ModificationRegulation));
                                            var xmlObject = (Scraping.ModificationRegulation)serializer.Deserialize(stringReader);

                                            ProcessModificationRegulation(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MonetaryPlaceOfPublication":
                                if (FindXmlElements.MonetaryPlaceOfPublicationBypass) break;
                                var monetaryPlaceOfPublication = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(monetaryPlaceOfPublication))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MonetaryPlaceOfPublication));
                                            var xmlObject = (Scraping.MonetaryPlaceOfPublication)serializer.Deserialize(stringReader);

                                            ProcessMonetaryPlaceOfPublication(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MonetaryExchangePeriod":
                                if (FindXmlElements.MonetaryExchangePeriodBypass) break;
                                var monetaryExchangePeriod = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(monetaryExchangePeriod))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MonetaryExchangePeriod));
                                            var xmlObject = (Scraping.MonetaryExchangePeriod)serializer.Deserialize(stringReader);

                                            ProcessMoneyExchangePeriod(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "QuotaDefinition":
                                if (FindXmlElements.QuotaDefinitionBypass) break;
                                var quotaDefinitions = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(quotaDefinitions))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.QuotaDefinition));
                                            var xmlObject = (Scraping.QuotaDefinition)serializer.Deserialize(stringReader);

                                            ProcessQuotaDefinition(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "MeursingTablePlan":
                                if (FindXmlElements.MeursingTablePlanBypass) break;
                                var meursingTablePLan = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(meursingTablePLan))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.MeursingTablePlan));
                                            var xmlObject = (Scraping.MeursingTablePlan)serializer.Deserialize(stringReader);

                                            ProcessMeursingTablePLan(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "ExplicitAbrogationRegulation":
                                if (FindXmlElements.ExplicitAbrogationRegulationBypass) break;
                                var explicitAbrogationRegulation = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(explicitAbrogationRegulation))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.ExplicitAbrogationRegulation));
                                            var xmlObject = (Scraping.ExplicitAbrogationRegulation)serializer.Deserialize(stringReader);

                                            ProcessExplicitAbrogationRegulation(xmlObject, dbContext, fileName);
                                        }
                                    }
                                    //Decrement active threads count
                                    Interlocked.Decrement(ref activeThreadCount);
                                });
                                break;
                            case "CompleteAbrogationRegulation":
                                if (FindXmlElements.CompleteAbrogationRegulationBypass) break;
                                var completeAbrogationRegulation = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(completeAbrogationRegulation))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.CompleteAbrogationRegulation));
                                            var xmlObject = (Scraping.CompleteAbrogationRegulation)serializer.Deserialize(stringReader);

                                            ProcessCompleteAbrogationRegulation(xmlObject, dbContext, fileName);
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
                foreach(footnoteDescriptionPeriod fnd in item.footnoteDescriptionPeriod ?? Enumerable.Empty<footnoteDescriptionPeriod>())
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
                    foreach (Description desc in fnd.footnoteDescription ?? Enumerable.Empty<Description>())
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
                foreach (Description d in item.footnoteTypeDescription ?? Enumerable.Empty<Description>())
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
                foreach (certificateDescriptionPeriod certificateDP in item.certificateDescriptionPeriod ?? Enumerable.Empty<certificateDescriptionPeriod>())
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
                    foreach (Description desc in certificateDP.certificateDescription ?? Enumerable.Empty<Description>())
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
                foreach (Description d in item.certificateTypeDescription ?? Enumerable.Empty<Description>())
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
                foreach (GeographicalAreaDescriptionPeriod geoAreaDP in item.geographicalAreaDescriptionPeriod ?? Enumerable.Empty<GeographicalAreaDescriptionPeriod>())
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
                    foreach (Description desc in geoAreaDP.geographicalAreaDescription ?? Enumerable.Empty<Description>())
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
                    foreach (geographicalAreaMembership gam in item.geographicalAreaMembership ?? Enumerable.Empty<geographicalAreaMembership>())
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
                foreach (DescriptionLang d in item.languageDescription ?? Enumerable.Empty<DescriptionLang>())
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
                foreach (var d in item.footnoteAssociationMeasure ?? Enumerable.Empty<ftnoteAssocMeasure>())
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
                foreach (var d in item.measureComponent ?? Enumerable.Empty<measureComponent>())
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
                foreach (var d in item.measureCondition ?? Enumerable.Empty<measureCondition>())
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
                    foreach (var c in d.measureConditionComponent ?? Enumerable.Empty<measureConditionComp>())
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
                foreach (var d in item.measureExcludedGeographicalArea ?? Enumerable.Empty<measureExlGeoArea>())
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
                foreach (var d in item.measurePartialTemporaryStop ?? Enumerable.Empty<measurePartTempStop>())
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
                _logger.LogError($"Error saving Proccess Meassure");
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
                foreach (var d in item.additionalCodeTypeDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.additionalCodeTypeMeasureType ?? Enumerable.Empty<additionalCodeTypeMeasureType>())
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
                _logger.LogError($"Error saving Proccess Aditional Code Type");
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
                foreach (var d in item.measureTypeSeriesDescription ?? Enumerable.Empty<Description>())
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
                _logger.LogError($"Error saving Meassure Type");
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
                foreach (var d in item.measurementUnitQualifierDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.goodsNomenclatureGroupDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.regulationRoleTypeDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.regulationRoleCombinations ?? Enumerable.Empty<regulationRoleCombinations>())
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
                foreach (var d in item.measurementUnitDescription ?? Enumerable.Empty<Description>())
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
                foreach (exportRefundNomenDescriptionPeriod certificateDP in item.exportRefundNomenclatureDescriptionPeriod ?? Enumerable.Empty<exportRefundNomenDescriptionPeriod>())
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
                    foreach (Description desc in certificateDP.exportRefundNomenclatureDescription ?? Enumerable.Empty<Description>())
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
                foreach (ExportRefundNomenFootnotesAssoc d in item.footnoteAssociationErn ?? Enumerable.Empty<ExportRefundNomenFootnotesAssoc>())
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
                foreach (ExportRefundNomenIndents d in item.exportRefundNomenclatureIndents ?? Enumerable.Empty<ExportRefundNomenIndents>())
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
                foreach (var d in item.measureConditionCodeDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.dutyExpressionDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.measureActionDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.regulationGroupDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.measureTypeDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.footnoteAssociationGoodsNomenclature ?? Enumerable.Empty<goodsNomenFootnotesAssoc>())
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
                foreach (goodsNomenDescriptionPeriod certificateDP in item.goodsNomenclatureDescriptionPeriod ?? Enumerable.Empty<goodsNomenDescriptionPeriod>())
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
                    foreach (Description desc in certificateDP.goodsNomenclatureDescription ?? Enumerable.Empty<Description>())
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
                foreach (var d in item.goodsNomenclatureIndents ?? Enumerable.Empty<GoodsNomenclatureIndents>())
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
                foreach (var d in item.goodsNomenclatureOrigin ?? Enumerable.Empty<GoodsNomenOrigin>())
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
                foreach (var d in item.goodsNomenclatureSuccessor ?? Enumerable.Empty<GoodsNomenSuccessor>())
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
                foreach (var d in item.prorogationRegulationAction ?? Enumerable.Empty<ProrogationRegulationAction>())
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
                foreach (var d in item.ftsRegulationAction ?? Enumerable.Empty<ftsRegulationAction>())
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
                foreach (var d in item.meursingCellComponent ?? Enumerable.Empty<MeursingCellComponent>())
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
                foreach (QuotaOrderNumberOrigin certificateDP in item.quotaOrderNumberOrigin ?? Enumerable.Empty<QuotaOrderNumberOrigin>())
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
                    foreach (QuotaOrderNumberOriginExcl desc in certificateDP.quotaOrderNumberOriginExclusions ?? Enumerable.Empty<QuotaOrderNumberOriginExcl>())
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
        #region ProcessMonetaryExchangePeriod
        private bool ProcessMoneyExchangePeriod(Scraping.MonetaryExchangePeriod item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MonetaryExchangePeriods.Add(new DBModels.MonetaryExchangePeriod(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MonetaryExchangePeriods.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MonetaryExchangePeriods.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MonetaryExchangePeriods.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MonetaryExchangePeriods.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }
                foreach (monetaryExchangeRate d in item.monetaryExchangeRate ?? Enumerable.Empty<monetaryExchangeRate>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MonetaryExchangeRates.Add(new DBModels.MonetaryExchangeRates(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MonetaryExchangeRates.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MonetaryExchangeRates.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MonetaryExchangeRates.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MonetaryExchangeRates.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception)
            {
                _logger.LogError($"Error saving Monetary Exchange Period");
            }

            return success;
        }
        #endregion

        #region ProcessQuotaDefinition
        private bool ProcessQuotaDefinition(Scraping.QuotaDefinition item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.QuotaDefinitions.Add(new DBModels.QuotaDefinition(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.QuotaDefinitions.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.QuotaDefinitions.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.QuotaDefinitions.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.QuotaDefinitions.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }
                foreach (quotaAssociation d in item.quotaAssociation ?? Enumerable.Empty<quotaAssociation>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaAssociations.Add(new DBModels.QuotaAssociation(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaAssociations.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaAssociations.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                foreach (quotaBalanceEvent d in item.quotaBalanceEvent ?? Enumerable.Empty<quotaBalanceEvent>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaBalanceEvents.Add(new DBModels.QuotaBalanceEvent(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaBalanceEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaBalanceEvents.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaBalanceEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaBalanceEvents.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                foreach (quotaBlockingPeriod d in item.quotaBlockingPeriod ?? Enumerable.Empty<quotaBlockingPeriod>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaBlockingPeriods.Add(new DBModels.QuotaBlockingPeriod(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaBlockingPeriods.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaBlockingPeriods.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaBlockingPeriods.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaBlockingPeriods.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
                foreach (quotaClosedTransEvent d in item.quotaClosedAndTransferredEvent ?? Enumerable.Empty<quotaClosedTransEvent>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaClosedTransferredEvents.Add(new DBModels.QuotaClosedTransferredEvent(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaClosedTransferredEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaClosedTransferredEvents.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaClosedTransferredEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaClosedTransferredEvents.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                }
                foreach (quotaCriticalEvent d in item.quotaCriticalEvent ?? Enumerable.Empty<quotaCriticalEvent>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaCriticalEvents.Add(new DBModels.QuotaCriticalEvent(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaCriticalEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaCriticalEvents.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaCriticalEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaCriticalEvents.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                }
                foreach (quotaExhaustionEvent d in item.quotaExhaustionEvent ?? Enumerable.Empty<quotaExhaustionEvent>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaExhaustionEvents.Add(new DBModels.QuotaExhaustionEvent(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaExhaustionEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaExhaustionEvents.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaExhaustionEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaExhaustionEvents.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                }
                foreach (quotaExtendedInformation d in item.quotaExtendedInformation ?? Enumerable.Empty<quotaExtendedInformation>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaExtendedInformations.Add(new DBModels.QuotaExtendedInformation(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaExtendedInformations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaExtendedInformations.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaExtendedInformations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaExtendedInformations.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                }
                foreach (quotaReopeningEvent d in item.quotaReopeningEvent ?? Enumerable.Empty<quotaReopeningEvent>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaReopeningEvents.Add(new DBModels.QuotaReopeningEvent(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaReopeningEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaReopeningEvents.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaReopeningEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaReopeningEvents.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                }
                foreach (quotaSuspensionPeriod d in item.quotaSuspensionPeriod ?? Enumerable.Empty<quotaSuspensionPeriod>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaSuspensionPeriods.Add(new DBModels.QuotaSuspensionPeriod(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaSuspensionPeriods.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaSuspensionPeriods.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaSuspensionPeriods.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaSuspensionPeriods.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                }
                foreach (quotaUnblockingEvent d in item.quotaUnblockingEvent ?? Enumerable.Empty<quotaUnblockingEvent>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaUnblockingEvents.Add(new DBModels.QuotaUnblockingEvent(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaUnblockingEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaUnblockingEvents.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaUnblockingEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaUnblockingEvents.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                }

                foreach (quotaUnsuspensionEvent d in item.quotaUnsuspensionEvent ?? Enumerable.Empty<quotaUnsuspensionEvent>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.QuotaUnsuspensionEvents.Add(new DBModels.QuotaUnsuspensionEvent(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.QuotaUnsuspensionEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.QuotaUnsuspensionEvents.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.QuotaUnsuspensionEvents.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.QuotaUnsuspensionEvents.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }

                }
            }
            catch (Exception)
            {
                _logger.LogError($"Error saving Quota Definitions");
            }

            return success;
        }
        #endregion

        #region ProcessMeursingTablePLan
        private bool ProcessMeursingTablePLan(Scraping.MeursingTablePlan item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MeursingTablePlans.Add(new DBModels.MeursingTablePlan(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MeursingTablePlans.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MeursingTablePlans.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MeursingTablePlans.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MeursingTablePlans.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                foreach (meursingHeading d in item.meursingHeading ?? Enumerable.Empty<meursingHeading>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MeursingHeadings.Add(new DBModels.MeursingHeading(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MeursingHeadings.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MeursingHeadings.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MeursingHeadings.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MeursingHeadings.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                    //Proccess Additional Code Descriptions
                    foreach (Description desc in d.meursingHeadingText ?? Enumerable.Empty<Description>())
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.MeursingHeadingDescriptions.Add(new DBModels.MeursingHeadingDescription(desc, d.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.MeursingHeadingDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.MeursingHeadingDescriptions.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.MeursingHeadingDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.MeursingHeadingDescriptions.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                    foreach (meursingSubheading desc in d.meursingSubheading ?? Enumerable.Empty<meursingSubheading>())
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.MeursingSubheadings.Add(new DBModels.MeursingSubheading(desc, d.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.MeursingSubheadings.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.MeursingSubheadings.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.MeursingSubheadings.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.MeursingSubheadings.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                    foreach (meursingHeadingFootnotesAssoc desc in d.footnoteAssociationMeursingHeading ?? Enumerable.Empty<meursingHeadingFootnotesAssoc>())
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.MeursingHeadingFootnotesAssociations.Add(new DBModels.MeursingHeadingFootnotesAssociation(desc, d.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.MeursingHeadingFootnotesAssociations.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.MeursingHeadingFootnotesAssociations.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.MeursingHeadingFootnotesAssociations.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.MeursingHeadingFootnotesAssociations.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
            }
            catch (Exception)
            {
                _logger.LogError($"Error saving Meursing Table Plans");
            }

            return success;
        }
        #endregion

        #region ProcessRegulationReplacement
        private bool ProcessRegulationReplacement(Scraping.RegulationReplacement item, DBContext dbContext, string fileName = "")
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
            }
            catch (Exception)
            {
                _logger.LogError($"Error saving Regulation Replacement");
            }

            return success;
        }

        #endregion

        #region ProcessExplicitAbrogationRegulation
        private bool ProcessExplicitAbrogationRegulation(Scraping.ExplicitAbrogationRegulation item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.ExplicitAbrogationRegulations.Add(new DBModels.ExplicitAbrogationRegulation(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.ExplicitAbrogationRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.ExplicitAbrogationRegulations.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.ExplicitAbrogationRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.ExplicitAbrogationRegulations.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                _logger.LogError($"Error saving Explicit Abrogation Regulations");
            }

            return success;
        }
        #endregion

        #region ProcessCompleteAbrogationRegulation
        private bool ProcessCompleteAbrogationRegulation(Scraping.CompleteAbrogationRegulation item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.CompleteAbrogationRegulations.Add(new DBModels.CompleteAbrogationRegulation(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.CompleteAbrogationRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.CompleteAbrogationRegulations.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.CompleteAbrogationRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.CompleteAbrogationRegulations.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                _logger.LogError($"Error saving Complete Abrogation Regulations");
            }

            return success;
        }
        #endregion

        #region ProcessMonetaryUnit
        private bool ProccessMonetaryUnit(Scraping.MonetaryUnit item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MonetaryUnits.Add(new DBModels.MonetaryUnit(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MonetaryUnits.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MonetaryUnits.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MonetaryUnits.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MonetaryUnits.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess monetary unit description
                foreach (Description d in item.monetaryUnitDescription ?? Enumerable.Empty<Description>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MonetaryUnitDescriptions.Add(new DBModels.MonetaryUnitDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MonetaryUnitDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MonetaryUnitDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MonetaryUnitDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MonetaryUnitDescriptions.Remove(dbObject);
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
                _logger.LogError($"Error saving Monetary Units");
            }

            return success;
        }
        #endregion

        #region ProcessAditionalCode
        private bool ProcessAditionalCode(Scraping.AdditionalCode item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.AdditionalCodes.Add(new DBModels.AdditionalCode(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.AdditionalCodes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.AdditionalCodes.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.AdditionalCodes.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.AdditionalCodes.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Additional Code Description Periods
                foreach (additionalCodeDescriptionPeriod fnd in item.additionalCodeDescriptionPeriod ?? Enumerable.Empty<additionalCodeDescriptionPeriod>())
                {
                    switch (fnd.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.AdditionalCodeDescriptionPeriods.Add(new DBModels.AdditionalCodeDescriptionPeriod(fnd, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.AdditionalCodeDescriptionPeriods.Where(x => x.hjid == fnd.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(fnd, fileName);
                                dbContext.AdditionalCodeDescriptionPeriods.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.AdditionalCodeDescriptionPeriods.Where(x => x.hjid == fnd.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.AdditionalCodeDescriptionPeriods.Remove(dbObject);
                            }
                            break;
                        default:
                            break;
                    }
                    //Proccess Additional Code Descriptions
                    foreach (Description desc in fnd.additionalCodeDescription ?? Enumerable.Empty<Description>())
                    {
                        switch (desc.metainfo.opType)
                        {
                            case OpType.C:
                                dbContext.AdditionalCodeDescriptions.Add(new DBModels.AdditionalCodeDescription(desc, fnd.hjid, fileName));
                                break;
                            case OpType.U:
                                var dbObject = dbContext.AdditionalCodeDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbObject.UpdateFields(desc, fileName);
                                    dbContext.AdditionalCodeDescriptions.Update(dbObject);
                                }
                                break;
                            case OpType.D:
                                dbObject = dbContext.AdditionalCodeDescriptions.Where(x => x.hjid == desc.hjid).FirstOrDefault();
                                if (dbObject != null)
                                {
                                    dbContext.AdditionalCodeDescriptions.Remove(dbObject);
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                foreach (ftnoteAssocAddCode d in item.footnoteAssociationAdditionalCode ?? Enumerable.Empty<ftnoteAssocAddCode>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.AdditionalCodeFootnoteAssociations.Add(new DBModels.AdditionalCodeFootnoteAssociation(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.AdditionalCodeFootnoteAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.AdditionalCodeFootnoteAssociations.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.AdditionalCodeFootnoteAssociations.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.AdditionalCodeFootnoteAssociations.Remove(dbObject);
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
                _logger.LogError($"Error saving Descriptions");
            }

            return success;
        }

        #endregion

        #region ProcessBaseRegulations
        private bool ProccessBaseRegulations(Scraping.BaseRegulation item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.BaseRegulations.Add(new DBModels.BaseRegulation(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.BaseRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.BaseRegulations.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.BaseRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.BaseRegulations.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                _logger.LogError($"Error saving Base Regulations");
            }

            return success;
        }
        #endregion

        #region ProcessModificationRegulation
        private bool ProcessModificationRegulation(Scraping.ModificationRegulation item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.ModificationRegulations.Add(new DBModels.ModificationRegulation(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.ModificationRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.ModificationRegulations.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.ModificationRegulations.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.ModificationRegulations.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                _logger.LogError($"Error saving Modification Regulations");
            }

            return success;
        }
        #endregion

        #region ProcessMonetaryPlaceOfPublication
        private bool ProcessMonetaryPlaceOfPublication(Scraping.MonetaryPlaceOfPublication item, DBContext dbContext, string fileName = "")
        {
            bool success = false;
            try
            {
                switch (item.metainfo.opType)
                {
                    case OpType.C:
                        dbContext.MonetaryPlaceOfPublications.Add(new DBModels.MonetaryPlaceOfPublication(item, fileName));
                        break;
                    case OpType.U:
                        var dbObject = dbContext.MonetaryPlaceOfPublications.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbObject.UpdateFields(item, fileName);
                            dbContext.MonetaryPlaceOfPublications.Update(dbObject);
                        }
                        break;
                    case OpType.D:
                        dbObject = dbContext.MonetaryPlaceOfPublications.Where(x => x.hjid == item.hjid).FirstOrDefault();
                        if (dbObject != null)
                        {
                            dbContext.MonetaryPlaceOfPublications.Remove(dbObject);
                        }
                        break;
                    default:
                        break;
                }

                //Proccess Monetary Place of Publications Description
                foreach (Description d in item.monetaryPlaceOfPublicationDescription ?? Enumerable.Empty<Description>())
                {
                    switch (d.metainfo.opType)
                    {
                        case OpType.C:
                            dbContext.MonetaryPlaceOfPublicationDescriptions.Add(new DBModels.MonetaryPlaceOfPublicationDescription(d, item.hjid, fileName));
                            break;
                        case OpType.U:
                            var dbObject = dbContext.MonetaryPlaceOfPublicationDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbObject.UpdateFields(d, fileName);
                                dbContext.MonetaryPlaceOfPublicationDescriptions.Update(dbObject);
                            }
                            break;
                        case OpType.D:
                            dbObject = dbContext.MonetaryPlaceOfPublicationDescriptions.Where(x => x.hjid == d.hjid).FirstOrDefault();
                            if (dbObject != null)
                            {
                                dbContext.MonetaryPlaceOfPublicationDescriptions.Remove(dbObject);
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
                _logger.LogError($"Error saving Monetary place of publication");
            }

            return success;
        }
        #endregion
    }

    internal static class FindXmlElements
    {
        internal static bool IsActive { get; set; } = true;
        internal static StringBuilder XmlString { get; set; } = new StringBuilder();
        internal static bool GoodNominclatureBypass { get; set; } = false;
        internal static bool FootnoteBypass { get; set; } = false;
        internal static bool AdditionalCodeBypass { get; set; } = false;
        internal static bool FootnoteTypeBypass { get; set; } = false;
        internal static bool MeasureBypass { get; set; } = false;
        internal static bool MonetaryPlaceOfPublicationBypass { get; set; } = false;
        internal static bool MonetaryExchangePeriodBypass { get; set; } = false;
        internal static bool ExportRefundNomenclatureBypass { get; set; } = false;
        internal static bool QuotaOrderNumberBypass { get; set; } = false;
        internal static bool MeasureActionBypass { get; set; } = false;
        internal static bool MeasureConditionCodeBypass { get; set; } = false;
        internal static bool MeursingAdditionalCodeBypass { get; set; } = false;
        internal static bool MeursingTablePlanBypass { get; set; } = false;
        internal static bool RegulationReplacementBypass { get; set; } = false;
        internal static bool FullTemporaryStopRegulationBypass { get; set; } = false;
        internal static bool ProrogationRegulationBypass { get; set; } = false;
        internal static bool CompleteAbrogationRegulationBypass { get; set; } = false;
        internal static bool PublicationSigleBypass { get; set; } = false;
        internal static bool MonetaryUnitBypass { get; set; } = false;
        internal static bool GoodsNomenclatureBypass { get; set; } = false;
        internal static bool ModificationRegulationBypass { get; set; } = false;
        internal static bool BaseRegulationBypass { get; set; } = false;
        internal static bool DutyExpressionBypass { get; set; } = false;
        internal static bool GoodsNomenclatureGroupBypass { get; set; } = false;
        internal static bool GeographicalAreaBypass { get; set; } = false;
        internal static bool MeasureTypeBypass { get; set; } = false;
        internal static bool MeasurementUnitQualifierBypass { get; set; } = false;
        internal static bool MeasurementUnitBypass { get; set; } = false;
        internal static bool CertificateBypass { get; set; } = false;
        internal static bool RegulationGroupBypass { get; set; } = false;
        internal static bool MeasureTypeSeriesBypass { get; set; } = false;
        internal static bool LanguageBypass { get; set; } = false;
        internal static bool AdditionalCodeTypeBypass { get; set; } = false;
        internal static bool CertificateTypeBypass { get; set; } = false;
        internal static bool RegulationRoleTypeBypass { get; set; } = false;
        internal static bool QuotaDefinitionBypass { get; set; } = false;
        internal static bool ExplicitAbrogationRegulationBypass { get; set; } = false;
    }
}
