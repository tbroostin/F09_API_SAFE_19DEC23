// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
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
    /// Provides access to Noncourse status data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class NoncourseStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _studentReferenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the NoncourseStatusesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="studentReferenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public NoncourseStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository studentReferenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _studentReferenceDataRepository = studentReferenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Noncourse Statuses.
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Domain.Student.Entities.NoncourseStatus">Noncourse Statuses</see></returns>
        /// [CacheControlFilter(Public = true, MaxAgeHours = 1, Revalidate = true)]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.NoncourseStatus>> GetAsync()
        {
            var noncourseStatusDtoCollection = new List<Ellucian.Colleague.Dtos.Student.NoncourseStatus>();
            var noncourseStatusCollection = await _studentReferenceDataRepository.GetNoncourseStatusesAsync();
            // Get the right adapter for the type mapping
            var noncourseStatusDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.NoncourseStatus, Ellucian.Colleague.Dtos.Student.NoncourseStatus>();
            // Map the NoncourseStatus entity to the NoncourseStatus DTO
            foreach (var status in noncourseStatusCollection)
            {
                noncourseStatusDtoCollection.Add(noncourseStatusDtoAdapter.MapToType(status));
            }

            return noncourseStatusDtoCollection;

        }
    }
}
