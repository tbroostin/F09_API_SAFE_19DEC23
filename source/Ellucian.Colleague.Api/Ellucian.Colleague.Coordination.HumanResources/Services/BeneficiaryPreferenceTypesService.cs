//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
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

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class BeneficiaryPreferenceTypesService : BaseCoordinationService, IBeneficiaryPreferenceTypesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public BeneficiaryPreferenceTypesService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
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
        /// Gets all beneficiary-preference-types
        /// </summary>
        /// <returns>Collection of BeneficiaryPreferenceTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes>> GetBeneficiaryPreferenceTypesAsync(bool bypassCache = false)
        {
            var beneficiaryPreferenceTypesCollection = new List<Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes>();

            var beneficiaryPreferenceTypesEntities = await _referenceDataRepository.GetBeneficiaryTypesAsync(bypassCache);
            if (beneficiaryPreferenceTypesEntities != null && beneficiaryPreferenceTypesEntities.Any())
            {
                foreach (var beneficiaryPreferenceTypes in beneficiaryPreferenceTypesEntities)
                {
                    beneficiaryPreferenceTypesCollection.Add(ConvertBeneficiaryPreferenceTypesEntityToDto(beneficiaryPreferenceTypes));
                }
            }
            return beneficiaryPreferenceTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a BeneficiaryPreferenceTypes from its GUID
        /// </summary>
        /// <returns>BeneficiaryPreferenceTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes> GetBeneficiaryPreferenceTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertBeneficiaryPreferenceTypesEntityToDto((await _referenceDataRepository.GetBeneficiaryTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("beneficiary-preference-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("beneficiary-preference-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a BeneficiaryTypes domain entity to its corresponding BeneficiaryPreferenceTypes DTO
        /// </summary>
        /// <param name="source">BeneficiaryTypes domain entity</param>
        /// <returns>BeneficiaryPreferenceTypes DTO</returns>
        private Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes ConvertBeneficiaryPreferenceTypesEntityToDto(BeneficiaryTypes source)
        {
            var beneficiaryPreferenceTypes = new Ellucian.Colleague.Dtos.BeneficiaryPreferenceTypes();

            beneficiaryPreferenceTypes.Id = source.Guid;
            beneficiaryPreferenceTypes.Code = source.Code;
            beneficiaryPreferenceTypes.Title = source.Description;
            beneficiaryPreferenceTypes.Description = null;           
                                                                        
            return beneficiaryPreferenceTypes;
        }

      
    }
   
}