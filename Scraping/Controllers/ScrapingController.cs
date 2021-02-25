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
                            case "RegulationReplacement":
                                if (FindXmlElements.RegulationReplacementBypass) break;
                                var regulationReplacement = (XNode.ReadFrom(reader) as XElement).ToString();
                                Task.Run(() =>
                                {
                                    //Increment active threads count
                                    Interlocked.Increment(ref activeThreadCount);
                                    using (DBContext dbContext = new DBContext())
                                    {
                                        using (StringReader stringReader = new StringReader(regulationReplacement))
                                        {
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.RegulationReplacement));
                                            var xmlObject = (Scraping.RegulationReplacement)serializer.Deserialize(stringReader);

                                            ProcessRegulationReplacement(xmlObject, dbContext, fileName);
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
                                            XmlSerializer serializer = new XmlSerializer(typeof(Scraping.explicitAbrogationRegulation));
                                            var xmlObject = (Scraping.explicitAbrogationRegulation)serializer.Deserialize(stringReader);

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
                foreach (geographicalAreaDescriptionPeriod geoAreaDP in item.geographicalAreaDescriptionPeriod)
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

                //Proccess additionalCodeTypeDescription
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

                //Proccess additionalCodeTypeMeasureType
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
                _logger.LogError($"Error saving Proccess Aditional Code Type");
            }

            return success;
        }
        #endregion ProccessMeasure

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

                //Proccess measureTypeSeriesDescription
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
                _logger.LogError($"Error saving Meassure Type");
            }

            return success;
        }
        #endregion ProccessMeasureTypeSeries

        #region ProcessDutyExpression
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

                //Proccess dutyExpressionDescription
                foreach (Description d in item.dutyExpressionDescription)
                    
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
                _logger.LogError($"Error saving Duty Expressions");
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
                foreach (Description d in item.monetaryUnitDescription)
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
                foreach (additionalCodeDescriptionPeriod fnd in item.additionalCodeDescriptionPeriod)
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
                    foreach (Description desc in fnd.additionalCodeDescription)
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
                foreach (ftnoteAssocAddCode d in item.footnoteAssociationAdditionalCode)
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
                foreach (Description d in item.monetaryPlaceOfPublicationDescription)
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

        #region ProcessMoneyExchangePeriod
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
                foreach (quotaAssociation d in item.quotaAssociation)
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
                foreach (quotaBalanceEvent d in item.quotaBalanceEvent)
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
                foreach (quotaBlockingPeriod d in item.quotaBlockingPeriod)
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
                foreach (quotaClosedTransEvent d in item.quotaClosedAndTransferredEvent)
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
                foreach (quotaCriticalEvent d in item.quotaCriticalEvent)
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
                foreach (quotaExhaustionEvent d in item.quotaExhaustionEvent)
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
                foreach (quotaExtendedInformation d in item.quotaExtendedInformation)
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
                foreach (quotaReopeningEvent d in item.quotaReopeningEvent)
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
                foreach (quotaSuspensionPeriod d in item.quotaSuspensionPeriod)
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
                foreach (quotaUnblockingEvent d in item.quotaUnblockingEvent)
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

                foreach (quotaUnsuspensionEvent d in item.quotaUnsuspensionEvent)
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

                foreach (meursingHeading d in item.meursingHeading)
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
                    foreach (Description desc in d.meursingHeadingText)
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
                    foreach (meursingSubheading desc in d.meursingSubheading)
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
                    foreach (meursingHeadingFootnotesAssoc desc in d.footnoteAssociationMeursingHeading)
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
        private bool ProcessExplicitAbrogationRegulation(Scraping.explicitAbrogationRegulation item, DBContext dbContext, string fileName = "")
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
        internal static bool ExplicitAbrogationRegulationBypass { get; set; } = true;
    }
}
