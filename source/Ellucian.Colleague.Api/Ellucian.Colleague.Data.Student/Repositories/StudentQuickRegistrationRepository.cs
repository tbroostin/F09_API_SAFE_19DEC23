// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.QuickRegistration;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Repository for student Colleague Self-Service Quick Registration operations
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentQuickRegistrationRepository: BaseStudentRepository, IStudentQuickRegistrationRepository
    {
        public StudentQuickRegistrationRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {
        }

        /// <summary>
        /// Retrieves a given student's quick registration information for the provided academic term codes
        /// </summary>
        /// <param name="studentId">Unique identifier for a student</param>
        /// <returns>A <see cref="StudentQuickRegistration"/> object</returns>
        public async Task<StudentQuickRegistration> GetStudentQuickRegistrationAsync(string studentId)
        {
            if(string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "A student ID is required when retrieving a student's Colleague Self-Service Quick Registration data.");
            }
            try
            {
                StudentQuickRegistration quickReg = new StudentQuickRegistration(studentId);
                var registrationConfiguration = await GetRegistrationConfigurationAsync();
                if (registrationConfiguration != null && registrationConfiguration.QuickRegistrationIsEnabled)
                {
                    // Retrieve DEGREE_PLAN_TERMS records for student for STWEB.QUICK.REG.TERMS
                    List<DegreePlanTerms> degreePlanTermsData = new List<DegreePlanTerms>();
                    string degreePlanTermsQueryString = "DPT.STUDENT EQ '" + studentId + "' AND DPT.TERM EQ '?'";
                    var degreePlanTermsIds = await DataReader.SelectAsync("DEGREE_PLAN_TERMS", degreePlanTermsQueryString, registrationConfiguration.QuickRegistrationTermCodes.ToArray());
                    BulkReadOutput<DegreePlanTerms> dptBulkReadOutput = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<DegreePlanTerms>(degreePlanTermsIds);
                    if (!dptBulkReadOutput.Equals(default(BulkReadOutput<DegreePlanTerms>)))
                    {
                        if (dptBulkReadOutput.BulkRecordsRead == null || !dptBulkReadOutput.BulkRecordsRead.Any())
                        {
                            logger.Error(string.Format("Bulk data retrieval for DEGREE_PLAN_TERMS file for records {0} did not return any valid records.", string.Join(",", degreePlanTermsIds)));
                        }
                        if (dptBulkReadOutput.InvalidRecords != null && dptBulkReadOutput.InvalidRecords.Any())
                        {
                            foreach (var ir in dptBulkReadOutput.InvalidRecords)
                            {
                                logger.Error("Invalid DEGREE_PLAN_TERMS record found. DEGREE.PLAN.TERMS.ID = {0}: {1}", ir.Key, ir.Value);
                            }
                        }
                        if (dptBulkReadOutput.BulkRecordsRead != null && dptBulkReadOutput.BulkRecordsRead.Any())
                        {
                            degreePlanTermsData.AddRange(dptBulkReadOutput.BulkRecordsRead);
                        }
                    }
                    // also select waitlist records for thes students
                    List<WaitList> waitlistData = new List<WaitList>();
                    string waitlistQueryString = "WAIT.STUDENT EQ '?'";
                    var waitlistIds = await DataReader.SelectAsync("WAIT.LIST", waitlistQueryString, new string[] { studentId });
                    if (waitlistIds != null && waitlistIds.Any())
                    {
                        BulkReadOutput<WaitList> waitListBulkReadOutput = await DataReader.BulkReadRecordWithInvalidKeysAndRecordsAsync<WaitList>(waitlistIds);
                        if (!waitListBulkReadOutput.Equals(default(BulkReadOutput<WaitList>)))
                        {
                            if (waitListBulkReadOutput.BulkRecordsRead == null || !waitListBulkReadOutput.BulkRecordsRead.Any())
                            {
                                logger.Error(string.Format("Bulk data retrieval for WAIT.LIST file for records {0} did not return any valid records.", string.Join(",", degreePlanTermsIds)));
                            }
                            if (waitListBulkReadOutput.InvalidRecords != null && waitListBulkReadOutput.InvalidRecords.Any())
                            {
                                foreach (var ir in waitListBulkReadOutput.InvalidRecords)
                                {
                                    logger.Error("Invalid WAIT.LIST record found. WAIT.LIST.ID = {0}: {1}", ir.Key, ir.Value);
                                }
                            }
                            if (waitListBulkReadOutput.BulkRecordsRead != null && waitListBulkReadOutput.BulkRecordsRead.Any())
                            {
                                waitlistData.AddRange(waitListBulkReadOutput.BulkRecordsRead);
                            }
                        }
                    }
                    quickReg = await BuildStudentQuickRegistrationAsync(studentId, degreePlanTermsData, waitlistData);
                }
                else
                {
                    logger.Debug(string.Format("Colleague Self-Service Quick Registration workflow is disabled; no quick registration sections will be returned for student {0} for any terms.", studentId));
                }
                return quickReg;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Quick registration data for student {0} could not be retrieved.", studentId);
                logger.Error(ex, errorMessage);
                throw new ApplicationException(errorMessage);
            }
        }

        /// <summary>
        /// Creates a <see cref="StudentQuickRegistration"/> object for a given student and collection of DEGREE_PLAN_TERMS records
        /// </summary>
        /// <param name="studentId">Unique identifier for a student</param>
        /// <param name="degreePlanTermsData">Collection of <see cref="DegreePlanTerms"/> data</param>
        /// <param name="waitlistData">Collection of <see cref="WaitList"/> data</param>
        /// <returns>A <see cref="StudentQuickRegistration"/> object</returns>
        private async Task<StudentQuickRegistration> BuildStudentQuickRegistrationAsync(string studentId, List<DegreePlanTerms> degreePlanTermsData, List<WaitList> waitlistData)
        {
            StudentQuickRegistration studentQuickRegistration = new StudentQuickRegistration(studentId);
            foreach(var degreePlanTerm in degreePlanTermsData)
            {
                try
                {
                    QuickRegistrationTerm quickRegTerm = new QuickRegistrationTerm(degreePlanTerm.DptTerm);
                    if (degreePlanTerm.PlannedCoursesEntityAssociation != null)
                    {
                        foreach (var dptSection in degreePlanTerm.PlannedCoursesEntityAssociation)
                        {
                            if (dptSection != null && !string.IsNullOrEmpty(dptSection.DptSectionsAssocMember))
                            {
                                try
                                {
                                    Domain.Student.Entities.DegreePlans.WaitlistStatus waitlistStatus = await(GetStudentSectionWaitlistStatusAsync(studentId, degreePlanTerm, dptSection, waitlistData));
                                    GradingType dptSecGradingType = ConvertStringToGradingTypeEnum(dptSection.DptGradingTypeAssocMember);
                                    QuickRegistrationSection section = new QuickRegistrationSection(dptSection.DptSectionsAssocMember, dptSection.DptCreditsAssocMember, dptSecGradingType, waitlistStatus);
                                    quickRegTerm.AddSection(section);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, string.Format("Unable to add section {0} to quick registration term {1} for student {2}.", dptSection.DptSectionsAssocMember, degreePlanTerm.DptTerm, studentId));
                                }
                            }
                        }
                    }
                    // Only add a quick reg term for the student if there is at least one section in that term
                    if (quickRegTerm.Sections.Any())
                    {
                        studentQuickRegistration.AddTerm(quickRegTerm);
                    }
                    else
                    {
                        logger.Debug(string.Format("Quick registration term {0} not included for student {1} because the student did not plan any valid sections in that term.", degreePlanTerm.DptTerm, studentId));
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, string.Format("Unable to add quick registration term {0} for student {1}.", degreePlanTerm.DptTerm, studentId));
                }
            }
            return studentQuickRegistration;
        }

        /// <summary>
        /// Converts a string value to a corresponding <see cref="GradingType"/> value
        /// </summary>
        /// <param name="dptGradingType">Grading type string</param>
        /// <returns>A <see cref="GradingType"/> value</returns>
        private GradingType ConvertStringToGradingTypeEnum(string dptGradingType)
        {
            GradingType gradingType = GradingType.Graded;
            if (dptGradingType.ToUpperInvariant() == "P")
            {
                gradingType = GradingType.PassFail;
            }
            if (dptGradingType.ToUpperInvariant() == "A")
            {
                gradingType = GradingType.Audit;
            }
            return gradingType;
        }

        private async Task<Domain.Student.Entities.DegreePlans.WaitlistStatus> GetStudentSectionWaitlistStatusAsync(string studentId, DegreePlanTerms term, DegreePlanTermsPlannedCourses dptpc, List<WaitList> waitlistData)
        {
            // Determine waitlist status of any associated section here.
            var status = Domain.Student.Entities.DegreePlans.WaitlistStatus.NotWaitlisted;
            var studentSectionWaitlists = waitlistData.Where(wl => wl != null && wl.WaitStudent == studentId && wl.WaitCourseSection == dptpc.DptSectionsAssocMember).ToList();
            foreach (var waitlistItem in studentSectionWaitlists)
            {
                // Make sure this section does not conflict with the section already associated with this planned course.
                // This handles the case where there are multiple sections waitlisted for the same course (if that's even possible)
                if (string.IsNullOrEmpty(dptpc.DptSectionsAssocMember) || waitlistItem.WaitCourseSection == dptpc.DptSectionsAssocMember)
                {
                    // while the student should only have 1 waitlist record per section, will have active statuses take priority in case of bad data.
                    if (!String.IsNullOrEmpty(waitlistItem.WaitStatus))
                    {
                        if ((await GetWaitlistStatusActionCodeAsync(waitlistItem.WaitStatus)) == "4")
                        {
                            status = Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister;
                        }
                        if ((await GetWaitlistStatusActionCodeAsync(waitlistItem.WaitStatus)) == "1")
                        {
                            if (status != Domain.Student.Entities.DegreePlans.WaitlistStatus.PermissionToRegister)
                            {
                                status = Domain.Student.Entities.DegreePlans.WaitlistStatus.Active;
                            }
                        }
                    }
                }
            }
            return status;
        }

        /// <summary>
        /// Retrieves waitlist information
        /// </summary>
        /// <returns>Waitlist information</returns>
        private async Task<ApplValcodes> GetWaitlistStatusesAsync()
        {
            var waitlistStatuses = await GetOrAddToCacheAsync<ApplValcodes>("WaitlistStatuses",
                async () =>
                {
                    ApplValcodes waitlistStatusesTable = await DataReader.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "WAIT.LIST.STATUSES");
                    if (waitlistStatusesTable == null)
                    {
                        // log this but don't throw exception because not all clients use wait lists.
                        var errorMessage = "Unable to access WAIT.LIST.STATUSES valcode table.";
                        logger.Info(errorMessage);
                        waitlistStatusesTable = new ApplValcodes() { ValsEntityAssociation = new List<ApplValcodesVals>() };
                    }
                    return waitlistStatusesTable;
                }, Level1CacheTimeoutValue);
            return waitlistStatuses;
        }


        /// <summary>
        /// Get the waitlist status action code for a given waitlist status code
        /// </summary>
        /// <param name="waitlistStatusCode">Waitlist status code</param>
        /// <returns>Waitlist status action code</returns>
        private async Task<string> GetWaitlistStatusActionCodeAsync(string waitlistStatusCode)
        {
            if (!String.IsNullOrEmpty(waitlistStatusCode))
            {
                var codeAssoc = (await GetWaitlistStatusesAsync()).ValsEntityAssociation.Where(v => v.ValInternalCodeAssocMember == waitlistStatusCode).FirstOrDefault();
                if (codeAssoc != null)
                {
                    return codeAssoc.ValActionCode1AssocMember;
                }
            }
            return null;
        }

    }
}
