﻿using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PermissionGroupTemplateMap : EntityTypeConfiguration<PermissionGroupTemplate>
    {
        public PermissionGroupTemplateMap()
        {
            ToTable("tPermissionGroupTemplate");

            HasKey(x => x.PermissionGroupTemplateId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            HasRequired(a => a.OrganizationTypeRegulatoryProgram)
                .WithMany(b => b.PermissionGroupTemplates)
                .HasForeignKey(c => c.OrganizationTypeRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}