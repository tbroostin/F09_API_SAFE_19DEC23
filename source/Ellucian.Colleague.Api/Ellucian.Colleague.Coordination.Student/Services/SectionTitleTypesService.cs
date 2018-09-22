//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class SectionTitleTypesService : BaseCoordinationService, ISectionTitleTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public SectionTitleTypesService(

            IStudentReferenceDataRepository referenceDataRepository,
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
        /// Gets all section-title-types
        /// </summary>
        /// <returns>Collection of SectionTitleTypes DTO objects</returns>
        public async Task<IEnumerable<Dtos.SectionTitleType>> GetSectionTitleTypesAsync(bool bypassCache = false)
        {
            var sectionTitleTypesCollection = new List<Dtos.SectionTitleType>();

            var sectionTitleTypesEntities = await _referenceDataRepository.GetSectionTitleTypesAsync(bypassCache);
            if (sectionTitleTypesEntities != null && sectionTitleTypesEntities.Any())
            {
                foreach (var sectionTitleTypes in sectionTitleTypesEntities)
                {
                    sectionTitleTypesCollection.Add(ConvertSectionTitleTypesEntityToDto(sectionTitleTypes));
                }
            }
            return sectionTitleTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a SectionTitleTypes from its GUID
        /// </summary>
        /// <returns>SectionTitleTypes DTO object</returns>
        public async Task<Dtos.SectionTitleType> GetSectionTitleTypeByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertSectionTitleTypesEntityToDto((await _referenceDataRepository.GetSectionTitleTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("section-title-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("section-title-types not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgSecTitleTypes domain entity to its corresponding SectionTitleTypes DTO
        /// </summary>
        /// <param name="source">IntgSecTitleTypes domain entity</param>
        /// <returns>SectionTitleTypes DTO</returns>
        private Dtos.SectionTitleType ConvertSectionTitleTypesEntityToDto(Domain.Student.Entities.SectionTitleType source)
        {
            var sectionTitleTypes = new Dtos.SectionTitleType();

            sectionTitleTypes.Id = source.Guid;
            sectionTitleTypes.Code = source.Code;
            sectionTitleTypes.Title = source.Description;
            sectionTitleTypes.Description = null;

            return sectionTitleTypes;
        }


    }
}
