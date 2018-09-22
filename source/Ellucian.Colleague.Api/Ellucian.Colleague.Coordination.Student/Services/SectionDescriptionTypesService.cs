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
    public class SectionDescriptionTypesService : BaseCoordinationService, ISectionDescriptionTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public SectionDescriptionTypesService(

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
        /// Gets all section-description-types
        /// </summary>
        /// <returns>Collection of SectionDescriptionTypes DTO objects</returns>
        public async Task<IEnumerable<Dtos.SectionDescriptionTypes>> GetSectionDescriptionTypesAsync(bool bypassCache = false)
        {
            var sectionDescriptionTypesCollection = new List<Dtos.SectionDescriptionTypes>();

            var sectionDescriptionTypesEntities = await _referenceDataRepository.GetSectionDescriptionTypesAsync(bypassCache);
            if (sectionDescriptionTypesEntities != null && sectionDescriptionTypesEntities.Any())
            {
                foreach (var sectionDescriptionTypes in sectionDescriptionTypesEntities)
                {
                    sectionDescriptionTypesCollection.Add(ConvertSectionDescriptionTypesEntityToDto(sectionDescriptionTypes));
                }
            }
            return sectionDescriptionTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a SectionDescriptionTypes from its GUID
        /// </summary>
        /// <returns>SectionDescriptionTypes DTO object</returns>
        public async Task<Dtos.SectionDescriptionTypes> GetSectionDescriptionTypeByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertSectionDescriptionTypesEntityToDto((await _referenceDataRepository.GetSectionDescriptionTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("section-description-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("section-description-types not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgSecDescTypes domain entity to its corresponding SectionDescriptionTypes DTO
        /// </summary>
        /// <param name="source">IntgSecDescTypes domain entity</param>
        /// <returns>SectionDescriptionTypes DTO</returns>
        private Dtos.SectionDescriptionTypes ConvertSectionDescriptionTypesEntityToDto(Domain.Student.Entities.SectionDescriptionType source)
        {
            var sectionDescriptionTypes = new Dtos.SectionDescriptionTypes();

            sectionDescriptionTypes.Id = source.Guid;
            sectionDescriptionTypes.Code = source.Code;
            sectionDescriptionTypes.Title = source.Description;
            sectionDescriptionTypes.Description = null;

            return sectionDescriptionTypes;
        }

    }

}
