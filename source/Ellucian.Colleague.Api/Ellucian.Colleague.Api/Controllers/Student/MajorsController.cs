// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using System.Web.Http;
using System.ComponentModel;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;
using slf4net;
using System.Net;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Majors data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class MajorsController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";

        /// <summary>
        /// Initializes a new instance of the MajorsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public MajorsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Majors.
        /// </summary>
        /// <returns>All <see cref="Major">Major</see> codes and descriptions.</returns>
        public async Task<IEnumerable<Major>> GetAsync()
        {
            try
            {
                var majorCollection = await _referenceDataRepository.GetMajorsAsync();

                // Get the right adapter for the type mapping
                var majorDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Major, Major>();

                // Map the degree plan entity to the degree plan DTO
                var majorDtoCollection = new List<Major>();
                foreach (var major in majorCollection)
                {
                    majorDtoCollection.Add(majorDtoAdapter.MapToType(major));
                }

                return majorDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (System.Exception e)
            {
                _logger.Error(e, "Unable to retrieve major information");
                throw CreateHttpResponseException("Unable to retrieve major information.", HttpStatusCode.BadRequest);
            }
        }
    }
}

