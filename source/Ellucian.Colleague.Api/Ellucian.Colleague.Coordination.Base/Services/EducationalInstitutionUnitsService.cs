// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class EducationalInstitutionUnitsService : BaseCoordinationService, IEducationalInstitutionUnitsService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private string _defaultHostGuid;

        public EducationalInstitutionUnitsService(IReferenceDataRepository referenceDataRepository,
            IPersonRepository personRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _configurationRepository = configurationRepository;
        }


        private IEnumerable<Domain.Base.Entities.Department> _departments = null;
        private async Task<IEnumerable<Domain.Base.Entities.Department>> GetAllDepartmentsAsync(bool bypassCache = false)
        {
            if (_departments == null)
            {
                _departments = await _referenceDataRepository.GetDepartmentsAsync(bypassCache);
            }
            return _departments;
        }

        private IEnumerable<Domain.Base.Entities.Division> _divisions = null;
        private async Task<IEnumerable<Domain.Base.Entities.Division>> GetAllDivisionsAsync(bool bypassCache = false)
        {
            if (_divisions == null)
            {
                _divisions = await _referenceDataRepository.GetDivisionsAsync(bypassCache);
            }
            return _divisions;
        }


        private IEnumerable<Domain.Base.Entities.School> _schools = null;
        private async Task<IEnumerable<Domain.Base.Entities.School>> GetAllSchoolsAsync(bool bypassCache = false)
        {
            if (_schools == null)
            {
                _schools = await _referenceDataRepository.GetSchools2Async(bypassCache);
            }
            return _schools;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all Educational-Institution-Units
        /// including department, division, or school code
        /// </summary>
        /// <returns>Collection of EducationalInstitutionUnits DTO objects</returns>
        public async Task<IEnumerable<EducationalInstitutionUnits2>> GetEducationalInstitutionUnits2Async(bool ignoreCache = false)
        {

            var educationalInstitutionUnitsCollection = new List<EducationalInstitutionUnits2>();

            // Add departments to collection of EducationalInstitutionUnits
            var departmentEntities = await this.GetAllDepartmentsAsync(ignoreCache);
            if (departmentEntities != null && departmentEntities.Any())
            {
                foreach (var department in departmentEntities)
                {
                    educationalInstitutionUnitsCollection.Add(await ConvertDepartmentEntityToEducationalInstitutionUnits2DtoAsync(department));
                }
            }

            //Add departments to collection of EducationalInstitutionUnits
            var divisionEntities = await GetAllDivisionsAsync(ignoreCache);
            if (divisionEntities != null && divisionEntities.Any())
            {
                foreach (var division in divisionEntities)
                {
                    educationalInstitutionUnitsCollection.Add(await ConvertDivisionEntityToEducationalInstitutionUnits2DtoAsync(division));
                }
            }

            //Add school to collection of EducationalInstitutionUnits
            var schoolEntities = await GetAllSchoolsAsync(ignoreCache);
            if (schoolEntities != null && schoolEntities.Any())
            {
                foreach (var school in schoolEntities)
                {
                    educationalInstitutionUnitsCollection.Add(await ConvertSchoolEntityToEducationalInstitutionUnits2DtoAsync(school));
                }
            }

            return educationalInstitutionUnitsCollection;
        }



        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all Educational-Institution-Units
        /// including department, division, or school code
        /// </summary>
        /// <returns>Collection of EducationalInstitutionUnits DTO objects</returns>
        public async Task<IEnumerable<EducationalInstitutionUnits3>> GetEducationalInstitutionUnits3Async(
            bool ignoreCache = false,
            Dtos.EnumProperties.EducationalInstitutionUnitType educationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.NotSet,
            Dtos.EnumProperties.Status departmentStatus = Dtos.EnumProperties.Status.NotSet)
        {

            var educationalInstitutionUnitsCollection = new List<EducationalInstitutionUnits3>();

            if ((educationalInstitutionUnitType == Dtos.EnumProperties.EducationalInstitutionUnitType.NotSet) ||
                (educationalInstitutionUnitType == Dtos.EnumProperties.EducationalInstitutionUnitType.Department))
            {
                // Add departments to collection of EducationalInstitutionUnits
                var departmentEntities = await GetAllDepartmentsAsync(ignoreCache);
                if (departmentEntities == null)
                {
                    throw new ColleagueWebApiException("An error occurred retrieving Departments");
                }

                var academicDepartments = departmentEntities.Where(x => x.DepartmentType != "H");

                if (academicDepartments != null && academicDepartments.Any())
                {
                    if (departmentStatus != Dtos.EnumProperties.Status.NotSet)
                    {
                        bool active = departmentStatus == Dtos.EnumProperties.Status.Active ? true : false;
                        academicDepartments = academicDepartments.Where(d => d.IsActive == active);
                    }

                    foreach (var department in academicDepartments)
                    {
                        educationalInstitutionUnitsCollection.Add(await ConvertDepartmentEntityToEducationalInstitutionUnits3DtoAsync(department, ignoreCache));
                    }
                }
            }

            if ((departmentStatus == Dtos.EnumProperties.Status.NotSet) &&
                ((educationalInstitutionUnitType == Dtos.EnumProperties.EducationalInstitutionUnitType.NotSet) ||
               (educationalInstitutionUnitType == Dtos.EnumProperties.EducationalInstitutionUnitType.Division)))
            {
                //Add departments to collection of EducationalInstitutionUnits
                var divisionEntities = await GetAllDivisionsAsync(ignoreCache);
                if (divisionEntities == null)
                {
                    throw new ColleagueWebApiException("An error occurred retrieving Divisions");
                }
                foreach (var division in divisionEntities)
                {
                    educationalInstitutionUnitsCollection.Add(await ConvertDivisionEntityToEducationalInstitutionUnits3DtoAsync(division, ignoreCache));
                }
            }

            if ((departmentStatus == Dtos.EnumProperties.Status.NotSet) &&
                ((educationalInstitutionUnitType == Dtos.EnumProperties.EducationalInstitutionUnitType.NotSet) ||
               (educationalInstitutionUnitType == Dtos.EnumProperties.EducationalInstitutionUnitType.School)))
            {
                //Add school to collection of EducationalInstitutionUnits
                var schoolEntities = await GetAllSchoolsAsync(ignoreCache);
                if (schoolEntities == null)
                {
                    throw new ColleagueWebApiException("An error occurred retrieving Schools");
                }

                foreach (var school in schoolEntities)
                {
                    educationalInstitutionUnitsCollection.Add(await ConvertSchoolEntityToEducationalInstitutionUnits3DtoAsync(school, ignoreCache));
                }
            }

            return educationalInstitutionUnitsCollection;
        }
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all Educational-Institution-Units
        /// </summary>
        /// <returns>Collection of EducationalInstitutionUnits DTO objects</returns>
        public async Task<IEnumerable<EducationalInstitutionUnits>> GetEducationalInstitutionUnitsAsync(bool ignoreCache = false)
        {

            var educationalInstitutionUnitsCollection = new List<EducationalInstitutionUnits>();

            // Add departments to collection of EducationalInstitutionUnits
            var departmentEntities = await GetAllDepartmentsAsync(ignoreCache);
            if (departmentEntities != null && departmentEntities.Any())
            {
                foreach (var department in departmentEntities)
                {
                    educationalInstitutionUnitsCollection.Add(await ConvertDepartmentEntityToEducationalInstitutionUnitsDtoAsync(department));
                }
            }

            //Add departments to collection of EducationalInstitutionUnits
            var divisionEntities = await GetAllDivisionsAsync(ignoreCache);
            if (divisionEntities != null && divisionEntities.Any())
            {
                foreach (var division in divisionEntities)
                {
                    educationalInstitutionUnitsCollection.Add(await ConvertDivisionEntityToEducationalInstitutionUnitsDtoAsync(division));
                }
            }

            //Add school to collection of EducationalInstitutionUnits
            var schoolEntities = await GetAllSchoolsAsync(ignoreCache);
            if (schoolEntities != null && schoolEntities.Any())
            {
                foreach (var school in schoolEntities)
                {
                    educationalInstitutionUnitsCollection.Add(await ConvertSchoolEntityToEducationalInstitutionUnitsDtoAsync(school));
                }
            }

            return educationalInstitutionUnitsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all Educational-Institution-Units from type
        /// including department, division, or school code
        /// </summary>
        /// <returns>Collection of EducationalInstitutionUnits2 DTO objects</returns>
        public async Task<IEnumerable<EducationalInstitutionUnits2>> GetEducationalInstitutionUnitsByType2Async(string type, bool ignoreCache = false)
        {
            var educationalInstitutionUnitsCollection = new List<EducationalInstitutionUnits2>();

            if (string.IsNullOrEmpty(type) || type == "department")
            {
                // Add departments to collection of EducationalInstitutionUnits
                var departmentEntities = await GetAllDepartmentsAsync(ignoreCache);
                if (departmentEntities != null && departmentEntities.Any())
                {
                    foreach (var department in departmentEntities)
                    {
                        educationalInstitutionUnitsCollection.Add(
                            await ConvertDepartmentEntityToEducationalInstitutionUnits2DtoAsync(department));
                    }
                }
            }

            if (string.IsNullOrEmpty(type) || type == "division")
            {
                //Add divisions to collection of EducationalInstitutionUnits
                var divisionEntities = await GetAllDivisionsAsync(ignoreCache);
                if (divisionEntities != null && divisionEntities.Any())
                {
                    foreach (var division in divisionEntities)
                    {
                        educationalInstitutionUnitsCollection.Add(await ConvertDivisionEntityToEducationalInstitutionUnits2DtoAsync(division));
                    }
                }
            }
            if (string.IsNullOrEmpty(type) || type == "school")
            {
                //Add schools to collection of EducationalInstitutionUnits
                var schoolEntities = await GetAllSchoolsAsync(ignoreCache);
                if (schoolEntities != null && schoolEntities.Any())
                {
                    foreach (var school in schoolEntities)
                    {
                        educationalInstitutionUnitsCollection.Add(await ConvertSchoolEntityToEducationalInstitutionUnits2DtoAsync(school));
                    }
                }
            }
                return educationalInstitutionUnitsCollection;
            
        }
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all Educational-Institution-Units from type
        /// </summary>
        /// <returns>Collection of EducationalInstitutionUnits DTO objects</returns>
        public async Task<IEnumerable<EducationalInstitutionUnits>> GetEducationalInstitutionUnitsByTypeAsync(string type, bool ignoreCache = false)
        {

            var educationalInstitutionUnitsCollection = new List<EducationalInstitutionUnits>();
            
            if (string.IsNullOrEmpty(type) || type == "department") {
                // Add departments to collection of EducationalInstitutionUnits
                var departmentEntities = await GetAllDepartmentsAsync(ignoreCache);
                if (departmentEntities != null && departmentEntities.Any())
                {
                    foreach (var department in departmentEntities)
                    {
                        educationalInstitutionUnitsCollection.Add(
                            await ConvertDepartmentEntityToEducationalInstitutionUnitsDtoAsync(department));
                    }
                }
            }
            if (string.IsNullOrEmpty(type) || type == "division")
            {
                //Add divisions to collection of EducationalInstitutionUnits
                var divisionEntities = await GetAllDivisionsAsync(ignoreCache);
                if (divisionEntities != null && divisionEntities.Any())
                {
                    foreach (var division in divisionEntities)
                    {
                        educationalInstitutionUnitsCollection.Add(await ConvertDivisionEntityToEducationalInstitutionUnitsDtoAsync(division));
                    }
                }
            }
            if (string.IsNullOrEmpty(type) || type == "department")
            {
                //Add schools to collection of EducationalInstitutionUnits
                var schoolEntities = await GetAllSchoolsAsync(ignoreCache);
                if (schoolEntities != null && schoolEntities.Any())
                {
                    foreach (var school in schoolEntities)
                    {
                        educationalInstitutionUnitsCollection.Add(await ConvertSchoolEntityToEducationalInstitutionUnitsDtoAsync(school));
                    }
                }
            }

            return educationalInstitutionUnitsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a Educational-Institution-Units from its GUID
        /// including departement, division, or school code
        /// </summary>
        /// <param name="guid">Remark GUID</param>
        /// <returns>EducationalInstitutionUnits3 DTO object</returns>
        public async Task<EducationalInstitutionUnits3> GetEducationalInstitutionUnitsByGuid3Async(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a EducationalInstitutionUnits.");
            }

            try
            {
                var lookupResult = await _referenceDataRepository.GetGuidLookupResultFromGuidAsync(guid);
                if (lookupResult == null)
                    throw new KeyNotFoundException(string.Concat("Educational Institution Units with ID ", guid, " was not found."));


                switch (lookupResult.Entity.ToUpperInvariant())
                {
                    case "DEPTS":
                        return await ConvertDepartmentEntityToEducationalInstitutionUnits3DtoAsync(await _referenceDataRepository.GetDepartmentByGuidAsync(guid));

                    case "DIVISIONS":
                        return await ConvertDivisionEntityToEducationalInstitutionUnits3DtoAsync(await _referenceDataRepository.GetDivisionByGuidAsync(guid));

                    case "SCHOOLS":
                        return await ConvertSchoolEntityToEducationalInstitutionUnits3DtoAsync(await _referenceDataRepository.GetSchoolByGuidAsync(guid));
                    default:
                        throw new KeyNotFoundException(string.Concat("Educational Institution Units with ID ", guid, " was not found."));
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Concat("Educational Institution Units with ID ", guid, " was not found."), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Concat("Educational Institution Units with ID ", guid, " was not found."), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a Educational-Institution-Units from its GUID
        /// including departement, division, or school code
        /// </summary>
        /// <param name="guid">Remark GUID</param>
        /// <returns>EducationalInstitutionUnits2 DTO object</returns>
        public async Task<EducationalInstitutionUnits2> GetEducationalInstitutionUnitsByGuid2Async(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a EducationalInstitutionUnits.");
            }

            try
            {
                var lookupResult = await _referenceDataRepository.GetGuidLookupResultFromGuidAsync(guid);
                if (lookupResult == null)
                    throw new KeyNotFoundException(string.Concat("Educational Institution Units with ID ", guid, " was not found."));


                switch (lookupResult.Entity.ToUpperInvariant())
                {
                    case "DEPTS":
                        return await ConvertDepartmentEntityToEducationalInstitutionUnits2DtoAsync(await _referenceDataRepository.GetDepartmentByGuidAsync(guid));

                    case "DIVISIONS":
                        return await ConvertDivisionEntityToEducationalInstitutionUnits2DtoAsync(await _referenceDataRepository.GetDivisionByGuidAsync(guid));

                    case "SCHOOLS":
                        return await ConvertSchoolEntityToEducationalInstitutionUnits2DtoAsync(await _referenceDataRepository.GetSchoolByGuidAsync(guid));
                    default:
                        throw new KeyNotFoundException(string.Concat("Educational Institution Units with ID ", guid, " was not found."));
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Concat("Educational Institution Units with ID ", guid, " was not found."), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Concat("Educational Institution Units with ID ", guid, " was not found."), ex);
            }
        }
        
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a Educational-Institution-Units from its GUID
        /// </summary>
        /// <param name="guid">Remark GUID</param>
        /// <returns>EducationalInstitutionUnits DTO object</returns>
        public async Task<EducationalInstitutionUnits> GetEducationalInstitutionUnitsByGuidAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get a EducationalInstitutionUnits.");
            }

            try
            {
                var lookupResult = await _referenceDataRepository.GetGuidLookupResultFromGuidAsync(guid);
                if (lookupResult == null)
                    throw new KeyNotFoundException(string.Concat("Educational Institution Units with ID ", guid, " was not found."));


                switch (lookupResult.Entity.ToUpperInvariant())
                {
                    case "DEPTS":
                        return await ConvertDepartmentEntityToEducationalInstitutionUnitsDtoAsync(await _referenceDataRepository.GetDepartmentByGuidAsync(guid));

                    case "DIVISIONS":
                        return await ConvertDivisionEntityToEducationalInstitutionUnitsDtoAsync(await _referenceDataRepository.GetDivisionByGuidAsync(guid));

                    case "SCHOOLS":
                        return await ConvertSchoolEntityToEducationalInstitutionUnitsDtoAsync(await _referenceDataRepository.GetSchoolByGuidAsync(guid));

                    default:
                        throw new KeyNotFoundException(string.Concat("Educational Institution Units with ID ", guid, " was not found."));
                }
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Concat("Educational Institution Units with ID ", guid, " was not found."), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(string.Concat("Educational Institution Units with ID ", guid, " was not found."), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert Department entity to an EducationalInstitutionUnits2 DTO
        /// </summary>
        /// <param name="source">Department domain entity</param>
        /// <param name="ignoreCache"></param>
        /// <returns>EducationalInstitutionUnits2 DTO</returns>
        private async Task<EducationalInstitutionUnits2> ConvertDepartmentEntityToEducationalInstitutionUnits2DtoAsync(Domain.Base.Entities.Department source, bool ignoreCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("department", "Department entity must be provided.");
            }

            var educationalInstitutionUnits = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits2
            {
                Id = source.Guid,
                EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                Title = source.Description,
                Code = source.Code
            };
            var parents = new EducationalInstitutionUnitParentDtoProperty();

            // For DEPTS the upper level is DIVISIONS therfore, we point the unit to the GUID assigned to the DEPTS.DIVISION entity.
            Domain.Base.Entities.Division division = null;
            if (!string.IsNullOrEmpty(source.Division))
            {
                var divisions = await GetAllDivisionsAsync(ignoreCache);
                if (divisions != null)
                {
                    division = divisions.FirstOrDefault( d => d != null && !string.IsNullOrWhiteSpace( d.Code ) && d.Code == source.Division );
                    if (division != null)
                        parents.Unit = new GuidObject2(division.Guid);
                }
            }

            // lookup department institutionId
            if (!string.IsNullOrEmpty(source.InstitutionId))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(source.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            // if department institutionId isnt found, lookup division institution id
            if ((parents.Institution == null) && (division != null) && (!string.IsNullOrWhiteSpace(division.InstitutionId)))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(division.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            if (parents.Institution == null)
            {
                // if division institutionId not found, use the schools institutionId
                var schoolCode = string.Empty;
                if (!string.IsNullOrEmpty(source.School))
                    schoolCode = source.School;
                else if ((division != null) && (!string.IsNullOrEmpty(division.SchoolCode)))
                    schoolCode = division.SchoolCode;

                if (!string.IsNullOrEmpty(schoolCode))
                {
                    var schools = await GetAllSchoolsAsync(ignoreCache);
                    if (schools != null)
                    {
                        var school = schools.FirstOrDefault( s => s != null && !string.IsNullOrWhiteSpace( s.Code ) && s.Code == schoolCode );
                        //if (school != null)
                        //parents.Institution = new GuidObject2(school.Guid);
                        if ((school != null) && (!string.IsNullOrEmpty(school.InstitutionId)))
                        {
                            var institution = await _personRepository.GetPersonGuidFromIdAsync(school.InstitutionId);
                            if (!string.IsNullOrEmpty(institution))
                                parents.Institution = new GuidObject2(institution);
                        }
                    }
                }
            }
            // if nothing found, use the default organization id
            if (parents.Institution == null)
                parents.Institution = new GuidObject2(await GetDefaultHostGuidAsync());

            educationalInstitutionUnits.Parents = parents;
            return educationalInstitutionUnits;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert Department entity to an EducationalInstitutionUnits3 DTO
        /// </summary>
        /// <param name="source">Department domain entity</param>
        /// <param name="ignoreCache"></param>
        /// <returns>EducationalInstitutionUnits2 DTO</returns>
        private async Task<EducationalInstitutionUnits3> ConvertDepartmentEntityToEducationalInstitutionUnits3DtoAsync(Domain.Base.Entities.Department source, bool ignoreCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("department", "Department entity must be provided.");
            }

            var educationalInstitutionUnits = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits3
            {
                Id = source.Guid,
                Type = Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                Title = source.Description,
                Code = source.Code,
                
            };
            var parents = new EducationalInstitutionUnitParentDtoProperty();

            // For DEPTS the upper level is DIVISIONS therfore, we point the unit to the GUID assigned to the DEPTS.DIVISION entity.
            Domain.Base.Entities.Division division = null;
            if (!string.IsNullOrEmpty(source.Division))
            {
                var divisions = await GetAllDivisionsAsync(ignoreCache);
                if (divisions != null)
                {
                    division = divisions.FirstOrDefault(d => d != null && !string.IsNullOrWhiteSpace(d.Code) && d.Code == source.Division);
                    if (division != null)
                        parents.Unit = new GuidObject2(division.Guid);
                }
            }

            // lookup department institutionId
            if (!string.IsNullOrEmpty(source.InstitutionId))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(source.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            // if department institutionId isnt found, lookup division institution id
            if ((parents.Institution == null) && (division != null) && (!string.IsNullOrEmpty(division.InstitutionId)))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(division.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            if (parents.Institution == null)
            {
                // if division institutionId not found, use the schools institutionId
                var schoolCode = string.Empty;
                if (!string.IsNullOrEmpty(source.School))
                    schoolCode = source.School;
                else if ((division != null) && (!string.IsNullOrEmpty(division.SchoolCode)))
                    schoolCode = division.SchoolCode;

                if (!string.IsNullOrEmpty(schoolCode))
                {
                    var schools = await GetAllSchoolsAsync(ignoreCache);
                    if (schools != null)
                    {
                        var school = schools.FirstOrDefault(s => s != null && !string.IsNullOrWhiteSpace(s.Code) && s.Code == schoolCode);
                        //if (school != null)
                        //    parents.Institution = new GuidObject2(school.Guid);
                        if ((school != null) && (!string.IsNullOrEmpty(school.InstitutionId)))
                        {
                            var institution = await _personRepository.GetPersonGuidFromIdAsync(school.InstitutionId);
                            if (!string.IsNullOrEmpty(institution))
                                parents.Institution = new GuidObject2(institution);
                        }
                    }
                }
            }
            // if nothing found, use the default organization id
            if (parents.Institution == null)
                parents.Institution = new GuidObject2(await GetDefaultHostGuidAsync());

            educationalInstitutionUnits.Parents = parents;
            return educationalInstitutionUnits;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert Department entity to an EducationalInstitutionUnits DTO
        /// </summary>
        /// <param name="source">Department domain entity</param>
        /// <param name="ignoreCache"></param>
        /// <returns>EducationalInstitutionUnits DTO</returns>
        private async Task<EducationalInstitutionUnits>ConvertDepartmentEntityToEducationalInstitutionUnitsDtoAsync(Domain.Base.Entities.Department source, bool ignoreCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("department", "Department entity must be provided.");
            }

            var educationalInstitutionUnits = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits
            {
                Id = source.Guid,
                EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                Title = source.Description
            };
            var parents = new EducationalInstitutionUnitParentDtoProperty();
          
            // For DEPTS the upper level is DIVISIONS therfore, we point the unit to the GUID assigned to the DEPTS.DIVISION entity.
           Domain.Base.Entities.Division division = null;
            if (!string.IsNullOrEmpty(source.Division))
            {
                var divisions = await GetAllDivisionsAsync(ignoreCache);
                if (divisions != null)
                {
                    division = divisions.FirstOrDefault( d => d != null && !string.IsNullOrWhiteSpace( d.Code ) && d.Code == source.Division );
                    if (division != null)
                        parents.Unit = new GuidObject2(division.Guid);
                }
            }

            // lookup department institutionId
            if (!string.IsNullOrEmpty(source.InstitutionId))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(source.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            // if department institutionId isnt found, lookup division institution id
            if ((parents.Institution == null) && (division != null) && (!string.IsNullOrEmpty(division.InstitutionId)))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(division.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            if (parents.Institution == null)
            {
                // if division institutionId not found, use the schools institutionId
                var schoolCode = string.Empty;
                if (!string.IsNullOrEmpty(source.School))
                    schoolCode = source.School;
                else if ((division != null) && (!string.IsNullOrEmpty(division.SchoolCode)))
                    schoolCode = division.SchoolCode;

                if (!string.IsNullOrEmpty(schoolCode))
                {
                    var schools = await GetAllSchoolsAsync(ignoreCache);
                    if (schools != null)
                    {
                        var school = schools.FirstOrDefault(s => s != null && !string.IsNullOrWhiteSpace(s.Code) && s.Code == schoolCode);
                        //if (school != null)
                        //    parents.Institution = new GuidObject2(school.Guid);
                        if ((school != null) && (!string.IsNullOrEmpty(school.InstitutionId)))
                        {
                            var institution = await _personRepository.GetPersonGuidFromIdAsync(school.InstitutionId);
                            if (!string.IsNullOrEmpty(institution))
                                parents.Institution = new GuidObject2(institution);
                        }
                    }
                }
            }
            // if nothing found, use the default organization id
            if (parents.Institution == null)
                parents.Institution = new GuidObject2(await GetDefaultHostGuidAsync());

            educationalInstitutionUnits.Parents = parents;
            return educationalInstitutionUnits;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert School entity to an EducationalInstitutionUnits2 DTO
        /// </summary>
        /// <param name="source">Division domain entity</param>
        /// <param name="ignoreCache"></param>
        /// <returns>EducationalInstitutionUnits2 DTO</returns>
        private async Task<EducationalInstitutionUnits2> ConvertSchoolEntityToEducationalInstitutionUnits2DtoAsync(Domain.Base.Entities.School source, bool ignoreCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("school", "School entity must be provided.");
            }
            var educationalInstitutionUnits = new EducationalInstitutionUnits2
            {
                Id = source.Guid,
                EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                Title = source.Description,
                Code = source.Code
            };

            var parents = new EducationalInstitutionUnitParentDtoProperty();

            // No unit associated with the school

            //The upper level of a SCHOOLS entity in Colleague is the institution ID therefore, for the Schools we need to have an institution defined
            if (!string.IsNullOrEmpty(source.InstitutionId))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(source.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            if (parents.Institution == null)
                parents.Institution = new GuidObject2(await GetDefaultHostGuidAsync());


            educationalInstitutionUnits.Parents = parents;
            return educationalInstitutionUnits;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert School entity to an EducationalInstitutionUnits2 DTO
        /// </summary>
        /// <param name="source">Division domain entity</param>
        /// <param name="ignoreCache"></param>
        /// <returns>EducationalInstitutionUnits3 DTO</returns>
        private async Task<EducationalInstitutionUnits3> ConvertSchoolEntityToEducationalInstitutionUnits3DtoAsync(Domain.Base.Entities.School source, bool ignoreCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("school", "School entity must be provided.");
            }
            var educationalInstitutionUnits = new EducationalInstitutionUnits3
            {
                Id = source.Guid,
                Type = Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                Title = source.Description,
                Code = source.Code
            };

            var parents = new EducationalInstitutionUnitParentDtoProperty();

            // No unit associated with the school

            //The upper level of a SCHOOLS entity in Colleague is the institution ID therefore, for the Schools we need to have an institution defined
            if (!string.IsNullOrEmpty(source.InstitutionId))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(source.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            if (parents.Institution == null)
                parents.Institution = new GuidObject2(await GetDefaultHostGuidAsync());


            educationalInstitutionUnits.Parents = parents;
            return educationalInstitutionUnits;
        }
        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert School entity to an EducationalInstitutionUnits DTO
        /// </summary>
        /// <param name="source">Division domain entity</param>
        /// <param name="ignoreCache"></param>
        /// <returns>EducationalInstitutionUnits DTO</returns>
        private async Task<EducationalInstitutionUnits> ConvertSchoolEntityToEducationalInstitutionUnitsDtoAsync(Domain.Base.Entities.School source, bool ignoreCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("school", "School entity must be provided.");
            }
            var educationalInstitutionUnits = new EducationalInstitutionUnits
            {
                Id = source.Guid,
                EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.School,
                Title = source.Description
            };

            var parents = new EducationalInstitutionUnitParentDtoProperty();

            // No unit associated with the school

            //The upper level of a SCHOOLS entity in Colleague is the institution ID therefore, for the Schools we need to have an institution defined
            if (!string.IsNullOrEmpty(source.InstitutionId))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(source.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            if (parents.Institution == null)
                parents.Institution = new GuidObject2(await GetDefaultHostGuidAsync());


            educationalInstitutionUnits.Parents = parents;
            return educationalInstitutionUnits;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert Division entity to an EducationalInstitutionUnits2 DTO
        /// </summary>
        /// <param name="source">Division domain entity</param>
        /// <param name="ignoreCache"></param>
        /// <returns>EducationalInstitutionUnits2 DTO</returns>
        private async Task<EducationalInstitutionUnits2> ConvertDivisionEntityToEducationalInstitutionUnits2DtoAsync(Domain.Base.Entities.Division source, bool ignoreCache = false)
        {
            var educationalInstitutionUnits = new EducationalInstitutionUnits2();

            if (source == null)
            {
                throw new ArgumentNullException("division", "Division entity must be provided.");
            }

            educationalInstitutionUnits.Id = source.Guid;
            educationalInstitutionUnits.EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Division;
            educationalInstitutionUnits.Title = source.Description;
            educationalInstitutionUnits.Code = source.Code;

            var parents = new EducationalInstitutionUnitParentDtoProperty();

            Domain.Base.Entities.School school = null;
            if (!string.IsNullOrEmpty(source.SchoolCode))
            {
                var schools = await GetAllSchoolsAsync(ignoreCache);
                if (schools != null)
                {
                    school = schools.FirstOrDefault( s => s != null && !string.IsNullOrWhiteSpace( s.Code ) && s.Code == source.SchoolCode );
                    if (school != null)
                    {
                        parents.Unit = new GuidObject2(school.Guid);
                    }
                }
            }
            // lookup division institutionId
            if (!string.IsNullOrEmpty(source.InstitutionId))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(source.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            // if division institutionId wasnt found, lookup school institutionId
            if ((parents.Institution == null) && (school != null) && !(string.IsNullOrEmpty(school.InstitutionId)))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(school.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            // if nothing found, use the default organization id
            if (parents.Institution == null)
                parents.Institution = new GuidObject2(await GetDefaultHostGuidAsync());

            educationalInstitutionUnits.Parents = parents;
            return educationalInstitutionUnits;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert Division entity to an EducationalInstitutionUnits3 DTO
        /// </summary>
        /// <param name="source">Division domain entity</param>
        /// <param name="ignoreCache"></param>
        /// <returns>EducationalInstitutionUnits3 DTO</returns>
        private async Task<EducationalInstitutionUnits3> ConvertDivisionEntityToEducationalInstitutionUnits3DtoAsync(Domain.Base.Entities.Division source, bool ignoreCache = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException("division", "Division entity must be provided.");
            }
            var educationalInstitutionUnits = new EducationalInstitutionUnits3()
            {
                Id = source.Guid,
                Type = Dtos.EnumProperties.EducationalInstitutionUnitType.Division,
                Title = source.Description,
                Code = source.Code
            };

            var parents = new EducationalInstitutionUnitParentDtoProperty();

            Domain.Base.Entities.School school = null;
            if (!string.IsNullOrEmpty(source.SchoolCode))
            {
                var schools = await GetAllSchoolsAsync(ignoreCache);
                if (schools != null)
                {
                    school = schools.FirstOrDefault(s => s != null && !string.IsNullOrWhiteSpace(s.Code) && s.Code == source.SchoolCode);
                    if (school != null)
                    {
                        parents.Unit = new GuidObject2(school.Guid);
                    }
                }
            }
            // lookup division institutionId
            if (!string.IsNullOrEmpty(source.InstitutionId))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(source.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            // if division institutionId wasnt found, lookup school institutionId
            if ((parents.Institution == null) && (school != null) && !(string.IsNullOrEmpty(school.InstitutionId)))
            {
                var institution = await _personRepository.GetPersonGuidFromIdAsync(school.InstitutionId);
                if (!string.IsNullOrEmpty(institution))
                    parents.Institution = new GuidObject2(institution);
            }
            // if nothing found, use the default organization id
            if (parents.Institution == null)
                parents.Institution = new GuidObject2(await GetDefaultHostGuidAsync());

            educationalInstitutionUnits.Parents = parents;
            return educationalInstitutionUnits;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Convert Division entity to an EducationalInstitutionUnits DTO
        /// </summary>
        /// <param name="source">Division domain entity</param>
        /// <param name="ignoreCache"></param>
        /// <returns>EducationalInstitutionUnits DTO</returns>
        private async Task<EducationalInstitutionUnits> ConvertDivisionEntityToEducationalInstitutionUnitsDtoAsync(Domain.Base.Entities.Division source, bool ignoreCache = false)
        {
            var educationalInstitutionUnits = new EducationalInstitutionUnits();

            if (source == null)
            {
                throw new ArgumentNullException("division", "Division entity must be provided.");
            }

            educationalInstitutionUnits.Id = source.Guid;
            educationalInstitutionUnits.EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Division;
            educationalInstitutionUnits.Title = source.Description;

            var parents = new EducationalInstitutionUnitParentDtoProperty();

            Domain.Base.Entities.School school = null;
            if (!string.IsNullOrEmpty(source.SchoolCode))
            {
                var schools = await GetAllSchoolsAsync(ignoreCache);
                if (schools != null)
                {
                    school = schools.FirstOrDefault( s => s != null && !string.IsNullOrWhiteSpace( s.Code ) && s.Code == source.SchoolCode );
                    if (school != null)
                    {
                        parents.Unit = new GuidObject2(school.Guid);
                    }
                }
            }
            // lookup division institutionId
            if(!string.IsNullOrEmpty(source.InstitutionId))
            {
                 var institution = await _personRepository.GetPersonGuidFromIdAsync(source.InstitutionId);
                        if (!string.IsNullOrEmpty(institution))
                            parents.Institution = new GuidObject2(institution);
            }
            // if division institutionId wasnt found, lookup school institutionId
            if ((parents.Institution == null) && (school != null) &&  !(string.IsNullOrEmpty(school.InstitutionId)))
            {
                 var institution = await _personRepository.GetPersonGuidFromIdAsync(school.InstitutionId);
                        if (!string.IsNullOrEmpty(institution))
                            parents.Institution = new GuidObject2(institution);
            }
            // if nothing found, use the default organization id
             if (parents.Institution == null)
                parents.Institution = new GuidObject2(await GetDefaultHostGuidAsync());

            educationalInstitutionUnits.Parents = parents;
            return educationalInstitutionUnits;
        }

        /// <summary>
        ///  Get default organization id from PID2
        /// </summary>
        /// <returns>guid for default organization id</returns>
        private async Task<string> GetDefaultHostGuidAsync()
        {
            if (!string.IsNullOrEmpty(_defaultHostGuid)) return _defaultHostGuid;
            var hostGuid = string.Empty;
            var defaultsConfiguration = _configurationRepository.GetDefaultsConfiguration();
            if (defaultsConfiguration != null)
            {
                var hostId = defaultsConfiguration.HostInstitutionCodeId;

                hostGuid = await _personRepository.GetPersonGuidFromIdAsync(hostId);
            }
            if (string.IsNullOrEmpty(hostGuid))
                throw new KeyNotFoundException("Unable to determine default institution from PID2.");
            _defaultHostGuid = hostGuid;

            return _defaultHostGuid;
        }
    }
}