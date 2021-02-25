using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Scraping.DBModels;

namespace Scraping
{
    public class DBContext : DbContext
    {
        private const string CONNECTION_STRING_ON_2005_10017 = @"server=(local);initial catalog=ScrapingDB;Integrated Security=True;MultipleActiveResultSets=True;App=EntityFramework;connection timeout=10";
         
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }
        public DBContext() : base(GetOptions(CONNECTION_STRING_ON_2005_10017)) { }

        private static DbContextOptions GetOptions(string connectionString)
        {
            return SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), connectionString).Options;
        }


        public DbSet<DBModels.Language> Languages { get; set; }
        public DbSet<DBModels.LanguageDescription> LanguageDescriptions { get; set; }
        public DbSet<DBModels.Footnote> Footnotes { get; set; }
        public DbSet<DBModels.FootnoteDescriptionPeriod> FootnoteDescriptionPeriods { get; set; }
        public DbSet<DBModels.FootnoteDescription> FootnoteDescriptions { get; set; }
        public DbSet<DBModels.FootnoteType> FootnoteTypes { get; set; }
        public DbSet<DBModels.FootnoteTypeDescription> FootnoteTypeDescriptions { get; set; }
        public DbSet<DBModels.Certificate> Certificates { get; set; }
        public DbSet<DBModels.CertificateDescriptionPeriod> CertificateDescriptionPeriods { get; set; }
        public DbSet<DBModels.CertificateDescription> CertificateDescriptions { get; set; }
        public DbSet<DBModels.CertificateType> CertificateTypes { get; set; }
        public DbSet<DBModels.CertificateTypeDescription> CertificateTypeDescriptions { get; set; }
        public DbSet<DBModels.GeographicalArea> GeographicalAreas { get; set; }
        public DbSet<DBModels.GeographicalAreaDescriptionPeriod> GeographicalAreaDescriptionPeriods { get; set; }
        public DbSet<DBModels.GeographicalAreaDescription> GeographicalAreaDescriptions { get; set; }
        public DbSet<DBModels.GeographicalAreaMembership> GeographicalAreaMemberships { get; set; }
        public DbSet<DBModels.Measure> Measures { get; set; }
        public DbSet<DBModels.MeasureAction> MeasureActions { get; set; }
        public DbSet<DBModels.MeasureActionDescription> MeasureActionDescriptions { get; set; }
        public DbSet<DBModels.MeasureComponent> MeasureComponents { get; set; }
        public DbSet<DBModels.MeasureCondition> MeasureConditions { get; set; }
        public DbSet<DBModels.MeasureConditionCode> MeasureConditionCodes { get; set; }
        public DbSet<DBModels.MeasureConditionCodeDescription> MeasureConditionCodeDescriptions { get; set; }
        public DbSet<DBModels.MeasureConditionComponent> MeasureConditionComponents { get; set; }
        public DbSet<DBModels.MeasureExcludedGeographicalArea> MeasureExcludedGeographicalAreas { get; set; }
        public DbSet<DBModels.MeasureFootnoteAssociation> MeasureFootnoteAssociations { get; set; }
        public DbSet<DBModels.MeasurePartialTemporaryStop> MeasurePartialTemporaryStops { get; set; }
        public DbSet<DBModels.AdditionalCodeType> AdditionalCodeTypes { get; set; }
        public DbSet<DBModels.AdditionalCodeTypeDescription> AdditionalCodeTypeDescriptions { get; set; }
        public DbSet<DBModels.AdditionalCodeTypeMeasureType> AdditionalCodeTypeMeasureTypes { get; set; }
        public DbSet<DBModels.MeasureTypeSeries> MeasureTypeSeries { get; set; }
        public DbSet<DBModels.MeasureTypeSeriesDescription> MeasureTypeSeriesDescriptions { get; set; }
        public DbSet<DBModels.DutyExpression> DutyExpressions { get; set; }
        public DbSet<DBModels.DutyExpressionDescription> DutyExpressionDescriptions { get; set; }
        public DbSet<DBModels.MonetaryUnit> MonetaryUnits { get; set; }
        public DbSet<DBModels.MonetaryUnitDescription> MonetaryUnitDescriptions { get; set; }
        public DbSet<DBModels.AdditionalCode> AdditionalCodes { get; set; }
        public DbSet<DBModels.AdditionalCodeDescription> AdditionalCodeDescriptions { get; set; }
        public DbSet<DBModels.AdditionalCodeDescriptionPeriod> AdditionalCodeDescriptionPeriods { get; set; }
        public DbSet<DBModels.AdditionalCodeFootnoteAssociation> AdditionalCodeFootnoteAssociations { get; set; }
        public DbSet<DBModels.BaseRegulation> BaseRegulations { get; set; }
        public DbSet<DBModels.ModificationRegulation> ModificationRegulations { get; set; }
        public DbSet<DBModels.MonetaryPlaceOfPublication> MonetaryPlaceOfPublications { get; set; }
        public DbSet<DBModels.MonetaryPlaceOfPublicationDescription> MonetaryPlaceOfPublicationDescriptions { get; set; }
        public DbSet<DBModels.MonetaryExchangePeriod> MonetaryExchangePeriods { get; set; }
        public DbSet<DBModels.QuotaDefinition> QuotaDefinitions { get; set; }
        public DbSet<DBModels.QuotaExhaustionEvent> QuotaExhaustionEvents { get; set; }
        public DbSet<DBModels.QuotaAssociation> QuotaAssociations { get; set; }
        public DbSet<DBModels.QuotaBalanceEvent> QuotaBalanceEvents { get; set; }
        public DbSet<DBModels.QuotaCriticalEvent> QuotaCriticalEvents { get; set; }
        public DbSet<DBModels.QuotaReopeningEvent> QuotaReopeningEvents { get; set; }
        public DbSet<DBModels.QuotaUnblockingEvent> QuotaUnblockingEvents { get; set; }
        public DbSet<DBModels.QuotaOrderNumber> QuotaOrderNumbers { get; set; }
        public DbSet<DBModels.QuotaUnsuspensionEvent> QuotaUnsuspensionEvents { get; set; }
        public DbSet<DBModels.QuotaSuspensionPeriod> QuotaSuspensionPeriods { get; set; }
        public DbSet<DBModels.QuotaClosedTransferredEvent> QuotaClosedTransferredEvents { get; set; }
        public DbSet<DBModels.QuotaExtendedInformation> QuotaExtendedInformations { get; set; }
        public DbSet<DBModels.QuotaBlockingPeriod> QuotaBlockingPeriods { get; set; }
        public DbSet<DBModels.MeursingTablePlan> MeursingTablePlans { get; set; }
        public DbSet<DBModels.MeursingHeading> MeursingHeadings { get; set; }
        public DbSet<DBModels.MeursingHeadingDescription> MeursingHeadingDescriptions { get; set; }
        public DbSet<DBModels.MeursingSubheading> MeursingSubheadings { get; set; }
        public DbSet<DBModels.MeursingHeadingFootnotesAssociation> MeursingHeadingFootnotesAssociations { get; set; }
        public DbSet<DBModels.RegulationReplacement> RegulationReplacements { get; set; }
        public DbSet<DBModels.ExplicitAbrogationRegulation> ExplicitAbrogationRegulations { get; set; }
        public DbSet<DBModels.CompleteAbrogationRegulation> CompleteAbrogationRegulations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}