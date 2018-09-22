/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// Coordination Service for AwardPackageChangeRequests
    /// </summary>
    [RegisterType]
    public class AwardPackageChangeRequestService : AwardYearCoordinationService, IAwardPackageChangeRequestService
    {
        private IAwardPackageChangeRequestRepository awardPackageChangeRequestRepository;
        private IStudentAwardRepository studentAwardRepository;
        private IFinancialAidReferenceDataRepository financialAidReferenceDataRepository;        
        private ICommunicationRepository communicationRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="financialAidOfficeRepository"></param>
        /// <param name="studentAwardYearRepository"></param>
        /// <param name="awardPackageChangeRequestRepository"></param>
        /// <param name="studentAwardRepository"></param>
        /// <param name="financialAidReferenceDataRepository"></param>
        /// <param name="studentRepository"></param>
        /// <param name="applicantRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public AwardPackageChangeRequestService(
            IAdapterRegistry adapterRegistry,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IAwardPackageChangeRequestRepository awardPackageChangeRequestRepository,
            IStudentAwardRepository studentAwardRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,            
            ICommunicationRepository communicationRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.awardPackageChangeRequestRepository = awardPackageChangeRequestRepository;
            this.studentAwardRepository = studentAwardRepository;
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.communicationRepository = communicationRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get a list of AwardPackageChangeRequests that were submitted by the given student
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<AwardPackageChangeRequest>> GetAwardPackageChangeRequestsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to get awardPackageChangeRequest resources for student {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var changeRequestEntitiesForStudent = await awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId);
            if (changeRequestEntitiesForStudent == null || changeRequestEntitiesForStudent.Count() == 0)
            {
                var message = string.Format("Student {0} has no AwardPackageChangeRequests", studentId);
                logger.Info(message);
                return new List<AwardPackageChangeRequest>();
            }

            var changeRequestDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardPackageChangeRequest, AwardPackageChangeRequest>();
            return changeRequestEntitiesForStudent.Select(entity => changeRequestDtoAdapter.MapToType(entity));
        }

        /// <summary>
        /// Get a single AwardPackageChangeRequest submitted by the given student with the given awardPackageChangeRequestId
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardPackageChangeRequestId"></param>
        /// <returns></returns>
        public async Task<AwardPackageChangeRequest> GetAwardPackageChangeRequestAsync(string studentId, string awardPackageChangeRequestId)
        {
            if (string.IsNullOrEmpty(awardPackageChangeRequestId))
            {
                throw new ArgumentNullException("awardPackageChangeRequestId");
            }

            var changeRequestDtosForStudent = await GetAwardPackageChangeRequestsAsync(studentId);
            if (changeRequestDtosForStudent == null || changeRequestDtosForStudent.Count() == 0)
            {
                var message = string.Format("Student {0} has no AwardPackageChangeRequests", studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var changeRequestDto = changeRequestDtosForStudent.FirstOrDefault(cr => cr.Id == awardPackageChangeRequestId);
            if (changeRequestDto == null)
            {
                var message = string.Format("AwardPackageChangeRequest resource {0} does not exist for Student {1}", awardPackageChangeRequestId, studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            return changeRequestDto;
        }

        /// <summary>
        /// Create an AwardPackageChangeRequest for the given student using the data in the newAwardPackageChangeRequest
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="newAwardPackageChangeRequest"></param>
        /// <returns></returns>
        public async Task<AwardPackageChangeRequest> CreateAwardPackageChangeRequestAsync(string studentId, AwardPackageChangeRequest newAwardPackageChangeRequest)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (newAwardPackageChangeRequest == null)
            {
                throw new ArgumentNullException("newAwardPackageChangeRequest");
            }

            if (!UserIsSelf(studentId))
            {
                var message = string.Format("{0} does not have permission to create awardPackageChangeRequest resources for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            if (string.IsNullOrEmpty(newAwardPackageChangeRequest.StudentId))
            {
                var message = string.Format("StudentId attribute of newAwardPackageChangeRequest is required");
                logger.Error(message);
                throw new InvalidOperationException(message);
            }
            if (string.IsNullOrEmpty(newAwardPackageChangeRequest.AwardYearId))
            {
                var message = string.Format("AwardYearId attribute of newAwardPackageChangeRequest is required");
                logger.Error(message);
                throw new InvalidOperationException(message);
            }
            if (string.IsNullOrEmpty(newAwardPackageChangeRequest.AwardId))
            {
                var message = string.Format("AwardId attribute of newAwardPackageChangeRequest is required");
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            if (studentId != newAwardPackageChangeRequest.StudentId)
            {
                var message = string.Format("studentId must match StudentId attribute of newAwardPackageChangeRequest");
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var changeRequestEntityAdapter = _adapterRegistry.GetAdapter<AwardPackageChangeRequest, Domain.FinancialAid.Entities.AwardPackageChangeRequest>();
            var changeRequestEntity = changeRequestEntityAdapter.MapToType(newAwardPackageChangeRequest);

            var studentAwardYear = await GetStudentAwardYearEntityAsync(studentId, changeRequestEntity.AwardYearId);
                        
            if (studentAwardYear == null)
            {
                var message = string.Format("Award Year {0} does not exist for studentId {1}", changeRequestEntity.AwardYearId, studentId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var awards = financialAidReferenceDataRepository.Awards;
            if (awards == null || awards.Count() == 0)
            {
                var message = string.Format("No awards exist");
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var awardStatuses = financialAidReferenceDataRepository.AwardStatuses;
            if (awardStatuses == null || awardStatuses.Count() == 0)
            {
                var message = string.Format("No award statuses exist");
                logger.Error(message);
                throw new InvalidOperationException(message);
            }
            
            var studentAward = await studentAwardRepository.GetStudentAwardAsync(studentId, studentAwardYear, newAwardPackageChangeRequest.AwardId, awards, awardStatuses);
                        
            if (studentAward == null)
            {
                var message = string.Format("Award {0} does not exist for Student {1} in year {2}", changeRequestEntity.AwardId, studentId, studentAwardYear.Code);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }
            
            StudentAwardDomainService.AssignPendingChangeRequests(studentAward, await awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId, studentAward));

            //If the incoming award is an unsubsidized loan, get the rest of the awards for the year to pass to the domain service for verification
            IEnumerable<Domain.FinancialAid.Entities.StudentAward> allStudentAwardsForYear = null;
            if (studentAward.Award.LoanType.HasValue && studentAward.Award.LoanType.Value == Domain.FinancialAid.Entities.LoanType.UnsubsidizedLoan)
            {
                allStudentAwardsForYear = await studentAwardRepository.GetStudentAwardsForYearAsync(studentId, studentAwardYear, awards, awardStatuses);
            }
            
            var newChangeRequestEntity = await awardPackageChangeRequestRepository.CreateAwardPackageChangeRequestAsync(
                AwardPackageChangeRequestDomainService.VerifyAwardPackageChangeRequest(changeRequestEntity, studentAward, awardStatuses, allStudentAwardsForYear),
                studentAward);
            
            if (newChangeRequestEntity == null)
            {
                var message = "Unable to create award package change request resource";
                logger.Error(message);
                throw new Exception(message);
            }

            var communications = AwardPackageChangeRequestDomainService.GetCommunications(newChangeRequestEntity, studentAward);
            
            if (communications != null && communications.Count() > 0)
            {
                foreach (var communication in communications)
                {
                    try
                    {
                        communicationRepository.SubmitCommunication(communication);
                    }
                    catch (Exception e)
                    {
                        var message = string.Format("Error submitting Communication {0} for student {1}", communication.ToString(), studentId);
                        logger.Warn(e, message);
                    }
                }
            }

            var awardPackageChangeRequestDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.AwardPackageChangeRequest, AwardPackageChangeRequest>();
            return awardPackageChangeRequestDtoAdapter.MapToType(newChangeRequestEntity);

        }
    }
}
