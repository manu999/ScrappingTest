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
        public DbSet<DBModels.MeasurementUnitQualifier> MeasurementUnitQualifiers { get; set; }
        public DbSet<DBModels.MeasurementUnitQualifierDescription> MeasurementUnitQualifierDescriptions { get; set; }
        public DbSet<DBModels.GoodsNomenclatureGroup> GoodsNomenclatureGroups { get; set; }
        public DbSet<DBModels.GoodsNomenclatureGroupDescription> GoodsNomenclatureGroupDescriptions { get; set; }
        public DbSet<DBModels.RegulationRoleType> RegulationRoleTypes { get; set; }
        public DbSet<DBModels.RegulationRoleTypeDescription> RegulationRoleTypeDescriptions { get; set; }
        public DbSet<DBModels.MeasurementUnit> MeasurementUnits { get; set; }
        public DbSet<DBModels.MeasurementUnitDescription> MeasurementUnitDescriptions { get; set; }
        public DbSet<DBModels.PublicationSigle> PublicationSigles { get; set; }
        public DbSet<DBModels.ExportRefundNomenclature> ExportRefundNomenclatures { get; set; }
        public DbSet<DBModels.ExportRefundNomenclatureDescription> ExportRefundNomenclatureDescriptions { get; set; }
        public DbSet<DBModels.ExportRefundNomenclatureDescriptionPeriod> ExportRefundNomenclatureDescriptionPeriods { get; set; }
        public DbSet<DBModels.RegulationGroup> RegulationGroups { get; set; }
        public DbSet<DBModels.RegulationGroupDescription> RegulationGroupDescriptions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}