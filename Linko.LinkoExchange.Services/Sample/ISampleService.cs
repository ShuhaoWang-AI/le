using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Sample
{
    public interface ISampleService
    {
        IEnumerable<SampleDto> GetSamples(SampleStatusName status, DateTime? startDate = null, DateTime? endDate = null);
        int SaveSample(SampleDto sample);
        void DeleteSample(int sampleId);
        SampleDto GetSampleDetails(int sampleId);
        bool IsSampleIncludedInReportPackage(int sampleId);
    }
}
