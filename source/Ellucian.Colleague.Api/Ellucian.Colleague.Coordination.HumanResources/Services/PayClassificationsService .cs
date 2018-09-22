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
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PayClassificationsService : BaseCoordinationService, IPayClassificationsService
    {
        private readonly IHumanResourcesReferenceDataRepository _ReferenceDataRepository;
        private readonly IPayClassificationsRepository _PayClassificationsRepository;

        public PayClassificationsService(

            IPayClassificationsRepository payClassificationsRepository,
			IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _PayClassificationsRepository = payClassificationsRepository;
            _ReferenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all pay-classifications
        /// </summary>
        /// <returns>Collection of PayClassifications DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PayClassifications>> GetPayClassificationsAsync(bool bypassCache = false)
        {
            var payClassificationsCollection = new List<Ellucian.Colleague.Dtos.PayClassifications>();

            var payClassificationsEntities = await _PayClassificationsRepository.GetPayClassificationsAsync(bypassCache);
            if (payClassificationsEntities != null && payClassificationsEntities.Any())
            {
                foreach (var payClassifications in payClassificationsEntities)
                {
                    payClassificationsCollection.Add(ConvertPayClassificationsEntityToDto(payClassifications));
                }
            }
            return payClassificationsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PayClassifications from its GUID
        /// </summary>
        /// <returns>PayClassifications DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PayClassifications> GetPayClassificationsByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "The GUID is a required argument.");
            }
            try
            {
                return ConvertPayClassificationsEntityToDto((await _PayClassificationsRepository.GetPayClassificationsByIdAsync(guid)));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No pay classification was found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No pay classification was found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all pay-classifications
        /// </summary>
        /// <returns>Collection of PayClassifications2 DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PayClassifications2>> GetPayClassifications2Async(bool bypassCache = false)
        {
            var payClassificationsCollection = new List<Ellucian.Colleague.Dtos.PayClassifications2>();

            var payClassificationsEntities = await _PayClassificationsRepository.GetPayClassificationsAsync(bypassCache);
            if (payClassificationsEntities != null && payClassificationsEntities.Any())
            {
                foreach (var payClassifications in payClassificationsEntities)
                {
                    payClassificationsCollection.Add(ConvertPayClassificationsEntityToDto2(payClassifications));
                }
            }
            return payClassificationsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PayClassifications from its GUID
        /// </summary>
        /// <returns>PayClassifications2 DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PayClassifications2> GetPayClassificationsByGuid2Async(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "The GUID is a required argument.");
            }
            try
            {
                return ConvertPayClassificationsEntityToDto2((await _PayClassificationsRepository.GetPayClassificationsByIdAsync(guid)));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No pay classification was found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No pay classification was found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PayClassification domain entity to its corresponding PayClassifications DTO
        /// </summary>
        /// <param name="source">PayClassification domain entity</param>
        /// <returns>PayClassifications DTO</returns>
        private Ellucian.Colleague.Dtos.PayClassifications ConvertPayClassificationsEntityToDto(PayClassification source)
        {
            var payClassifications = new Ellucian.Colleague.Dtos.PayClassifications();

            payClassifications.Id = source.Guid;
            payClassifications.Code = source.Code;
            payClassifications.Title = source.Description;
            payClassifications.CompensationType = source.CompensationType == "H" ? PayClassificationsCompensationType.Wages : PayClassificationsCompensationType.Salary;
            payClassifications.ClassificationType = source.ClassificationType == "M" ? PayClassificationsClassificationType.Matrix : PayClassificationsClassificationType.Range;
            payClassifications.Status = source.Status == "A" ? PayClassificationsStatus.Active : PayClassificationsStatus.Inactive;
                                                                                                          
            return payClassifications;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PayClassification domain entity to its corresponding PayClassifications2 DTO
        /// </summary>
        /// <param name="source">PayClassification domain entity</param>
        /// <returns>PayClassifications2 DTO</returns>
        private Ellucian.Colleague.Dtos.PayClassifications2 ConvertPayClassificationsEntityToDto2(PayClassification source)
        {
            var payClassifications = new Ellucian.Colleague.Dtos.PayClassifications2();

            payClassifications.Id = source.Guid;
            payClassifications.Code = source.Code;
            payClassifications.Title = source.Description;
            payClassifications.CompensationType = new List<PayClassificationsCompensationType>() { source.CompensationType == "H" ? PayClassificationsCompensationType.Wages : PayClassificationsCompensationType.Salary };
            payClassifications.ClassificationType = source.ClassificationType == "M" ? PayClassificationsClassificationType.Matrix : PayClassificationsClassificationType.Range;
            payClassifications.Status = source.Status == "A" ? PayClassificationsStatus.Active : PayClassificationsStatus.Inactive;

            return payClassifications;
        }
    }
}