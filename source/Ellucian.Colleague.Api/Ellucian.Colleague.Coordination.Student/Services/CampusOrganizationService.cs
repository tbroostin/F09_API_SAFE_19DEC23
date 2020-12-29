// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Dtos.Resources;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CampusOrganizationService : BaseCoordinationService, ICampusOrganizationService
    {
        IPersonBaseRepository _personBaseRepository;
        private readonly ICampusOrganizationRepository _campusOrganizationRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger logger;
        private readonly IConfigurationRepository _configurationRepository;

        private const string _dataOrigin = "Colleague";
        private IEnumerable<Domain.Student.Entities.CampusOrganizationType> filteredCampOrgType;
        private IEnumerable<Domain.Student.Entities.CampusInvRole> campusInvolvementRoles = null;


        public CampusOrganizationService(IAdapterRegistry adapterRegistry, IPersonBaseRepository personBaseRepository, ICampusOrganizationRepository campusOrganizationRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository, IConfigurationRepository configurationRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _campusOrganizationRepository = campusOrganizationRepository;
            _personBaseRepository = personBaseRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _configurationRepository = configurationRepository;
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.logger = logger;
        }

        #region Campus Organization

        /// <summary>
        /// Gets all campus organizations
        /// </summary>
        /// <param name="bypassCache">bypassCache</param>
        /// <returns>IEnumerable<Dtos.CampusOrganization></returns>
        public async Task<IEnumerable<Dtos.CampusOrganization>> GetCampusOrganizationsAsync(bool bypassCache)
        {
            List<Dtos.CampusOrganization> campusOrganizations = new List<Dtos.CampusOrganization>();

            var campusOrganizationEntities = await _campusOrganizationRepository.GetCampusOrganizationsAsync(bypassCache);

            if (campusOrganizationEntities != null && campusOrganizationEntities.Any())
            {
                var campusOrganizationTypeIds = campusOrganizationEntities.Where(i => !string.IsNullOrEmpty(i.CampusOrganizationTypeId)).Select(i => i.CampusOrganizationTypeId);
                filteredCampOrgType = (await _studentReferenceDataRepository.GetCampusOrganizationTypesAsync(true)).Where(i => campusOrganizationTypeIds.Contains(i.Code));

                foreach (var campusOrganizationEntity in campusOrganizationEntities)
                {
                    Dtos.CampusOrganization campusOrgDto = await ConvertCampusOrganizationEntityToDtoAsync(campusOrganizationEntity);
                    campusOrganizations.Add(campusOrgDto);
                }
            }

            return campusOrganizations;
        }

        /// <summary>
        /// Gets campus organization by id
        /// </summary>
        /// <param name="id">campus organization id</param>
        /// <returns>Dtos.CampusOrganization</returns>
        public async Task<Dtos.CampusOrganization> GetCampusOrganizationByGuidAsync(string id)
        {
            var campusOrganizationEntities = await _campusOrganizationRepository.GetCampusOrganizationsAsync(true);

            var campOrg = campusOrganizationEntities.FirstOrDefault(org => org.Guid.Equals(id, StringComparison.OrdinalIgnoreCase));

            filteredCampOrgType = (await _studentReferenceDataRepository.GetCampusOrganizationTypesAsync(true)).Where(i => campOrg.CampusOrganizationTypeId.Equals(i.Code, StringComparison.OrdinalIgnoreCase));

            if (campOrg == null)
            {
                throw new KeyNotFoundException("Could not find campus organization for id: " + id);
            }
            Dtos.CampusOrganization campusOrgDto = await ConvertCampusOrganizationEntityToDtoAsync(campOrg);
            return campusOrgDto;
        }

        /// <summary>
        /// Gets CampusOrganization2 objects by campusOrgIds
        /// </summary>
        /// <param name="campusOrgIds">List of campus organization ids</param>
        /// <returns>CampusOrganization2 DTO</returns>
        public async Task<IEnumerable<CampusOrganization2>> GetCampusOrganizations2ByCampusOrgIdsAsync(List<string> campusOrgIds)
        {
            if (campusOrgIds == null)
            {
                throw new ArgumentNullException("campusOrgIds");
            }
            if (!campusOrgIds.Any())
            {
                throw new ArgumentException("campusOrgIds are required to get CampusOrganization2 objects");
            }
           
            var campusOrganization2Entities = await _campusOrganizationRepository.GetCampusOrganizations2Async(campusOrgIds);
            
            if (campusOrganization2Entities == null)
            {
                var message = "Null CampusOrganization2s returned from the repository";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            var campusOrganization2EntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.CampusOrganization2, CampusOrganization2>();

            var campusOrganization2Dtos = campusOrganization2Entities.Select(cmpOrgs2 => campusOrganization2EntityToDtoAdapter.MapToType(cmpOrgs2));

            return campusOrganization2Dtos;
        }

        /// <summary>
        /// Converts campus organization entity to dto
        /// </summary>
        /// <param name="campusOrganizationEntity">campus organization entity</param>
        /// <returns>Dtos.CampusOrganization</returns>
        private async Task<Dtos.CampusOrganization> ConvertCampusOrganizationEntityToDtoAsync(Domain.Student.Entities.CampusOrganization campusOrganizationEntity)
        {
            Dtos.CampusOrganization campusOrgDto = new Dtos.CampusOrganization();
            campusOrgDto.Id = campusOrganizationEntity.Guid;
            campusOrgDto.Code = campusOrganizationEntity.Code;
            campusOrgDto.CampusOrganizationName = campusOrganizationEntity.Description;
            campusOrgDto.ParentOrganization = await ConvertParentOrganizationIdToDtoAsync(campusOrganizationEntity.ParentOrganizationId);
            campusOrgDto.CampusOrganizationType = ConvertCampusOrgTypeIdToDto(campusOrganizationEntity.CampusOrganizationTypeId);
            return campusOrgDto;
        }

        /// <summary>
        /// Returns the guid of the parent organization.
        /// </summary>
        /// <param name="parentOrganizationId">parentOrganizationId</param>
        /// <returns>Dtos.GuidObject2</returns>
        private async Task<Dtos.GuidObject2> ConvertParentOrganizationIdToDtoAsync(string parentOrganizationId)
        {
            Dtos.GuidObject2 guidObject = null;

            if (!string.IsNullOrEmpty(parentOrganizationId))
            {
                var id = await _personBaseRepository.GetPersonGuidFromOpersAsync(parentOrganizationId);
                if (!string.IsNullOrEmpty(id))
                {
                    guidObject = new Dtos.GuidObject2(id);
                }
            }
            return guidObject;
        }

        /// <summary>
        /// Returns guid for campus organization type.
        /// </summary>
        /// <param name="campusOrganizationTypeId">organization type id</param>
        /// <returns>Dtos.GuidObject2</returns>
        private Dtos.GuidObject2 ConvertCampusOrgTypeIdToDto(string campusOrganizationTypeId)
        {
            Dtos.GuidObject2 guidObject = null;
            if (!string.IsNullOrEmpty(campusOrganizationTypeId))
            {
                //var campusOrgTypeEntities = await _studentReferenceDataRepository.GetCampusOrganizationTypesAsync(true);

                if (filteredCampOrgType != null && filteredCampOrgType.Any())
                {
                    var campusOrgType = filteredCampOrgType.FirstOrDefault(orgType => orgType.Code.Equals(campusOrganizationTypeId, StringComparison.OrdinalIgnoreCase));

                    if (campusOrgType != null)
                    {
                        guidObject = new Dtos.GuidObject2(campusOrgType.Guid);
                    }
                }
            }
            return guidObject;
        }

        #endregion

        #region Campus Involvement
        /// <summary>
        /// Returns campus involvements
        /// </summary>
        /// <param name="offset">offset</param>
        /// <param name="limit">limit</param>
        /// <param name="bypassCache">bypassCache</param>
        /// <returns>Tuple<IEnumerable<Dtos.CampusInvolvement>, int></returns>
        public async Task<Tuple<IEnumerable<Dtos.CampusInvolvement>, int>> GetCampusInvolvementsAsync(int offset, int limit, bool bypassCache)
        {
            // get user permissions
            CheckUserCampusInvolvementViewPermissions();

            var campusInvolvementsList = new List<Dtos.CampusInvolvement>();

            var responses = await _campusOrganizationRepository.GetCampusInvolvementsAsync(offset, limit);
            campusInvolvementsList = await ConvertCampusInvolvementEntityToDtoAsync(responses.Item1) as List<Dtos.CampusInvolvement>;
            return new Tuple<IEnumerable<Dtos.CampusInvolvement>, int>(campusInvolvementsList, responses.Item2);
        }

        /// <summary>
        /// Converts campus involvement entities to dtos
        /// After discussion this property with HEDM, it turns out that we don't need 
        /// to have any elaborate logic to determine the academic periods that might 
        /// correlate to a membership period (e.g. startOn and endOn in this schema). 
        /// This property wasn't intended for that, but was included in the schema 
        /// so that Banner could share campus involvements when they only have a term 
        /// specified rather than a date range.  As a result, we don't need to populate this for the GET. 
        /// For a POST or PUT we would need to consume this into a special INTG element, but we don't have 
        /// to deal w/ that work until we support the Create, Update, or Delete operations. 
        /// </summary>
        /// <param name="campusInvolvementEntities"></param>
        /// <returns>IEnumerable<Dtos.CampusInvolvement></returns>
        private async Task<IEnumerable<Dtos.CampusInvolvement>> ConvertCampusInvolvementEntityToDtoAsync(IEnumerable<Domain.Student.Entities.CampusInvolvement> campusInvolvementEntities)
        {
            List<Dtos.CampusInvolvement> campusInvolvementDtos = new List<Dtos.CampusInvolvement>();

            var campOrgIds = campusInvolvementEntities.Where(i => !string.IsNullOrEmpty(i.CampusOrganizationId)).Select(ordId => ordId.CampusOrganizationId).Distinct();

            var campusOrgEntities = await _campusOrganizationRepository.GetCampusOrganizationsAsync(true);
            campusInvolvementRoles = await _studentReferenceDataRepository.GetCampusInvolvementRolesAsync(true);


            var filteredCampusOrgEntities = campusOrgEntities.Where(i => campOrgIds.Contains(i.Code));

            if (campusInvolvementEntities != null && campusInvolvementEntities.Any())
            {
                foreach (var campusInvolvementEntity in campusInvolvementEntities)
                {
                    Dtos.CampusInvolvement campusInvolvementDto = new Dtos.CampusInvolvement();
                    campusInvolvementDto.Id = campusInvolvementEntity.CampusInvolvementId;
                    campusInvolvementDto.PersonId = await ConvertParentOrganizationIdToDtoAsync(campusInvolvementEntity.PersonId);
                    campusInvolvementDto.AcademicPeriod = null;
                    campusInvolvementDto.CampusOrganizationId = ConvertCampusOrgEntityIdToGuid(campusInvolvementEntity.CampusOrganizationId, filteredCampusOrgEntities);
                    campusInvolvementDto.InvolvementStartOn = campusInvolvementEntity.StartOn;
                    campusInvolvementDto.InvolvementEndOn = campusInvolvementEntity.EndOn;
                    campusInvolvementDto.InvolvementRole = await ConvertInvolvementRoleToGuid(campusInvolvementEntity.RoleId);

                    campusInvolvementDtos.Add(campusInvolvementDto);
                }
            }

            return campusInvolvementDtos;
        }       

        /// <summary>
        /// Returns campus involvement by Id
        /// After discussion this property with HEDM, it turns out that we don't need 
        /// to have any elaborate logic to determine the academic periods that might 
        /// correlate to a membership period (e.g. startOn and endOn in this schema). 
        /// This property wasn't intended for that, but was included in the schema 
        /// so that Banner could share campus involvements when they only have a term 
        /// specified rather than a date range.  As a result, we don't need to populate this for the GET. 
        /// For a POST or PUT we would need to consume this into a special INTG element, but we don't have 
        /// to deal w/ that work until we support the Create, Update, or Delete operations.
        /// </summary>         
        /// <param name="id">id</param>
        /// <returns>Dtos.CampusInvolvement</returns>
        public async Task<Dtos.CampusInvolvement> GetCampusInvolvementByGuidAsync(string id)
        {
            // get user permissions
            CheckUserCampusInvolvementViewPermissions();

            var campusInvolvementEntity = await _campusOrganizationRepository.GetGetCampusInvolvementByIdAsync(id);

            var campusOrgEntities = await _campusOrganizationRepository.GetCampusOrganizationsAsync(true);
            campusInvolvementRoles = await _studentReferenceDataRepository.GetCampusInvolvementRolesAsync(true);

            var filteredCampusOrgEntities = campusOrgEntities.Where(i => i.Code.Equals(campusInvolvementEntity.CampusOrganizationId, StringComparison.OrdinalIgnoreCase));

            Dtos.CampusInvolvement campusInvolvementDto = new Dtos.CampusInvolvement();
            campusInvolvementDto.Id = campusInvolvementEntity.CampusInvolvementId;
            campusInvolvementDto.PersonId = await ConvertParentOrganizationIdToDtoAsync(campusInvolvementEntity.PersonId);
            campusInvolvementDto.AcademicPeriod = null;
            campusInvolvementDto.CampusOrganizationId = ConvertCampusOrgEntityIdToGuid(campusInvolvementEntity.CampusOrganizationId, filteredCampusOrgEntities);
            campusInvolvementDto.InvolvementStartOn = campusInvolvementEntity.StartOn;
            campusInvolvementDto.InvolvementEndOn = campusInvolvementEntity.EndOn;
            campusInvolvementDto.InvolvementRole = await ConvertInvolvementRoleToGuid(campusInvolvementEntity.RoleId);


            return campusInvolvementDto;
        }

        /// <summary>
        /// Converts involvement code to guid object2
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        private async Task<Dtos.GuidObject2> ConvertInvolvementRoleToGuid(string roleId)
        {
            Dtos.GuidObject2 guidObject = null;
            
            if (string.IsNullOrEmpty(roleId))
            {
                return guidObject;
            }

            if (campusInvolvementRoles == null)
            {
                campusInvolvementRoles = await _studentReferenceDataRepository.GetCampusInvolvementRolesAsync(true);
            }

            var campusInvRole = campusInvolvementRoles.FirstOrDefault(i => i.Code.Equals(roleId, StringComparison.OrdinalIgnoreCase));
            if (campusInvRole != null)
            {
                guidObject = new Dtos.GuidObject2(campusInvRole.Guid);
            }

            return guidObject;
        }

        /// <summary>
        /// Gets campus organization id
        /// </summary>
        /// <param name="campusOrgEntityId"></param>
        /// <param name="filteredCampusOrgEntities"></param>
        /// <returns>Dtos.GuidObject2</returns>
        private Dtos.GuidObject2 ConvertCampusOrgEntityIdToGuid(string campusOrgEntityId, IEnumerable<Domain.Student.Entities.CampusOrganization> filteredCampusOrgEntities)
        {
            Dtos.GuidObject2 campusOrgId = null;

            var entity = filteredCampusOrgEntities.FirstOrDefault(i => i.Code.Equals(campusOrgEntityId, StringComparison.OrdinalIgnoreCase));
            if (entity != null)
            {
                campusOrgId = new Dtos.GuidObject2(entity.Guid);
            }
            return campusOrgId;
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view a person.
        /// </summary>
        private void CheckUserCampusInvolvementViewPermissions()
        {
            // access is ok if the current user has the view campus involvements permission
            if (!HasPermission(StudentPermissionCodes.ViewCampusInvolvements))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view campus-involvements.");
                throw new PermissionsException("User is not authorized to view campus-involvements.");
            }
        }

        #endregion

        #region CampusInvolvementRoles

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all campus involvement roles
        /// </summary>
        /// <returns>Collection of CampusInvolvementRole DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CampusInvolvementRole>> GetCampusInvolvementRolesAsync(bool bypassCache = false)
        {
            var campusInvRoleCollection = new List<Ellucian.Colleague.Dtos.CampusInvolvementRole>();

            var campusInvRoleEntities = await _studentReferenceDataRepository.GetCampusInvolvementRolesAsync(bypassCache);
            if (campusInvRoleEntities != null && campusInvRoleEntities.Count() > 0)
            {
                foreach (var campusInvRole in campusInvRoleEntities)
                {
                    campusInvRoleCollection.Add(ConvertCampusInvolvementRoleEntityToDto(campusInvRole));
                }
            }
            return campusInvRoleCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an campus involvement role from its GUID
        /// </summary>
        /// <returns>CampusInvolvementRole DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CampusInvolvementRole> GetCampusInvolvementRoleByGuidAsync(string guid)
        {
            try
            {
                return ConvertCampusInvolvementRoleEntityToDto((await _studentReferenceDataRepository.GetCampusInvolvementRolesAsync(true)).Where(cir => cir.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Campus involvement role not found for GUID " + guid, ex);
            }
        }

        #endregion

        #region CampusOrganizationTypes

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all campus organization types
        /// </summary>
        /// <returns>Collection of CampusOrganizationType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CampusOrganizationType>> GetCampusOrganizationTypesAsync(bool bypassCache = false)
        {
            var campusOrgTypeCollection = new List<Ellucian.Colleague.Dtos.CampusOrganizationType>();

            var campusOrgTypeEntities = await _studentReferenceDataRepository.GetCampusOrganizationTypesAsync(bypassCache);
            if (campusOrgTypeEntities != null && campusOrgTypeEntities.Count() > 0)
            {
                foreach (var campusOrgType in campusOrgTypeEntities)
                {
                    campusOrgTypeCollection.Add(ConvertCampusOrganizationTypeEntityToDto(campusOrgType));
                }
            }
            return campusOrgTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an campus organization type from its GUID
        /// </summary>
        /// <returns>CampusOrganizationType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CampusOrganizationType> GetCampusOrganizationTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertCampusOrganizationTypeEntityToDto((await _studentReferenceDataRepository.GetCampusOrganizationTypesAsync(true)).Where(co => co.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Campus organization type not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an CampusInvolvementRole domain entity to its corresponding CampusInvolvementRole DTO
        /// </summary>
        /// <param name="source">CampusInvRole domain entity</param>
        /// <returns>CampusInvolvementRole DTO</returns>
        private Ellucian.Colleague.Dtos.CampusInvolvementRole ConvertCampusInvolvementRoleEntityToDto(Ellucian.Colleague.Domain.Student.Entities.CampusInvRole source)
        {
            var campusInvolvementRole = new Ellucian.Colleague.Dtos.CampusInvolvementRole();

            campusInvolvementRole.Id = source.Guid;
            campusInvolvementRole.Code = source.Code;
            campusInvolvementRole.Title = source.Description;
            campusInvolvementRole.Description = null;

            return campusInvolvementRole;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an CampusOrganizationType domain entity to its corresponding CampusOrganizationType DTO
        /// </summary>
        /// <param name="source">CampusOrgType domain entity</param>
        /// <returns>CampusOrganizationType DTO</returns>
        private Ellucian.Colleague.Dtos.CampusOrganizationType ConvertCampusOrganizationTypeEntityToDto(Ellucian.Colleague.Domain.Student.Entities.CampusOrganizationType source)
        {
            var campusOrganizationType = new Ellucian.Colleague.Dtos.CampusOrganizationType();

            campusOrganizationType.Id = source.Guid;
            campusOrganizationType.Code = source.Code;
            campusOrganizationType.Title = source.Description;
            campusOrganizationType.Description = null;

            return campusOrganizationType;
        }
    }
        #endregion
}
