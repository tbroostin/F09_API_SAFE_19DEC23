// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class InterestsService : BaseCoordinationService, IInterestsService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private const string _dataOrigin = "Colleague";

        public InterestsService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger,
                                         IConfigurationRepository configRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configRepository)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all interests
        /// </summary>
        /// <returns>Collection of Interest DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Interest>> GetHedmInterestsAsync(bool bypassCache = false)
        {
            var interestCollection = new List<Ellucian.Colleague.Dtos.Interest>();

            var interestEntities = await _referenceDataRepository.GetInterestsAsync(bypassCache);
            if (interestEntities != null && interestEntities.Count() > 0)
            {
                foreach (var interest in interestEntities)
                {
                    interestCollection.Add(await ConvertInterestEntityToInterestDtoAsync(interest));
                }
            }
            return interestCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all interest areas
        /// </summary>
        /// <returns>Collection of InterestArea DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.InterestArea>> GetInterestAreasAsync(bool bypassCache = false)
        {
            var interestTypeCollection = new List<Ellucian.Colleague.Dtos.InterestArea>();

            var interestTypeEntities = await _referenceDataRepository.GetInterestTypesAsync(bypassCache);
            if (interestTypeEntities != null && interestTypeEntities.Count() > 0)
            {
                foreach (var interestType in interestTypeEntities)
                {
                    interestTypeCollection.Add(ConvertInterestTypeEntityToInterestAreaDto(interestType));
                }
            }
            return interestTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an interest from its ID
        /// </summary>
        /// <returns>Interest DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Interest> GetHedmInterestByIdAsync(string id)
        {
            try
            {
                return await ConvertInterestEntityToInterestDtoAsync((await _referenceDataRepository.GetInterestsAsync(true)).Where(rt => rt.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Interest not found for ID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an interest area from its ID
        /// </summary>
        /// <returns>InterestArea DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.InterestArea> GetInterestAreasByIdAsync(string id)
        {
            try
            {
                return ConvertInterestTypeEntityToInterestAreaDto((await _referenceDataRepository.GetInterestTypesAsync(true)).Where(rt => rt.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Interest Areas not found for ID " + id, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Interest domain entity to its corresponding Interest DTO
        /// </summary>
        /// <param name="source">Interest domain entity</param>
        /// <returns>Interest DTO</returns>
        private async Task<Dtos.Interest> ConvertInterestEntityToInterestDtoAsync(Interest source)
        {
            var interest = new Dtos.Interest();
            if (source != null)
            {
                interest.Id = source.Guid;
                interest.Code = source.Code;
                interest.Title = source.Description;
                interest.Description = null;
          
                var interestType = (await _referenceDataRepository.GetInterestTypesAsync(false)).FirstOrDefault(x => x.Code == source.Type);
                string interestTypeGuid = (interestType != null) ? interestType.Guid : null;
                if (!string.IsNullOrEmpty(interestTypeGuid))
                    interest.Area = new Dtos.GuidObject2(interestTypeGuid);

                
            }
            return interest;
        }


        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a InterestType domain entity to its corresponding InterestArea DTO
        /// </summary>
        /// <param name="source">InterestType domain entity</param>
        /// <returns>InterestArea DTO</returns>
        private Dtos.InterestArea ConvertInterestTypeEntityToInterestAreaDto(InterestType source)
        {
            var interestArea = new Dtos.InterestArea();
            interestArea.Id = source.Guid;
            interestArea.Code = source.Code;
            interestArea.Title = source.Description;
            interestArea.Description = null;
            return interestArea;
        }
    }
}