//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Coordination.HumanResoures.Services
{
    [RegisterType]
    public class PositionClassificationsService : BaseCoordinationService, IPositionClassificationsService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public PositionClassificationsService(

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
        /// Gets all position-classifications
        /// </summary>
        /// <returns>Collection of PositionClassifications DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PositionClassification>> GetPositionClassificationsAsync(bool bypassCache = false)
        {
            var positionClassificationsCollection = new List<Ellucian.Colleague.Dtos.PositionClassification>();

            var positionClassificationsEntities = await _referenceDataRepository.GetEmploymentClassificationsAsync(bypassCache);
            if (positionClassificationsEntities != null && positionClassificationsEntities.Any())
            {
                foreach (var positionClassifications in positionClassificationsEntities)
                {
                    positionClassificationsCollection.Add(ConvertPositionClassificationsEntityToDto(positionClassifications));
                }
            }
            return positionClassificationsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PositionClassifications from its GUID
        /// </summary>
        /// <returns>PositionClassifications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PositionClassification> GetPositionClassificationsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertPositionClassificationsEntityToDto((await _referenceDataRepository.GetEmploymentClassificationsAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No position classification found for guid {0}.", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No position classification found for guid {0}.", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Classifications domain entity to its corresponding PositionClassifications DTO
        /// </summary>
        /// <param name="source">Classifications domain entity</param>
        /// <returns>PositionClassifications DTO</returns>
        private Ellucian.Colleague.Dtos.PositionClassification ConvertPositionClassificationsEntityToDto(Domain.HumanResources.Entities.EmploymentClassification source)
        {
            var positionClassifications = new Ellucian.Colleague.Dtos.PositionClassification();

            positionClassifications.Id = source.Guid;
            positionClassifications.Code = source.Code;
            positionClassifications.Title = source.Description;
            positionClassifications.Description = null;

            return positionClassifications;
        }


    }
}