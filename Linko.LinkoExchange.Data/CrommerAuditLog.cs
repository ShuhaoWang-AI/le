//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Linko.LinkoExchange.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class CrommerAuditLog
    {
        public string EventName { get; set; }
        public string Comments { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public int EventId { get; set; }
        public Nullable<int> AuthorityId { get; set; }
    }
}
