﻿// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentConfigurationService : IStudentConfigurationService
    {
        private readonly IStudentConfigurationRepository _configurationRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        public StudentConfigurationService(IStudentConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry, ILogger logger)
        {
            _configurationRepository = configurationRepository;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Get Student Profile configurations
        /// </summary>
        /// <returns>StudentProfileConfiguration object</returns>
        public async Task<Dtos.Student.StudentProfileConfiguration> GetStudentProfileConfigurationAsync()
        {
            Dtos.Student.StudentProfileConfiguration studenProfileConfigurationDto = new Dtos.Student.StudentProfileConfiguration();
            Ellucian.Colleague.Domain.Student.Entities.StudentProfileConfiguration configuration = await _configurationRepository.GetStudentProfileConfigurationAsync();
            var configurationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentProfileConfiguration, Dtos.Student.StudentProfileConfiguration>();
            studenProfileConfigurationDto = configurationDtoAdapter.MapToType(configuration);
            return studenProfileConfigurationDto;
        }

        /// <summary>
        /// Gets Course Catalog Configurations
        /// </summary>
        /// <returns>CourseCatalogConfiguration3 object</returns>
        public async Task<Dtos.Student.CourseCatalogConfiguration3> GetCourseCatalogConfiguration3Async()
        {
            Dtos.Student.CourseCatalogConfiguration3 configurationDto = new Dtos.Student.CourseCatalogConfiguration3();
            Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration configuration = await _configurationRepository.GetCourseCatalogConfiguration3Async();
            var catalogConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration3>();
            configurationDto = catalogConfigurationDtoAdapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Gets Course Catalog Configurations
        /// </summary>
        /// <returns>CourseCatalogConfiguration3 object</returns>
        public async Task<Dtos.Student.CourseCatalogConfiguration4> GetCourseCatalogConfiguration4Async()
        {
            Dtos.Student.CourseCatalogConfiguration4 configurationDto = new Dtos.Student.CourseCatalogConfiguration4();
            Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration configuration = await _configurationRepository.GetCourseCatalogConfiguration4Async();
            var catalogConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration4>();
            configurationDto = catalogConfigurationDtoAdapter.MapToType(configuration);
            return configurationDto;
        }

        /// <summary>
        /// Get Academic Record configuration information
        /// </summary>
        /// <returns>The AcademicRecordConfiguration object</returns>
        public async Task<Dtos.Student.AcademicRecordConfiguration> GetAcademicRecordConfigurationAsync()
        {
            var configurationDto = new Dtos.Student.AcademicRecordConfiguration();
            var configuration = await _configurationRepository.GetAcademicRecordConfigurationAsync();
            var academicRecordConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.AcademicRecordConfiguration, Dtos.Student.AcademicRecordConfiguration>();
            configurationDto = academicRecordConfigurationDtoAdapter.MapToType(configuration);
            return configurationDto;
        }
    }
}
