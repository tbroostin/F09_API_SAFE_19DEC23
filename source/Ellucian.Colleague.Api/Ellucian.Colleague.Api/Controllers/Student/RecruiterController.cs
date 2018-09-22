// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Controllers;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to update Application status.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Student)]
    public class RecruiterController : BaseCompressedApiController
    {
        private readonly ILogger _logger;
        private readonly IRecruiterRepository _recruiterRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        /// <summary>
        /// Initializes a new instance of the RecruiterController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="recruiterRepository">Repository of type <see cref="IRecruiterRepository">IRecruiterRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public RecruiterController(IAdapterRegistry adapterRegistry, IRecruiterRepository recruiterRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _recruiterRepository = recruiterRepository;
            _logger = logger;
        }

        /// <summary>
        /// Import a Recruiter application/prospect into Colleague.
        /// </summary>
        /// <param name="application">Application/prospect import data</param>
        /// <returns>Http 200 response</returns>
        public async Task<HttpResponseMessage> PostApplicationAsync(Application application)
        {
            // Map Application DTO to the domain entity
            var applicationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.Application, Ellucian.Colleague.Domain.Student.Entities.Application>();
            var applicationEntity = applicationDtoAdapter.MapToType(application);
           await  _recruiterRepository.ImportApplicationAsync(applicationEntity);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        
        /// <summary>
        /// Updates an existing application's status.
        /// </summary>
        /// <param name="application">Application update data</param>
        /// <returns>Http 200 response</returns>
        public async Task<HttpResponseMessage> PostApplicationStatusAsync(Application application)
        {
            // Map Application DTO to the domain entity
            var applicationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.Application, Ellucian.Colleague.Domain.Student.Entities.Application>();
            var applicationEntity = applicationDtoAdapter.MapToType(application);
            await _recruiterRepository.UpdateApplicationAsync(applicationEntity);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Import a Recruiter test score into Colleague.
        /// </summary>
        /// <param name="testScore">Test score data</param>
        /// <returns>Http 200 response</returns>
        public async Task<HttpResponseMessage> PostTestScoresAsync(TestScore testScore)
        {
            // Map TestScore DTO to the domain entity
            var testScoreDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.TestScore, Ellucian.Colleague.Domain.Student.Entities.TestScore>();
            var testScoreEntity = testScoreDtoAdapter.MapToType(testScore);
            await _recruiterRepository.ImportTestScoresAsync(testScoreEntity);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Import a Recruiter transcript course into Colleague.
        /// </summary>
        /// <param name="transcriptCourse">transcript course data</param>
        /// <returns>Http 200 response</returns>
        public async Task<HttpResponseMessage> PostTranscriptCoursesAsync(TranscriptCourse transcriptCourse)
        {
            // Map TranscriptCourse DTO to the domain entity
            var transcriptCourseDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.TranscriptCourse, Ellucian.Colleague.Domain.Student.Entities.TranscriptCourse>();
            var transcriptCourseEntity = transcriptCourseDtoAdapter.MapToType(transcriptCourse);
            await _recruiterRepository.ImportTranscriptCoursesAsync(transcriptCourseEntity);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Import Recruiter communication history into Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history data</param>
        /// <returns>Http 200 response</returns>
        public async Task<HttpResponseMessage> PostCommunicationHistoryAsync(CommunicationHistory communicationHistory)
        {
            // Map CommunicationHistory DTO to the domain entity
            var communicationHistoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Base.CommunicationHistory, Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory>();
            var communicationHistoryEntity = communicationHistoryDtoAdapter.MapToType(communicationHistory);
            await _recruiterRepository.ImportCommunicationHistoryAsync(communicationHistoryEntity);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Request communication history from Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history request</param>
        /// <returns>Http 200 response</returns>
        public async Task<HttpResponseMessage> PostCommunicationHistoryRequestAsync(CommunicationHistory communicationHistory)
        {
            // Map CommunicationHistory DTO to the domain entity
            var communicationHistoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Base.CommunicationHistory, Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory>();
            var communicationHistoryEntity = communicationHistoryDtoAdapter.MapToType(communicationHistory);
            await _recruiterRepository.RequestCommunicationHistoryAsync(communicationHistoryEntity);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test connection from Colleague to Recruiter.
        /// </summary>
        /// <param name="connectionStatus">connection status request (empty or optional RecruiterOrganizationName)</param>
        /// <returns>Connection status response</returns>
        public async Task<ConnectionStatus> PostConnectionStatusAsync(ConnectionStatus connectionStatus)
        {
            // Map ConnectionStatus DTO to the domain entity, set resultEntity from Repository that retrieves connection status,
            // then map domain entity back to a DTO
            var connectionStatusDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.ConnectionStatus, Ellucian.Colleague.Domain.Student.Entities.ConnectionStatus>();
            var connectionStatusEntity = connectionStatusDtoAdapter.MapToType(connectionStatus);
            var resultEntity = await _recruiterRepository.PostConnectionStatusAsync(connectionStatusEntity);
            var oppositeAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ConnectionStatus, Ellucian.Colleague.Dtos.Student.ConnectionStatus>();
            var resultDto = oppositeAdapter.MapToType(resultEntity);
            return resultDto;
        }
    }
}
