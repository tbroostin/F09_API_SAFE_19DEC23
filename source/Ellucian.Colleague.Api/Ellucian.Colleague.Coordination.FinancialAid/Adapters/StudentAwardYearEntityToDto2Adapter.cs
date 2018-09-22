/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates*/
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom adapter to convert StudentAwardYear entity to StudentAwardYear2 DTO
    /// </summary>
    public class StudentAwardYearEntityToDto2Adapter : AutoMapperAdapter<Domain.FinancialAid.Entities.StudentAwardYear, Dtos.Student.StudentAwardYear2>
    {

        public StudentAwardYearEntityToDto2Adapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.AwardLetterHistoryItem, Dtos.Student.AwardLetterHistoryItem>();            
        }
    }
}
    
