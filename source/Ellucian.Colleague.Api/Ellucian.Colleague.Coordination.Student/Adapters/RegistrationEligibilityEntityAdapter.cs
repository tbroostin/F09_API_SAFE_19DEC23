using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    public class RegistrationEligibilityEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibility, RegistrationEligibility>
    {
        public RegistrationEligibilityEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage, RegistrationMessage>();
            AddMappingDependency<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibilityTerm, Ellucian.Colleague.Dtos.Student.RegistrationEligibilityTerm>();
        }
    }
}
