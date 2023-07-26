// Copyright 2015-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Domain.Student.Entities;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentPetitionRepository : BaseColleagueRepository, IStudentPetitionRepository
    {
        private string colleagueTimeZone;
        public StudentPetitionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Not cached
            CacheTimeout = 0;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// get petitions & faculty consents for a given student asynchronously
        /// </summary>
        /// <param name="studentId"></param>
        /// <returns><IEnumerable<StudentPetition>></returns>
        public async Task<IEnumerable<StudentPetition>> GetStudentPetitionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                var message = "Student Id must be provided";
                logger.Info(message);
                throw new ArgumentNullException(message);
            }
            try
            {
                var criteria = "WITH STPE.STUDENT EQ '" + studentId + "'";
                Collection<StudentPetitions> petitionData = await DataReader.BulkReadRecordAsync<StudentPetitions>(criteria);

                string[] commentIds = new string[] { };
                if (petitionData != null && petitionData.Count() > 0)
                {
                    commentIds = petitionData.Where(c => c.StpeStuPetitionCmntsId != null)
                        .SelectMany(c => c.StpeStuPetitionCmntsId)
                        .Distinct().ToArray();
                }
                Collection<StuPetitionCmnts> commentData = await DataReader.BulkReadRecordAsync<StuPetitionCmnts>(commentIds);
                var studentPetitionsEntity = BuildStudentPetitions(studentId, petitionData, commentData);
                return studentPetitionsEntity;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }

            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// get overload petitions for a given student asynchronously
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>A collection of <see cref="StudentOverloadPetition">StudentOverloadPetition</see> object.</returns>
        public async Task<IEnumerable<StudentOverloadPetition>> GetStudentOverloadPetitionsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("StudentId", "studentId cannot be null or empty.");
            }
            try
            {
                var criteria = "WITH STPE.STUDENT EQ '" + studentId + "'";
                Collection<StudentPetitions> petitionData = await DataReader.BulkReadRecordAsync<StudentPetitions>(criteria);
                var studentPetitionsEntity = BuildStudentOverloadPetitions(studentId, petitionData);
                return studentPetitionsEntity;
            }
            catch (ColleagueSessionExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                string error = "Exception occurred while trying to retrieve student overload petitions";
                logger.Error(ex, error);
                throw new ApplicationException(error);
            }
        }

        private List<StudentPetition> BuildStudentPetitions(string studentId, Collection<StudentPetitions> petitionData, Collection<StuPetitionCmnts> commentData)
        {
            List<StudentPetition> studentPetitions = new List<StudentPetition>();
            if (petitionData != null && petitionData.Any())
            {
                foreach (StudentPetitions studentPetition in petitionData)
                {
                    if (studentId != studentPetition.StpeStudent)
                    {
                        LogDataError("StudentPetitions", studentPetition.Recordkey, studentPetition, null, string.Format("Student Petition retrieved is not for the provided studentId {0}", studentId));
                    }
                    else
                    {
                        var petitionId = studentPetition.Recordkey;
                        if (studentPetition.PetitionsEntityAssociation != null)
                        {
                            foreach (var petition in studentPetition.PetitionsEntityAssociation)
                            {
                                try
                                {
                                    var courseId = petition.StpeCoursesAssocMember;
                                    string facConsentCmnts = null;
                                    string stuPetitionCmnts = null;
                                    var sectionId = petition.StpeSectionAssocMember;
                                    StuPetitionCmnts comments = null;
                                    if (null != commentData)
                                        comments = commentData.FirstOrDefault(c => c.Recordkey == petition.StpeStuPetitionCmntsIdAssocMember);
                                    if (null != comments)
                                    {
                                        facConsentCmnts = (string.IsNullOrEmpty(comments.StpcConsentComments)) ? null : comments.StpcConsentComments.Replace(DmiString._VM, '\n');
                                        stuPetitionCmnts = (string.IsNullOrEmpty(comments.StpcPetitionComments)) ? null : comments.StpcPetitionComments.Replace(DmiString._VM, '\n');
                                    }

                                    if (!string.IsNullOrEmpty(petition.StpeFacultyConsentAssocMember))
                                    {
                                        var facultyConsent = new StudentPetition(id: petitionId, courseId: courseId, sectionId: sectionId, studentId: studentId, type: StudentPetitionType.FacultyConsent, statusCode: petition.StpeFacultyConsentAssocMember);
                                        DateTimeOffset dateTimeChanged = petition.StpeFacultyConsentTimeAssocMember.ToPointInTimeDateTimeOffset(
                                                petition.StpeFacultyConsentDateAssocMember, colleagueTimeZone) ?? new DateTimeOffset();
                                        facultyConsent.DateTimeChanged = dateTimeChanged;
                                        facultyConsent.Comment = facConsentCmnts;
                                        facultyConsent.ReasonCode = petition.StpeConsentReasonCodeAssocMember;
                                        facultyConsent.TermCode = studentPetition.StpeTerm;
                                        facultyConsent.SetBy = petition.StpeFacultyConsentSetByAssocMember;
                                        facultyConsent.UpdatedBy = studentPetition.StudentPetitionsChgopr;
                                        facultyConsent.StartDate = studentPetition.StpeStartDate;
                                        facultyConsent.EndDate = studentPetition.StpeEndDate;
                                        studentPetitions.Add(facultyConsent);
                                    }

                                    if (!string.IsNullOrEmpty(petition.StpePetitionStatusAssocMember))
                                    {
                                        var stuPetition = new StudentPetition(id: petitionId, courseId: courseId, sectionId: sectionId, studentId: studentId, type: StudentPetitionType.StudentPetition, statusCode: petition.StpePetitionStatusAssocMember);
                                        DateTimeOffset dateTimeChanged = petition.StpePetitionStatusTimeAssocMember.ToPointInTimeDateTimeOffset(
                                            petition.StpePetitionStatusDateAssocMember, colleagueTimeZone) ?? new DateTimeOffset();
                                        stuPetition.DateTimeChanged = dateTimeChanged;
                                        stuPetition.Comment = stuPetitionCmnts;
                                        stuPetition.ReasonCode = petition.StpePetitionReasonCodeAssocMember;
                                        stuPetition.TermCode = studentPetition.StpeTerm;
                                        stuPetition.SetBy = petition.StpePetitionStatusSetByAssocMember;
                                        stuPetition.UpdatedBy = studentPetition.StudentPetitionsChgopr;
                                        stuPetition.StartDate = studentPetition.StpeStartDate;
                                        stuPetition.EndDate = studentPetition.StpeEndDate;
                                        studentPetitions.Add(stuPetition);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogDataError("StudentPetitions", studentPetition.Recordkey, studentPetition, ex);
                                }
                            }
                        }
                    }
                }
            }
            return studentPetitions;
        }

        /// <summary>
        /// Build student overload petitions
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="petitionData">Petition Data</param>
        /// <returns>StudentOverloadPetitions</returns>
        private List<StudentOverloadPetition> BuildStudentOverloadPetitions(string studentId, Collection<StudentPetitions> petitionData)
        {
            List<StudentOverloadPetition> studentOverloadPetitions = new List<StudentOverloadPetition>();
            if (petitionData != null && petitionData.Any())
            {
                foreach (StudentPetitions studentPetition in petitionData)
                {
                    if (studentId != studentPetition.StpeStudent)
                    {
                        LogDataError("StudentPetitions", studentPetition.Recordkey, studentPetition, null, string.Format("Student Overload Petition retrieved is not for the provided studentId {0}", studentId));
                    }
                    else
                    {
                        var petitionId = studentPetition.Recordkey;
                        try
                        {
                            var overloadPetition = new StudentOverloadPetition(petitionId, studentId, studentPetition.StpeOverloadPetition);
                            DateTimeOffset dateTimeChanged = studentPetition.StudentPetitionsChgtime.ToPointInTimeDateTimeOffset(studentPetition.StudentPetitionsChgdate, colleagueTimeZone) ?? new DateTimeOffset();
                            overloadPetition.DateTimeChanged = dateTimeChanged;
                            overloadPetition.TermCode = studentPetition.StpeTerm;
                            studentOverloadPetitions.Add(overloadPetition);
                        }
                        catch (Exception ex)
                        {
                            LogDataError("StudentOverloadPetitions", studentPetition.Recordkey, studentPetition, ex);
                        }
                    }
                }
            }
            return studentOverloadPetitions;
        }
    }
}


