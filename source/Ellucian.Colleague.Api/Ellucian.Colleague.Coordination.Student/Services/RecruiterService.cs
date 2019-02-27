// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Coordination service for Recruiter operations
    /// </summary>
    [RegisterType]
    public class RecruiterService : BaseCoordinationService, IRecruiterService
    {
        private readonly IRecruiterRepository _recruiterRepository;

        /// <summary>
        /// Initializes a new instance of the RecruiterService class.
        /// </summary>
        /// <param name="recruiterRepository">Repository of type <see cref="IRecruiterRepository">IRecruiterRepository</see></param>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="currentUserFactory">Current user factory of type <see cref="ICurrentUserFactory">ICurrentUserFactory</see></param>
        /// <param name="roleRepository">Repository of type <see cref="IRoleRepository">IRoleRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public RecruiterService(IRecruiterRepository recruiterRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger) : 
            base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _recruiterRepository = recruiterRepository;
        }

        /// <summary>
        /// Import a Recruiter application/prospect into Colleague.
        /// </summary>
        /// <param name="application">Application/prospect import data</param>
        public async Task ImportApplicationAsync(Application application)
        {
            await CheckRecruiterPermissionsAsync();

            // Map Application DTO to the domain entity
            var applicationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.Application, Ellucian.Colleague.Domain.Student.Entities.Application>();
            var applicationEntity = applicationDtoAdapter.MapToType(application);
            await _recruiterRepository.ImportApplicationAsync(applicationEntity);
            return;
        }

        /// <summary>
        /// Updates an existing application's status.
        /// </summary>
        /// <param name="application">Application update data</param>
        public async Task UpdateApplicationStatusAsync(Application application)
        {
            await CheckRecruiterPermissionsAsync();

            // Map Application DTO to the domain entity
            var applicationDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.Application, Ellucian.Colleague.Domain.Student.Entities.Application>();
            var applicationEntity = applicationDtoAdapter.MapToType(application);
            await _recruiterRepository.UpdateApplicationAsync(applicationEntity);
            return;
        }

        /// <summary>
        /// Import a Recruiter test score into Colleague.
        /// </summary>
        /// <param name="testScore">Test score data</param>
        public async Task ImportTestScoresAsync(TestScore testScore)
        {
            await CheckRecruiterPermissionsAsync();

            // Map TestScore DTO to the domain entity
            var testScoreDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.TestScore, Ellucian.Colleague.Domain.Student.Entities.TestScore>();
            var testScoreEntity = testScoreDtoAdapter.MapToType(testScore);
            await _recruiterRepository.ImportTestScoresAsync(testScoreEntity);
            return;
        }

        /// <summary>
        /// Import a Recruiter transcript course into Colleague.
        /// </summary>
        /// <param name="transcriptCourse">transcript course data</param>
        public async Task ImportTranscriptCoursesAsync(TranscriptCourse transcriptCourse)
        {
            await CheckRecruiterPermissionsAsync();

            // Map TranscriptCourse DTO to the domain entity
            var transcriptCourseDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.TranscriptCourse, Ellucian.Colleague.Domain.Student.Entities.TranscriptCourse>();
            var transcriptCourseEntity = transcriptCourseDtoAdapter.MapToType(transcriptCourse);
            await _recruiterRepository.ImportTranscriptCoursesAsync(transcriptCourseEntity);
            return;
        }

        /// <summary>
        /// Import Recruiter communication history into Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history data</param>
        public async Task ImportCommunicationHistoryAsync(CommunicationHistory communicationHistory)
        {
            await CheckRecruiterPermissionsAsync();

            // Map CommunicationHistory DTO to the domain entity
            var communicationHistoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Base.CommunicationHistory, Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory>();
            var communicationHistoryEntity = communicationHistoryDtoAdapter.MapToType(communicationHistory);
            await _recruiterRepository.ImportCommunicationHistoryAsync(communicationHistoryEntity);
            return;
        }

        /// <summary>
        /// Request communication history from Colleague.
        /// </summary>
        /// <param name="communicationHistory">communication history request</param>
        public async Task RequestCommunicationHistoryAsync(CommunicationHistory communicationHistory)
        {
            await CheckRecruiterPermissionsAsync();

            // Map CommunicationHistory DTO to the domain entity
            var communicationHistoryDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Base.CommunicationHistory, Ellucian.Colleague.Domain.Base.Entities.CommunicationHistory>();
            var communicationHistoryEntity = communicationHistoryDtoAdapter.MapToType(communicationHistory);
            await _recruiterRepository.RequestCommunicationHistoryAsync(communicationHistoryEntity);
            return;
        }

        /// <summary>
        /// Test connection from Colleague to Recruiter.
        /// </summary>
        /// <param name="connectionStatus">connection status request (empty or optional RecruiterOrganizationName)</param>
        public async Task<ConnectionStatus> PostConnectionStatusAsync(ConnectionStatus connectionStatus)
        {
            await CheckRecruiterPermissionsAsync();

            // Map ConnectionStatus DTO to the domain entity, set resultEntity from Repository that retrieves connection status,
            // then map domain entity back to a DTO
            var connectionStatusDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Student.ConnectionStatus, Ellucian.Colleague.Domain.Student.Entities.ConnectionStatus>();
            var connectionStatusEntity = connectionStatusDtoAdapter.MapToType(connectionStatus);
            var resultEntity = await _recruiterRepository.PostConnectionStatusAsync(connectionStatusEntity);
            var oppositeAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.ConnectionStatus, Ellucian.Colleague.Dtos.Student.ConnectionStatus>();
            var resultDto = oppositeAdapter.MapToType(resultEntity);
            return resultDto;
        }

        /// <summary>
        /// Determines if the authenticated user has permission to perform Recruiter operations
        /// </summary>
        /// <returns></returns>
        protected async Task CheckRecruiterPermissionsAsync()
        {
            var userPermissions = await GetUserPermissionCodesAsync();
            if (userPermissions.Contains(BasePermissionCodes.PerformRecruiterOperations))
            {
                return;
            }
            throw new PermissionsException("User does not have permission to perform Recruiter operations.");
        }
    }
}
