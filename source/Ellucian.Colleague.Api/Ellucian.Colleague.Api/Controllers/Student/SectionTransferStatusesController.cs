// Copyright 2014 Ellucian Company L.P. and its affiliates.
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
    /// Provides access to section transfer statuses
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class SectionTransferStatusesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the TestsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public SectionTransferStatusesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets information for all section transfer statuses
        /// </summary>
        /// <returns>All <see cref="Ellucian.Colleague.Domain.Student.Entities.SectionTransferStatus">section transfer statuses</see></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Student.SectionTransferStatus>> GetAsync()
        {
            var transferStatusDtoCollection = new List<Ellucian.Colleague.Dtos.Student.SectionTransferStatus>();
            var transferStatusCollection =await _referenceDataRepository.GetSectionTransferStatusesAsync();
            // Get the right adapter for the type mapping
            var testDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionTransferStatus, Ellucian.Colleague.Dtos.Student.SectionTransferStatus>();
            // Map the Test entity to the grade DTO
            foreach (var test in transferStatusCollection)
            {
                transferStatusDtoCollection.Add(testDtoAdapter.MapToType(test));
            }

            return transferStatusDtoCollection;
        }
    }
}
