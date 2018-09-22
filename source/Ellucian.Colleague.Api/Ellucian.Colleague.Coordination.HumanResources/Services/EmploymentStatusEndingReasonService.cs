//Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
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
    public class EmploymentStatusEndingReasonService : BaseCoordinationService, IEmploymentStatusEndingReasonService
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
        public EmploymentStatusEndingReasonService(
            IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this._referenceDataRepository = referenceDataRepository;
        }

        /// <summary>
        /// Gets all employment status ending reasons
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.EmploymentStatusEndingReason>> GetEmploymentStatusEndingReasonsAsync(bool bypassCache = false)
        {
            var employmentStatusEndingReasonsCollection = new List<Dtos.EmploymentStatusEndingReason>();

            var employeeTerminationReasonEntities = await _referenceDataRepository.GetEmploymentStatusEndingReasonsAsync(bypassCache);
            if (employeeTerminationReasonEntities != null && employeeTerminationReasonEntities.Any())
            {
                foreach (var employeeTerminationReason in employeeTerminationReasonEntities)
                {
                    employmentStatusEndingReasonsCollection.Add(ConvertEmploymentStatusEndingReasonEntityToDto(employeeTerminationReason));
                }
            }
            return employmentStatusEndingReasonsCollection.Any() ? employmentStatusEndingReasonsCollection : null;
        }

        /// <summary>
        /// A employment status ending reason by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Dtos.EmploymentStatusEndingReason> GetEmploymentStatusEndingReasonByIdAsync(string id)
        {
            try
            {
                var entities = await _referenceDataRepository.GetEmploymentStatusEndingReasonsAsync(true);

                var entity = entities.FirstOrDefault(i => i.Guid.Equals(id));
                if (entity == null)
                {
                    throw new KeyNotFoundException("Employment status ending reason not found for GUID " + id);
                }
                return ConvertEmploymentStatusEndingReasonEntityToDto(entity);
            }
            catch (KeyNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("Employment status ending reason not found for GUID " + id, ex);
            }
        }

        /// <summary>
        /// Converts domain entity into dto.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Dtos.EmploymentStatusEndingReason ConvertEmploymentStatusEndingReasonEntityToDto(Domain.HumanResources.Entities.EmploymentStatusEndingReason source)
        {
            var employmentStatusEndingReason = new Dtos.EmploymentStatusEndingReason();

            employmentStatusEndingReason.Id = source.Guid;
            employmentStatusEndingReason.Code = source.Code;
            employmentStatusEndingReason.Title = source.Description;
            employmentStatusEndingReason.Description = null;

            return employmentStatusEndingReason;
        }
    }
}
