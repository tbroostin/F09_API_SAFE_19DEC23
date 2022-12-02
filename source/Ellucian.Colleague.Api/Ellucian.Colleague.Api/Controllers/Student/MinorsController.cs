// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using Ellucian.Colleague.Domain.Base.Repositories;
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
    /// Provides access to Minors data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class MinorsController : BaseCompressedApiController
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
        public MinorsController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve all Minors.
        /// </summary>
        /// <returns>All <see cref="Minor">Minor</see> codes and descriptions.</returns>
        public async Task<IEnumerable<Minor>> GetAsync()
        {
            try
            {
                var minorCollection = await _referenceDataRepository.GetMinorsAsync();

                // Get the right adapter for the type mapping
                var minorDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Minor, Minor>();

                // Map the degree plan entity to the degree plan DTO
                var minorDtoCollection = new List<Minor>();
                foreach (var minor in minorCollection)
                {
                    minorDtoCollection.Add(minorDtoAdapter.MapToType(minor));
                }

                return minorDtoCollection;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                _logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (System.Exception e)
            {
                _logger.Error(e, "Unable to retrieve minor information");
                throw CreateHttpResponseException("Unable to retrieve minor information.", HttpStatusCode.BadRequest);
            }
        }
    }
}

