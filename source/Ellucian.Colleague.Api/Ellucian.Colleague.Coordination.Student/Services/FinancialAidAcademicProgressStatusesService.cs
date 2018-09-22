//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class FinancialAidAcademicProgressStatusesService : BaseCoordinationService, IFinancialAidAcademicProgressStatusesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public FinancialAidAcademicProgressStatusesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all financial-aid-academic-progress-statuses
        /// </summary>
        /// <returns>Collection of FinancialAidAcademicProgressStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressStatuses>> GetFinancialAidAcademicProgressStatusesAsync(RestrictedVisibility? restrictedVisibilityValue = null, bool bypassCache = false)
        {
            var financialAidAcademicProgressStatusesCollection = new List<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressStatuses>();
            var spaStatuses = new List<SapStatuses>();
            var newRestrictedVisibilityValue = string.Empty;
            if (restrictedVisibilityValue.HasValue && restrictedVisibilityValue.Value == RestrictedVisibility.Yes)
            {
                newRestrictedVisibilityValue = "D";
            }
            var financialAidAcademicProgressStatusesEntities = await _referenceDataRepository.GetSapStatusesAsync(newRestrictedVisibilityValue, bypassCache);
            
            if (financialAidAcademicProgressStatusesEntities != null && financialAidAcademicProgressStatusesEntities.Any())
            {
                foreach (var financialAidAcademicProgressStatusesEntity in financialAidAcademicProgressStatusesEntities)
                {
                    financialAidAcademicProgressStatusesCollection.Add(ConvertFinancialAidAcademicProgressStatusesEntityToDto(financialAidAcademicProgressStatusesEntity));
                }
            }
            return financialAidAcademicProgressStatusesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FinancialAidAcademicProgressStatuses from its GUID
        /// </summary>
        /// <returns>FinancialAidAcademicProgressStatuses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressStatuses> GetFinancialAidAcademicProgressStatusesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertFinancialAidAcademicProgressStatusesEntityToDto((await _referenceDataRepository.GetSapStatusesAsync("", bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("financial-aid-academic-progress-statuses not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("financial-aid-academic-progress-statuses not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a SapStatuses domain entity to its corresponding FinancialAidAcademicProgressStatuses DTO
        /// </summary>
        /// <param name="source">SapStatuses domain entity</param>
        /// <returns>FinancialAidAcademicProgressStatuses DTO</returns>
        private Ellucian.Colleague.Dtos.FinancialAidAcademicProgressStatuses ConvertFinancialAidAcademicProgressStatusesEntityToDto(SapStatuses source)
        {
            var financialAidAcademicProgressStatuses = new Ellucian.Colleague.Dtos.FinancialAidAcademicProgressStatuses();

            financialAidAcademicProgressStatuses.Id = source.Guid;
            financialAidAcademicProgressStatuses.Code = source.Code;
            financialAidAcademicProgressStatuses.Title = source.Description;
            financialAidAcademicProgressStatuses.Description = null;

            return financialAidAcademicProgressStatuses;
        }
    }
}