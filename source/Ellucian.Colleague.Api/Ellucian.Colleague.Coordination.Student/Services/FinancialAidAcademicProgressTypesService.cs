//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
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
    public class FinancialAidAcademicProgressTypesService : BaseCoordinationService, IFinancialAidAcademicProgressTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public FinancialAidAcademicProgressTypesService(

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
        /// Gets all financial-aid-academic-progress-types
        /// </summary>
        /// <returns>Collection of FinancialAidAcademicProgressTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes>> GetFinancialAidAcademicProgressTypesAsync(bool bypassCache = false)
        {
            var financialAidAcademicProgressTypesCollection = new List<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes>();

            var financialAidAcademicProgressTypesEntities = await _referenceDataRepository.GetSapTypesAsync(bypassCache);
            if (financialAidAcademicProgressTypesEntities != null && financialAidAcademicProgressTypesEntities.Any())
            {
                foreach (var financialAidAcademicProgressTypes in financialAidAcademicProgressTypesEntities)
                {
                    financialAidAcademicProgressTypesCollection.Add(ConvertFinancialAidAcademicProgressTypesEntityToDto(financialAidAcademicProgressTypes));
                }
            }
            return financialAidAcademicProgressTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FinancialAidAcademicProgressTypes from its GUID
        /// </summary>
        /// <returns>FinancialAidAcademicProgressTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes> GetFinancialAidAcademicProgressTypesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertFinancialAidAcademicProgressTypesEntityToDto((await _referenceDataRepository.GetSapTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("financial-aid-academic-progress-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("financial-aid-academic-progress-types not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Saptype domain entity to its corresponding FinancialAidAcademicProgressTypes DTO
        /// </summary>
        /// <param name="source">Saptype domain entity</param>
        /// <returns>FinancialAidAcademicProgressTypes DTO</returns>
        private Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes ConvertFinancialAidAcademicProgressTypesEntityToDto(Domain.Student.Entities.SapType source)
        {
            var financialAidAcademicProgressTypes = new Ellucian.Colleague.Dtos.FinancialAidAcademicProgressTypes();

            financialAidAcademicProgressTypes.Id = source.Guid;
            financialAidAcademicProgressTypes.Code = source.Code;
            financialAidAcademicProgressTypes.Title = source.Description;
            financialAidAcademicProgressTypes.Description = null;

            return financialAidAcademicProgressTypes;
        }
    }
}