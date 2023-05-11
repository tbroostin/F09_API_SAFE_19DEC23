/*Copyright 2014-2022 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// LoanRequestService
    /// </summary>
    [RegisterType]
    public class LoanRequestService : AwardYearCoordinationService, ILoanRequestService
    {
        /// <summary>
        /// The LoanRequestRepository that provides database access and domain entity creation
        /// </summary>
        private readonly ILoanRequestRepository loanRequestRepository;

        private readonly IStudentRepository studentRepository;
        private readonly IApplicantRepository applicantRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Instantiate a new LoanRequestService
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="loanRequestRepository">LoanRequestRepository</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public LoanRequestService(IAdapterRegistry adapterRegistry,
            ILoanRequestRepository loanRequestRepository,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IStudentRepository studentRepository,
            IApplicantRepository applicantRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.loanRequestRepository = loanRequestRepository;
            this.studentRepository = studentRepository;
            this.applicantRepository = applicantRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get a LoanRequest with the given Id
        /// </summary>
        /// <param name="id">Id of the loanRequest object to get</param>
        /// <returns>LoanRequest DTO with the given Id</returns>
        /// <exception cref="ArgumentNullException">Thrown if the id argument is null or empty</exception>
        /// <exception cref="PermissionsException">Thrown if the current user is not self, or the current user does not have access permissions to view
        /// student Financial Aid data</exception>
        public async Task<LoanRequest> GetLoanRequestAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            var loanRequestEntity = await loanRequestRepository.GetLoanRequestAsync(id);
            if (loanRequestEntity == null)
            {
                var message = string.Format("Unable to find requested LoanRequest resource {0}", id);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            if (!UserHasAccessPermission(loanRequestEntity.StudentId))
            {
                var message = string.Format("{0} does not have permission to get LoanRequest resource {1}", CurrentUser.PersonId, id);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var loanRequestDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.LoanRequest, Dtos.FinancialAid.LoanRequest>();
            return loanRequestDtoAdapter.MapToType(loanRequestEntity);
        }

        /// <summary>
        /// Create a new LoanRequest
        /// </summary>
        /// <param name="loanRequest">LoanRequest object with new data</param>
        /// <returns>New LoanRequest object</returns>
        /// <exception cref="ArgumentNullException">Thrown if loanRequest argument is null</exception>
        /// <exception cref="ArgumentException">Thrown if StudentId or AwardYear attributes of loanRequest argument are null or empty, 
        /// or if the TotalRequestAmount attribute of the loanRequest argument is less than or equal to zero</exception>
        /// <exception cref="PermissionsException">Thrown if the Current User tries to a create a loan request for a student other than self.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the AwardYear attribute in the loanRequest argument is not an active StudentAwardYear</exception>
        /// <exception cref="ExistingResourceException">Thrown if a pending LoanRequest already exists for the Student and AwardYear in the loanRequest argument</exception>        
        public async Task<LoanRequest> CreateLoanRequestAsync(LoanRequest loanRequest)
        {
            if (loanRequest == null)
            {
                throw new ArgumentNullException("loanRequest");
            }
            if (string.IsNullOrEmpty(loanRequest.StudentId))
            {
                throw new ArgumentException("loanRequest.StudentId is required", "loanRequest");
            }
            if (string.IsNullOrEmpty(loanRequest.AwardYear))
            {
                throw new ArgumentException("loanRequest.AwardYear is required", "loanRequest");
            }
            if (loanRequest.TotalRequestAmount <= 0)
            {
                throw new ArgumentException("loanRequest.TotalRequestAmount is required", "loanRequest");
            }
            if (loanRequest.LoanRequestPeriods == null || loanRequest.LoanRequestPeriods.Count == 0)
            {
                throw new ArgumentException("loanRequest.LoanRequestPeriods cannot be empty", "loanRequest");
            }
            if (!UserIsSelf(loanRequest.StudentId))
            {
                var message = string.Format("{0} does not have permission to create LoanRequest resource for {1}", CurrentUser.PersonId, loanRequest.StudentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            //get the student or applicant's current FA counselor Id. this will be the counselor this loan request is assigned to.
            Person studentOrApplicant = null;
            try
            {
                studentOrApplicant = await studentRepository.GetAsync(loanRequest.StudentId);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception a) 
            {
                if (a.Message == "SECURITY \n00002 \nYour previous session is no longer valid.  Please log in again. \n")
                {
                    throw new ColleagueSessionExpiredException("Session has expired, please login again.");
                }
            }

            if (studentOrApplicant == null)
            {
                try
                {
                    studentOrApplicant = await applicantRepository.GetApplicantAsync(loanRequest.StudentId);
                }
                catch (ColleagueSessionExpiredException)
                {
                    throw;
                }
                catch (Exception b) 
                {
                    if (b.Message == "SECURITY \n00002 \nYour previous session is no longer valid.  Please log in again. \n")
                    {
                        throw new ColleagueSessionExpiredException("Session has expired, please login again.");
                    }
                }
            }

            //if the studentOrApplicant is still null, throw an exception
            if (studentOrApplicant == null)
            {
                throw new InvalidOperationException("Cannot create loan request for non-student/non-applicant person.");
            }

            var activeStudentAwardYear = (await GetActiveStudentAwardYearEntitiesAsync(loanRequest.StudentId)).FirstOrDefault(y => y.Code == loanRequest.AwardYear);

            var assignedToId = LoanRequestDomainService.GetLoanRequestAssignment(studentOrApplicant, activeStudentAwardYear);

            //set the other properties of input LoanRequest Dto 
            loanRequest.Id = "-1";
            loanRequest.RequestDate = DateTime.Today;
            loanRequest.AssignedToId = assignedToId;
            loanRequest.Status = LoanRequestStatus.Pending;
            loanRequest.StatusDate = DateTime.Today;

            //get and call adapter to convert to domain entity
            var loanRequestEntityAdapter = _adapterRegistry.GetAdapter<LoanRequest, Domain.FinancialAid.Entities.LoanRequest>();
            var inputLoanRequestEntity = loanRequestEntityAdapter.MapToType(loanRequest);

            try
            {
                //call repository to create loan request
                var newLoanRequestEntity = await loanRequestRepository.CreateLoanRequestAsync(inputLoanRequestEntity, activeStudentAwardYear);
                if (newLoanRequestEntity == null)
                {
                    var message = string.Format("Unknown resource id for new LoanRequest resource for student id {0} and awardYear {1}", loanRequest.StudentId, loanRequest.AwardYear);
                    logger.Error(message);
                    throw new ColleagueWebApiException(message);
                }

                //call adapter to convert new domain entity to Dto
                var loanRequestDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.LoanRequest, LoanRequest>();
                return loanRequestDtoAdapter.MapToType(newLoanRequestEntity);
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
        }
    }
}
