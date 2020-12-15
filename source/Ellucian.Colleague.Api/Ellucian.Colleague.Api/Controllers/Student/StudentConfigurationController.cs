﻿// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to get student parameters and settings.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class StudentConfigurationController : BaseCompressedApiController
    {
        private readonly IStudentConfigurationService _configurationService;
        private readonly IStudentConfigurationRepository _configurationRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// StudentConfigurationController class constructor
        /// </summary>
        /// <param name="configurationRepository">Repository of type <see cref="IStudentConfigurationRepository">IStudentConfigurationRepository</see></param>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="configurationService">Service of type <see cref="IStudentConfigurationService">IStudentConfigurationService</see></param>
        public StudentConfigurationController(IStudentConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry, ILogger logger, IStudentConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _configurationRepository = configurationRepository;
            _adapterRegistry = adapterRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the configuration information needed to render a new graduation application asynchronously.
        /// </summary>
        /// <returns>The <see cref="GraduationConfiguration">Graduation Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. NotFound if the required setup is not complete or available.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<GraduationConfiguration> GetGraduationConfigurationAsync()
        {
            GraduationConfiguration configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration configuration = await _configurationRepository.GetGraduationConfigurationAsync();
                var graduationConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.GraduationConfiguration, Ellucian.Colleague.Dtos.Student.GraduationConfiguration>();
                configurationDto = graduationConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed to render a new transcript request or enrollment verification in self-service asynchronously.
        /// </summary>
        /// <returns>The <see cref="StudentRequestConfiguration">StudentRequestConfiguration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<StudentRequestConfiguration> GetStudentRequestConfigurationAsync()
        {
            StudentRequestConfiguration configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.StudentRequestConfiguration configuration = await _configurationRepository.GetStudentRequestConfigurationAsync();
                var studentRequestConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentRequestConfiguration, Ellucian.Colleague.Dtos.Student.StudentRequestConfiguration>();
                configurationDto = studentRequestConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for faculty grading asynchronously.
        /// </summary>
        /// <returns>The <see cref="FacultyGradingConfiguration">Faculty Grading Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. NotFound if the required setup is not complete or available.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<FacultyGradingConfiguration> GetFacultyGradingConfigurationAsync()
        {
            FacultyGradingConfiguration configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration configuration = await _configurationRepository.GetFacultyGradingConfigurationAsync();
                var configurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration, Ellucian.Colleague.Dtos.Student.FacultyGradingConfiguration>();
                configurationDto = configurationDtoAdapter.MapToType(configuration);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the student profile configuration information needed for student profile asynchronously.
        /// </summary>
        /// <returns>The <see cref="StudentProfileConfiguration">Student Profile Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. NotFound if the required setup is not complete or available.</exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<StudentProfileConfiguration> GetStudentProfileConfigurationAsync()
        {
            try
            {
                return await _configurationService.GetStudentProfileConfigurationAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException("Error retrieving Student Profile Configuration for faculty", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the configuration information needed for course catalog searches asynchronously.
        /// </summary>
        /// <returns>The <see cref="CourseCatalogConfiguration">Course Catalog Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        [Obsolete("Obsolete as of API version 1.26, use version 2 of this API")]
        public async Task<CourseCatalogConfiguration> GetCourseCatalogConfigurationAsync()
        {
            CourseCatalogConfiguration configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration configuration = await _configurationRepository.GetCourseCatalogConfigurationAsync();
                var catalogConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration>();
                configurationDto = catalogConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for course catalog searches asynchronously.
        /// </summary>
        /// <returns>The <see cref="CourseCatalogConfiguration2">Course Catalog Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        [Obsolete("Obsolete as of API version 1.29, use version 3 of this API")]
        public async Task<CourseCatalogConfiguration2> GetCourseCatalogConfiguration2Async()
        {
            CourseCatalogConfiguration2 configurationDto = null;
            try
            {
                Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration configuration = await _configurationRepository.GetCourseCatalogConfiguration2Async();
                var catalogConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.CourseCatalogConfiguration, Ellucian.Colleague.Dtos.Student.CourseCatalogConfiguration2>();
                configurationDto = catalogConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for registration processing asynchronously.
        /// </summary>
        /// <returns>The <see cref="RegistrationConfiguration">Registration Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<RegistrationConfiguration> GetRegistrationConfigurationAsync()
        {
            RegistrationConfiguration configurationDto = null;
            try
            {
                Domain.Student.Entities.RegistrationConfiguration configuration = await _configurationRepository.GetRegistrationConfigurationAsync();
                var catalogConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationConfiguration, RegistrationConfiguration>();
                configurationDto = catalogConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException("Could not retrieve registration configuration data.", HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for Colleague Self-Service instant enrollment
        /// </summary>
        /// <returns>The <see cref="InstantEnrollmentConfiguration"/></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<InstantEnrollmentConfiguration> GetInstantEnrollmentConfigurationAsync()
        {
            InstantEnrollmentConfiguration configurationDto = null;
            try
            {
                Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration configuration = await _configurationRepository.GetInstantEnrollmentConfigurationAsync();
                var instantEnrollmentConfigurationDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentConfiguration, InstantEnrollmentConfiguration>();
                configurationDto = instantEnrollmentConfigurationDtoAdapter.MapToType(configuration);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException("Could not retrieve Colleague Self-Service instant enrollment configuration data.", HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }

        /// <summary>
        /// Retrieves the configuration information needed for course catalog searches asynchronously.
        /// </summary>
        /// <returns>The <see cref="CourseCatalogConfiguration3">Course Catalog Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
        public async Task<CourseCatalogConfiguration3> GetCourseCatalogConfiguration3Async()
        {
            try
            {
                return await _configurationService.GetCourseCatalogConfiguration3Async();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to get the course catalog configuration information.");
                throw CreateHttpResponseException("Unable to get the course catalog configuration information.", HttpStatusCode.BadRequest);
            }
        }
    }
}