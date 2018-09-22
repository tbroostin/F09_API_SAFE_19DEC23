// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
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

        /// <summary>
        /// Initializes a new instance of the MajorsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="transcriptGroupingService">Service of type <see cref="ITranscriptGroupingService">ITranscriptGroupingService</see></param>
        public TranscriptGroupingsController(IAdapterRegistry adapterRegistry, ITranscriptGroupingService transcriptGroupingService)
        {
            _adapterRegistry = adapterRegistry;
            _transcriptGroupingService = transcriptGroupingService;
        }

        /// <summary>
        /// Return the set of transcript groupings that are user-selectable
        /// </summary>
        /// <returns>A set of transcript groupings</returns>
        public async Task<IEnumerable<TranscriptGrouping>> GetSelectableTranscriptGroupingsAsync()
        {
            var transcriptGroupings = await _transcriptGroupingService.GetSelectableTranscriptGroupingsAsync();
            return transcriptGroupings;
        }
    }
}

