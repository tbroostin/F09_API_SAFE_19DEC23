using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using static System.String;

namespace Ellucian.Colleague.Coordination.F09.Services
{
    [RegisterType]
    public class F09TuitionPaymentPlanService : BaseCoordinationService, IF09TuitionPaymentPlanService
    {
        private readonly ITuitionPaymentPlanRepository _repository;

        public F09TuitionPaymentPlanService(IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository, ILogger logger, ITuitionPaymentPlanRepository repository,
            IStaffRepository staffRepository = null, IConfigurationRepository configurationRepository = null)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository
                )
        {
            _repository = repository;
        }

        public async Task<Dtos.F09.F09PaymentFormDto> GetTuitionPaymentFormAsync(string studentId)
        {
            if (IsNullOrWhiteSpace(studentId)) { throw  new ArgumentNullException(nameof(studentId));}

            var domain = await _repository.GetTuitionFormAsync(studentId);
            var adapter = _adapterRegistry.GetAdapter<Domain.F09.Entities.F09PaymentForm, Dtos.F09.F09PaymentFormDto>();
            return adapter.MapToType(domain);
        }
    }
}
