// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Instant Enrollment Service layer
    /// </summary>
    [RegisterType]
    public class InstantEnrollmentService : BaseCoordinationService, IInstantEnrollmentService
    {
        private readonly IInstantEnrollmentRepository _instantEnrollmentRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IStudentProgramRepository _studentProgramRepository;
        private List<Section> _instantEnrollmentSections;

        public InstantEnrollmentService(IAdapterRegistry adapterRegistry, IInstantEnrollmentRepository instantEnrollmentRepository,
            ISectionRepository sectionRepository, IStudentProgramRepository studentProgramRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger) :
            base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _instantEnrollmentRepository = instantEnrollmentRepository;
            _sectionRepository = sectionRepository;
            _instantEnrollmentSections = null;
            _studentProgramRepository = studentProgramRepository;

        }

        /// <summary>
        /// Accepts a list of proposed course sections, along with demographic information, and returns the anticipated cost of the sections.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentProposedRegistration"/> containing the information to evaluate.</param>
        /// <returns>A <see cref="InstantEnrollmentProposedRegistrationResult"/> containing the cost of the proposed sections.</returns>
        public async Task<InstantEnrollmentProposedRegistrationResult> ProposedRegistrationForClassesAsync(InstantEnrollmentProposedRegistration proposedRegistration)
        {
            InstantEnrollmentProposedRegistrationResult proposedRegistrationResultDto = null;
            try
            {
                if (proposedRegistration == null)
                {
                    throw new ArgumentNullException("proposedRegistration", "proposed registration is required in order to mock registrations for selected classes for instant enrollment");

                }
                CheckUserInstantEnrollmentAllowAllPermission(proposedRegistration.PersonId);

                if (proposedRegistration.ProposedSections == null || proposedRegistration.ProposedSections.Count == 0)
                {
                    throw new ArgumentException("ProposedSections", "proposed registration should have proposed sections to register for and retrieve associated cost for instant enrollment");
                }
                var proposedSectionIds = proposedRegistration.ProposedSections.Where(s => !string.IsNullOrEmpty(s.SectionId)).Select(ps => ps.SectionId).ToList();
                await ValidateInstantEnrollmentSections(proposedSectionIds);

                //convert proposed registration dto to entity
                var proposedRegistrationDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentProposedRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistration>();
                var proposedRegistrationEntity = proposedRegistrationDtoToEntityAdapter.MapToType(proposedRegistration);
                //call repository
                var proposedRegistrationResult = await _instantEnrollmentRepository.GetProposedRegistrationResultAync(proposedRegistrationEntity);
                //convert proposed registration result entity to dto
                if (proposedRegistrationResult != null)
                {
                    var proposedRegistrationResultEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentProposedRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentProposedRegistrationResult>();
                    proposedRegistrationResultDto = proposedRegistrationResultEntityToDtoAdapter.MapToType(proposedRegistrationResult);
                }
                return proposedRegistrationResultDto;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured while completing mock registration for selected classes in Instant Enrollment");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Accepts a list of course sections and demographic information and registers person for classes when the total cost for registration is zero.
        /// </summary>
        /// <param name="zeroCostRegistration">A <see cref="InstantEnrollmentZeroCostRegistration"/> containing the information for registration.</param>
        /// <returns>A <see cref="InstantEnrollmentZeroCostRegistrationResult"/> containing the results of the registration attempt.</returns>
        public async Task<InstantEnrollmentZeroCostRegistrationResult> ZeroCostRegistrationForClassesAsync(InstantEnrollmentZeroCostRegistration zeroCostRegistration)
        {
            InstantEnrollmentZeroCostRegistrationResult zeroCostRegistrationResultDto = null;
            try
            {
                if (zeroCostRegistration == null)
                {
                    throw new ArgumentNullException("zeroCostRegistration", "zero cost registration is required in order to registration selected classes for instant enrollment.");

                }
                CheckUserInstantEnrollmentAllowAllPermission(zeroCostRegistration.PersonId);

                if (zeroCostRegistration.ProposedSections == null || zeroCostRegistration.ProposedSections.Count == 0)
                {
                    throw new ArgumentException("ProposedSections", "zero cost registration requires at least one proposed section to register for and retrieve associated cost for instant enrollment.");
                }
                var proposedSectionIds = zeroCostRegistration.ProposedSections.Select(ps => ps.SectionId).ToList();
                await ValidateInstantEnrollmentSections(proposedSectionIds);

                //convert the zero cost registration dto to an entity
                var zeroCostRegistrationDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistration>();
                var zeroCostRegistrationEntity = zeroCostRegistrationDtoToEntityAdapter.MapToType(zeroCostRegistration);

                //call repository
                var zeroCostRegistrationResult = await _instantEnrollmentRepository.GetZeroCostRegistrationResultAsync(zeroCostRegistrationEntity);

                //convert zero cost registration result entity to a dto
                if (zeroCostRegistrationResult != null)
                {
                    var zeroCostRegistrationResultEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult>();
                    zeroCostRegistrationResultDto = zeroCostRegistrationResultEntityToDtoAdapter.MapToType(zeroCostRegistrationResult);
                }
                return zeroCostRegistrationResultDto;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured while completing a zero cost registration for selected classes in Instant Enrollment.");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Accepts a list of course sections and demographic information, then registers the person for the sections and pays
        /// for them using electronic transfer.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentEcheckRegistration"/> containing the information to evaluate.</param>
        /// <returns>A <see cref="InstantEnrollmentEcheckRegistrationResult"/> containing the results of the registration attempt.</returns>
        public async Task<InstantEnrollmentEcheckRegistrationResult> EcheckRegistrationForClassesAsync(InstantEnrollmentEcheckRegistration criteria)
        {
            InstantEnrollmentEcheckRegistrationResult echeckRegistrationResultDto = null;
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria", "echeck registration is required in order to register and pay for selected classes for instant enrollment");

                }
                CheckUserInstantEnrollmentAllowAllPermission(criteria.PersonId);

                if (criteria.ProposedSections == null || criteria.ProposedSections.Count == 0)
                {
                    throw new ArgumentException("criteria", "proposed registration should have sections for which to register for instant enrollment");
                }
                var proposedSectionIds = criteria.ProposedSections.Select(ps => ps.SectionId).ToList();
                await ValidateInstantEnrollmentSections(proposedSectionIds);

                //convert proposed registration dto to entity
                var echeckRegistrationDtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistration>();
                var echeckRegistrationEntity = echeckRegistrationDtoToEntityAdapter.MapToType(criteria);
                //call repository
                var echeckRegistrationResult = await _instantEnrollmentRepository.GetEcheckRegistrationResultAsync(echeckRegistrationEntity);
                //convert proposed registration result entity to dto
                if (echeckRegistrationResult != null)
                {
                    var echeckRegistrationResultEntityToDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentEcheckRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentEcheckRegistrationResult>();
                    echeckRegistrationResultDto = echeckRegistrationResultEntityToDtoAdapter.MapToType(echeckRegistrationResult);
                }
                return echeckRegistrationResultDto;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured while completing echeck registration for selected classes in Instant Enrollment");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Start an instant enrollment payment gateway transaction, which includes registering the student and creating the student if needed.
        /// </summary>
        /// <param name="criteria">A <see cref="InstantEnrollmentPaymentGatewayRegistration"/> containing the information needed to start the payment gateway transaction.</param>
        /// <returns></returns>
        public async Task<InstantEnrollmentStartPaymentGatewayRegistrationResult> StartInstantEnrollmentPaymentGatewayTransaction(InstantEnrollmentPaymentGatewayRegistration criteria)
        {
            InstantEnrollmentStartPaymentGatewayRegistrationResult resultDto = null;
            try
            {
                if (criteria == null)
                {
                    throw new ArgumentNullException("criteria");

                }
                CheckUserInstantEnrollmentAllowAllPermission(criteria.PersonId);

                if (criteria.ProposedSections == null || criteria.ProposedSections.Count == 0)
                {
                    throw new ArgumentException("criteria", "Proposed registration should have sections for which to register for instant enrollment");
                }
                var proposedSectionIds = criteria.ProposedSections.Select(ps => ps.SectionId).ToList();
                await ValidateInstantEnrollmentSections(proposedSectionIds);

                //convert proposed registration dto to entity
                var dtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentGatewayRegistration>();
                var startPaymentGatewayRegistrationEntity = dtoToEntityAdapter.MapToType(criteria);
                //call repository
                var resultEntity = await _instantEnrollmentRepository.StartInstantEnrollmentPaymentGatewayTransactionAsync(startPaymentGatewayRegistrationEntity);
                //convert proposed registration result entity to dto
                if (resultEntity != null)
                {
                    var entityToDtoAdapater = _adapterRegistry.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult, Dtos.Student.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult>();
                    resultDto = entityToDtoAdapater.MapToType(resultEntity);
                }
                return resultDto;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Exception occured while initiating payment gateway registration for selected classes in Instant Enrollment");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves instant enrollment payment acknowledgement paragraph text for a given <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentPaymentAcknowledgementParagraphRequest"/></param>
        /// <returns>Instant enrollment payment acknowledgement paragraph text</returns>
        public async Task<IEnumerable<string>> GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(InstantEnrollmentPaymentAcknowledgementParagraphRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request", "An instant enrollment payment acknowledgement paragraph request is required to get instant enrollment payment acknowledgement paragraph text.");
            }
            CheckUserInstantEnrollmentAllowAllPermission(request.PersonId);

            try
            {
                List<string> paragraphText = new List<string>();
                var dtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPaymentAcknowledgementParagraphRequest>();
                var instantEnrollmentPaymentAcknowledgementParagraphRequestEntity = dtoToEntityAdapter.MapToType(request);
                var textFromRepository = await _instantEnrollmentRepository.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(instantEnrollmentPaymentAcknowledgementParagraphRequestEntity);
                paragraphText.AddRange(textFromRepository);
                return paragraphText;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string exceptionMsg = string.Format("An error occurred while attempting to retrieve instant enrollment payment acknowledgement paragraph text for person {0}", request.PersonId);
                if (!string.IsNullOrEmpty(request.CashReceiptId))
                {
                    exceptionMsg += string.Format(" for cash receipt {0}", request.CashReceiptId);
                }
                logger.Error(ex, exceptionMsg);
                throw new ApplicationException(exceptionMsg);
            }
        }

        /// <summary>
        /// Query persons matching the criteria using the ELF duplicate checking criteria configured for Instant Enrollment.
        /// </summary>
        /// <param name="person">The <see cref="Dtos.Base.PersonMatchCriteriaInstantEnrollment">criteria</see> to query by.</param>
        /// <returns>Result of a person biographic/demographic matching inquiry for Instant Enrollment</returns>
        public async Task<InstantEnrollmentPersonMatchResult> QueryPersonMatchResultsInstantEnrollmentByPostAsync(PersonMatchCriteriaInstantEnrollment criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria", "Criteria required to query");

            CheckUserInstantEnrollmentAllowAllPermission(null);

            var adapter =
                _adapterRegistry.GetAdapter<PersonMatchCriteriaInstantEnrollment, Domain.Student.Entities.InstantEnrollment.PersonMatchCriteriaInstantEnrollment>();
            var entCriteria = adapter.MapToType(criteria);
            var entResult = await _instantEnrollmentRepository.GetMatchingPersonResultsInstantEnrollmentAsync(entCriteria);

            var result = new InstantEnrollmentPersonMatchResult();
            if (entResult != null)
            {
                var dtoAdapter =
                    _adapterRegistry.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentPersonMatchResult, InstantEnrollmentPersonMatchResult>();
                result = dtoAdapter.MapToType(entResult);
            }

            return result;
        }

        /// <summary>
        /// Retrieves instant enrollment payment cash receipt acknowledgement for a given for a given <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/>
        /// </summary>
        /// <param name="request">A <see cref="InstantEnrollmentCashReceiptAcknowledgementRequest"/></param>
        /// <returns>the cash receipt and sections for an instant enrollment payment</returns>
        public async Task<InstantEnrollmentCashReceiptAcknowledgement> GetInstantEnrollmentCashReceiptAcknowledgementAsync(InstantEnrollmentCashReceiptAcknowledgementRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request", "An instant enrollment cash receipt acknowledgement request is required to get instant enrollment cash receipt acknowledgement.");
            }

            try
            {
                var dtoToEntityAdapter = _adapterRegistry.GetAdapter<Dtos.Student.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgementRequest, Domain.Student.Entities.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgementRequest>();
                var cashReceiptAcknowledgementRequestEntity = dtoToEntityAdapter.MapToType(request);

                //get person id (payer id ) from the cash receipt
                var cashReceiptAcknowledgementEntity = await _instantEnrollmentRepository.GetInstantEnrollmentCashReceiptAcknowledgementAsync(cashReceiptAcknowledgementRequestEntity);

                if (cashReceiptAcknowledgementEntity != null &&
                   (cashReceiptAcknowledgementEntity.Status == Domain.Student.Entities.InstantEnrollment.EcommerceProcessStatus.Success ||
                    cashReceiptAcknowledgementEntity.Status == Domain.Student.Entities.InstantEnrollment.EcommerceProcessStatus.None))
                {
                    string payerId = cashReceiptAcknowledgementEntity.ReceiptPayerId;
                    CheckUserInstantEnrollmentAllowAllPermission(payerId);

                    //TODO: e-check registration ctx needs to be modified to return a person id before this can be enabled
                    //when cash receipt id is passed, a person id must also be passed as part the request criteria and must match the payer id on the cash receipt
                    //there will not be a person id when transaction id is passed
                    //if (!string.IsNullOrEmpty(payerId) && !string.IsNullOrEmpty(request.CashReceiptId) && payerId != request.PersonId)
                    //{ 
                    //        logger.Info("Payer Id on the cash receipt does not match the person id ");
                    //        throw new PermissionsException();
                    //}
                    //else
                    //{
                    //    logger.Info("Could not retrieve the receipt with the specified information");
                    //    throw new InvalidOperationException();
                    //}
                }

                var adapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgement, Dtos.Student.InstantEnrollment.InstantEnrollmentCashReceiptAcknowledgement>();
                var cashReceiptAcknowledgementDto = adapter.MapToType(cashReceiptAcknowledgementEntity);

                return cashReceiptAcknowledgementDto;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string exceptionMsg = "An error occurred while attempting to retrieve an instant enrollment cash receipt acknowledgement";
                if (!string.IsNullOrEmpty(request.TransactionId))
                {
                    exceptionMsg += string.Format(" for e-commerce transaction id {0}", request.TransactionId);
                }
                if (!string.IsNullOrEmpty(request.CashReceiptId))
                {
                    exceptionMsg += string.Format(" for cash receipt id {0}", request.CashReceiptId);
                }
                logger.Error(ex, exceptionMsg);
                throw new ApplicationException(exceptionMsg);
            }
        }

        /// <summary>
        /// Retrieves the programs in which the specified student is enrolled.
        /// </summary>
        /// <param name="studentId">Student's ID</param>
        /// <param name="currentOnly">Boolean to indicate whether this request is for active student programs, or ended/past programs as well</param>
        /// <returns>All <see cref="Dtos.Student.StudentProgram2">Programs</see> in which the specified student is enrolled.</returns>
        public async Task<IEnumerable<Dtos.Student.StudentProgram2>> GetInstantEnrollmentStudentPrograms2Async(string studentId, bool currentOnly = true)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                throw new ArgumentNullException("studentId", "An instant enrollment student id is required to retrieve student programs.");
            }

            CheckUserInstantEnrollmentAllowAllPermission(studentId);

            try
            {
                IEnumerable<Domain.Student.Entities.StudentProgram> studentPrograms = await _studentProgramRepository.GetAsync(studentId);

                // Limit set of student programs to current programs if requested 
                // - an active program has a start date less than or equal to today and an end date that is blank or greater than or equal to today
                if (currentOnly == true)
                {
                    studentPrograms = studentPrograms.Where(x =>
                        (x.StartDate != null && x.StartDate <= DateTime.Today) &&
                        (x.EndDate == null || x.EndDate >= DateTime.Today));
                }

                List<Dtos.Student.StudentProgram2> studentProgramDtos = new List<Dtos.Student.StudentProgram2>();
                if (studentPrograms.Count() > 0)
                {
                    var studentProgramDtoAdapter = _adapterRegistry.GetAdapter<Domain.Student.Entities.StudentProgram, Dtos.Student.StudentProgram2>();
                    foreach (var prog in studentPrograms)
                    {
                        var studentProgramDto = studentProgramDtoAdapter.MapToType(prog);
                        studentProgramDtos.Add(studentProgramDto);
                    }
                }
                return studentProgramDtos;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string exceptionMsg = string.Format("An error occurred while attempting to retrieve student programs for person {0}", studentId);
                logger.Error(ex, exceptionMsg);
                throw;
            }
        }

        /// <summary>
        /// A helper method to determine if the logged in user is the preson whose data is being accessed.
        /// </summary>
        /// <param name="studentId">ID of student from data</param>
        /// <returns>Indicates whether the user is the student</returns>
        private bool UserIsSelf(string studentId)
        {
            return CurrentUser.IsPerson(studentId);
        }

        /// <summary>
        /// Verifies that all of the course section IDs in a collection are valid instant enrollment course section IDs
        /// </summary>
        /// <param name="sectionIds">Collection of course section IDs</param>
        private async Task ValidateInstantEnrollmentSections(List<string> sectionIds)
        {
            var ieSectionIds = new List<string>();
            try
            {
                if (_instantEnrollmentSections == null)
                {
                    _instantEnrollmentSections = (await _sectionRepository.GetInstantEnrollmentSectionsAsync()).ToList();
                }
                if (_instantEnrollmentSections != null)
                {
                    ieSectionIds = _instantEnrollmentSections.Where(ies => ies != null).Select(cs => cs.Id).ToList();
                }
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorMsg = "Unable to retrieve instant enrollment section information to validate sections.";
                logger.Error(ex, errorMsg);
                throw new ApplicationException(errorMsg);
            }

            if (sectionIds != null && ieSectionIds.Any() && sectionIds.Any())
            {
                var invalidSecIds = new List<string>();
                foreach (var secId in sectionIds)
                {
                    if (!ieSectionIds.Contains(secId))
                    {
                        invalidSecIds.Add(secId);
                    }
                }
                if (invalidSecIds.Any())
                {
                    var errorMsg = string.Format("The following course sections are not valid for instant enrollment: {0}", string.Join(",", invalidSecIds));
                    logger.Error(errorMsg);
                    throw new ApplicationException(errorMsg);
                }
            }
            return;
        }

        /// <summary>
        /// Verify the current user has permission to access Instant Enrollment on behalf of the proposed student.
        /// </summary>
        /// <param name="id">The person id of the proposed student.  This can be null in the case of an anonymous student.</param>
        private void CheckUserInstantEnrollmentAllowAllPermission(string id)
        {
            // They're allowed to see data if they are the logged in user or have the Instant Enrollment permission
            if (!UserIsSelf(id) && !HasPermission(StudentPermissionCodes.InstantEnrollmentAllowAll))
            {
                logger.Info(CurrentUser + " does not have permission code " + StudentPermissionCodes.InstantEnrollmentAllowAll);
                throw new PermissionsException();
            }
        }
    }
}