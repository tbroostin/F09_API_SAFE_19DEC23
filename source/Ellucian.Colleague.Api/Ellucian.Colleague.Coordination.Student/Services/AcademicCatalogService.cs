// Copyright 2015-2018  Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Student;
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
    public class AcademicCatalogService : BaseCoordinationService, IAcademicCatalogService
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private ILogger _logger;
        private string _defaultHostGuid;

        public AcademicCatalogService(IAdapterRegistry adapterRegistry, ICatalogRepository catalogRepository, IStudentReferenceDataRepository studentReferenceDataRepository, IPersonRepository personRepository,
            ICurrentUserFactory currentUserFactory, IConfigurationRepository configurationRepository, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _catalogRepository = catalogRepository;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _personRepository = personRepository;
            _configurationRepository = configurationRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all Academic Catalogs
        /// </summary>
        /// <returns>Collection of AcademicCatalog2 DTO objects</returns>
        public async Task<IEnumerable<AcademicCatalog2>> GetAcademicCatalogs2Async(bool bypassCache = false)
        {
            ICollection<Domain.Student.Entities.Requirements.Catalog> catalogCollection = null;
            IEnumerable<Domain.Student.Entities.AcademicProgram> programs = null;
            string defaultHost = string.Empty;
            try
            {
                catalogCollection = await _catalogRepository.GetAsync(bypassCache);
                programs = await _studentReferenceDataRepository.GetAcademicProgramsAsync(false);
                defaultHost = await GetDefaultHostGuidAsync();
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Format("Errors getting catalog, program or default institution: {0}", ex.Message), "Bad.Data");
            }

            // Map the Catalog entity to the AcademicCatalog DTO
            var academicCatalogDtoCollection = new List<AcademicCatalog2>();
            foreach (var catalog in catalogCollection)
            {
                var catalogDto = ConvertCatalogEntitytoAcademicCatalog2DtoAsync(catalog, programs, defaultHost);
                if (catalogDto != null)
                {
                    academicCatalogDtoCollection.Add(catalogDto);
                }
            }
            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return academicCatalogDtoCollection;
        }

        /// <summary>
        /// Gets all Academic Catalogs
        /// </summary>
        /// <param name="bypassCache">bypassCache</param>
        /// <returns>Collection of Catalog DTO objects</returns>
        public async Task<IEnumerable<Catalog>> GetAllAcademicCatalogsAsync(bool bypassCache = false)
        {
            var catalogCollection = await _catalogRepository.GetAsync(bypassCache);
            List<Catalog> catalogsList = new List<Catalog>();
            foreach (var item in catalogCollection)
            {
                Catalog cat = new Catalog()
                {
                    CatalogYear = item.Code,
                    HideInWhatIf = item.HideInWhatIf,
                    CatalogStartDate = item.StartDate.ToShortDateString()
                };
                catalogsList.Add(cat);
            }
            return catalogsList;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an Academic Catalog from its GUID
        /// </summary>
        /// <returns>AcademicCatalog2 DTO object</returns>
        public async Task<AcademicCatalog2> GetAcademicCatalogByGuid2Async(string guid)
        {
            ICollection<Domain.Student.Entities.Requirements.Catalog> catalogCollections = null;
            IEnumerable<Domain.Student.Entities.AcademicProgram> programs = null;
            string defaultHost = string.Empty;
            try
            {
                catalogCollections = await _catalogRepository.GetAsync(true);
                programs = await _studentReferenceDataRepository.GetAcademicProgramsAsync(false);
                defaultHost = await GetDefaultHostGuidAsync();
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(string.Format("Errors getting catalog, program or default institution: {0}", ex.Message), "Bad.Data");
            }

            if (catalogCollections == null)
            {
                throw new KeyNotFoundException(string.Format("No academic-catalogs resource was found for GUID '{0}'", guid));
            }

            var catalogEntity = catalogCollections.FirstOrDefault(cc => cc.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase));
            if (catalogEntity == null)
            {
                throw new KeyNotFoundException(string.Format("No academic-catalogs resource was found for GUID '{0}'", guid));
            }

            var catalogDto = ConvertCatalogEntitytoAcademicCatalog2DtoAsync(catalogEntity, programs, defaultHost);

            if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
            {
                throw IntegrationApiException;
            }

            return catalogDto;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Catalog domain entity to its corresponding Academic Catalog DTO
        /// </summary>
        /// <param name="source">Academic Catalog domain entity</param>
        /// <returns>AcademicCatalog DTO</returns>
        private Dtos.AcademicCatalog2 ConvertCatalogEntitytoAcademicCatalog2DtoAsync(Domain.Student.Entities.Requirements.Catalog source, IEnumerable<Domain.Student.Entities.AcademicProgram> programs, string defaultHost)
        {
            var academicCatalog = new AcademicCatalog2()
            {
                Id = source.Guid,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                Code = source.Code,
                Title = source.Description
            };
            // v6.1.0, status is optional therefore, no longer return a derrived status that has no real meaning.
            // academicCatalog.status = source.IsActive ? LifeCycleStatus.Active : LifeCycleStatus.Inactive;

            if (source.AcadPrograms != null && programs != null)
            {
                var acadProgramCollection = new List<GuidObject2>();
                foreach (var acadProgram in source.AcadPrograms)
                {
                    try
                    {
                        var program = programs.FirstOrDefault(x => x.Code == acadProgram);
                        if (program != null)
                        {
                            acadProgramCollection.Add(new GuidObject2(program.Guid));
                        }
                        else
                        {
                            IntegrationApiExceptionAddError(string.Format("The academic program code '{0}' is referenced in the catalog but not properly defined.", acadProgram), "Bad.Data", source.Guid, source.Code);
                        }
                    }
                    catch
                    {
                        IntegrationApiExceptionAddError(string.Format("The academic program code '{0}' is referenced in the catalog but not properly defined.", acadProgram), "Bad.Data", source.Guid, source.Code);
                    }
                }
                academicCatalog.AcademicPrograms = acadProgramCollection;
            }
            if (!string.IsNullOrEmpty(defaultHost))
                academicCatalog.Institution = new GuidObject2(defaultHost);

            return academicCatalog;
        }

        private async Task<string> GetDefaultHostGuidAsync()
        {
            if (string.IsNullOrEmpty(_defaultHostGuid))
            {
                var hostGuid = string.Empty;
                var defaultsConfiguration = _configurationRepository.GetDefaultsConfiguration();
                if (defaultsConfiguration != null)
                {
                    var hostID = defaultsConfiguration.HostInstitutionCodeId;

                    hostGuid = await _personRepository.GetPersonGuidFromIdAsync(hostID);
                }
                if (string.IsNullOrEmpty(hostGuid))
                    throw new KeyNotFoundException(string.Concat("Unable to determine default institution from PID2."));
                _defaultHostGuid = hostGuid;
            }

            return _defaultHostGuid;
        }
    }
}