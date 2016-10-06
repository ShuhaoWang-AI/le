using System.Data.Entity;
using Linko.LinkoExchange.Core.Domain;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Linko.LinkoExchange.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base ("DefaultConnection", throwIfV1Schema: false) 
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext ();
        }


        public DbSet<EmailTemplate> EmailTemplates { get; set; }
    }
}