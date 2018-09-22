//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class FinancialDocumentTypesService : BaseCoordinationService, IFinancialDocumentTypesService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public FinancialDocumentTypesService(

            IColleagueFinanceReferenceDataRepository referenceDataRepository,
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
        /// Gets all financial-document-types
        /// </summary>
        /// <returns>Collection of FinancialDocumentTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialDocumentTypes>> GetFinancialDocumentTypesAsync(bool bypassCache = false)
        {
            var financialDocumentTypesCollection = new List<Ellucian.Colleague.Dtos.FinancialDocumentTypes>();

            var financialDocumentTypesEntities = await _referenceDataRepository.GetGlSourceCodesValcodeAsync(bypassCache);
            if (financialDocumentTypesEntities != null && financialDocumentTypesEntities.Any())
            {
                foreach (var financialDocumentTypes in financialDocumentTypesEntities)
                {
                    financialDocumentTypesCollection.Add(ConvertFinancialDocumentTypesEntityToDto(financialDocumentTypes));
                }
            }
            return financialDocumentTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FinancialDocumentTypes from its GUID
        /// </summary>
        /// <returns>FinancialDocumentTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialDocumentTypes> GetFinancialDocumentTypesByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                return ConvertFinancialDocumentTypesEntityToDto((await _referenceDataRepository.GetGlSourceCodesValcodeAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("financial-document-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("financial-document-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a GlSourceCodes domain entity to its corresponding FinancialDocumentTypes DTO
        /// </summary>
        /// <param name="source">GlSourceCodes domain entity</param>
        /// <returns>FinancialDocumentTypes DTO</returns>
        private Ellucian.Colleague.Dtos.FinancialDocumentTypes ConvertFinancialDocumentTypesEntityToDto(GlSourceCodes source)
        {
            var financialDocumentTypes = new Ellucian.Colleague.Dtos.FinancialDocumentTypes();

            financialDocumentTypes.Id = source.Guid;
            financialDocumentTypes.Code = source.Code;
            financialDocumentTypes.Title = source.Description;
            financialDocumentTypes.Description = null;

            return financialDocumentTypes;
        }


    }

}