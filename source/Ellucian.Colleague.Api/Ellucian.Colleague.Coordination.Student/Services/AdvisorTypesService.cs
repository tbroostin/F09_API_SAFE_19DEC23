//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdvisorTypesService : StudentCoordinationService, IAdvisorTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public AdvisorTypesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IStudentRepository studentRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, studentRepository, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all advisor-types
        /// </summary>
        /// <returns>Collection of AdvisorTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdvisorTypes>> GetAdvisorTypesAsync(bool bypassCache = false)
        {
            var advisorTypesCollection = new List<Ellucian.Colleague.Dtos.AdvisorTypes>();

            var advisorTypesEntities = await _referenceDataRepository.GetAdvisorTypesAsync(bypassCache);
            if (advisorTypesEntities != null && advisorTypesEntities.Any())
            {
                foreach (var advisorTypes in advisorTypesEntities)
                {
                    advisorTypesCollection.Add(ConvertAdvisorTypesEntityToDto(advisorTypes));
                }
            }
            return advisorTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 7</remarks>
        /// <summary>
        /// Get a AdvisorTypes from its GUID
        /// </summary>
        /// <returns>AdvisorTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdvisorTypes> GetAdvisorTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertAdvisorTypesEntityToDto((await _referenceDataRepository.GetAdvisorTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("advisor-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("advisor-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AdvisorTypes domain entity to its corresponding AdvisorTypes DTO
        /// </summary>
        /// <param name="source">AdvisorTypes domain entity</param>
        /// <returns>AdvisorTypes DTO</returns>
        private Ellucian.Colleague.Dtos.AdvisorTypes ConvertAdvisorTypesEntityToDto(AdvisorType source)
        {
            var advisorTypes = new Ellucian.Colleague.Dtos.AdvisorTypes();

            advisorTypes.Id = source.Guid;
            advisorTypes.Code = source.Code;
            advisorTypes.Title = source.Description;
            advisorTypes.Description = null;           
                                                                        
            return advisorTypes;
        }

      
    }

}