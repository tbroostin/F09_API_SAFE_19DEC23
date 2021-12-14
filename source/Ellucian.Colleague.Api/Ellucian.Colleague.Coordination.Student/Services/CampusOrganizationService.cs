// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Exceptions;
using System.Net;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class CampusOrganizationService : BaseCoordinationService, ICampusOrganizationService
    {
        IPersonBaseRepository _personBaseRepository;
        private readonly ICampusOrganizationRepository _campusOrganizationRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly ILogger logger;
        private readonly IConfigurationRepository _configurationRepository;

        private const string _dataOrigin = "Colleague";
        private IEnumerable<Domain.Student.Entities.CampusOrganizationType> filteredCampOrgType;
        private IEnumerable<Domain.Student.Entities.CampusInvRole> campusInvolvementRoles = null;


        public CampusOrganizationService(IAdapterRegistry adapterRegistry, IPersonBaseRepository personBaseRepository, ICampusOrganizationRepository campusOrganizationRepository,
            IStudentReferenceDataRepository studentReferenceDataRepository, IPersonRepository personRepository, IConfigurationRepository configurationRepository, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _campusOrganizationRepository = campusOrganizationRepository;
            _personBaseRepository = personBaseRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _personRepository = personRepository;
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
            Tuple<IEnumerable<Domain.Student.Entities.CampusInvolvement>, int> campusInvolvementEntitiesTuple = null;
            try
            {
                campusInvolvementEntitiesTuple = await _campusOrganizationRepository.GetCampusInvolvementsAsync(offset, limit);

                var campusInvolvementEntities = campusInvolvementEntitiesTuple.Item1;
                var totalCount = campusInvolvementEntitiesTuple.Item2;

                var campusInvolvementsDto = new List<Dtos.CampusInvolvement>();
                if (campusInvolvementEntities != null && campusInvolvementEntities.Any())
                {
                    campusInvolvementsDto = (await BuildCampusInvolvementsDtoAsync(campusInvolvementEntities)).ToList();

                    return campusInvolvementsDto.Any() ? new Tuple<IEnumerable<Dtos.CampusInvolvement>, int>(campusInvolvementsDto, totalCount) :
                        new Tuple<IEnumerable<Dtos.CampusInvolvement>, int>(new List<Dtos.CampusInvolvement>(), 0);
                }
                else
                {
                    return new Tuple<IEnumerable<Dtos.CampusInvolvement>, int>(new List<Dtos.CampusInvolvement>(), 0);
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
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
        /// <param name="guid">guid</param>
        /// <returns>Dtos.CampusInvolvement</returns>
        public async Task<Dtos.CampusInvolvement> GetCampusInvolvementByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                IntegrationApiExceptionAddError("GUID is required to get a campus involvement.", "Missing.GUID");
                throw IntegrationApiException;
            }

            var campusInvolvementDto = new Dtos.CampusInvolvement();
            Domain.Student.Entities.CampusInvolvement campusInvolvementEntity = null;
            try
            {
                campusInvolvementEntity = await _campusOrganizationRepository.GetGetCampusInvolvementByIdAsync(guid);
                if (campusInvolvementEntity != null)
                {
                    var campusInvolvementsDto = (await BuildCampusInvolvementsDtoAsync(new List<Domain.Student.Entities.CampusInvolvement>()
                    {campusInvolvementEntity})).ToList();

                    if (campusInvolvementsDto.Any())
                    {
                        campusInvolvementDto = campusInvolvementsDto.FirstOrDefault();
                    }
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: guid);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException)
            {
                throw new KeyNotFoundException("campus-involvements not found for GUID " + guid);
            }
            if (campusInvolvementEntity == null)
            {
                throw new KeyNotFoundException("campus-involvements not found for GUID " + guid);
            }
            return campusInvolvementDto;
        }        

        /// <summary>
        /// BuildCampusInvolvementsDtoAsync
        /// </summary>
        /// <param name="sources">Collection of campus involvement domain entities</param>
        /// <param name="bypassCache">bypassCache flag.  Defaulted to false</param>
        /// <returns>Collection of CampusInvolvement DTO objects </returns>
        private async Task<IEnumerable<Dtos.CampusInvolvement>> BuildCampusInvolvementsDtoAsync(IEnumerable<Domain.Student.Entities.CampusInvolvement> sources)
        {
            if ((sources == null) || (!sources.Any()))
            {
                return null;
            }
            var campusInvolvementDtos = new List<Dtos.CampusInvolvement>();
            Dictionary<string, string> personGuidCollection = null;
            Dictionary<string, string> campusOrganizationsGuidCollection = null;
            Dictionary<string, string> campusInvolvementRolesGuidCollection = null;

            try
            {
                var personIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.PersonId)))
                     .Select(x => x.PersonId).Distinct().ToList();
                personGuidCollection = await this._personRepository.GetPersonGuidsCollectionAsync(personIds);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            try
            {
                var campusOrganizationIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.CampusOrganizationId)))
                     .Select(x => x.CampusOrganizationId).Distinct().ToList();
                campusOrganizationsGuidCollection = await _campusOrganizationRepository.GetGuidsCollectionAsync(campusOrganizationIds, "CAMPUS.ORGS");
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            try
            {
                var campusInvolvementRoleIds = sources
                     .Where(x => (!string.IsNullOrEmpty(x.RoleId)))
                     .Select(x => x.RoleId).Distinct().ToList();
                campusInvolvementRolesGuidCollection = await _campusOrganizationRepository.GetGuidsCollectionAsync(campusInvolvementRoleIds, "ROLES");
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message);
            }

            foreach (var source in sources)
            {
                try
                {
                    campusInvolvementDtos.Add(ConvertCampusInvolvementEntityToDtoAsync(source,
                        personGuidCollection, campusOrganizationsGuidCollection, campusInvolvementRolesGuidCollection));
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message);
                }
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return campusInvolvementDtos;
        }

        /// <summary>
        /// Converts campus involvement entity to dto
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
        /// <returns>Dtos.CampusInvolvement</returns>
        private Dtos.CampusInvolvement ConvertCampusInvolvementEntityToDtoAsync(Domain.Student.Entities.CampusInvolvement source,
            Dictionary<string, string> personGuidCollection,
            Dictionary<string, string> campusOrganizationsGuidCollection,
            Dictionary<string, string> campusInvolvementRolesGuidCollection)
        {
            if (source == null)
                return null;

            var campusInvolvementDto = new Dtos.CampusInvolvement();
            var guid = source.CampusInvolvementId;
            var id = source.CampusOrganizationId + "*" + source.PersonId;

            campusInvolvementDto.Id = guid;
            campusInvolvementDto.PersonId = ConvertPersonIdToGuid(source.PersonId, personGuidCollection, guid, id);
            campusInvolvementDto.AcademicPeriod = null;
            campusInvolvementDto.CampusOrganizationId = ConvertCampusOrganizationIdToGuid(source.CampusOrganizationId, campusOrganizationsGuidCollection, guid, id);
            campusInvolvementDto.InvolvementStartOn = source.StartOn;
            campusInvolvementDto.InvolvementEndOn = source.EndOn;
            campusInvolvementDto.InvolvementRole = ConvertCampusInvolvementRoleIdToGuid(source.RoleId, campusInvolvementRolesGuidCollection, guid, id);
            return campusInvolvementDto;
        }               

        /// <summary>
        /// Convert a personID to a Guid Object
        /// </summary>
        /// <param name="personId">a personID</param>
        /// <param name="personGuidCollection">a dictionary of associated person guids and ids</param>
        /// <param name="guid">campus involvement guid</param>
        /// <param name="id">campus org members id</param>
        /// <returns>guidObject</returns>
        private Dtos.GuidObject2 ConvertPersonIdToGuid(string personId, Dictionary<string, string> personGuidCollection, string guid, string id)
        {
            if (personId == null)
            {
                return null;
            }
            var personGuid = string.Empty;

            if (personGuidCollection == null)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID : '{0}'", personId),
                           guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
            }
            else
            {

                personGuidCollection.TryGetValue(personId, out personGuid);
                if (string.IsNullOrEmpty(personGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for person ID : '{0}'", personId),
                              guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (string.IsNullOrEmpty(personGuid)) ? null : new GuidObject2(personGuid);
        }

        /// <summary>
        /// Convert a campus organization id to a Guid Object
        /// </summary>
        /// <param name="campusOrganizationId">a campus organization id</param>
        /// <param name="campusOrganizationsGuidCollection">a dictionnary of associated campus organization guids and ids</param>
        /// <param name="guid">campus involvement guid</param>
        /// <param name="id">campus org members id</param>
        private Dtos.GuidObject2 ConvertCampusOrganizationIdToGuid(string campusOrganizationId, Dictionary<string, string> campusOrganizationsGuidCollection, string guid, string id)
        {
            if (campusOrganizationId == null)
            {
                return null;
            }
            var campusOrganizationGuid = string.Empty;

            if (campusOrganizationsGuidCollection == null)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for campus organization ID : '{0}'", campusOrganizationId),
                           guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
            }
            else
            {

                campusOrganizationsGuidCollection.TryGetValue(campusOrganizationId, out campusOrganizationGuid);
                if (string.IsNullOrEmpty(campusOrganizationGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for campus organization ID : '{0}'", campusOrganizationId),
                              guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (string.IsNullOrEmpty(campusOrganizationGuid)) ? null : new GuidObject2(campusOrganizationGuid);
        }

        /// <summary>
        /// Convert a campus involvement role id to a Guid Object
        /// </summary>
        /// <param name="campusInvolvementRoleId">a campus involvement role id</param>
        /// <param name="campusInvolvementRolesGuidCollection">a dictionary of associated campus involvement roles guids and ids</param>
        /// <param name="guid">campus involvement guid</param>
        /// <param name="id">campus org members id</param>
        private Dtos.GuidObject2 ConvertCampusInvolvementRoleIdToGuid(string campusInvolvementRoleId, Dictionary<string, string> campusInvolvementRolesGuidCollection, string guid, string id)
        {
            if (campusInvolvementRoleId == null)
            {
                return null;
            }
            var campusInvolvementRoleGuid = string.Empty;

            if (campusInvolvementRolesGuidCollection == null)
            {
                IntegrationApiExceptionAddError(string.Format("Unable to locate guid for campus involvement role ID : '{0}'", campusInvolvementRoleId),
                           guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
            }
            else
            {

                campusInvolvementRolesGuidCollection.TryGetValue(campusInvolvementRoleId, out campusInvolvementRoleGuid);
                if (string.IsNullOrEmpty(campusInvolvementRoleGuid))
                {
                    IntegrationApiExceptionAddError(string.Format("Unable to locate guid for campus involvement role ID : '{0}'", campusInvolvementRoleId),
                              guid: guid, id: id, httpStatusCode: HttpStatusCode.NotFound);
                }
            }
            return (string.IsNullOrEmpty(campusInvolvementRoleGuid)) ? null : new GuidObject2(campusInvolvementRoleGuid);
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
