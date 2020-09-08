//Copyright 2016-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PayrollDeductionArrangementChangeReasonsService : BaseCoordinationService, IPayrollDeductionArrangementChangeReasonsService
    {
        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        /// <summary>
        /// ..ctor
        /// </summary>
        /// <param name="referenceDataRepository"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public PayrollDeductionArrangementChangeReasonsService(
            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IConfigurationRepository configurationRepository,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            this._referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Gets all payroll deduction arrangement change reasons.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.PayrollDeductionArrangementChangeReason>> GetPayrollDeductionArrangementChangeReasonsAsync(bool bypassCache = false)
        {
            var payrollDeductionArrangementChangeReasons = new List<Dtos.PayrollDeductionArrangementChangeReason>();

            var payrollDeductionArrangementChangeReasonEntities = await _referenceDataRepository.GetPayrollDeductionArrangementChangeReasonsAsync(bypassCache);
            if (payrollDeductionArrangementChangeReasonEntities != null && payrollDeductionArrangementChangeReasonEntities.Any())
            {
                foreach (var payrollDeductionArrangementChangeReason in payrollDeductionArrangementChangeReasonEntities)
                {
                    payrollDeductionArrangementChangeReasons.Add(ConvertPayrollDeductionArrangementChangeReasonEntityToDto(payrollDeductionArrangementChangeReason));
                }
            }
            return payrollDeductionArrangementChangeReasons.Any() ? payrollDeductionArrangementChangeReasons : null;
        }

        /// <summary>
        /// Gets a payroll deduction arrangement change reason by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.PayrollDeductionArrangementChangeReason> GetPayrollDeductionArrangementChangeReasonByIdAsync(string id)
        {
            try
            {
                var entities = await _referenceDataRepository.GetPayrollDeductionArrangementChangeReasonsAsync(true);

                var entity = entities.FirstOrDefault(i => i.Guid.Equals(id));
                if (entity == null)
                {
                    throw new KeyNotFoundException("Payroll deduction arrangement change reason not found for GUID " + id);
                }
                return ConvertPayrollDeductionArrangementChangeReasonEntityToDto(entity);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw new Exception("Unknown error getting payroll deduction arrangement change reasons.");
            }
        }

        /// <summary>
        /// Converts domain entity into dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.PayrollDeductionArrangementChangeReason ConvertPayrollDeductionArrangementChangeReasonEntityToDto(Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason source)
        {
            var payrollDeductionArrangementChangeReason = new Dtos.PayrollDeductionArrangementChangeReason();

            payrollDeductionArrangementChangeReason.Id = source.Guid;
            payrollDeductionArrangementChangeReason.Code = source.Code;
            payrollDeductionArrangementChangeReason.Title = source.Description;
            payrollDeductionArrangementChangeReason.Description = null;

            return payrollDeductionArrangementChangeReason;
        }
    }
}
