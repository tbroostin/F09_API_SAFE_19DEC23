// Copyright 2017-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class OtherHonorService : BaseCoordinationService, IOtherHonorService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private const string _dataOrigin = "Colleague";

        public OtherHonorService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         IPersonRepository personRepository,
                                         IConfigurationRepository configurationRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository : configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all Other honors
        /// </summary>
        /// <returns>Collection of Other Honor DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.OtherHonor>> GetOtherHonorsAsync(bool bypassCache)
        {
            var otherHonorsCollection = new List<Ellucian.Colleague.Dtos.OtherHonor>();

            var otherHonorsEntities = await _referenceDataRepository.GetOtherHonorsAsync(bypassCache);
            if (otherHonorsEntities != null && otherHonorsEntities.Count() > 0)
            {
                foreach (var otherHonor in otherHonorsEntities)
                {
                    otherHonorsCollection.Add(ConvertOtherHonorEntityToDto(otherHonor));
                }
            }
            return otherHonorsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an Other Honor from its GUID
        /// </summary>
        /// <returns>OtherHonor DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.OtherHonor> GetOtherHonorByGuidAsync(string guid)
        {
            try
            {
                return ConvertOtherHonorEntityToDto((await _referenceDataRepository.GetOtherHonorsAsync(true)).Where(al => al.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Other Honor not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Other Honor not found for GUID " + guid, ex);
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException("Other Honor not found for GUID " + guid, ex);
            }
        }
        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts an OtherHonor domain entity to its corresponding OtherHonor DTO
        /// </summary>
        /// <param name="source">OtherHonor domain entity</param>
        /// <returns>OtherHonor DTO</returns>
        private Ellucian.Colleague.Dtos.OtherHonor ConvertOtherHonorEntityToDto(Ellucian.Colleague.Domain.Base.Entities.OtherHonor source)
        {
            var otherHonor = new Ellucian.Colleague.Dtos.OtherHonor();

            otherHonor.Id = source.Guid;
            otherHonor.Code = source.Code;
            otherHonor.Title = source.Description;
            otherHonor.Description = null;
            //Defaulted to "Distinction"
            otherHonor.AcademicHonorType = Dtos.AcademicHonorType.distinction;
            return otherHonor;
        }
    }
}
