//Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// This class coordinates access to and from StudentAward resources.
    /// </summary>
    [RegisterType]
    public class StudentAwardService : AwardYearCoordinationService, IStudentAwardService
    {

        private readonly IStudentAwardRepository studentAwardRepository;
        private readonly IAwardPackageChangeRequestRepository awardPackageChangeRequestRepository;
        private readonly IStudentLoanLimitationRepository studentLoanLimitationRepository;
        private readonly IFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        private readonly ICommunicationRepository communicationRepository;
        private readonly ITermRepository termRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection framework
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="studentAwardRepository">StudentAwardRepository</param>
        /// <param name="studentLoanLimitationRepository">StudentLoanLimitationRepository</param>
        /// <param name="financialAidReferenceDataRepository">FinancialAidReferenceDataRepository</param>
        /// <param name="financialAidOfficeRepository"></param>
        /// <param name="studentAwardYearRepository"></param>
        /// <param name="termRepository"></param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public StudentAwardService(IAdapterRegistry adapterRegistry,
            IStudentAwardRepository studentAwardRepository,
            IStudentLoanLimitationRepository studentLoanLimitationRepository,
            IFinancialAidReferenceDataRepository financialAidReferenceDataRepository,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IStudentAwardYearRepository studentAwardYearRepository,
            IAwardPackageChangeRequestRepository awardPackageChangeRequestRepository,
            ICommunicationRepository communicationRepository,
            ITermRepository termRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.studentAwardRepository = studentAwardRepository;
            this.studentLoanLimitationRepository = studentLoanLimitationRepository;
            this.financialAidReferenceDataRepository = financialAidReferenceDataRepository;
            this.financialAidOfficeRepository = financialAidOfficeRepository;
            this.studentAwardYearRepository = studentAwardYearRepository;
            this.awardPackageChangeRequestRepository = awardPackageChangeRequestRepository;
            this.communicationRepository = communicationRepository;
            this.termRepository = termRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get all StudentAwards for all years the student has data
        /// </summary>
        /// <param name="studentId">The PERSON id of the student for whom to retrieve data </param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentAward objects representing all of the student's award data</returns>
        /// <exception cref="ArgumentNullException">Thrown when studentId argument is empty or null</exception>
        public async Task<IEnumerable<StudentAward>> GetStudentAwardsAsync(string studentId, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to get student awards for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var allAwards = financialAidReferenceDataRepository.Awards;
            var allAwardStatuses = financialAidReferenceDataRepository.AwardStatuses;

            var studentAwardYears = await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly);
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                var message = string.Format("No award years exist for student {0}", studentId);
                logger.Error(message);
                return new List<StudentAward>();
            }

            //Retrieve StudentAwards from Colleague
            var allStudentAwards = await studentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYears, allAwards, allAwardStatuses);
            if (allStudentAwards == null || allStudentAwards.Count() == 0)
            {
                var message = string.Format("No awards exist in any of the award years for student {0}", studentId);
                logger.Debug(message);
                return new List<StudentAward>();
            }

            var awardPackageChangeRequests = await awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId);
            StudentAwardDomainService.AssignPendingChangeRequests(allStudentAwards, awardPackageChangeRequests);

            //Filter awards using configuration parameters
            var filteredAwards = ApplyConfigurationService.FilterStudentAwards(allStudentAwards);

            //Get the adapter - This should retrieve our custom StudentAwardDtoAdapter which has an AutoMappingDependency to StudentAwardPeriod
            var studentAwardDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAward, StudentAward>();

            //Turn the entity objects into Dtos
            var studentAwardDtoList = new List<StudentAward>();
            if (filteredAwards == null || filteredAwards.Count() == 0)
            {
                var message = string.Format("Office Configurations have filtered out all student awards for student {0}", studentId);
                logger.Debug(message);
                return studentAwardDtoList;
            }

            foreach (var studentAward in filteredAwards)
            {
                studentAwardDtoList.Add(studentAwardDtoAdapter.MapToType(studentAward));
            }

            return studentAwardDtoList;
        }

        /// <summary>
        /// Get all StudentAwards for a particular award year
        /// </summary>
        /// <param name="studentId">The PERSON id of the student for whom to retrieve data</param>
        /// <param name="awardYear">The award year for which to retrieve StudentAwards</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentAward objects representing one year's worth of the student's award data</returns>
        public async Task<IEnumerable<StudentAward>> GetStudentAwardsAsync(string studentId, string awardYear, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYear))
            {
                throw new ArgumentNullException("awardYear");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to get student awards for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var allAwards = financialAidReferenceDataRepository.Awards;
            var allAwardStatuses = financialAidReferenceDataRepository.AwardStatuses;

            var activeStudentAwardYear = (await GetStudentAwardYearEntitiesAsync(studentId, getActiveYearsOnly)).FirstOrDefault(y => y.Code == awardYear);

            if (activeStudentAwardYear == null)
            {
                var message = string.Format("awardYear {0} does not exist for student {1}", awardYear, studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            //Retrieve StudentAwards from Colleague
            var studentAwards = await studentAwardRepository.GetStudentAwardsForYearAsync(studentId, activeStudentAwardYear, allAwards, allAwardStatuses);
            if (studentAwards == null || studentAwards.Count() == 0)
            {
                var message = string.Format("awardYear {0} has no awards for student {1}", awardYear, studentId);
                logger.Debug(message);
                return new List<StudentAward>();
            }

            var awardPackageChangeRequests = await awardPackageChangeRequestRepository.GetAwardPackageChangeRequestsAsync(studentId);
            StudentAwardDomainService.AssignPendingChangeRequests(studentAwards, awardPackageChangeRequests);

            //Filter awards using configuration parameters
            var filteredAwards = ApplyConfigurationService.FilterStudentAwards(studentAwards);

            //Get the adapter - This should retrieve our custom StudentAwardDtoAdapter which has an AutoMappingDependency to StudentAwardPeriod
            var studentAwardDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAward, StudentAward>();

            //Turn the entity objects into Dtos
            var studentAwardDtoList = new List<StudentAward>();
            if (filteredAwards == null || filteredAwards.Count() == 0)
            {
                var message = string.Format("FA Office {0} filtered out all StudentAwards for student {1} awardYear {2}", activeStudentAwardYear.CurrentOffice.Id, studentId, awardYear);
                logger.Debug(message);
                return studentAwardDtoList;
            }

            foreach (var studentAward in filteredAwards)
            {
                studentAwardDtoList.Add(studentAwardDtoAdapter.MapToType(studentAward));
            }

            return studentAwardDtoList;
        }

        /// <summary>
        /// Get a single StudentAward for the given year, studentId, and awardId
        /// </summary>
        /// <param name="studentId">The PERSON id of the student for whom to retrieve data</param>
        /// <param name="awardYear">The award year from which to retrieve student award data.</param>
        /// <param name="awardId">The specific awardId to return</param>
        /// <returns>A single StudentAward DTO</returns>
        /// <exception cref="ArgumentNullException">Thrown when any of the method arguments are empty or null</exception>
        /// <exception cref="Exception">Thrown when a StudentAward resource does not exist for the given arguments</exception>
        public async Task<StudentAward> GetStudentAwardsAsync(string studentId, string awardYear, string awardId)
        {

            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId");
            }

            var studentAward = (await GetStudentAwardsAsync(studentId, awardYear)).FirstOrDefault(sa => sa.AwardId == awardId);
            if (studentAward == null)
            {
                var message = string.Format("Unable to retrieve studentAward resource for student {0}, awardYear {1} and awardId {2}", studentId, awardYear, awardId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            return studentAward;
        }

        /// <summary>
        /// Update a list of studentAwards for the given studentId and awardYear
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of the student whose awards will be updated</param>
        /// <param name="year">Award Year for which the StudentAwards should be updated</param>
        /// <param name="studentAwards">The list of StudentAwards containing updated data</param>
        /// <param name="getActiveYearsOnly">flag indicating whether to retrieve active years data only</param>
        /// <returns>A list of StudentAwards with updated data</returns>
        public async Task<IEnumerable<StudentAward>> UpdateStudentAwardsAsync(string studentId, string year, IEnumerable<StudentAward> studentAwards, bool getActiveYearsOnly = false)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException("year");
            }
            if (studentAwards == null)
            {
                throw new ArgumentNullException("studentAwards");
            }
            if (studentAwards.Count() == 0)
            {
                return new List<StudentAward>();
            }

            if (!UserIsSelf(studentId))
            {
                // User does not have permissions and error needs to be thrown and logged
                var message = string.Format("{0} does not have permission to update StudentAwards for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            if (studentAwards.Any(sa => sa.StudentId != studentId))
            {
                throw new InvalidOperationException(string.Format("All studentAwards must apply to the same student id {0}", studentId));
            }
            if (studentAwards.Any(sa => sa.AwardYearId != year))
            {
                throw new InvalidOperationException(string.Format("All StudentAwards must apply to the same award year {0}", year));
            }

            var studentAwardYearEntities = await GetStudentAwardYearEntitiesAsync(CurrentUser.PersonId, getActiveYearsOnly);
            if (studentAwardYearEntities == null || studentAwardYearEntities.Count() == 0)
            {
                var message = string.Format("Student {0} has no award years", CurrentUser.PersonId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            var studentAwardYear = studentAwardYearEntities.FirstOrDefault(y => y.Code == year);
            if (studentAwardYear == null)
            {
                var message = string.Format("Award Year {0} does not exist for studentId {1}", year, studentId);
                logger.Error(message);
                throw new KeyNotFoundException(message);
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

            //Retrieve fa offices - need to get SuppressMaximumLoanLimits flag from configuration for the year
            var faOffices = await financialAidOfficeRepository.GetFinancialAidOfficesAsync();
            var faConfiguration = faOffices != null ? faOffices.FirstOrDefault(o => o.Id == studentAwardYear.FinancialAidOfficeId)
                .Configurations.FirstOrDefault(c => c.AwardYear == studentAwardYear.Code) : null;
            bool suppressMaximumLoanLimits = faConfiguration != null ? faConfiguration.SuppressMaximumLoanLimits : false;

            var newStudentAwards = new List<Domain.FinancialAid.Entities.StudentAward>();
            var studentAwardEntityAdapter = new StudentAwardDtoToEntityAdapter(_adapterRegistry, logger);
            foreach (var studentAwardDto in studentAwards)
            {
                var award = awards.FirstOrDefault(a => a.Code == studentAwardDto.AwardId);
                if (award == null)
                {
                    var message = string.Format("Award {0} does not exist", studentAwardDto.AwardId);
                    logger.Error(message);
                    throw new KeyNotFoundException(message);
                }

                newStudentAwards.Add(studentAwardEntityAdapter.MapToType(studentAwardDto, studentAwardYear, award, awardStatuses));
            }

            var currentStudentAwards = await studentAwardRepository.GetStudentAwardsForYearAsync(CurrentUser.PersonId, studentAwardYear, awards, awardStatuses);
            if (currentStudentAwards == null || currentStudentAwards.Count() == 0)
            {
                var message = string.Format("No student awards exist for student {0} awardYear {1}", CurrentUser.PersonId, studentAwardYear.Code);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            //validate that newStudentAward exists in currentStudentAwards
            foreach (var newStudentAward in newStudentAwards)
            {
                if (!currentStudentAwards.Contains(newStudentAward))
                {
                    var message = string.Format(string.Format("No student award exists for awardId {0} ", newStudentAward.Award.Code));
                    logger.Error(message);
                    throw new InvalidOperationException(message);
                }
            }

            var studentLoanLimitations = await studentLoanLimitationRepository.GetStudentLoanLimitationsAsync(CurrentUser.PersonId, new List<Domain.FinancialAid.Entities.StudentAwardYear>() { studentAwardYear });
                
            if (studentLoanLimitations == null || studentLoanLimitations.Count() == 0)
            {
                var message = string.Format("No student loan limitations exist for student {0} awardYear {1}", CurrentUser.PersonId, studentAwardYear.Code);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }
            try
            {
                var updatedStudentAwards = await studentAwardRepository.UpdateStudentAwardsAsync(
                    studentAwardYear,
                    StudentAwardDomainService.VerifyUpdatedStudentAwards(studentAwardYear, newStudentAwards, currentStudentAwards, studentLoanLimitations, suppressMaximumLoanLimits),
                    awards,
                    awardStatuses);


                if (updatedStudentAwards == null)
                {
                    var message = string.Format("Unable to Update Student Awards for student {0} awardYear {1}", CurrentUser.PersonId, studentAwardYear.Code);
                    logger.Error(message);
                    throw new ApplicationException(message);
                }

                var communications = StudentAwardDomainService.GetCommunicationsForUpdatedAwards(studentAwardYear, updatedStudentAwards, currentStudentAwards);
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
                            logger.Debug(e, message);
                        }
                    }
                }
                //API 1.36 removing the filter on returning awards as it caused the My Awards
                //Page to not render properly for award flagged as removed from view
                //This led to display issues as well as sub/unsub combination action issues
                //Filter out awards/award periods that are not viewable
                //var filteredAwards = ApplyConfigurationService.FilterStudentAwards(updatedStudentAwards);

                var studentAwardDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAward, StudentAward>();
                //return filteredAwards.Select(sa => studentAwardDtoAdapter.MapToType(sa));
                return updatedStudentAwards.Select(sa => studentAwardDtoAdapter.MapToType(sa));
            }
            catch (ColleagueSessionExpiredException csee)
            {
                throw;
            }
        }

        /// <summary>
        /// Update a single StudentAward for the given studentId, awardYear, and awardId
        /// </summary>
        /// <param name="studentId">Colleague PERSON id of the student whose award will be updated</param>
        /// <param name="year">Award Year of the StudentAward that will be updated</param>
        /// <param name="awardId">The Award Id of the StudentAward that will be update</param>
        /// <param name="studentAward">The StudentAward containing updated data</param>
        /// <returns>A StudentAward with updated data</returns>
        public async Task<StudentAward> UpdateStudentAwardsAsync(string studentId, string year, string awardId, StudentAward studentAward)
        {
            if (string.IsNullOrEmpty(awardId))
            {
                throw new ArgumentNullException("awardId");
            }
            if (studentAward == null)
            {
                throw new ArgumentNullException("studentAward");
            }
            if (awardId != studentAward.AwardId)
            {
                var message = string.Format("awardId {0} must match awardId {1} of studentAward resource", awardId, studentAward.AwardId);
                logger.Error(message);
                throw new InvalidOperationException(message);
            }

            return (await UpdateStudentAwardsAsync(studentId, year, new List<StudentAward>() { studentAward })).First();
        }

        /// <summary>
        /// Get all StudentAwards for a particular award year or years and summarize.
        /// </summary>
        /// <param name="studentIds">The PERSON ids of the students for whom to retrieve data</param>
        /// <param name="awardYear">The award year for which to retrieve StudentAwards (optional)</param>
        /// <param name="term">The Term to use to get FA award years (optional)</param>
        /// <returns>A list of StudentAward objects representing one year's worth of the student's award data</returns>
        public IEnumerable<StudentAwardSummary> QueryStudentAwardSummary(StudentAwardSummaryQueryCriteria criteria)
        {
            var studentIds = criteria.StudentIds;
            var awardYear = criteria.AwardYear;
            var term = criteria.Term;

            if (studentIds == null || studentIds.Count() <= 0)
            {
                throw new ArgumentNullException("studentIds");
            }
            // Check Student View Permissions
            if (!HasPermission(StudentPermissionCodes.ViewStudentInformation))
            {
                var message = string.Format("{0} does not have permission to view Student Information", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            // If the term has multiple FA years defined, return all Fafsa Data for all FA Years.
            List<string> awardYears = new List<string>();
            if (string.IsNullOrEmpty(awardYear))
            {
                if (!string.IsNullOrEmpty(term))
                {
                    Ellucian.Colleague.Domain.Student.Entities.Term termData = termRepository.Get(term);

                    var termFaYears = termData.FinancialAidYears;
                    foreach (var faYear in termFaYears)
                    {
                        awardYears.Add(faYear.ToString());
                    }
                    if (awardYears == null)
                    {
                        awardYears.Add(termData.ReportingYear.ToString());
                    }
                    if (awardYears == null)
                    {
                        throw new ArgumentException("Could not determine FA Year from Term");
                    }
                }
            }
            else
            {
                awardYears.Add(awardYear);
            }

            List<Domain.FinancialAid.Entities.StudentAward> allStudentAwards = new List<Domain.FinancialAid.Entities.StudentAward>();
            var allAwards = financialAidReferenceDataRepository.Awards;
            var allAwardStatuses = financialAidReferenceDataRepository.AwardStatuses;
            //var allAwardCategories = financialAidReferenceDataRepository.AwardCategories;
            var allAwardCodes = financialAidReferenceDataRepository.Awards;

            // Go through each academic year since we may have multiple coming
            // from a single term record.                        
            if (awardYears != null && awardYears.Count() > 0 && studentIds != null && studentIds.Count() > 0)
            {
                foreach (var studentId in studentIds)
                {
                    if (!string.IsNullOrEmpty(studentId))
                    {
                        try
                        {
                            var studentAwardYears = Task.Run(async() => await GetStudentAwardYearEntitiesAsync(studentId)).GetAwaiter().GetResult();
                            if (studentAwardYears != null)
                            {
                                foreach (var year in awardYears)
                                {
                                    var studentAwardYear = studentAwardYears.FirstOrDefault(y => y.Code == year);
                                    if (studentAwardYear != null)
                                    {
                                        try
                                        {
                                            var studentAwards = studentAwardRepository.GetStudentAwardSummaryForYear(studentId, studentAwardYear, allAwards, allAwardStatuses);
                                            allStudentAwards.AddRange(studentAwards);
                                        }
                                        catch (Exception e)
                                        {
                                            // Report the error and then go on to the next student.
                                            logger.Error(string.Format("Exception thrown for studentAwardSummary, StudentId {0} for year {1}", studentId, year));
                                            logger.Error(e.GetBaseException().Message);
                                            logger.Error(e.GetBaseException().StackTrace);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            // Report the error and then go on to the next student.
                            var message = "Exception thrown for studentAwardSummary, StudentId: '" + studentId + "'";
                            logger.Error(e, message);
                            logger.Error(e.GetBaseException().Message);
                            logger.Error(e.GetBaseException().StackTrace);
                        }
                    }
                }
            }

            //Get the adapter - This should retrieve our custom StudentAwardDtoAdapter which has an AutoMappingDependency to StudentAwardPeriod
            var StudentAwardDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.StudentAwardSummary, StudentAwardSummary>();

            //Turn the entity objects into Dtos
            var StudentAwardSummaryDtoList = new List<StudentAwardSummary>();
            if (allStudentAwards == null || allStudentAwards.Count() == 0)
            {
                return StudentAwardSummaryDtoList;
            }

            foreach (var studentAward in allStudentAwards)
            {
                var studentAwardSummary = new Domain.FinancialAid.Entities.StudentAwardSummary(studentAward.StudentId, studentAward.StudentAwardYear.Code, studentAward.Award.Code);
                // Update Award Summary record with data from the Student Awards
                var awardEntity = allAwardCodes.FirstOrDefault(a => a.Code == studentAward.Award.Code);
                studentAwardSummary.AwardType = (awardEntity != null && awardEntity.AwardCategory != null) ? awardEntity.AwardCategory.Code : string.Empty;
                studentAwardSummary.FundSource = awardEntity.Type;

                // Get Accepted and Offered amounts
                studentAwardSummary.AmountAccepted = 0;
                studentAwardSummary.AmountOffered = 0;

                if (studentAward.StudentAwardPeriods != null && studentAward.StudentAwardPeriods.Count() > 0)
                {
                    foreach (var awardPeriod in studentAward.StudentAwardPeriods)
                    {
                        if (!awardPeriod.AwardAmount.HasValue) { awardPeriod.AwardAmount = 0; }
                        if (awardPeriod.AwardStatus != null)
                        {
                            if (awardPeriod.AwardStatus.Category == Domain.FinancialAid.Entities.AwardStatusCategory.Accepted)
                            {
                                studentAwardSummary.AmountOffered += awardPeriod.AwardAmount;
                                studentAwardSummary.AmountAccepted += awardPeriod.AwardAmount;
                            }
                            else
                            {
                                studentAwardSummary.AmountOffered += awardPeriod.AwardAmount;
                            }
                        }
                        else
                        {
                            // There will be a null awardPeriod.AwardStatus if no TA.TERM.ACTION found during BuildStudentAwardPeriods.  
                            // Log that no awards totals appended for the TA.ACYR record.
                            logger.Error(string.Format("No award status found for StudentId {0} for award {1} for term {2}.  Record excluded from award totals.", awardPeriod.StudentId, studentAwardSummary.FundType, awardPeriod.AwardPeriodId));
                        }
                    }
                }
                StudentAwardSummaryDtoList.Add(StudentAwardDtoAdapter.MapToType(studentAwardSummary));
            }
            return StudentAwardSummaryDtoList;
        }



    }
}
