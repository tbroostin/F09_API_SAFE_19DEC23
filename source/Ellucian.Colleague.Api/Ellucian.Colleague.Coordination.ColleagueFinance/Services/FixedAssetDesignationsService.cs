//Copyright 2020 Ellucian Company L.P. and its affiliates.

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
    public class FixedAssetDesignationsService : BaseCoordinationService, IFixedAssetDesignationsService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public FixedAssetDesignationsService(

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
        /// Gets all fixed-asset-designations
        /// </summary>
        /// <returns>Collection of FixedAssetDesignations DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FixedAssetDesignations>> GetFixedAssetDesignationsAsync(bool bypassCache = false)
        {
            var fixedAssetDesignationsCollection = new List<Ellucian.Colleague.Dtos.FixedAssetDesignations>();

            var fixedAssetDesignationsEntities = await _referenceDataRepository.GetFxaTransferFlagsAsync(bypassCache);
            if (fixedAssetDesignationsEntities != null && fixedAssetDesignationsEntities.Any())
            {
                foreach (var fixedAssetDesignations in fixedAssetDesignationsEntities)
                {
                    fixedAssetDesignationsCollection.Add(ConvertFixedAssetDesignationsEntityToDto(fixedAssetDesignations));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return fixedAssetDesignationsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a FixedAssetDesignations from its GUID
        /// </summary>
        /// <returns>FixedAssetDesignations DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.FixedAssetDesignations> GetFixedAssetDesignationsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertFixedAssetDesignationsEntityToDto((await _referenceDataRepository.GetFxaTransferFlagsAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No fixed-asset-designations was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No fixed-asset-designations was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a FxaTransferFlags domain entity to its corresponding FixedAssetDesignations DTO
        /// </summary>
        /// <param name="source">FxaTransferFlags domain entity</param>
        /// <returns>FixedAssetDesignations DTO</returns>
        private Ellucian.Colleague.Dtos.FixedAssetDesignations ConvertFixedAssetDesignationsEntityToDto(FxaTransferFlags source)
        {
            var fixedAssetDesignations = new Ellucian.Colleague.Dtos.FixedAssetDesignations();

            fixedAssetDesignations.Id = source.Guid;
            fixedAssetDesignations.Code = source.Code;
            fixedAssetDesignations.Title = source.Description;
            fixedAssetDesignations.Description = null;

            return fixedAssetDesignations;
        }


    }
}