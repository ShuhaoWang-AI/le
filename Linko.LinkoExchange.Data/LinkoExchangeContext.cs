using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Reflection;
using Linko.LinkoExchange.Core.Domain;
using Microsoft.AspNet.Identity.EntityFramework;
using Module = Linko.LinkoExchange.Core.Domain.Module;
using TimeZone = Linko.LinkoExchange.Core.Domain.TimeZone;

namespace Linko.LinkoExchange.Data
{
	public class LinkoExchangeContext : IdentityDbContext<UserProfile>
	{
		#region constructors and destructor

		public LinkoExchangeContext(string nameOrConnectionString)
			: base(nameOrConnectionString:nameOrConnectionString)
		{
			// Disable database initialization when the DB is not found
			Database.SetInitializer<LinkoExchangeContext>(strategy:null);
		}

		#endregion

		#region public properties

		public DbContextTransaction Transaction { get; private set; }

		private CustomTransactionWrapper CustomTransaction { get; set; }

		#endregion

		#region DbSets

		public DbSet<AuditLogTemplate> AuditLogTemplates { get; set; }
		public DbSet<CromerrAuditLog> CromerrAuditLogs { get; set; }
		public DbSet<EmailAuditLog> EmailAuditLogs { get; set; }
		public DbSet<Invitation> Invitations { get; set; }
		public DbSet<Jurisdiction> Jurisdictions { get; set; }
		public DbSet<Module> Modules { get; set; }
		public DbSet<Organization> Organizations { get; set; }
		public DbSet<OrganizationRegulatoryProgram> OrganizationRegulatoryPrograms { get; set; }
		public DbSet<OrganizationRegulatoryProgramModule> OrganizationRegulatoryProgramModules { get; set; }
		public DbSet<OrganizationRegulatoryProgramSetting> OrganizationRegulatoryProgramSettings { get; set; }
		public DbSet<OrganizationRegulatoryProgramUser> OrganizationRegulatoryProgramUsers { get; set; }
		public DbSet<OrganizationSetting> OrganizationSettings { get; set; }
		public DbSet<OrganizationType> OrganizationTypes { get; set; }
		public DbSet<OrganizationTypeRegulatoryProgram> OrganizationTypeRegulatoryPrograms { get; set; }
		public DbSet<Permission> Permissions { get; set; }
		public DbSet<PermissionGroup> PermissionGroups { get; set; }
		public DbSet<PermissionGroupPermission> PermissionGroupPermissions { get; set; }
		public DbSet<PermissionGroupTemplate> PermissionGroupTemplates { get; set; }
		public DbSet<PermissionGroupTemplatePermission> PermissionGroupTemplatePermissions { get; set; }
		public DbSet<Question> Questions { get; set; }
		public DbSet<QuestionType> QuestionTypes { get; set; }
		public DbSet<RegulatoryProgram> RegulatoryPrograms { get; set; }
		public DbSet<SettingTemplate> SettingTemplates { get; set; }
		public DbSet<SignatoryRequest> SignatoryRequests { get; set; }
		public DbSet<SignatoryRequestStatus> SignatoryRequestStatuses { get; set; }
		public DbSet<SystemSetting> SystemSettings { get; set; }
		public DbSet<TimeZone> TimeZones { get; set; }
		public DbSet<UserPasswordHistory> UserPasswordHistories { get; set; }
		public DbSet<UserQuestionAnswer> UserQuestionAnswers { get; set; }

		public DbSet<CtsEventType> CtsEventTypes { get; set; }
		public DbSet<Parameter> Parameters { get; set; }
		public DbSet<MonitoringPoint> MonitoringPoints { get; set; }
		public DbSet<MonitoringPointParameterLimit> MonitoringPointParameterLimits { get; set; }
		public DbSet<SampleFrequency> SampleFrequencies { get; set; }
		public DbSet<SampleRequirement> SampleRequirements { get; set; }
		public DbSet<MonitoringPointParameter> MonitoringPointParameters { get; set; }
		public DbSet<CollectionMethod> CollectionMethods { get; set; }
		public DbSet<Unit> Units { get; set; }

		public DbSet<CollectionMethodType> CollectionMethodTypes { get; set; }
		public DbSet<LimitBasis> LimitBases { get; set; }
		public DbSet<LimitType> LimitTypes { get; set; }

		public DbSet<ParameterGroup> ParameterGroups { get; set; }
		public DbSet<ParameterGroupParameter> ParameterGroupParameters { get; set; }

		public DbSet<FileType> FileTypes { get; set; }
		public DbSet<FileStore> FileStores { get; set; }
		public DbSet<FileStoreData> FileStoreDatas { get; set; }

		public DbSet<CopyOfRecordCertificate> CopyOfRecordCertificates { get; set; }
		public DbSet<CopyOfRecord> CopyOfRecords { get; set; }

		public DbSet<Sample> Samples { get; set; }
		public DbSet<SampleResult> SampleResults { get; set; }

		public DbSet<ReportElementType> ReportElementTypes { get; set; }
		public DbSet<ReportElementCategory> ReportElementCategories { get; set; }

		public DbSet<ReportPackageTemplateAssignment> ReportPackageTemplateAssignments { get; set; }
		public DbSet<ReportPackageTemplate> ReportPackageTempates { get; set; }
		public DbSet<ReportPackageTemplateElementType> ReportPackageTemplateElementTypes { get; set; }
		public DbSet<ReportPackageTemplateElementCategory> ReportPackageTemplateElementCategories { get; set; }

		public DbSet<RepudiationReason> RepudiationReasons { get; set; }
		public DbSet<ReportStatus> ReportStatuses { get; set; }
		public DbSet<ReportPackage> ReportPackages { get; set; }
		public DbSet<ReportPackageElementCategory> ReportPackageElementCategories { get; set; }
		public DbSet<ReportPackageElementType> ReportPackageElementTypes { get; set; }
		public DbSet<ReportFile> ReportFiles { get; set; }
		public DbSet<ReportSample> ReportSamples { get; set; }
		public DbSet<TermCondition> TermConditions { get; set; }
		public DbSet<PrivacyPolicy> PrivacyPolicies { get; set; }

		public DbSet<DataSource> DataSources { get; set; }
		public DbSet<DataSourceMonitoringPoint> DataSourceMonitoringPoints { get; set; }
		public DbSet<DataSourceCtsEventType> DataSourceCtsEventTypes { get; set; }
		public DbSet<DataSourceCollectionMethod> DataSourceCollectionMethods { get; set; }
		public DbSet<DataSourceParameter> DataSourceParameters { get; set; }
		public DbSet<DataSourceUnit> DataSourceUnits { get; set; }
		public DbSet<ImportTempFile> ImportTempFiles { get; set; }
		public DbSet<UnitDimension> UnitDimensions { get; set; }
		public DbSet<SystemUnit> SystemUnits { get; set; }

		public DbSet<DataOptionality> DataOptionalities { get; set; }
		public DbSet<DataFormat> DataFormats { get; set; }
		public DbSet<FileVersionTemplate> FileVersionTemplates { get; set; }
		public DbSet<SystemField> SystemFields { get; set; }
		public DbSet<FileVersionField> FileVersionFields { get; set; }
		public DbSet<FileVersion> FileVersions { get; set; }
		public DbSet<FileVersionTemplateField> FileVersionTemplateFields { get; set; }

		#endregion

		#region utilities

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
			                              .Where(type => !string.IsNullOrEmpty(value:type.Namespace))
			                              .Where(type => type.BaseType != null
			                                             && type.BaseType.IsGenericType
			                                             && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));
			foreach (var type in typesToRegister)
			{
				dynamic configurationInstance = Activator.CreateInstance(type:type);
				modelBuilder.Configurations.Add(configurationInstance);
			}

			base.OnModelCreating(modelBuilder:modelBuilder);

			modelBuilder.Entity<UserProfile>().ToTable(tableName:"tUserProfile");
		}

		public virtual DbContextTransaction BeginTransaction()
		{
			return Transaction ?? (Transaction = Database.BeginTransaction());
		}

		public void Commit()
		{
			if (Transaction == null)
			{
				throw new ArgumentNullException(paramName:$"Transaction");
			}

			try
			{
				Transaction?.Commit();
			}
			finally
			{
				Transaction.Dispose();
			}
		}

		[Obsolete]
		public virtual CustomTransactionScope CreateAutoCommitScope()
		{
			return BeginTranactionScope("Obsoleted");
		}

		public virtual CustomTransactionScope BeginTranactionScope(MethodBase from)
		{
			return BeginTranactionScope(from.DeclaringType?.Name + "." + from.Name);
		}

		private CustomTransactionScope BeginTranactionScope(string from)
		{
			if (CustomTransaction == null)
			{
				CustomTransaction = new CustomTransactionWrapper()
				{
					Transaction = BeginTransaction(),
					CallStacks = new Stack<string>()
				};
			}
			return new CustomTransactionScope(CustomTransaction, from);
		}

		#endregion
	}
}