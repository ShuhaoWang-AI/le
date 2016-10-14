﻿using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
 
    class AuditLogTemplateMap : EntityTypeConfiguration<AuditLogTemplate>
    {
        public AuditLogTemplateMap()
        {
            ToTable("tAuditLogTemplate");

            HasKey(i => i.AuditLogTemplateId);

            HasKey(i => i.Name);

            Property(i => i.TemplateType); 

            Property(i => i.EventCategory);

            Property(i => i.EventType);

            Property(i => i.SubjectTemplate); 

            Property(i => i.MessageTemplate); 
        }
    }
}