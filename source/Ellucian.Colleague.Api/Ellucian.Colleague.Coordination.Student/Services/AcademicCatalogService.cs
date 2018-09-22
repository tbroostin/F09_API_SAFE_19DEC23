// Copyright 2015-2018  Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
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
            var catalogCollection = await _catalogRepository.GetAsync(bypassCache);

            // Map the Catalog entity to the AcademicCatalog DTO
            var academicCatalogDtoCollection = new List<AcademicCatalog2>();
            foreach (var catalog in catalogCollection)
            {
                academicCatalogDtoCollection.Add(await ConvertCatalogEntitytoAcademicCatalog2DtoAsync(catalog));
            }

            return academicCatalogDtoCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get an Academic Catalog from its GUID
        /// </summary>
        /// <returns>AcademicCatalog2 DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AcademicCatalog2> GetAcademicCatalogByGuid2Async(string guid)
        {
            try
            {
                var catalogCollection = await _catalogRepository.GetAsync(false);
                return await ConvertCatalogEntitytoAcademicCatalog2DtoAsync(catalogCollection.Where(ac => ac.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Academic Catalog not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Converts a Catalog domain entity to its corresponding Academic Catalog DTO
        /// </summary>
        /// <param name="source">Academic Catalog domain entity</param>
        /// <returns>AcademicCatalog DTO</returns>
        private async Task<Dtos.AcademicCatalog2> ConvertCatalogEntitytoAcademicCatalog2DtoAsync(Domain.Student.Entities.Requirements.Catalog source)
        {
            var academicCatalog = new Dtos.AcademicCatalog2();
            academicCatalog.Id = source.Guid;
            academicCatalog.StartDate = source.StartDate;
            academicCatalog.EndDate = source.EndDate;
            academicCatalog.Code = source.Code;
            academicCatalog.Title = source.Description;
            academicCatalog.status = source.IsActive ? LifeCycleStatus.Active : LifeCycleStatus.Inactive;

            if (source.AcadPrograms != null)
            {
                var acadProgramCollection = new List<GuidObject2>();
                foreach (var acadProgram in source.AcadPrograms)
                {
                    var program = (await _studentReferenceDataRepository.GetAcademicProgramsAsync(false)).FirstOrDefault(x => x.Code == acadProgram);
                    if (program != null)
                    {
                        acadProgramCollection.Add(new GuidObject2(program.Guid));
                    }
                }
                academicCatalog.AcademicPrograms = acadProgramCollection;
            }
            academicCatalog.Institution = new GuidObject2(await GetDefaultHostGuidAsync());
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