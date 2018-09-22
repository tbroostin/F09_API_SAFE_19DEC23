// Copyright 2013 Ellucian Company L.P. and its affiliates.
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to FederalCourseClassification data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class FederalCourseClassificationsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// FederalCourseClassificationsController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public FederalCourseClassificationsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all FederalCourseClassification codes and their associated descriptions
        /// </summary>
        /// <returns>List of <see cref="FederalCourseClassification">FederalCourseClassifications</see></returns>
        public async Task<IEnumerable<FederalCourseClassification>> GetAsync()
        {
            var FederalCourseClassificationCollection = await _referenceDataRepository.GetFederalCourseClassificationsAsync();

            // Get the right adapter for the type mapping
            var FederalCourseClassificationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.FederalCourseClassification, FederalCourseClassification>();

            // Map the FederalCourseClassification entity to the program DTO
            var FederalCourseClassificationDtoCollection = new List<FederalCourseClassification>();
            foreach (var FederalCourseClassification in FederalCourseClassificationCollection)
            {
                FederalCourseClassificationDtoCollection.Add(FederalCourseClassificationDtoAdapter.MapToType(FederalCourseClassification));
            }

            return FederalCourseClassificationDtoCollection;
        }
    }
}