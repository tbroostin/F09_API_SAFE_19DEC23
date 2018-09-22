﻿// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using slf4net;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Local Government Course Classification Codes data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class LocalCourseClassificationsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _localCourseClassificationRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the LocalCourseClassificationsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="localCourseClassificationRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public LocalCourseClassificationsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository localCourseClassificationRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _localCourseClassificationRepository = localCourseClassificationRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets information for all Local Government Course Classifications
        /// </summary>
        /// <returns>List of <see cref="LocalCourseClassification"/></returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.LocalCourseClassification>> GetAsync()
        {
            var localCourseClassificationDtoCollection = new List<Ellucian.Colleague.Dtos.Student.LocalCourseClassification>();
            var localCourseClassificationCollection = await _localCourseClassificationRepository.GetLocalCourseClassificationsAsync();
            // Get the right adapter for the type mapping
            var localCourseClassificationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.LocalCourseClassification, Ellucian.Colleague.Dtos.Student.LocalCourseClassification>();
            // Map the grade entity to the grade DTO
            foreach (var localCourseClassification in localCourseClassificationCollection)
            {
                localCourseClassificationDtoCollection.Add(localCourseClassificationDtoAdapter.MapToType(localCourseClassification));
            }

            return localCourseClassificationDtoCollection;

        }
    }
}
