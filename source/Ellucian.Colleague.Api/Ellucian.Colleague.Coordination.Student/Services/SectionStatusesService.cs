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
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class SectionStatusesService : BaseCoordinationService, ISectionStatusesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;

        public SectionStatusesService(

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
        /// Gets all section-statuses
        /// </summary>
        /// <returns>Collection of SectionStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.SectionStatuses>> GetSectionStatusesAsync(bool bypassCache = false)
        {
            var sectionStatusesCollection = new List<Ellucian.Colleague.Dtos.SectionStatuses>();

            var sectionStatusesEntities = await _referenceDataRepository.GetSectionStatusesAsync(bypassCache);
            if (sectionStatusesEntities != null && sectionStatusesEntities.Any())
            {
                foreach (var sectionStatuses in sectionStatusesEntities)
                {
                    sectionStatusesCollection.Add(ConvertSectionStatusesEntityToDto(sectionStatuses));
                }
            }
            return sectionStatusesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a SectionStatuses from its GUID
        /// </summary>
        /// <returns>SectionStatuses DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.SectionStatuses> GetSectionStatusesByGuidAsync(string guid)
        {
            try
            {
                return ConvertSectionStatusesEntityToDto((await _referenceDataRepository.GetSectionStatusesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("section-statuses not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("section-statuses not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a SectionStatuses domain entity to its corresponding SectionStatuses DTO
        /// </summary>
        /// <param name="source">SectionStatuses domain entity</param>
        /// <returns>SectionStatuses DTO</returns>
        private Ellucian.Colleague.Dtos.SectionStatuses ConvertSectionStatusesEntityToDto(Domain.Student.Entities.SectionStatuses source)
        {
            var sectionStatuses = new Ellucian.Colleague.Dtos.SectionStatuses();

            sectionStatuses.Id = source.Guid;
            sectionStatuses.Code = source.Code;
            sectionStatuses.Title = source.Description;
            sectionStatuses.Description = null;
            if (source.SectionStatusIntg != null)
            {
                switch (source.SectionStatusIntg)
                {
                    case SectionStatusIntegration.Cancelled:
                        sectionStatuses.Category = SectionStatus2.Cancelled;
                        break;
                    case SectionStatusIntegration.Closed:
                        sectionStatuses.Category = SectionStatus2.Closed;
                        break;
                    case SectionStatusIntegration.Open:
                        sectionStatuses.Category = SectionStatus2.Open;
                        break;
                    case SectionStatusIntegration.Pending:
                        sectionStatuses.Category = SectionStatus2.Pending;
                        break;
                    default:
                        sectionStatuses.Category = ConvertSectionStatusesCategoryDomainEnumToSectionStatusesCategoryDtoEnum(source.Category);
                        break;
                }
            }
            else
            {
                sectionStatuses.Category = ConvertSectionStatusesCategoryDomainEnumToSectionStatusesCategoryDtoEnum(source.Category);
            }                      
            return sectionStatuses;
        }

      
   
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a SectionStatusesCategory domain enumeration value to its corresponding SectionStatusesCategory DTO enumeration value
        /// </summary>
        /// <param name="source">SectionStatusesCategory domain enumeration value</param>
        /// <returns>SectionStatusesCategory DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.SectionStatus2 ConvertSectionStatusesCategoryDomainEnumToSectionStatusesCategoryDtoEnum(SectionStatusIntegration source)
        {
            switch (source)
            {

                case SectionStatusIntegration.Open:
                    return Dtos.SectionStatus2.Open;
                case SectionStatusIntegration.Closed:
                    return Dtos.SectionStatus2.Closed;
                case SectionStatusIntegration.Pending:
                    return Dtos.SectionStatus2.Pending;
                case SectionStatusIntegration.Cancelled:
                    return Dtos.SectionStatus2.Cancelled;
                default:
                    return Dtos.SectionStatus2.Open;
            }
        }
   }
}