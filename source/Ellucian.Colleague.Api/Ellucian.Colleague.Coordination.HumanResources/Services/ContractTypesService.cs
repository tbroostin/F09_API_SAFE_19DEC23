//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
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
    public class ContractTypesService : BaseCoordinationService, IContractTypesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public ContractTypesService(

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
        /// Gets all contract-types
        /// </summary>
        /// <returns>Collection of ContractTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ContractTypes>> GetContractTypesAsync(bool bypassCache = false)
        {
            var contractTypesCollection = new List<Ellucian.Colleague.Dtos.ContractTypes>();

            var contractTypesEntities = await _referenceDataRepository.GetHrStatusesAsync(bypassCache);
            if (contractTypesEntities != null && contractTypesEntities.Any())
            {
                foreach (var contractTypes in contractTypesEntities)
                {
                    contractTypesCollection.Add(ConvertContractTypesEntityToDto(contractTypes));
                }
            }
            return contractTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ContractTypes from its GUID
        /// </summary>
        /// <returns>ContractTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ContractTypes> GetContractTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertContractTypesEntityToDto((await _referenceDataRepository.GetHrStatusesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("contract-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("contract-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a HrStatuses domain entity to its corresponding ContractTypes DTO
        /// </summary>
        /// <param name="source">HrStatuses domain entity</param>
        /// <returns>ContractTypes DTO</returns>
        private Ellucian.Colleague.Dtos.ContractTypes ConvertContractTypesEntityToDto(HrStatuses source)
        {
            var contractTypes = new Ellucian.Colleague.Dtos.ContractTypes();

            contractTypes.Id = source.Guid;
            contractTypes.Code = source.Code;
            contractTypes.Title = source.Description;
            contractTypes.Description = null;

            return contractTypes;
        }


    }
}