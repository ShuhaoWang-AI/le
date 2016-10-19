﻿using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationMap : EntityTypeConfiguration<Organization>
    {
        public OrganizationMap()
        {
            ToTable("tOrganization");

            HasKey(i => i.OrganizationId);
            Property(i => i.Name);
            Property(i => i.AddressLine1);
            Property(i => i.AddressLine2);
            Property(i => i.City).HasColumnName("CityName");
            Property(i => i.ZipCode);
        }
    }
}
