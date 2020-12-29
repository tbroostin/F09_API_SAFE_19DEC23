using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class ProcurementReturnReasonService : BaseCoordinationService, IProcurementReturnReasonService
    {
        
        private readonly IReferenceDataRepository _referenceDataRepository;

        // This constructor initializes the private attributes.
        public ProcurementReturnReasonService(IReferenceDataRepository _referenceDataRepository,IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {            
            this._referenceDataRepository = _referenceDataRepository;
        }

        /// <summary>
        /// Gets all Procurement Return Reason Codes with descriptions
        /// </summary>
        /// <returns>Collection of Retunrn reason code</returns>
        public async Task<IEnumerable<Dtos.ColleagueFinance.ProcurementReturnReason>> GetProcurementReturnReasonsAsync()
        {
            var returnReasonCodeCollection = new List<Dtos.ColleagueFinance.ProcurementReturnReason>();

            var returnReasonCodesEntities = await _referenceDataRepository.GetItemConditionsAsync(true);
            if (returnReasonCodesEntities != null && returnReasonCodesEntities.Any())
            {
                //sort the entities on code, then by description
                returnReasonCodesEntities = returnReasonCodesEntities.OrderBy(x => x.Code);

                foreach (var returnReasonCodesEntity in returnReasonCodesEntities)
                {
                    //convert returnReasonCode entity to dto
                    returnReasonCodeCollection.Add(ConvertReturnReasonCodeEntityToDto(returnReasonCodesEntity));
                }
                
            }
            return returnReasonCodeCollection;
        }

        /// <summary>
        /// retuns ProcurementReturnReason DTO from ItemCondition entity
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        private ProcurementReturnReason ConvertReturnReasonCodeEntityToDto(ItemCondition condition) {
            ProcurementReturnReason reason = new ProcurementReturnReason();
            reason.Code = condition.Code;
            reason.Description = condition.Description;
            return reason;
        }
    }
}
