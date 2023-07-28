// Copyright 2020-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities.TransferWork;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
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
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentTransferWorkRepository : BaseColleagueRepository, IStudentTransferWorkRepository
    {
        /// <summary>
        /// Constructor for StudentTransferWorkRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        /// <param name="apiSettings"></param>
        public StudentTransferWorkRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Using level 1 cache time out value for data that rarely changes.
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Get the Equivalency work for a student that isn't associated with an external institution.
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Returns a list of non institutional equivalencies</returns>
        public async Task<IEnumerable<Equivalency>> GetStudentNonInstitutionalEquivalencyWorkAsync(string studentId)
        {
            // Get the non course equivalency work that are not associated with another institution, 
            // for example Test Scores and other Non Course Work.
            return await GetStudentEquivalenciesAsync(studentId, "");
        }

        /// <summary>
        /// Get student transfer equivalency work for a student
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <returns>Returns a list of transfer and non course equivalency work for a student.</returns>
        public async Task<IEnumerable<TransferEquivalencies>> GetStudentTransferWorkAsync(string studentId)
        {
            List<TransferEquivalencies> transferEquivalenciesList = new List<TransferEquivalencies>();
            try
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    throw new ArgumentNullException(studentId, "Student Id must be specified");
                }
                // Get Person Record
                Base.DataContracts.Person person = await DataReader.ReadRecordAsync<Base.DataContracts.Person>(studentId);

                // Get Person Institutions Attended
                if (person != null && person.PersonInstitutionsAttend != null && person.PersonInstitutionsAttend.Any())
                {
                    List<string> personInstitutionsAttend = person.PersonInstitutionsAttend;
                    foreach (var institution in personInstitutionsAttend)
                    {
                        transferEquivalenciesList.Add(await GetStudentTransferEquivalenciesAsync(studentId, institution));
                    }
                }
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Session has expired while retrieving student transfer and non course equivelancy work";
                logger.Error(tex, message);
                throw;
            }
            catch (Exception ex)
            {
                var message = "Exception occurred while trying to retrieve student transfer and non course equivelancy work";
                logger.Error(message);
                throw;
            }

            return transferEquivalenciesList;
        }

        /// <summary>
        /// Private function to retrieve the transfer course and non course equivalency work for a student and institution (which can be empty
        /// to return test and other non course work not associated specifically with an external institution).
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="institutionId">External Institutions ID</param>
        /// <returns>Return course and non course equivalency work for a student.</returns>
        private async Task<TransferEquivalencies> GetStudentTransferEquivalenciesAsync(string studentId, string institutionId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId is required.");
            }

            List<Equivalency> equivalencies = await GetStudentEquivalenciesAsync(studentId, institutionId);

            TransferEquivalencies institutionTransferWork = new TransferEquivalencies(institutionId, equivalencies);
            return institutionTransferWork;
        }

        /// <summary>
        /// Get the student equivalencies
        /// </summary>
        /// <param name="studentId">Student ID</param>
        /// <param name="institutionId">Institution ID (optional)</param>
        /// <returns>List of Student Equivalencies</returns>
        private async Task<List<Equivalency>> GetStudentEquivalenciesAsync(string studentId, string institutionId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId is required.");
            }

            var request = new GetStudentInstTxfrWorkRequest()
            {
                PersonIn = studentId,
                InstitutionIn = institutionId
            };
            var response = await transactionInvoker.ExecuteAsync<GetStudentInstTxfrWorkRequest, GetStudentInstTxfrWorkResponse>(request);

            List<Equivalency> equivalencies = new List<Domain.Student.Entities.TransferWork.Equivalency>();

            foreach (var item in response.TransferWork)
            {
                var equivalency = new Domain.Student.Entities.TransferWork.Equivalency();

                // Add the Academic Program to the Equivalency
                var academicPrograms = string.IsNullOrEmpty(item.AcademicPrograms) ? new List<string>() : item.AcademicPrograms.Split(DmiString._SM).ToList();
                if (academicPrograms != null && academicPrograms.Any())
                {
                    foreach (var academicProgram in academicPrograms)
                    {
                        equivalency.AddAcademicProgram(academicProgram);
                    }
                }

                // Process the sub-valued associated lists so that we can process them and build the proper models.
                var txfrIsCourseFlag = item.TxfrIsCourseFlag.Split(DmiString._SM).ToList();
                var txfrCourses = string.IsNullOrEmpty(item.TxfrCourses) ? new List<string>() : item.TxfrCourses.Split(DmiString._SM).ToList();
                var txfrTitles = string.IsNullOrEmpty(item.TxfrTitles) ? new List<string>() : item.TxfrTitles.Split(DmiString._SM).ToList();
                var txfrCredits = string.IsNullOrEmpty(item.TxfrCredits) ? new List<string>() : item.TxfrCredits.Split(DmiString._SM).ToList();
                var txfrScores = string.IsNullOrEmpty(item.TxfrScores) ? new List<string>() : item.TxfrScores.Split(DmiString._SM).ToList();
                var txfrGradeIds = string.IsNullOrEmpty(item.TxfrGrades) ? new List<string>() : item.TxfrGrades.Split(DmiString._SM).ToList();
                var txfrEndDates = string.IsNullOrEmpty(item.TxfrCompletionDates) ? new List<string>() : item.TxfrCompletionDates.Split(DmiString._SM).ToList();
                var txfrStatuses = string.IsNullOrEmpty(item.TxfrStatuses) ? new List<string>() : item.TxfrStatuses.Split(DmiString._SM).ToList();

                for (int i = 0; i < txfrIsCourseFlag.Count(); i++)
                {
                    try
                    {
                        var isCourseFlag = txfrIsCourseFlag[i];
                        var course = txfrCourses.Count() > i ? txfrCourses[i] : "";
                        var title = txfrTitles.Count() > i ? txfrTitles[i] : "";
                        var credits = txfrCredits.Count() > i ? Convert.ToDecimal(txfrCredits[i]) : 0;
                        var score = txfrScores.Count() > i ? Convert.ToDecimal(txfrScores[i]) : 0;
                        var gradeId = txfrGradeIds.Count() > i ? txfrGradeIds[i] : "";
                        var endDate = txfrEndDates.Count() > i && !string.IsNullOrEmpty(txfrEndDates[i]) ? (DateTime?)DmiString.PickDateToDateTime(int.Parse(txfrEndDates[i])) : null;
                        var status = txfrStatuses.Count() > i ? txfrStatuses[i] : "";

                        if (isCourseFlag.Equals("Y"))
                        {
                            equivalency.AddExternalCourseWork(
                                new ExternalCourseWork(
                                    course,
                                    title,
                                    credits,
                                    gradeId,
                                    endDate)
                                );
                        }
                        else
                        {
                            equivalency.AddExternalNonCourseWork(
                                new ExternalNonCourseWork(
                                    course,
                                    title,
                                    gradeId,
                                    score,
                                    endDate,
                                    status)
                                );
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Unable to process transfer course work or non course work for equivalencies.");
                        throw;
                    }
                }

                var equivIsCourseFlag = item.EquivIsCourseFlag.Split(DmiString._SM).ToList();
                var equivCourseIds = item.EquivCourseId != null ? item.EquivCourseId.Split(DmiString._SM).ToList() : new List<string>();
                var equivCourseNames = item.EquivCourseNames != null ? item.EquivCourseNames.Split(DmiString._SM).ToList() : new List<string>();
                var equivCourseTitles = item.EquivCourseTitles != null ? item.EquivCourseTitles.Split(DmiString._SM).ToList() : new List<string>();
                var equivSubjects = item.EquivSubjects != null ? item.EquivSubjects.Split(DmiString._SM).ToList() : new List<string>();
                var equivCourseLevels = item.EquivCourseLevels != null ? item.EquivCourseLevels.Split(DmiString._SM).ToList() : new List<string>();
                var equivCredits = item.EquivCredits != null ? item.EquivCredits.Split(DmiString._SM).ToList() : new List<string>();
                var equivGradeIds = item.EquivGrades != null ? item.EquivGrades.Split(DmiString._SM).ToList() : new List<string>();
                var equivAcadLevelIds = item.EquivAcadLevelIds != null ? item.EquivAcadLevelIds.Split(DmiString._SM).ToList() : new List<string>();
                var equivCreditTypes = item.EquivCreditTypes != null ? item.EquivCreditTypes.Split(DmiString._SM).ToList() : new List<string>();
                var equivCreditStatuses = item.EquivStatuses != null ? item.EquivStatuses.Split(DmiString._SM).ToList() : new List<string>();
                var equivDepartments = item.EquivDepartments != null ? item.EquivDepartments.Split(DmiString._SM).ToList() : new List<string>();

                for (int i = 0; i < equivIsCourseFlag.Count(); i++)
                {
                    try
                    {
                        var isCourseFlag = equivIsCourseFlag[i];
                        var courseId = equivCourseIds.Count() > i ? equivCourseIds[i] : "";
                        var name = equivCourseNames.Count() > i ? equivCourseNames[i] : "";
                        var title = equivCourseTitles.Count() > i ? equivCourseTitles[i] : "";
                        var credits = equivCredits.Count() > i ? string.IsNullOrEmpty(equivCredits[i]) ? 0 : Convert.ToDecimal(equivCredits[i]) : 0;
                        var gradeId = equivGradeIds.Count() > i ? equivGradeIds[i] : "";
                        var acadLevelId = equivAcadLevelIds.Count() > i ? equivAcadLevelIds[i] : "";
                        var creditType = equivCreditTypes.Count() > i ? equivCreditTypes[i] : "";
                        var creditStatus = equivCreditStatuses.Count() > i ? equivCreditStatuses[i] : "";

                        // There may be more than one department - these are sub-sub value mark delimited from the CTX.
                        var departments = equivDepartments.Count() > i ? equivDepartments[i].Split(DmiString._TM).ToList() : new List<string>();

                        if (isCourseFlag.Equals("Y"))
                        {
                            equivalency.AddEquivalentCourseCredit(
                                new EquivalentCoursCredit(
                                    courseId,
                                    name,
                                    title,
                                    credits,
                                    gradeId,
                                    acadLevelId,
                                    creditType,
                                    creditStatus)
                                );
                        }
                        else
                        {
                            var subject = equivSubjects.Count() > i ? equivSubjects[i] : "";
                            var courseLevel = equivCourseLevels.Count() > i ? equivCourseLevels[i] : "";

                            equivalency.AddEquivalentGeneralCredit(
                                new EquivalentGeneralCredit(
                                    name,
                                    title,
                                    subject,
                                    courseLevel,
                                    credits,
                                    acadLevelId,
                                    creditType,
                                    creditStatus,
                                    departments)
                                );
                        }

                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Unable to process institutional equivalency course or general credits.");
                        throw;
                    }
                }

                equivalencies.Add(equivalency);
            }
            return equivalencies;
        }
    }
}
