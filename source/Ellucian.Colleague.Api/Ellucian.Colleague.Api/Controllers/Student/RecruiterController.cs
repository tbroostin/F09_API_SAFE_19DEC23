// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Student
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
        private readonly IRecruiterService _recruiterService;

        /// <summary>
        /// Initializes a new instance of the RecruiterController class.
        /// </summary>
        /// <param name="recruiterService">Coordination service of type <see cref="IRecruiterService">IRecruiterService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public RecruiterController(IRecruiterService recruiterService, ILogger logger)
        {
            _recruiterService = recruiterService;
            _logger = logger;
        }

        /// <summary>
        /// Import a Recruiter application/prospect into Colleague.
        /// </summary>
        /// <param name="application">Application/prospect import data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can import Recruiter applications/prospects into Colleague.
        /// </accessComments>
        public async Task<HttpResponseMessage> PostApplicationAsync(Application application)
        {
            try
            {
                await _recruiterService.ImportApplicationAsync(application);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Updates an existing application's status.
        /// </summary>
        /// <param name="application">Application update data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can update an existing application's status.
        /// </accessComments>
        public async Task<HttpResponseMessage> PostApplicationStatusAsync(Application application)
        {
            try
            {
                await _recruiterService.UpdateApplicationStatusAsync(application);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Import a Recruiter test score into Colleague.
        /// </summary>
        /// <param name="testScore">Test score data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can import a Recruiter test score into Colleague.
        /// </accessComments>
        public async Task<HttpResponseMessage> PostTestScoresAsync(TestScore testScore)
        {
            try
            {
                await _recruiterService.ImportTestScoresAsync(testScore);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Import a Recruiter transcript course into Colleague.
        /// </summary>
        /// <param name="transcriptCourse">transcript course data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can import a Recruiter transcript course into Colleague.
        /// </accessComments>
        public async Task<HttpResponseMessage> PostTranscriptCoursesAsync(TranscriptCourse transcriptCourse)
        {
            try
            {
                await _recruiterService.ImportTranscriptCoursesAsync(transcriptCourse);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Import Recruiter communication history into Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history data</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can import Recruiter communication history into Colleague.
        /// </accessComments>
        public async Task<HttpResponseMessage> PostCommunicationHistoryAsync(CommunicationHistory communicationHistory)
        {
            try
            {
                await _recruiterService.ImportCommunicationHistoryAsync(communicationHistory);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Request communication history from Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history request</param>
        /// <returns>Http 200 response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can request Recruiter communication history into Colleague.
        /// </accessComments>
        public async Task<HttpResponseMessage> PostCommunicationHistoryRequestAsync(CommunicationHistory communicationHistory)
        {
            try
            {
                await _recruiterService.RequestCommunicationHistoryAsync(communicationHistory);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Test connection from Colleague to Recruiter.
        /// </summary>
        /// <param name="connectionStatus">connection status request (empty or optional RecruiterOrganizationName)</param>
        /// <returns>Connection status response</returns>
        /// <accessComments>
        /// Authenticated users with the PERFORM.RECRUITER.OPERATIONS permission can test the connection from Colleague to Recruiter.
        /// </accessComments>
        public async Task<ConnectionStatus> PostConnectionStatusAsync(ConnectionStatus connectionStatus)
        {
            try
            {
                ConnectionStatus resultDto = await _recruiterService.PostConnectionStatusAsync(connectionStatus);
                return resultDto;
            }
            catch (PermissionsException ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}
