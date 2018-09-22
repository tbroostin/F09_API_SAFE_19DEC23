// Copyright 2015-18 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Student.Repositories;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AcademicProgramService : BaseCoordinationService, IAcademicProgramService
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AcademicProgramService class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="personRepository">Repository of type <see cref="IPersonRepository">IPersonRepository</see></param>
        /// <param name="catalogRepository">Repository of type <see cref="ICatalogRepository">ICatalogRepository</see></param>
        /// <param name="institutionRepositor">Repository of type <see cref="IInstitutionRepository">IInstitutionRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AcademicProgramService(IAdapterRegistry adapterRegistry,  IStudentReferenceDataRepository studentReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository, IPersonRepository personRepository, ICatalogRepository catalogRepository,
            IInstitutionRepository institutionRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {          
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _referenceDataRepository = referenceDataRepository;
            _personRepository = personRepository;
            _catalogRepository = catalogRepository;
            _institutionRepository = institutionRepository;
            _logger = logger;
        }

        private IEnumerable<Domain.Base.Entities.OtherDegree> _otherDegrees = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherDegree>> GetOtherDegreesAsync(bool bypassCache)
        {
            if (_otherDegrees == null)
            {
                _otherDegrees = await _referenceDataRepository.GetOtherDegreesAsync(bypassCache);
            }
            return _otherDegrees;
        }

        private IEnumerable<Domain.Base.Entities.OtherCcd> _otherCcds = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherCcd>> GetOtherCcdAsync(bool bypassCache)
        {
            if (_otherCcds == null)
            {
                _otherCcds = await _referenceDataRepository.GetOtherCcdsAsync(bypassCache);
            }
            return _otherCcds;
        }

        private IEnumerable<Domain.Base.Entities.OtherMajor> _otherMajor = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherMajor>> GetOtherMajorsAsync(bool bypassCache)
        {
            if (_otherMajor == null)
            {
                _otherMajor = await _referenceDataRepository.GetOtherMajorsAsync(bypassCache);
            }
            return _otherMajor;
        }

        private IEnumerable<Domain.Base.Entities.OtherMinor> _otherMinor = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherMinor>> GetOtherMinorsAsync(bool bypassCache)
        {
            if (_otherMinor == null)
            {
                _otherMinor = await _referenceDataRepository.GetOtherMinorsAsync(bypassCache);
            }
            return _otherMinor;
        }

        private IEnumerable<Domain.Base.Entities.OtherSpecial> _otherSpecials = null;
        private async Task<IEnumerable<Domain.Base.Entities.OtherSpecial>> GetOtherSpecialsAsync(bool bypassCache)
        {
            if (_otherSpecials == null)
            {
                _otherSpecials = await _referenceDataRepository.GetOtherSpecialsAsync(bypassCache);
            }
            return _otherSpecials;
        }

        private IEnumerable<Domain.Base.Entities.Location> _otherLocation = null;
        private async Task<IEnumerable<Domain.Base.Entities.Location>> GetLocationsAsync(bool bypassCache)
        {
            if (_otherLocation == null)
            {
                _otherLocation = await _referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _otherLocation;
        }

        private IEnumerable<Domain.Base.Entities.Department> _otherDepartments = null;
        private async Task<IEnumerable<Domain.Base.Entities.Department>> GetDepartmentsAsync(bool bypassCache)
        {
            if (_otherDepartments == null)
            {
                _otherDepartments = await _referenceDataRepository.GetDepartmentsAsync(bypassCache);
            }
            return _otherDepartments;
        }

        private IEnumerable<Domain.Student.Entities.AcademicLevel> _academicLevels = null;
        private async Task<IEnumerable<Domain.Student.Entities.AcademicLevel>> GetAcademicLevelsAsync(bool bypassCache)
        {
            if (_academicLevels == null)
            {
                _academicLevels = await _studentReferenceDataRepository.GetAcademicLevelsAsync(bypassCache);
            }
            return _academicLevels;
        }

        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Catalog> _catalogs = null;
        private async Task<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Catalog>> GetCatalogAsync(bool bypassCache)
        {
            if (_catalogs == null)
            {
                _catalogs = await _catalogRepository.GetAsync(bypassCache);
            }
            return _catalogs;
        }

        /// <summary>
        /// Get all academicPrograms.
        /// </summary>
        /// <returns>List of <see cref="Ellucian.Colleague.Dtos.Student.AcademicProgram">AcademicProgram</see> data.</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.AcademicProgram>> GetAsync()
        {
            var academicProgramCollection = await _studentReferenceDataRepository.GetAcademicProgramsAsync();

            // Get the right adapter for the type mapping
            var academicProgramDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.AcademicProgram, Ellucian.Colleague.Dtos.Student.AcademicProgram>();

            // Map the student type entity to the student type DTO
            var academicProgramDtoCollection = new List<Ellucian.Colleague.Dtos.Student.AcademicProgram>();
            foreach (var academicProgram in academicProgramCollection)
            {
                academicProgramDtoCollection.Add(academicProgramDtoAdapter.MapToType(academicProgram));
            }

            return academicProgramDtoCollection;
        }

        /// <summary>
        /// Get all academicPrograms for EeDM version 6.
        /// </summary>
        /// <returns>List of <see cref="Ellucian.Colleague.Dtos.AcademicProgram">AcademicProgram</see> data.</returns>
        public async Task<IEnumerable<Colleague.Dtos.AcademicProgram2>> GetAcademicProgramsV6Async(bool bypassCache)
        {
            var academicProgramCollection = await _studentReferenceDataRepository.GetAcademicProgramsAsync(bypassCache);

            // Map the student type entity to the student type DTO
            var academicProgramDtoCollection = new List<Colleague.Dtos.AcademicProgram2>();

            if (academicProgramCollection == null)
            {
                return academicProgramDtoCollection;
            }
            var institutionIds = academicProgramCollection.Select(x => x.AuthorizingInstitute.FirstOrDefault())
               .Where(y => y != null)
               .ToArray();
            var institutions = await _institutionRepository.GetInstitutionIdsFromListAsync(institutionIds);
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(institutionIds);

            foreach (var academicProgram in academicProgramCollection)
            {
                academicProgramDtoCollection.Add(await ConvertAcademicProgramEntityToDtoV6(academicProgram,
                   personGuidCollection, institutions, bypassCache));
            }

            return academicProgramDtoCollection;
        }

        /// <summary>
        /// Get all academicPrograms for EeDM version 10.
        /// </summary>
        /// <returns>List of <see cref="Ellucian.Colleague.Dtos.AcademicProgram">AcademicProgram</see> data.</returns>
        public async Task<IEnumerable<Colleague.Dtos.AcademicProgram3>> GetAcademicPrograms3Async(bool bypassCache)
        {
            var academicProgramCollection = await _studentReferenceDataRepository.GetAcademicProgramsAsync(bypassCache);

            // Map the student type entity to the student type DTO
            var academicProgramDtoCollection = new List<Colleague.Dtos.AcademicProgram3>();

            if (academicProgramCollection == null)
            {
                return academicProgramDtoCollection;
            }

            var institutionIds = academicProgramCollection.Select(x => x.AuthorizingInstitute.FirstOrDefault())
              .Where(y => y != null)
              .ToArray();
            var institutions = await _institutionRepository.GetInstitutionIdsFromListAsync(institutionIds);
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(institutionIds);

            foreach (var academicProgram in academicProgramCollection)
            {
                academicProgramDtoCollection.Add(await ConvertAcademicProgramEntityToDto3Async(academicProgram, personGuidCollection, 
                    institutions, bypassCache));
            }

            return academicProgramDtoCollection;
        }

        /// <summary>
        /// Gets all academic-programs
        /// </summary>
        /// <param name="academicCatalog">academicCatalog guid from filter</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <returns>Collection of <see cref="Dtos.AcademicProgram4">academicPrograms</see> objects</returns> 
        public async Task<IEnumerable<Colleague.Dtos.AcademicProgram4>> GetAcademicPrograms4Async( 
            string academicCatalog, bool bypassCache = false)
        {
            var academicProgramDtoCollection = new List<Colleague.Dtos.AcademicProgram4>();
            var academicProgramCollection = await _studentReferenceDataRepository.GetAcademicProgramsAsync(bypassCache);

            if (academicProgramCollection == null)
            {
                return academicProgramDtoCollection;
            }

            // filter on academic catalog
            if (!string.IsNullOrEmpty(academicCatalog))
            {
               var catalogs = await this.GetCatalogAsync(bypassCache);
               if (catalogs == null)
                {
                    return academicProgramDtoCollection;
                }
                var catalog = catalogs.FirstOrDefault(c => c.Guid == academicCatalog);
                if (catalog == null)
                {
                    return academicProgramDtoCollection;
                }
               
                academicProgramCollection = academicProgramCollection.Where(p => catalog.AcadPrograms.Contains(p.Code));
            }

            var institutionIds = academicProgramCollection.Select(x => x.AuthorizingInstitute.FirstOrDefault())
                .Where(y => y != null).ToArray();
            var institutions = await _institutionRepository.GetInstitutionIdsFromListAsync(institutionIds);
            var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(institutionIds);

            foreach (var academicProgram in academicProgramCollection)
            {
                academicProgramDtoCollection.Add(await ConvertAcademicProgramEntityToDto4Async(academicProgram, personGuidCollection,
                    institutions, bypassCache));
            }

            return academicProgramDtoCollection;
        }

        /// <summary>
        /// Get an Academic Program from its GUID
        /// </summary>
        /// <returns>An AcademicProgram <see cref="Ellucian.Colleague.Dtos.AcademicProgram2">object</returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicProgram2> GetAcademicProgramByGuidV6Async(string guid)
        {
            try
            {
                var program = await _studentReferenceDataRepository.GetAcademicProgramByGuidAsync(guid);
                if (program == null)
                {
                    throw new KeyNotFoundException("Academic Program not found for GUID " + guid);
                }
                IEnumerable<string> institutions = null;
                Dictionary<string, string> personGuidCollection = null;

                var institution = program.AuthorizingInstitute.FirstOrDefault();
                if (!string.IsNullOrEmpty(institution))
                {
                    institutions = await _institutionRepository.GetInstitutionIdsFromListAsync(new string[] { institution });
                    personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(new string[] { institution });
                }

                return await ConvertAcademicProgramEntityToDtoV6(program, personGuidCollection, institutions);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Program not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Get an Academic Program from its GUID for EEDM version 10
        /// </summary>
        /// <returns>An AcademicProgram <see cref="Ellucian.Colleague.Dtos.AcademicProgram2">object</returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicProgram3> GetAcademicProgramByGuid3Async(string guid)
        {
            try
            {
                var program = await _studentReferenceDataRepository.GetAcademicProgramByGuidAsync(guid);
                if (program == null)
                {
                    throw new KeyNotFoundException("Academic Program not found for GUID " + guid);
                }
                IEnumerable<string> institutions = null;
                Dictionary<string, string> personGuidCollection = null;

                var institution = program.AuthorizingInstitute.FirstOrDefault();
                if (!string.IsNullOrEmpty(institution))
                {
                    institutions = await _institutionRepository.GetInstitutionIdsFromListAsync(new string[] { institution });
                    personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(new string[] { institution });
                }

                return await ConvertAcademicProgramEntityToDto3Async(program, personGuidCollection, institutions);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Program not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Get an Academic Program from its GUID 
        /// </summary>
        /// <returns>An AcademicProgram <see cref="Ellucian.Colleague.Dtos.AcademicProgram4">object</returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicProgram4> GetAcademicProgramByGuid4Async(string guid)
        {
            try
            {
                var program = await _studentReferenceDataRepository.GetAcademicProgramByGuidAsync(guid);

                if (program == null)
                {
                    throw new KeyNotFoundException("Academic Program not found for GUID " + guid);
                }
                IEnumerable<string> institutions = null;
                Dictionary<string, string> personGuidCollection = null;

                var institution = program.AuthorizingInstitute.FirstOrDefault();
                if (!string.IsNullOrEmpty(institution))
                {
                    institutions = await _institutionRepository.GetInstitutionIdsFromListAsync(new string[] { institution });
                    personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(new string[] { institution });
                }
                return await ConvertAcademicProgramEntityToDto4Async(program, personGuidCollection, institutions);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("Academic Program not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Program not found for GUID " + guid, ex);
            }
        }

        #region private methods

        private async Task<Colleague.Dtos.AcademicProgram2> ConvertAcademicProgramEntityToDtoV6(Colleague.Domain.Student.Entities.AcademicProgram academicProgramEntity,
             Dictionary<string, string> personGuidCollection, IEnumerable<string> institutions, bool bypassCache = false)
        {
            var academicProgramDto = new Colleague.Dtos.AcademicProgram2();
            academicProgramDto.Id = academicProgramEntity.Guid;
            academicProgramDto.Code = academicProgramEntity.Code;
            academicProgramDto.Title = academicProgramEntity.Description;
            academicProgramDto.Description = academicProgramEntity.LongDescription;

            //process approval agency as authorizing institution
            if (academicProgramEntity.AuthorizingInstitute.Any())
            {
                var institutionId = academicProgramEntity.AuthorizingInstitute.First();
                if (!string.IsNullOrEmpty(institutionId))
                {
                    bool isInstitution = false;
                    string institutionGuid = string.Empty;
                    if (institutions != null)
                    {
                        isInstitution = institutions.Contains(institutionId);
                    }

                    personGuidCollection.TryGetValue(institutionId, out institutionGuid);
                    if (string.IsNullOrEmpty(institutionGuid))
                    {
                        throw new KeyNotFoundException(string.Concat("AuthorizingInstitute, ", "Unable to locate guid for : '", institutionId, "'"));
                    }

                    academicProgramDto.Authorizing = (isInstitution)
                        ? new Dtos.AcademicProgramAuthorizing() { AuthorizingInstitution = new Dtos.GuidObject2(institutionGuid) }
                        : new Dtos.AcademicProgramAuthorizing() { Organization = new Dtos.GuidObject2(institutionGuid) };
                }
            }

            academicProgramDto.Sites = await ConvertSiteCodeToGuidAsync(academicProgramEntity.Location, bypassCache);
            academicProgramDto.AcademicLevel = await ConvertAcadLevelCodeToGuid(academicProgramEntity.AcadLevelCode, bypassCache);
            academicProgramDto.Credentials = await ConvertCredentialCodeToGuid(academicProgramEntity.DegreeCode, academicProgramEntity.CertificateCodes, bypassCache);
            academicProgramDto.Disciplines = await ConvertDisciplineCodeToGuid(academicProgramEntity.MajorCodes, academicProgramEntity.MinorCodes, academicProgramEntity.SpecializationCodes, bypassCache);
            academicProgramDto.StartDate = academicProgramEntity.StartDate;
            academicProgramDto.EndDate = academicProgramEntity.EndDate;
            academicProgramDto.ProgramOwners = await ConvertDeptCodeToGuid(academicProgramEntity.DeptartmentCodes, bypassCache);

            return academicProgramDto;
        }

        private async Task<Colleague.Dtos.AcademicProgram3> ConvertAcademicProgramEntityToDto3Async(Colleague.Domain.Student.Entities.AcademicProgram academicProgramEntity,
            Dictionary<string, string> personGuidCollection, IEnumerable<string> institutions, bool bypassCache = false)
        {
            var academicProgramDto = new Colleague.Dtos.AcademicProgram3();
            academicProgramDto.Id = academicProgramEntity.Guid;
            academicProgramDto.Code = academicProgramEntity.Code;
            academicProgramDto.Title = academicProgramEntity.Description;
            academicProgramDto.Description = academicProgramEntity.LongDescription;

            //process approval agency as authorizing institution
            if (academicProgramEntity.AuthorizingInstitute.Any())
            {
                var institutionId = academicProgramEntity.AuthorizingInstitute.First();
                if (!string.IsNullOrEmpty(institutionId))
                {
                    bool isInstitution = false;
                    string institutionGuid = string.Empty;
                    if (institutions != null)
                    {
                        isInstitution = institutions.Contains(institutionId);
                    }

                    personGuidCollection.TryGetValue(institutionId, out institutionGuid);
                    if (string.IsNullOrEmpty(institutionGuid))
                    {
                        throw new KeyNotFoundException(string.Concat("AuthorizingInstitute, ", "Unable to locate guid for : '", institutionId, "'"));
                    }

                    academicProgramDto.Authorizing = (isInstitution)
                        ? new Dtos.AcademicProgramAuthorizing() { AuthorizingInstitution = new Dtos.GuidObject2(institutionGuid) }
                        : new Dtos.AcademicProgramAuthorizing() { Organization = new Dtos.GuidObject2(institutionGuid) };
                }
            }

            academicProgramDto.Sites = await ConvertSiteCodeToGuidAsync(academicProgramEntity.Location, bypassCache);
            academicProgramDto.AcademicLevel = await ConvertAcadLevelCodeToGuid(academicProgramEntity.AcadLevelCode, bypassCache);
            academicProgramDto.Credentials = await ConvertCredentialCodeToGuid(academicProgramEntity.DegreeCode, academicProgramEntity.CertificateCodes, bypassCache);
            academicProgramDto.Disciplines = await ConvertDisciplineCodeToGuid(academicProgramEntity.MajorCodes, academicProgramEntity.MinorCodes, academicProgramEntity.SpecializationCodes, bypassCache);
            academicProgramDto.StartDate = academicProgramEntity.StartDate;
            academicProgramDto.EndDate = academicProgramEntity.EndDate;
            academicProgramDto.ProgramOwners = await ConvertDeptCodeToGuid(academicProgramEntity.DeptartmentCodes, bypassCache);

            return academicProgramDto;
        }

        private async Task<Colleague.Dtos.AcademicProgram4> ConvertAcademicProgramEntityToDto4Async(Colleague.Domain.Student.Entities.AcademicProgram academicProgramEntity,
             Dictionary<string, string> personGuidCollection, IEnumerable<string> institutions, bool bypassCache = false)

        {
            var academicProgramDto = new Colleague.Dtos.AcademicProgram4();
            academicProgramDto.Id = academicProgramEntity.Guid;
            academicProgramDto.Code = academicProgramEntity.Code;
            academicProgramDto.Title = academicProgramEntity.Description;
            academicProgramDto.Description = academicProgramEntity.LongDescription;

            //process approval agency as authorizing institution
            if (academicProgramEntity.AuthorizingInstitute.Any())
            {
                var institutionId = academicProgramEntity.AuthorizingInstitute.First();
                if (!string.IsNullOrEmpty(institutionId))
                {
                    bool isInstitution = false;
                    string institutionGuid = string.Empty;
                    if (institutions != null)
                    {
                        isInstitution = institutions.Contains(institutionId);
                    }

                    personGuidCollection.TryGetValue(institutionId, out institutionGuid);
                    if (string.IsNullOrEmpty(institutionGuid))
                    {
                        throw new KeyNotFoundException(string.Concat("AuthorizingInstitute, ", "Unable to locate guid for : '", institutionId, "'"));
                    }

                    academicProgramDto.Authorizing = (isInstitution)
                        ? new Dtos.AcademicProgramAuthorizing() { AuthorizingInstitution = new Dtos.GuidObject2(institutionGuid) }
                        : new Dtos.AcademicProgramAuthorizing() { Organization = new Dtos.GuidObject2(institutionGuid) };
                }
            }

            academicProgramDto.Sites = await ConvertSiteCodeToGuidAsync(academicProgramEntity.Location, bypassCache);
            academicProgramDto.AcademicLevel = await ConvertAcadLevelCodeToGuid(academicProgramEntity.AcadLevelCode, bypassCache);
            academicProgramDto.Credentials = await ConvertCredentialCodeToGuid(academicProgramEntity.DegreeCode, academicProgramEntity.CertificateCodes, bypassCache);
            academicProgramDto.Disciplines = await ConvertDisciplineCodeToGuid(academicProgramEntity.MajorCodes, academicProgramEntity.MinorCodes, academicProgramEntity.SpecializationCodes, bypassCache);
            academicProgramDto.StartDate = academicProgramEntity.StartDate;
            academicProgramDto.EndDate = academicProgramEntity.EndDate;
            academicProgramDto.ProgramOwners = await ConvertDeptCodeToGuid(academicProgramEntity.DeptartmentCodes, bypassCache);

            return academicProgramDto;
        }

        private async Task<List<Ellucian.Colleague.Dtos.GuidObject2>> ConvertDeptCodeToGuid(List<string> departmentCodes, bool bypassCache = false)
        {
            List<Ellucian.Colleague.Dtos.GuidObject2> guidObjects = null;

            if ((departmentCodes != null) && (departmentCodes.Any()))
            {
                var departmentEntities = await this.GetDepartmentsAsync(bypassCache); // _referenceDataRepository.GetDepartmentsAsync(bypassCache);

                foreach (var dept in departmentCodes)
                {
                    if (departmentEntities != null && departmentEntities.Any())
                    {
                        var department = departmentEntities.FirstOrDefault(a => a.Code == dept);

                        if (department != null)
                        {
                            if (guidObjects == null)
                            {
                                guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                            }
                            guidObjects.Add(new Dtos.GuidObject2(department.Guid));
                        }
                    }
                }
            }
            return guidObjects;
        }

        private async Task<List<Ellucian.Colleague.Dtos.GuidObject2>> ConvertSiteCodeToGuidAsync(List<string> locationCodes, bool bypassCache = false)
        {
            List<Ellucian.Colleague.Dtos.GuidObject2> guidObjects = null;

            if ((locationCodes != null) && (locationCodes.Any()))
            {
                var locations = await this.GetLocationsAsync(bypassCache); 
                if ((locations != null) && (locations.Any()))
                {
                    foreach (var loc in locationCodes)
                    {
                        var location = locations.FirstOrDefault(a => a.Code == loc);
                        if (location != null)
                        {
                            if (guidObjects == null)
                            {
                                guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                            }

                            guidObjects.Add(new Dtos.GuidObject2(location.Guid));
                        }
                    }
                }
            }
            return guidObjects;
        }

        private async Task<Ellucian.Colleague.Dtos.GuidObject2> ConvertAcadLevelCodeToGuid(string acadLevelCode, bool bypassCache = false)
        {
            Ellucian.Colleague.Dtos.GuidObject2 guidObject = null;

            if (!string.IsNullOrEmpty(acadLevelCode))
            {
                var academicLevels = await this.GetAcademicLevelsAsync(bypassCache); 
                if (academicLevels != null && academicLevels.Any())
                {
                    var acadLevel = academicLevels.FirstOrDefault(a => a.Code == acadLevelCode);
                    if (acadLevel != null)
                    {
                        guidObject = new Ellucian.Colleague.Dtos.GuidObject2(acadLevel.Guid);
                    }
                }
            }
            return guidObject;
        }

        private async Task<List<Ellucian.Colleague.Dtos.GuidObject2>> ConvertCredentialCodeToGuid(string degreeCode, List<string> certificateCodes, bool bypassCache = false)
        {
            List<Ellucian.Colleague.Dtos.GuidObject2> guidObjects = null;

            if (!string.IsNullOrEmpty(degreeCode))
            {
                var otherDegreeEntities = await GetOtherDegreesAsync(bypassCache); 
                if (otherDegreeEntities != null && otherDegreeEntities.Any())
                {
                    var credential = otherDegreeEntities.FirstOrDefault(a => a.Code == degreeCode);

                    if (credential != null)
                    {
                        if (guidObjects == null)
                        {
                            guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                        }
                        guidObjects.Add(new Dtos.GuidObject2(credential.Guid));
                    }
                }
            }

            if ((certificateCodes != null) && (certificateCodes.Any()))
            {
                var certificateEntities = await GetOtherCcdAsync(bypassCache); // _referenceDataRepository.GetOtherCcds(bypassCache);

                foreach (var ccd in certificateCodes)
                {
                    if (certificateEntities != null && certificateEntities.Any())
                    {
                        var credential = certificateEntities.FirstOrDefault(a => a.Code == ccd);

                        if (credential != null)
                        {
                            if (guidObjects == null)
                            {
                                guidObjects = new List<Ellucian.Colleague.Dtos.GuidObject2>();
                            }
                            guidObjects.Add(new Dtos.GuidObject2(credential.Guid));
                        }
                    }
                }
            }

            return guidObjects;
        }

        private async Task<List<Ellucian.Colleague.Dtos.AcademicProgramDisciplines>> ConvertDisciplineCodeToGuid(List<string> majorCodes, List<string> minorCodes, List<string> specializationCodes, bool bypassCache = false)
        {
            List<Ellucian.Colleague.Dtos.AcademicProgramDisciplines> disciplineObjects = null;

            if ((majorCodes != null) && (majorCodes.Any()))
            {
                foreach (var major in majorCodes)
                {
                    var discipline = (await GetOtherMajorsAsync(bypassCache)).FirstOrDefault(a => a.Code == major);
                    if (discipline != null)
                    {
                        if (disciplineObjects == null)
                        {
                            disciplineObjects = new List<Ellucian.Colleague.Dtos.AcademicProgramDisciplines>();
                        }
                        disciplineObjects.Add(new Dtos.AcademicProgramDisciplines() {Discipline = new Dtos.GuidObject2(discipline.Guid)});
                    }
                }
            }

            if ((minorCodes != null) && (minorCodes.Any()))
            {
                foreach (var minor in minorCodes)
                {
                    var discipline = (await GetOtherMinorsAsync(bypassCache)).FirstOrDefault(a => a.Code == minor);
                    if (discipline != null)
                    {
                        if (disciplineObjects == null)
                        {
                            disciplineObjects = new List<Ellucian.Colleague.Dtos.AcademicProgramDisciplines>();
                        }
                        disciplineObjects.Add(new Dtos.AcademicProgramDisciplines() {Discipline = new Dtos.GuidObject2(discipline.Guid)});
                    }
                }
            }

            if ((specializationCodes != null) && (specializationCodes.Any()))
            {
                foreach (var specializationCode in specializationCodes)
                {
                    var discipline = (await GetOtherSpecialsAsync(bypassCache)).FirstOrDefault(a => a.Code == specializationCode);
                    if (discipline != null)
                    {
                        if (disciplineObjects == null)
                        {
                            disciplineObjects = new List<Ellucian.Colleague.Dtos.AcademicProgramDisciplines>();
                        }
                        disciplineObjects.Add(new Dtos.AcademicProgramDisciplines() {Discipline = new Dtos.GuidObject2(discipline.Guid)});
                    }
                }
            }

            return disciplineObjects;
        }
        
        #endregion
    }
}