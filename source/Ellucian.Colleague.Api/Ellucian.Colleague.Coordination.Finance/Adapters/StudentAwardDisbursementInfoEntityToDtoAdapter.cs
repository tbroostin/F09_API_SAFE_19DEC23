// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class StudentAwardDisbursementInfoEntityToDtoAdapter : AutoMapperAdapter<Domain.Finance.Entities.AccountActivity.StudentAwardDisbursementInfo, Dtos.Finance.AccountActivity.StudentAwardDisbursementInfo>
    {
        /// <summary>
        /// Constructor for the custom StudentAwardDisbursementInfoAdapter
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public StudentAwardDisbursementInfoEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            :base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.Finance.Entities.AccountActivity.StudentAwardDisbursement, Dtos.Finance.AccountActivity.StudentAwardDisbursement>();
        }
    }
}
