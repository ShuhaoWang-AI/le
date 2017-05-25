using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Enum
{
    public enum ReportStatusName
    {
        Draft, // for Industry
        ReadyToSubmit, // for Industry
        Submitted, // for Industry
        Repudiated, // for Industry
        SubmittedPendingReview, // for Authority
        RepudiatedPendingReview, // for Authority
        All // for Authority
    }
}
