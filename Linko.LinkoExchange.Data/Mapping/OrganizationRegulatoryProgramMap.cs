﻿using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationRegulatoryProgramMap : EntityTypeConfiguration<OrganizationRegulatoryProgram>
    {
        #region constructors and destructor

        public OrganizationRegulatoryProgramMap()
        {
            ToTable(tableName:"tOrganizationRegulatoryProgram");

            HasKey(x => x.OrganizationRegulatoryProgramId);

            HasRequired(a => a.RegulatoryProgram)
                .WithMany(b => b.OrganizationRegulatoryPrograms)
                .HasForeignKey(c => c.RegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.Organization)
                .WithMany(b => b.OrganizationRegulatoryPrograms)
                .HasForeignKey(c => c.OrganizationId)
                .WillCascadeOnDelete(value:false);

            HasOptional(a => a.RegulatorOrganization)
                .WithMany(b => b.RegulatorOrganizationRegulatoryPrograms)
                .HasForeignKey(c => c.RegulatorOrganizationId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.AssignedTo).IsOptional().HasMaxLength(value:50);

            Property(x => x.ReferenceNumber).IsOptional().HasMaxLength(value:50);

            Property(x => x.IsEnabled).IsRequired();

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}