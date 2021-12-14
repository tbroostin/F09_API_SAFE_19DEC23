// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.AnonymousGrading;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    /// <summary>
    /// Repository for actions related to course section preliminary anonymous grading
    /// </summary>

    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]

    public class PreliminaryAnonymousGradeRepository : StudentConfigurationRepository, IPreliminaryAnonymousGradeRepository
    {
        // Sets the maximum number of records to bulk read at one time
        readonly int readSize;

        public PreliminaryAnonymousGradeRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Use default cache timeout value
            CacheTimeout = Level1CacheTimeoutValue;

            // Use the bulk read size from API settings, or fall back to 5000
            this.readSize = ((apiSettings != null) && (apiSettings.BulkReadSize > 0)) ? apiSettings.BulkReadSize : 5000;
        }

        /// <summary>
        /// Get preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="primarySectionId">ID of the course section for which to retrieve preliminary anonymous grade information</param>
        /// <param name="crossListedSectionIds">IDs of any crosslisted course sections for which to retrieve preliminary anonymous grade information</param>
        /// <returns>Preliminary anonymous grade information for the specified course section</returns>
        /// <exception cref="ArgumentNullException">A course section ID is required when retrieving preliminary anonymous grade information.</exception>
        /// <exception cref="ConfigurationException">Academic record configuration from AC.DEFAULTS is null.</exception>
        /// <exception cref="ConfigurationException">Generate Random IDs field from ACPR is not set. In order to retrieve preliminary anonymous grade information, ACPR > Generate Random IDs must be set to either (S)ection or (T)erm.</exception>
        /// <exception cref="ColleagueException">An error occurred while building preliminary anonymous grades for course section: ACPR > Generate Random IDs is set to (T)erm but no STUDENT.TERMS records were found for section's associated STUDENT.COURSE.SEC / STUDENT.ACAD.CRED records.</exception>
        public async Task<SectionPreliminaryAnonymousGrading> GetPreliminaryAnonymousGradesBySectionIdAsync(string primarySectionId, 
            IEnumerable<string> crossListedSectionIds)
        {
            if (string.IsNullOrEmpty(primarySectionId))
            {
                throw new ArgumentNullException("primarySectionId", "A course section ID is required when retrieving preliminary anonymous grade information.");
            }

            // Read academic records configuration to determine how the institution generates random IDs
            AcademicRecordConfiguration config = await RetrieveAndValidateAcademicRecordConfiguration();

            // Gather primary section ID with provided crosslisted section IDs into a single list
            List<string> courseSectionIds = new List<string>() { primarySectionId };
            if (crossListedSectionIds != null)
            {
                courseSectionIds.AddRange(crossListedSectionIds.Where(cls => !string.IsNullOrEmpty(cls)));
                courseSectionIds = courseSectionIds.Distinct().ToList();
            }

            // Build list of STUDENT.COURSE.SEC records to be read based on the provided course section IDs (using the SCS.COURSE.SECTION pointer)
            List<string> studentCourseSecIds = new List<string>();
            for (int i = 0; i < courseSectionIds.Count(); i += readSize)
            {
                var subList = courseSectionIds.Skip(i).Take(readSize).ToArray();
                string[] subListStudentCourseSecIds = await DataReader.SelectAsync("STUDENT.COURSE.SEC", "WITH SCS.COURSE.SECTION = '?'", subList);
                studentCourseSecIds.AddRange(subListStudentCourseSecIds);
            }

            // Read STUDENT.COURSE.SEC records
            List<StudentCourseSec> studentCourseSecRecords = new List<StudentCourseSec>();
            studentCourseSecRecords = await BulkReadRecordWithLoggingAsync<StudentCourseSec>("STUDENT.COURSE.SEC", studentCourseSecIds.ToArray(), readSize, true, true);

            // Read PRELIM.STU.GRD.WORK records; this is a co-file of STUDENT.COURSE.SEC, so the same keys are used
            List<PrelimStuGrdWork> prelimStuGrdWorkRecords = new List<PrelimStuGrdWork>();
            prelimStuGrdWorkRecords = await BulkReadRecordWithLoggingAsync<PrelimStuGrdWork>("PRELIM.STU.GRD.WORK", studentCourseSecIds.ToArray(), readSize, true, true);

            // Read STUDENT.ACAD.CRED and STUDENT.TERMS records if institution generates random grading IDs by (T)erm
            List<StudentAcadCred> studentAcadCredRecords = new List<StudentAcadCred>();
            List<StudentTerms> studentTermsRecords = new List<StudentTerms>();
            if (config.AnonymousGradingType == AnonymousGradingType.Term)
            {
                List<string> studentAcadCredIds = studentCourseSecRecords.Select(scs => scs.ScsStudentAcadCred).ToList();
                studentAcadCredRecords = await BulkReadRecordWithLoggingAsync<StudentAcadCred>("STUDENT.ACAD.CRED", studentAcadCredIds.ToArray(), readSize, true, true);

                List<string> studentTermsIds = studentAcadCredRecords.Select(sac => sac.StcPersonId + "*" + sac.StcTerm + "*" + sac.StcAcadLevel).Distinct().ToList();
                if (!studentTermsIds.Any())
                {
                    string noStudentTermsMessage = string.Format("An error occurred while building preliminary anonymous grades for course section {0}: ACPR > Generate Random IDs is set to (T)erm but no STUDENT.TERMS records were found for section {0}'s associated STUDENT.COURSE.SEC / STUDENT.ACAD.CRED records.",
                                primarySectionId);
                    if (crossListedSectionIds != null && crossListedSectionIds.Any())
                    {
                        noStudentTermsMessage = string.Format("An error occurred while building preliminary anonymous grades for course section {0} and cross-listed sections {1}: ACPR > Generate Random IDs is set to (T)erm but no STUDENT.TERMS records were found for these sections' associated STUDENT.COURSE.SEC / STUDENT.ACAD.CRED records.",
                                primarySectionId,
                                string.Join(",", crossListedSectionIds));
                    }
                    logger.Error(noStudentTermsMessage);
                    throw new ColleagueException(noStudentTermsMessage);
                }
                studentTermsRecords = await BulkReadRecordWithLoggingAsync<StudentTerms>("STUDENT.TERMS", studentTermsIds.ToArray(), readSize, true, true);
            }

            // Build preliminary anonymous grade information from retrieved data
            SectionPreliminaryAnonymousGrading preliminaryAnonymousGrades = BuildPreliminaryAnonymousGrades(studentCourseSecRecords, prelimStuGrdWorkRecords, 
                studentAcadCredRecords, studentTermsRecords, config.AnonymousGradingType, primarySectionId);

            return preliminaryAnonymousGrades;
        }

        /// <summary>
        /// Update preliminary anonymous grade information for the specified course section
        /// </summary>
        /// <param name="sectionId">ID of the course section for which to process preliminary anonymous grade updates</param>
        /// <param name="preliminaryAnonymousGrades">Preliminary anonymous grade updates to process</param>
        /// <returns>Preliminary anonymous grade update results</returns>
        /// <exception cref="ArgumentNullException">A course section ID is required when updating preliminary anonymous grade information.</exception>
        /// <exception cref="ArgumentNullException">At least one preliminary anonymous grade is required when updating preliminary anonymous grade information.</exception>
        /// <exception cref="TimeoutException">Colleague transaction error occurred while updating preliminary anonymous grades for the course section.</exception>
        /// <exception cref="ColleagueTransactionException">Colleague transaction request returned a null response.</exception>
        /// <exception cref="ColleagueException">An error occurred while updating preliminary anonymous grades for the course section.</exception>
        /// <exception cref="ColleagueException">UPDATE.PRELIM.ANON.GRADES request for course section returned null processing results.</exception>
        /// <exception cref="ColleagueException">UPDATE.PRELIM.ANON.GRADES request for course section received a different number of results than the number that was sent.</exception>
        public async Task<IEnumerable<PreliminaryAnonymousGradeUpdateResult>> UpdatePreliminaryAnonymousGradesBySectionIdAsync(string sectionId, IEnumerable<PreliminaryAnonymousGrade> preliminaryAnonymousGrades)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "A course section ID is required when updating preliminary anonymous grade information.");
            }
            if (preliminaryAnonymousGrades == null || !preliminaryAnonymousGrades.Any())
            {
                throw new ArgumentNullException("preliminaryAnonymousGrades", "At least one preliminary anonymous grade is required when updating preliminary anonymous grade information.");
            }

            // Build Colleague Transaction request to process updates
            UpdatePreliminaryAnonymousGradesRequest request = new UpdatePreliminaryAnonymousGradesRequest();
            request.CourseSectionsId = sectionId;
            request.GradesToProcess = new List<GradesToProcess>();
            foreach(var grade in preliminaryAnonymousGrades)
            {
                request.GradesToProcess.Add(new GradesToProcess()
                {
                    InGradesId = grade.FinalGradeId,
                    InExpirationDate = grade.FinalGradeExpirationDate,
                    InStudentCourseSecId = grade.StudentCourseSectionId
                });
            }

            // Execute the Colleague Transaction
            UpdatePreliminaryAnonymousGradesResponse response = null;
            try
            {
                response = await transactionInvoker.ExecuteAsync<UpdatePreliminaryAnonymousGradesRequest, UpdatePreliminaryAnonymousGradesResponse>(request);
            }
            catch (ColleagueTransactionException ce)
            {
                string message = string.Format("Colleague transaction error occurred while updating preliminary anonymous grades for course section {0}.", request.CourseSectionsId);
                logger.Error(ce, message);
                if (ce.DmiErrorSubset != null && ce.DmiErrorSubset.ErrorCategory == "SECURITY" && ce.DmiErrorSubset.ErrorCode == "00002")
                {
                    throw new TimeoutException("Security exception occurred; session is expired");
                }
                else
                {
                    throw;
                }
            }

            // Process the Colleague Transaction response
            if (response == null)
            {
                throw new ColleagueTransactionException(string.Format("UPDATE.PRELIM.ANON.GRADES request for course section {0} returned a null response.", request.CourseSectionsId));
            }
            // Check for any errors from the response
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                string message = string.Format("An error occurred while updating preliminary anonymous grades for course section {0}: {1}", 
                    request.CourseSectionsId,
                    response.ErrorMessage);
                logger.Error(message);
                throw new ColleagueException(message);
            }
            // The results in the response cannot be null
            if (response.ProcessingResults == null)
            {
                string message = string.Format("UPDATE.PRELIM.ANON.GRADES request for course section {0} returned null processing results.",
                    request.CourseSectionsId);
                logger.Error(message);
                throw new ColleagueException(message);
            }
            // The number of results in the response should match the number of updates in the request
            if (response.ProcessingResults.Count != request.GradesToProcess.Count)
            {
                string message = string.Format("UPDATE.PRELIM.ANON.GRADES request for course section {0} sent {1} grades to process but returned {2} results.",
                    request.CourseSectionsId,
                    request.GradesToProcess.Count,
                    response.ProcessingResults.Count);
                logger.Error(message);
                throw new ColleagueException(message);
            }
            // Build results from Colleague Transaction response
            List<PreliminaryAnonymousGradeUpdateResult> results = new List<PreliminaryAnonymousGradeUpdateResult>();
            foreach(var result in response.ProcessingResults)
            {
                try
                {
                    var status = result.OutUpdateSuccessful ? PreliminaryAnonymousGradeUpdateStatus.Success : PreliminaryAnonymousGradeUpdateStatus.Failure;
                    var resultEntity = new PreliminaryAnonymousGradeUpdateResult(result.OutStudentCourseSecId, status, result.OutMessage);
                    results.Add(resultEntity);
                }
                catch (Exception ex)
                {
                    string message = string.Format("An error occurred while processing one of the preliminary anonymous grade update results for course section {0}.",
                        request.CourseSectionsId);
                    logger.Error(ex, message);
                }
            }

            // Return the results
            return results;
        }


        /// <summary>
        /// Builds preliminary anonymous grade data from relevant Colleague records
        /// </summary>
        /// <param name="studentCourseSecRecords">STUDENT.COURSE.SEC records</param>
        /// <param name="prelimStuGrdWorkRecords">PRELIM.STU.GRD.WORK records</param>
        /// <param name="studentAcadCredRecords">STUDENT.ACAD.CRED records</param>
        /// <param name="studentTermsRecords">STUDENT.TERMS records</param>
        /// <param name="anonymousGradingType">Generate Random IDs setting from ACPR</param>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        private SectionPreliminaryAnonymousGrading BuildPreliminaryAnonymousGrades(IEnumerable<StudentCourseSec> studentCourseSecRecords,
            IEnumerable<PrelimStuGrdWork> prelimStuGrdWorkRecords, IEnumerable<StudentAcadCred> studentAcadCredRecords, IEnumerable<StudentTerms> studentTermsRecords,
            AnonymousGradingType anonymousGradingType, string sectionId)
        {
            SectionPreliminaryAnonymousGrading sectionPreliminaryAnonymousGrading = new SectionPreliminaryAnonymousGrading(sectionId);
            StudentAcadCred stc = null;
            PrelimStuGrdWork psgw = null;
            StudentTerms sttr = null;

            switch (anonymousGradingType)
            {
                // ACPR > Random Grading IDs = (S)ection
                case AnonymousGradingType.Section:
                    foreach (var scs in studentCourseSecRecords)
                    {
                        try
                        {
                            psgw = prelimStuGrdWorkRecords.Where(psgwr => psgwr != null && psgwr.Recordkey == scs.Recordkey).FirstOrDefault();
                            string finalGradeId = psgw != null ? psgw.PsgFinalGrade : string.Empty;
                            DateTime? finalGradeExpirationDate = psgw != null ? psgw.PsgFinalGradeExpireDate : null;
                            if (!scs.ScsRandomId.HasValue)
                            {
                                throw new ColleagueException(string.Format("Random grading IDs are generated by section but STUDENT.COURSE.SEC record {0} does not have a random grading ID.",
                                    scs.Recordkey));
                            }
                            PreliminaryAnonymousGrade pag = new PreliminaryAnonymousGrade(scs.ScsRandomId.ToString(), finalGradeId, scs.ScsCourseSection, scs.Recordkey, finalGradeExpirationDate);
                            if (pag.CourseSectionId == sectionId)
                            {
                                sectionPreliminaryAnonymousGrading.AddAnonymousGradeForSection(pag);
                            }
                            else
                            {
                                sectionPreliminaryAnonymousGrading.AddAnonymousGradeForCrosslistedSection(pag);
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("While building preliminary anonymous grades for course section {0}, an error occurred while processing STUDENT.COURSE.SEC record {1}: {2}",
                                sectionId,
                                scs.Recordkey,
                                ex.Message);
                            logger.Error(ex, message);
                            AnonymousGradeError error = new AnonymousGradeError(scs.Recordkey,
                                stc != null ? stc.Recordkey : string.Empty,
                                psgw != null ? psgw.Recordkey : string.Empty,
                                sttr != null ? sttr.Recordkey : string.Empty,
                                ex.Message);
                            sectionPreliminaryAnonymousGrading.AddError(error);
                        }
                    }
                    break;
                // ACPR > Random Grading IDs = (T)erm
                case AnonymousGradingType.Term:
                    foreach (var scs in studentCourseSecRecords)
                    {
                        try { 
                            stc = studentAcadCredRecords.Where(stacr => stacr != null && scs.ScsStudentAcadCred == stacr.Recordkey).FirstOrDefault();
                            if (stc == null)
                            {
                                    throw new ColleagueException(string.Format("STUDENT.ACAD.CRED record {0} associated with STUDENT.COURSE.SEC record {1} could not be found.",
                                        scs.ScsStudentAcadCred,
                                        scs.Recordkey));
                            }
                            psgw = prelimStuGrdWorkRecords.Where(psgwr => psgwr != null && psgwr.Recordkey == scs.Recordkey).FirstOrDefault();
                            string finalGradeId = psgw != null ? psgw.PsgFinalGrade : string.Empty;
                            DateTime? finalGradeExpirationDate = psgw != null ? psgw.PsgFinalGradeExpireDate : null;
                            sttr = studentTermsRecords.Where(sttrr => sttrr != null && sttrr.Recordkey == stc.StcPersonId + "*" + stc.StcTerm + "*" + stc.StcAcadLevel).FirstOrDefault();
                            if (sttr == null)
                            {
                                throw new ColleagueException(string.Format("STUDENT.TERMS record {0} associated with STUDENT.ACAD.CRED record {1} / STUDENT.COURSE.SEC record {2} could not be found.",
                                    stc.StcPersonId + "*" + stc.StcTerm + "*" + stc.StcAcadLevel,
                                    stc.Recordkey,
                                    scs.Recordkey));
                            }
                            if (!sttr.SttrRandomId.HasValue)
                            {
                                throw new ColleagueException(string.Format("Random grading IDs are generated by term but STUDENT.TERMS record {0} does not have a random grading ID.",
                                    sttr.Recordkey));
                            }
                            PreliminaryAnonymousGrade pag = new PreliminaryAnonymousGrade(sttr.SttrRandomId.ToString(), finalGradeId, scs.ScsCourseSection, scs.Recordkey, finalGradeExpirationDate);
                            if (pag.CourseSectionId == sectionId)
                            {
                                sectionPreliminaryAnonymousGrading.AddAnonymousGradeForSection(pag);
                            }
                            else
                            {
                                sectionPreliminaryAnonymousGrading.AddAnonymousGradeForCrosslistedSection(pag);
                            }
                        }
                        catch (Exception ex)
                        {
                            string message = string.Format("While building preliminary anonymous grades for course section {0}, an error occurred while processing STUDENT.COURSE.SEC record {1} and STUDENT.TERMS record {2}: {3}",
                                sectionId,
                                scs.Recordkey,
                                sttr.Recordkey,
                                ex.Message);
                            logger.Error(ex, message);
                            AnonymousGradeError error = new AnonymousGradeError(scs.Recordkey,
                                stc != null ? stc.Recordkey : string.Empty,
                                psgw != null ? psgw.Recordkey : string.Empty,
                                sttr != null ? sttr.Recordkey : string.Empty,
                                ex.Message);
                            sectionPreliminaryAnonymousGrading.AddError(error);
                        }
                    }
                    break;
                default:
                    break;
            }

            return sectionPreliminaryAnonymousGrading;
        }

        /// <summary>
        /// Retrieves and verifies academic record configuration information
        /// </summary>
        /// <returns>Academic record configuration</returns>
        /// <exception cref="ConfigurationException">Academic record configuration from AC.DEFAULTS is null.</exception>
        /// <exception cref="ConfigurationException">Generate Random IDs field from ACPR is not set. In order to retrieve preliminary anonymous grade information, ACPR > Generate Random IDs must be set to either (S)ection or (T)erm.</exception>
        private async Task<AcademicRecordConfiguration> RetrieveAndValidateAcademicRecordConfiguration()
        {
            // Read academic records configuration to determine how the institution generates random IDs
            var config = await GetAcademicRecordConfigurationAsync();
            if (config.AnonymousGradingType == AnonymousGradingType.None)
            {
                string acadRecordConfigMessage = "Generate Random IDs field from ACPR is blank. In order to retrieve preliminary anonymous grade information, ACPR > Generate Random IDs must be set to either (S)ection or (T)erm.";
                logger.Error(acadRecordConfigMessage);
                throw new ConfigurationException(acadRecordConfigMessage);
            }
            return config;
        }
    }
}
