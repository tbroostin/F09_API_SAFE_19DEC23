// Copyright 2013-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IRecruiterRepository
    {
        Task UpdateApplicationAsync(Application application);
        Task ImportApplicationAsync(Application application);
        Task ImportTestScoresAsync(TestScore testScore);
        Task ImportTranscriptCoursesAsync(TranscriptCourse transcriptCourse);
        Task ImportCommunicationHistoryAsync(CommunicationHistory communicationHistory);
        Task RequestCommunicationHistoryAsync(CommunicationHistory communicationHistory);
        Task<ConnectionStatus> PostConnectionStatusAsync(ConnectionStatus connectionStatus);
    }
}
