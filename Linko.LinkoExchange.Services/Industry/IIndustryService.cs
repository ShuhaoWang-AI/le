using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Industry
{
    public class IndustryDTO
    {
        public int OrgRegProgId { get; set; }

    }

    public interface IIndustryService
    {
        IndustryDTO GetIndustryById(int orgRegProgId);
        
        /// When enabling, we need to check the parent (RegulatorOrganizationId)
        /// to see if there are any available licenses left
        ///
        /// Otherwise throw exception
        void UpdateEnableDisableFlag(int orgRegProgId, bool isEnabled);

        List<IndustryDTO> GetIndustriesForRegulator(int regOrgId);

        void AddIndustry(int regOrdId, IndustryDTO industry);

        void UpdateIndustry(IndustryDTO industry);
    }
}
