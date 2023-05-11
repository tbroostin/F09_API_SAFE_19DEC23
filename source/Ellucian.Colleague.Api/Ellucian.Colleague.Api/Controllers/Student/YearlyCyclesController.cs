// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
{
    /// <summary>
    /// Provides access to the yearly cycle data for courses.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class YearlyCyclesController : BaseCompressedApiController
    {
        private readonly IStudentReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the YearlyCyclesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IStudentReferenceDataRepository">IStudentReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">Logger</see></param>
        public YearlyCyclesController(IAdapterRegistry adapterRegistry, IStudentReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.referenceDataRepository = referenceDataRepository;
            this.adapterRegistry = adapterRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves all Yearly Cycles.
        /// </summary>
        /// <returns>All <see cref="YearlyCycle">Yearly Cycle</see> codes and descriptions.</returns>
        /// <accessComments>Any authenticated user can retrieve yearly cycles.</accessComments>
        public async Task<IEnumerable<YearlyCycle>> GetAsync()
        {
            try
            {
                var yearlyCycleCollection = await referenceDataRepository.GetYearlyCyclesAsync();

                // Get the right adapter for the type mapping
                var yearlyCycleDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.YearlyCycle, YearlyCycle>();

                // Map the YearlyCycle entity to the program DTO
                var yearlyCycleDtoCollection = new List<YearlyCycle>();
                if (yearlyCycleCollection != null && yearlyCycleCollection.Any())
                {
                    foreach (var yc in yearlyCycleCollection)
                    {
                        yearlyCycleDtoCollection.Add(yearlyCycleDtoAdapter.MapToType(yc));
                    }
                }
                return yearlyCycleDtoCollection;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving Yearly Cycle information";
                logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (System.Exception e)
            {
                this.logger.Error(e, "Unable to retrieve the Yearly Cycle information");
                throw CreateHttpResponseException("Unable to retrieve data");
            }
        }
    }
}