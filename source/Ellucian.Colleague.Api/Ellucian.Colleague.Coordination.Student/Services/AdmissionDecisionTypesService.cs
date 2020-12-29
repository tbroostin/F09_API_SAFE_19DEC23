//Copyright 2017-2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdmissionDecisionTypesService : BaseCoordinationService, IAdmissionDecisionTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public AdmissionDecisionTypesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 11</remarks>
        /// <summary>
        /// Gets all admission-decision-types
        /// </summary>
        /// <returns>Collection of AdmissionDecisionTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionDecisionType2>> GetAdmissionDecisionTypesAsync(bool bypassCache = false)
        {
            var admissionDecisionTypesCollection = new List<Ellucian.Colleague.Dtos.AdmissionDecisionType2>();

            var admissionDecisionTypesEntities = await _referenceDataRepository.GetAdmissionDecisionTypesAsync(bypassCache);
            if (admissionDecisionTypesEntities != null && admissionDecisionTypesEntities.Any())
            {
                foreach (var admissionDecisionTypes in admissionDecisionTypesEntities)
                {
                    admissionDecisionTypesCollection.Add(ConvertAdmissionDecisionTypes2EntityToDto(admissionDecisionTypes));
                }
            }
            return admissionDecisionTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Gets all admission-application-status-types
        /// </summary>
        /// <returns>Collection of AdmissionApplicationStatusTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationStatusType>> GetAdmissionApplicationStatusTypesAsync(bool bypassCache = false)
        {
            var admissionApplicationStatusTypesCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplicationStatusType>();

            var admissionApplicationStatusTypesEntities = await _referenceDataRepository.GetAdmissionApplicationStatusTypesAsync(bypassCache);
            if (admissionApplicationStatusTypesEntities != null && admissionApplicationStatusTypesEntities.Any())
            {
                foreach (var admissionApplicationStatusTypes in admissionApplicationStatusTypesEntities)
                {
                    admissionApplicationStatusTypesCollection.Add(ConvertAdmissionApplicationStatusTypesEntityToDto(admissionApplicationStatusTypes));
                }
            }
            return admissionApplicationStatusTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 11</remarks>
        /// <summary>
        /// Get a AdmissionDecisionTypes from its GUID
        /// </summary>
        /// <returns>AdmissionDecisionTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionDecisionType2> GetAdmissionDecisionTypesByGuidAsync(string guid, bool bypassCache = false)
        {
            Domain.Student.Entities.AdmissionDecisionType admissionDecisionType = null;
            try
            {
                if (bypassCache == true)
                {
                    admissionDecisionType = await _referenceDataRepository.GetAdmissionDecisionTypeByGuidAsync(guid);
                }
                else
                {
                    admissionDecisionType = (await _referenceDataRepository.GetAdmissionDecisionTypesAsync(true)).Where(r => r.Guid == guid).First();
                }
                return ConvertAdmissionDecisionTypes2EntityToDto(admissionDecisionType);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("admission-decision-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("admission-decision-types not found for GUID " + guid, ex);
            }
            catch
            {
                throw new KeyNotFoundException("admission-decision-types not found for GUID " + guid);
            }            
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Get a AdmissionApplicationStatusTypes from its GUID
        /// </summary>
        /// <returns>AdmissionApplicationStatusTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionApplicationStatusType> GetAdmissionApplicationStatusTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertAdmissionApplicationStatusTypesEntityToDto((await _referenceDataRepository.GetAdmissionApplicationStatusTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("admission-application-status-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("admission-application-status-types not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 11</remarks>
        /// <summary>
        /// Converts a AdmissionDecisionTypes domain entity to its corresponding AdmissionDecisionTypes DTO
        /// </summary>
        /// <param name="source">AdmissionDecisionTypes domain entity</param>
        /// <returns>AdmissionDecisionTypes DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionDecisionType2 ConvertAdmissionDecisionTypes2EntityToDto(Ellucian.Colleague.Domain.Student.Entities.AdmissionDecisionType source)
        {
            var admissionDecisionTypes = new Ellucian.Colleague.Dtos.AdmissionDecisionType2();

            admissionDecisionTypes.Id = source.Guid;
            admissionDecisionTypes.Code = source.Code;
            admissionDecisionTypes.Title = source.Description;
            admissionDecisionTypes.Description = null;
            
            if (source.AdmissionApplicationStatusTypesCategory == Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.NotSet)
            {
                admissionDecisionTypes.Category = null;
            }
            else
            {
                admissionDecisionTypes.Category = new List<AdmissionApplicationStatusTypesCategory2?>() { ConvertDomainEnumToCategoryDtoEnum2(source.AdmissionApplicationStatusTypesCategory) };
            }

            return admissionDecisionTypes;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Converts a AdmissionApplicationStatusTypes domain entity to its corresponding AdmissionApplicationStatusTypes DTO
        /// </summary>
        /// <param name="source">AdmissionApplicationStatusTypes domain entity</param>
        /// <returns>AdmissionApplicationStatusTypes DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionApplicationStatusType ConvertAdmissionApplicationStatusTypesEntityToDto(Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusType source)
        {
            var admissionApplicationStatusTypes = new Ellucian.Colleague.Dtos.AdmissionApplicationStatusType();

            admissionApplicationStatusTypes.Id = source.Guid;
            admissionApplicationStatusTypes.Code = source.Code;
            admissionApplicationStatusTypes.Title = source.Description;
            admissionApplicationStatusTypes.Description = null;

            admissionApplicationStatusTypes.Category = ConvertDomainEnumToCategoryDtoEnum(source.AdmissionApplicationStatusTypesCategory);

            return admissionApplicationStatusTypes;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 11</remarks>
        /// <summary>
        /// Converts a AdmissionApplicationStatusTypesCategory domain enumeration value to its corresponding AdmissionApplicationStatusTypesCategory DTO enumeration value
        /// </summary>
        /// <param name="source">AdmissionApplicationStatusTypesCategory domain enumeration value</param>
        /// <returns>AdmissionApplicationStatusTypesCategory DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory2 ConvertDomainEnumToCategoryDtoEnum2(Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory source)
        {
            switch (source)
            {
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Applied:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory2.Applied;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Complete:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory2.Complete;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Accepted:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory2.Accepted;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.WaitListed:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory2.Waitlisted;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Rejected:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory2.Rejected;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.MovedToStudent:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory2.MovedToStudent;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Withdrawn:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory2.Withdrawn;
                default:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory2.NotSet;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Converts a AdmissionApplicationStatusTypesCategory domain enumeration value to its corresponding AdmissionApplicationStatusTypesCategory DTO enumeration value
        /// </summary>
        /// <param name="source">AdmissionApplicationStatusTypesCategory domain enumeration value</param>
        /// <returns>AdmissionApplicationStatusTypesCategory DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory ConvertDomainEnumToCategoryDtoEnum(Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory source)
        {
            switch (source)
            {
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Started:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory.Started;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Submitted:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory.Submitted;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Readyforreview:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory.Readyforreview;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Decisionmade:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory.Decisionmade;
                case Ellucian.Colleague.Domain.Student.Entities.AdmissionApplicationStatusTypesCategory.Enrollmentcomplete:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory.Enrollmentcomplete;
                default:
                    return Dtos.EnumProperties.AdmissionApplicationStatusTypesCategory.Started;
            }
        }
    }
}