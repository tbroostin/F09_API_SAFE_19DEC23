// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
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
        
        private readonly IStudentConfigurationRepository _configurationRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger; 

        /// <summary>
        /// StudentConfigurationController class constructor
        /// </summary>
        /// <param name="configurationRepository">Repository of type <see cref="IStudentConfigurationRepository">IStudentConfigurationRepository</see></param>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public StudentConfigurationController(IStudentConfigurationRepository configurationRepository, IAdapterRegistry adapterRegistry, ILogger logger)
        {
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
        /// Retrieves the configuration information needed for course catalog searches asynchronously.
        /// </summary>
        /// <returns>The <see cref="CourseCatalogConfiguration">Course Catalog Configuration</see></returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="System.Net.Http.HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>. </exception>
        /// <accessComments>Any authenticated user can get this resource.</accessComments>
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
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
            return configurationDto;
        }
    }
}