//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
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
    public class FinancialAidFundClassificationsService : BaseCoordinationService, IFinancialAidFundClassificationsService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository configurationRepository;

        public FinancialAidFundClassificationsService(
            IStudentReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all financial-aid-fund-classifications
        /// </summary>
        /// <returns>Collection of FinancialAidFundClassifications DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidFundClassifications>> GetFinancialAidFundClassificationsAsync(bool bypassCache = false)
        {
            var financialAidFundClassificationsCollection = new List<Ellucian.Colleague.Dtos.FinancialAidFundClassifications>();

            var financialAidFundClassificationsEntities = await _referenceDataRepository.GetFinancialAidFundClassificationsAsync(bypassCache);
            if (financialAidFundClassificationsEntities != null && financialAidFundClassificationsEntities.Any())
            {
                foreach (var financialAidFundClassifications in financialAidFundClassificationsEntities)
                {
                    financialAidFundClassificationsCollection.Add(ConvertFinancialAidFundClassificationsEntityToDto(financialAidFundClassifications));
                }
            }
            return financialAidFundClassificationsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a FinancialAidFundClassifications from its GUID
        /// </summary>
        /// <returns>FinancialAidFundClassifications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FinancialAidFundClassifications> GetFinancialAidFundClassificationsByGuidAsync(string guid)
        {
            try
            {
                return ConvertFinancialAidFundClassificationsEntityToDto((await _referenceDataRepository.GetFinancialAidFundClassificationsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("financial-aid-fund-classifications not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("financial-aid-fund-classifications not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FinancialAidFundClassifications domain entity to its corresponding FinancialAidFundClassifications DTO
        /// </summary>
        /// <param name="source">FinancialAidFundClassifications domain entity</param>
        /// <returns>FinancialAidFundClassifications DTO</returns>
        private Ellucian.Colleague.Dtos.FinancialAidFundClassifications ConvertFinancialAidFundClassificationsEntityToDto(FinancialAidFundClassification source)
        {
            var financialAidFundClassifications = new Ellucian.Colleague.Dtos.FinancialAidFundClassifications();

            financialAidFundClassifications.Id = source.Guid;
            financialAidFundClassifications.Code = source.Code;
            financialAidFundClassifications.Title = source.Description;
            financialAidFundClassifications.Description = source.Description2;           
                                                                        
            return financialAidFundClassifications;
        }

      
    }
}