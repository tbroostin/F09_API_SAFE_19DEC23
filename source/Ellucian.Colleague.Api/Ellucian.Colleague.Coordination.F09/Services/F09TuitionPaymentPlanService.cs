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
using Ellucian.Colleague.Dtos.F09;
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

        public async Task<Dtos.F09.F09PaymentFormDto> GetTuitionPaymentFormAsync(string studentId, F09TuitionPaymentPlanType type = F09TuitionPaymentPlanType.Enroll)
        {
            if (IsNullOrWhiteSpace(studentId)) { throw  new ArgumentNullException(nameof(studentId));}


            var domain = await new Lazy<Task<Domain.F09.Entities.F09PaymentForm>>(async () =>
            {
                switch (type)
                {
                    case F09TuitionPaymentPlanType.Change:
                        return await _repository.GetChangeTuitionFormAsync(studentId);
                    case F09TuitionPaymentPlanType.Enroll:
                    default:
                        return await _repository.GetTuitionFormAsync(studentId);
                }
            }).Value;
            var adapter = _adapterRegistry.GetAdapter<Domain.F09.Entities.F09PaymentForm, Dtos.F09.F09PaymentFormDto>();
            return adapter.MapToType(domain);
        }

        public async Task<Dtos.F09.F09PaymentInvoiceDto> SubmitTuitionPaymentFormAsync(Dtos.F09.F09TuitionPaymentPlanDto dto)
        {
            await ValidatePaymentPlan(dto);

            var domainAdapter =
                _adapterRegistry
                    .GetAdapter<Dtos.F09.F09TuitionPaymentPlanDto, Domain.F09.Entities.F09TuitionPaymentPlan>();
            var domain = domainAdapter.MapToType(dto);
            var invoice = await _repository.SubmitTuitionFormAsync(domain);

            var adapter =
                _adapterRegistry.GetAdapter<Domain.F09.Entities.F09PaymentInvoice, Dtos.F09.F09PaymentInvoiceDto>();
            return adapter.MapToType(invoice);
        }

        public async Task<string> SubmitTuitionChangeFormAsync(Dtos.F09.F09TuitionPaymentPlanDto dto)
        {
            await ValidatePaymentPlan(dto,true);

            var domainAdapter =
                _adapterRegistry
                    .GetAdapter<Dtos.F09.F09TuitionPaymentPlanDto, Domain.F09.Entities.F09TuitionPaymentPlan>();
            var domain = domainAdapter.MapToType(dto);
            var changeInfo = await _repository.SubmitChangeTuitionFormAsync(domain);

            return changeInfo;
        }

        private async Task ValidatePaymentPlan(Dtos.F09.F09TuitionPaymentPlanDto dto, bool allowNullPaymentMethod = false)
        {
            if(dto == null) throw new ArgumentNullException(nameof(dto));
            if(IsNullOrWhiteSpace(dto.StudentId)) throw new ArgumentNullException(nameof(dto.StudentId), "Student Id is Required");

            var paymentForm = await GetTuitionPaymentFormAsync(dto.StudentId);

            // validate the options
            if ((allowNullPaymentMethod || !String.IsNullOrWhiteSpace(dto.PaymentMethod)) && !(paymentForm.PaymentMethods.ContainsKey(dto.PaymentMethod) ||
                  paymentForm.PaymentMethods.ContainsValue(dto.PaymentMethod)))
            {
                var err = $"Selected payment method '{dto.PaymentMethod}' is not valid";
                logger.Error(err);
                throw new ArgumentException(err);
            }

            if (!paymentForm.PaymentOptions.ContainsKey(dto.PaymentOption))
            {
                var err = $"Selected payment option '{dto.PaymentOption}' is not valid";
                logger.Error(err);
                throw new ArgumentException(err);
            }

        }
    }
}

