// Copyright 2014-2022 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Coordination.Student.Services;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using System.Net;
using slf4net;
using System;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to transcript grouping data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class TranscriptGroupingsController : BaseCompressedApiController
    {
        private readonly ITranscriptGroupingService _transcriptGroupingService;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the MajorsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="transcriptGroupingService">Service of type <see cref="ITranscriptGroupingService">ITranscriptGroupingService</see></param>
        /// <param name="logger">Interface to Logger</param>
        public TranscriptGroupingsController(IAdapterRegistry adapterRegistry, ITranscriptGroupingService transcriptGroupingService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _transcriptGroupingService = transcriptGroupingService;
            _logger = logger;
        }

        /// <summary>
        /// Return the set of transcript groupings that are user-selectable
        /// </summary>
        /// <returns>A set of transcript groupings</returns>
        public async Task<IEnumerable<TranscriptGrouping>> GetSelectableTranscriptGroupingsAsync()
        {
            try
            {
                var transcriptGroupings = await _transcriptGroupingService.GetSelectableTranscriptGroupingsAsync();
                return transcriptGroupings;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving transcript groupings";
                _logger.Error(tex, message);
                throw CreateHttpResponseException(message, HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }

        }
    }
}

