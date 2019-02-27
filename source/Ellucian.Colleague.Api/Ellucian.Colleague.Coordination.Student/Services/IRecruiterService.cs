// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface to a Recruiter Service
    /// </summary>
    public interface IRecruiterService
    {
        /// <summary>
        /// Updates an existing application's status.
        /// </summary>
        /// <param name="application">Application update data</param>
        Task UpdateApplicationStatusAsync(Application application);

        /// <summary>
        /// Import a Recruiter application/prospect into Colleague.
        /// </summary>
        /// <param name="application">Application/prospect import data</param>
        Task ImportApplicationAsync(Application application);

        /// <summary>
        /// Import a Recruiter test score into Colleague.
        /// </summary>
        /// <param name="testScore">Test score data</param>
        Task ImportTestScoresAsync(TestScore testScore);

        /// <summary>
        /// Import a Recruiter transcript course into Colleague.
        /// </summary>
        /// <param name="transcriptCourse">transcript course data</param>
        Task ImportTranscriptCoursesAsync(TranscriptCourse transcriptCourse);

        /// <summary>
        /// Import Recruiter communication history into Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history data</param>
        Task ImportCommunicationHistoryAsync(CommunicationHistory communicationHistory);

        /// <summary>
        /// Request communication history from Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history request</param>
        Task RequestCommunicationHistoryAsync(CommunicationHistory communicationHistory);

        /// <summary>
        /// Test connection from Colleague to Recruiter.
        /// </summary>
        /// <param name="connectionStatus">connection status request (empty or optional RecruiterOrganizationName)</param>
        Task<ConnectionStatus> PostConnectionStatusAsync(ConnectionStatus connectionStatus);

    }
}
