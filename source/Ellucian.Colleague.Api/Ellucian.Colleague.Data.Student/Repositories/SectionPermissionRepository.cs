// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Web.Dependency;
using Ellucian.Web.Cache;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.Exceptions;
using slf4net;
using Ellucian.Web.Http.Configuration;
using System.Collections.ObjectModel;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class SectionPermissionRepository : BaseColleagueRepository, ISectionPermissionRepository
    {
        private string colleagueTimeZone;
        public SectionPermissionRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Not cached
            CacheTimeout = 0;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        public async Task<SectionPermission> GetSectionPermissionAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException(sectionId);
            }
            var criteria = "WITH STPE.SECTION EQ '" + sectionId + "'";
            var petitionData = await DataReader.BulkReadRecordAsync<StudentPetitions>(criteria);

            string[] commentIds = new string[] { };
            if (petitionData != null && petitionData.Count() > 0)
            {
                commentIds = petitionData
                    .SelectMany(c => c.PetitionsEntityAssociation)
                    .Where(c => c.StpeStuPetitionCmntsIdAssocMember != null)
                    .Select(c => c.StpeStuPetitionCmntsIdAssocMember).Distinct().ToArray();
            }
            var commentData = await DataReader.BulkReadRecordAsync<StuPetitionCmnts>(commentIds);
            var sectionPermissionEntity = BuildSectionPermission(sectionId, petitionData, commentData);
            return sectionPermissionEntity;
        }



        private SectionPermission BuildSectionPermission(string sectionId, Collection<StudentPetitions> petitionData, Collection<StuPetitionCmnts> commentData)
        {
            SectionPermission sectionEntity = new SectionPermission(sectionId);

            if (petitionData == null)
                return sectionEntity;

            foreach (var studentPetition in petitionData)
            {
                var studentId = studentPetition.StpeStudent;
                var petitionId = studentPetition.Recordkey;


                foreach (var petition in studentPetition.PetitionsEntityAssociation)
                {
                    var courseId = petition.StpeCoursesAssocMember;
                    string facConsentCmnts = null;
                    string stuPetitionCmnts = null;

                    if (petition.StpeSectionAssocMember == sectionId)
                    {
                        StuPetitionCmnts comments = null;
                        if (commentData != null)
                            comments = commentData.FirstOrDefault(c => c.Recordkey == petition.StpeStuPetitionCmntsIdAssocMember);

                        if (null != comments)
                        {
                            facConsentCmnts = (string.IsNullOrEmpty(comments.StpcConsentComments)) ? null : comments.StpcConsentComments.Replace(Convert.ToChar(DynamicArray.VM), '\n');
                            stuPetitionCmnts = (string.IsNullOrEmpty(comments.StpcPetitionComments)) ? null : comments.StpcPetitionComments.Replace(Convert.ToChar(DynamicArray.VM), '\n');
                        }
                        if (!string.IsNullOrEmpty(petition.StpeFacultyConsentAssocMember))
                        {
                            // If there is a part of the student petition that cannot be loaded (duplicate or soemthing) load the rest.
                            // This might mean that one petition will generate multiple errors but so be it.
                            try
                            {
                                var facultyConsent = new StudentPetition(id: petitionId, courseId: courseId, sectionId: sectionId, studentId: studentId, type: StudentPetitionType.FacultyConsent, statusCode: petition.StpeFacultyConsentAssocMember);
                                DateTimeOffset dateTimeChanged = petition.StpeFacultyConsentTimeAssocMember.ToPointInTimeDateTimeOffset(
                                    petition.StpeFacultyConsentDateAssocMember, colleagueTimeZone) ?? new DateTimeOffset(); ;
                                facultyConsent.ReasonCode = petition.StpeConsentReasonCodeAssocMember;
                                facultyConsent.TermCode = studentPetition.StpeTerm;
                                facultyConsent.Comment = facConsentCmnts;
                                facultyConsent.DateTimeChanged = dateTimeChanged;
                                facultyConsent.SetBy = petition.StpeFacultyConsentSetByAssocMember;
                                facultyConsent.UpdatedBy = studentPetition.StudentPetitionsChgopr;
                                facultyConsent.StartDate = studentPetition.StpeStartDate;
                                facultyConsent.EndDate = studentPetition.StpeEndDate;
                                sectionEntity.AddFacultyConsent(facultyConsent);
                            }
                            catch (Exception ex)
                            {
                                LogDataError("StudentPetitions", studentPetition.Recordkey, studentPetition, ex);
                            }
                        }
                        if (!string.IsNullOrEmpty(petition.StpePetitionStatusAssocMember))
                        {
                            try
                            {
                                var stuPetition = new StudentPetition(id: petitionId, courseId: courseId, sectionId: sectionId, studentId: studentId, type: StudentPetitionType.StudentPetition, statusCode: petition.StpePetitionStatusAssocMember);
                                DateTimeOffset dateTimeChanged = petition.StpePetitionStatusTimeAssocMember.ToPointInTimeDateTimeOffset(
                                    petition.StpePetitionStatusDateAssocMember, colleagueTimeZone) ?? new DateTimeOffset();
                                stuPetition.DateTimeChanged = dateTimeChanged;
                                stuPetition.Comment = stuPetitionCmnts;
                                stuPetition.TermCode = studentPetition.StpeTerm;
                                stuPetition.ReasonCode = petition.StpePetitionReasonCodeAssocMember;
                                stuPetition.SetBy = petition.StpePetitionStatusSetByAssocMember;
                                stuPetition.UpdatedBy = studentPetition.StudentPetitionsChgopr;
                                stuPetition.StartDate = studentPetition.StpeStartDate;
                                stuPetition.EndDate = studentPetition.StpeEndDate;
                                sectionEntity.AddStudentPetition(stuPetition);
                            }
                            catch (Exception ex)
                            {
                                LogDataError("StudentPetitions", studentPetition.Recordkey, studentPetition, ex);
                            }
                        }
                    }
                }

            }
            return sectionEntity;
        }

        /// <summary>
        /// Add a student petition
        /// </summary>
        /// <param name="studentPetition">The student petition to add</param>
        /// <returns>Newly created student petition</returns>
        public async Task<StudentPetition> AddStudentPetitionAsync(StudentPetition studentPetition)
        {
            if (studentPetition == null)
            {
                throw new ArgumentNullException("studentPetition", "studentPetition is required.");
            }

            CreateStudentPetitionRequest createRequest = new CreateStudentPetitionRequest();
            if (studentPetition.Comment != null)
            {
                // We may have line break characters in the data. Split them out and add each line separately
                // to preserve any line-to-line formatting the user entered. Note that these characters could be
                // \n or \r\n (two variations of a new line character) or \r (a carriage return). We will change
                // any of the new line or carriage returns to the same thing, and then split the string on that.
                char newLineCharacter = '\n';
                string alternateNewLineCharacter = "\r\n";
                string carriageReturnCharacter = "\r";
                string commentText = studentPetition.Comment.Replace(alternateNewLineCharacter, newLineCharacter.ToString());
                commentText = commentText.Replace(carriageReturnCharacter, newLineCharacter.ToString());
                var commentLines = commentText.Split(newLineCharacter);
                foreach (var line in commentLines)
                {
                    createRequest.Comment.Add(line);
                }
            }
            createRequest.ReasonCode = studentPetition.ReasonCode;
            createRequest.SectionId = studentPetition.SectionId;
            createRequest.StudentId = studentPetition.StudentId;
            createRequest.IsFacultyConsent = studentPetition.Type == StudentPetitionType.FacultyConsent ? true : false;
            createRequest.StatusCode = studentPetition.StatusCode;

            CreateStudentPetitionResponse createResponse = null;
            try
            {
                createResponse = await transactionInvoker.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(createRequest);
            }
            catch
            {
                logger.Error("Error occurred during CreateStudentPetition transaction execution.");
                throw new ColleagueWebApiException();
            }
            if (createResponse != null && !createResponse.ErrorOccurred && !string.IsNullOrEmpty(createResponse.StudentPetitionsId))
            {
                // Update was successful
                StudentPetition newStudentPetition = null;
                try
                {
                    // Update appears successful and we have a returned ID - Get the new student petition of type added from Colleague. Verify the section and the Student
                    newStudentPetition = await GetAsync(createResponse.StudentPetitionsId, studentPetition.SectionId, studentPetition.Type);
                    if (newStudentPetition.StudentId != studentPetition.StudentId || newStudentPetition.SectionId != studentPetition.SectionId)
                    {
                        logger.Error("StudentPetition for student " + studentPetition.StudentId + " section " + studentPetition.SectionId + " appeared successful but new studentPetition could not be retrieved");
                        throw new ColleagueWebApiException();
                    }
                }
                catch (KeyNotFoundException)
                {
                    logger.Error("Could not retrieve the newly created student petition specified by id " + createResponse.StudentPetitionsId);
                    throw new ColleagueWebApiException();
                }
                catch (Exception)
                {
                    logger.Error("Error occurred while retrieving newly added student petition using id " + createResponse.StudentPetitionsId);
                    throw;
                }
                return newStudentPetition;
            }
            // Update failed, return an appropriate exception
            if (createResponse == null)
            {
                logger.Error("Null response returned by create student petition.");
                throw new ColleagueWebApiException();
            }

            if (!string.IsNullOrEmpty(createResponse.ExistingPetitionId))
            {
                logger.Error("Student " + studentPetition.StudentId + " already has a Student Petition or Faculty Consent for section " + studentPetition.SectionId + ". Cannot create a new one.");
                throw new ExistingStudentPetitionException("Student Petition already exists. Not added", createResponse.ExistingPetitionId, studentPetition.SectionId, studentPetition.Type.ToString());
            }
            else
            {
                // For all other errors:
                // - Invalid data in request
                // - transaction appeared successful but no waiver id returned
                logger.Error("Error creating a student petition for Student " + studentPetition.StudentId + " Section " + studentPetition.SectionId + ". Transaction Message: " + createResponse.ErrorMessage);
                throw new ColleagueWebApiException();
            }

        }

        /// <summary>
        /// Update a student petition
        /// </summary>
        /// <param name="studentPetition">The student petition to update</param>
        /// <returns>Updated student petition</returns>
        public async Task<StudentPetition> UpdateStudentPetitionAsync(StudentPetition studentPetition)
        {
            if (studentPetition == null)
            {
                throw new ArgumentNullException("studentPetition", "studentPetition is required.");
            }

            CreateStudentPetitionRequest updateRequest = new CreateStudentPetitionRequest();
            if (studentPetition.Comment != null)
            {
                // We may have line break characters in the data. Split them out and add each line separately
                // to preserve any line-to-line formatting the user entered. Note that these characters could be
                // \n or \r\n (two variations of a new line character) or \r (a carriage return). We will change
                // any of the new line or carriage returns to the same thing, and then split the string on that.
                char newLineCharacter = '\n';
                string alternateNewLineCharacter = "\r\n";
                string carriageReturnCharacter = "\r";
                string commentText = studentPetition.Comment.Replace(alternateNewLineCharacter, newLineCharacter.ToString());
                commentText = commentText.Replace(carriageReturnCharacter, newLineCharacter.ToString());
                var commentLines = commentText.Split(newLineCharacter);
                foreach (var line in commentLines)
                {
                    updateRequest.Comment.Add(line);
                }
            }
            updateRequest.ReasonCode = studentPetition.ReasonCode;
            updateRequest.SectionId = studentPetition.SectionId;
            updateRequest.StudentId = studentPetition.StudentId;
            updateRequest.IsFacultyConsent = studentPetition.Type == StudentPetitionType.FacultyConsent ? true : false;
            updateRequest.StatusCode = studentPetition.StatusCode;

            CreateStudentPetitionResponse updateResponse = null;
            try
            {
                updateResponse = await transactionInvoker.ExecuteAsync<CreateStudentPetitionRequest, CreateStudentPetitionResponse>(updateRequest);

                if (updateResponse == null || updateResponse.ErrorOccurred)
                {
                    logger.Error(updateResponse.ErrorMessage);
                    throw new ColleagueWebApiException(string.Format("Error occurred while retrieving updated student petition. Message: '{0}'", updateResponse.ErrorMessage));
                }

                // Update was successful
                StudentPetition updatedStudentPetition = null;
                try
                {
                    // Update appears successful and we have a returned ID - Get the new student petition of type added from Colleague. Verify the section and the Student
                    updatedStudentPetition = await GetAsync(updateResponse.StudentPetitionsId, studentPetition.SectionId, studentPetition.Type);
                    if (updatedStudentPetition.StudentId != studentPetition.StudentId || updatedStudentPetition.SectionId != studentPetition.SectionId)
                    {
                        string message = "StudentPetition for student " + studentPetition.StudentId + " section " + studentPetition.SectionId + " appeared successful but updated studentPetition could not be retrieved";
                        logger.Error(message);
                        throw new ColleagueWebApiException(message);
                    }
                }
                catch (KeyNotFoundException knfex)
                {
                    string message = string.Format("Could not retrieve the updated student petition specified by id {0}. Message: '{1}'", updateResponse.StudentPetitionsId, knfex.Message);
                    logger.Error(knfex, message);
                    throw new KeyNotFoundException(message);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error occurred while retrieving updated student petition using id " + updateResponse.StudentPetitionsId);
                    throw ex;
                }
                return updatedStudentPetition;
            }
            catch (Exception ex)
            {
                string message = string.Format("Error occurred during CreateStudentPetition transaction execution. Message: '{0}'", ex.Message);
                logger.Error(ex, message);
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Returns the student petition requested by ID and type and section Id
        /// </summary>
        /// <param name="studentPetitionId">Id of the student petition</param>
        /// <param name="type">Indicates which type of item should be returned.  One Id can reference each type.</param>
        /// <param name="sectionId">The section ID for the requested student petition.  One Id can reference many sections. </param>
        /// <returns>StudentPetition domain object</returns>
        public async Task<StudentPetition> GetAsync(string studentPetitionId, string sectionId, StudentPetitionType type)
        {
            if (string.IsNullOrEmpty(studentPetitionId))
            {
                throw new ArgumentNullException("studentPetitionId", "studentPetitionId must be provided.");
            }

            if (string.IsNullOrEmpty(studentPetitionId))
            {
                throw new ArgumentNullException("sectionId", "sectionId must be provided.");
            }

            var studentPetitionRecord = await DataReader.ReadRecordAsync<StudentPetitions>(studentPetitionId);

            if (studentPetitionRecord == null)
            {
                logger.Error("StudentPetition record not found for student petition Id " + studentPetitionId);
                throw new KeyNotFoundException();
            }
            var commentIds = studentPetitionRecord.PetitionsEntityAssociation
                .Where(c => !string.IsNullOrEmpty(c.StpeStuPetitionCmntsIdAssocMember))
                .Select(c => c.StpeStuPetitionCmntsIdAssocMember).Distinct().ToArray();
            var commentData = await DataReader.BulkReadRecordAsync<StuPetitionCmnts>(commentIds);
            return BuildStudentPetitionByType(sectionId, studentPetitionRecord, commentData, type);
        }

        private StudentPetition BuildStudentPetitionByType(string sectionId, StudentPetitions studentPetitionData, Collection<StuPetitionCmnts> commentData, StudentPetitionType type)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Must provide the section id for the desired StudentPetition.");
            }
            if (studentPetitionData == null)
            {
                throw new ArgumentNullException("studentPetitionData", "Must provide the data contract for the desired StudentPetition.");
            }

            var studentId = studentPetitionData.StpeStudent;
            var petitionId = studentPetitionData.Recordkey;
            StudentPetition studentPetition = null;

            foreach (var petition in studentPetitionData.PetitionsEntityAssociation)
            {
                var courseId = petition.StpeCoursesAssocMember;
                string facConsentCmnts = null;
                string stuPetitionCmnts = null;


                if (petition.StpeSectionAssocMember == sectionId)
                {

                    StuPetitionCmnts comments = null;
                    if (commentData != null)
                        comments = commentData.FirstOrDefault(c => c.Recordkey == petition.StpeStuPetitionCmntsIdAssocMember);

                    if (null != comments)
                    {
                        facConsentCmnts = (string.IsNullOrEmpty(comments.StpcConsentComments)) ? null : comments.StpcConsentComments.Replace(Convert.ToChar(DynamicArray.VM), '\n');
                        stuPetitionCmnts = (string.IsNullOrEmpty(comments.StpcPetitionComments)) ? null : comments.StpcPetitionComments.Replace(Convert.ToChar(DynamicArray.VM), '\n');
                    }
                    if (type == StudentPetitionType.FacultyConsent)
                    {
                        // Do we have a faculty consent for this section in this student petition
                        if (!string.IsNullOrEmpty(petition.StpeFacultyConsentAssocMember))
                        {
                            // If there is a part of the student petition that cannot be loaded (duplicate or soemthing) load the rest.
                            // This might mean that one petition will generate multiple errors but so be it.
                            try
                            {
                                var facultyConsent = new StudentPetition(id: petitionId, courseId: courseId, sectionId: sectionId, studentId: studentId, type: StudentPetitionType.FacultyConsent, statusCode: petition.StpeFacultyConsentAssocMember);
                                DateTimeOffset dateTimeChanged = petition.StpeFacultyConsentTimeAssocMember.ToPointInTimeDateTimeOffset(
                                    petition.StpeFacultyConsentDateAssocMember, colleagueTimeZone) ?? new DateTimeOffset();
                                facultyConsent.DateTimeChanged = dateTimeChanged;
                                facultyConsent.Comment = facConsentCmnts;
                                facultyConsent.ReasonCode = petition.StpeConsentReasonCodeAssocMember;
                                facultyConsent.TermCode = studentPetitionData.StpeTerm;
                                facultyConsent.SetBy = petition.StpeFacultyConsentSetByAssocMember;
                                facultyConsent.UpdatedBy = studentPetitionData.StudentPetitionsChgopr;
                                facultyConsent.StartDate = studentPetitionData.StpeStartDate;
                                facultyConsent.EndDate = studentPetitionData.StpeEndDate;
                                studentPetition = facultyConsent;
                            }
                            catch (Exception ex)
                            {
                                LogDataError("StudentPetitions", studentPetitionData.Recordkey, studentPetitionData, ex);
                            }
                        }
                        else
                        {
                            // Faculty consent not found. Throw KeyNotFound exception.
                            throw new KeyNotFoundException("No faculty consent found for this student petition Id and section Id.");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(petition.StpePetitionStatusAssocMember))
                        {
                            try
                            {
                                var stuPetition = new StudentPetition(id: petitionId, courseId: courseId, sectionId: sectionId, studentId: studentId, type: StudentPetitionType.StudentPetition, statusCode: petition.StpePetitionStatusAssocMember);
                                DateTimeOffset dateTimeChanged = petition.StpePetitionStatusTimeAssocMember.ToPointInTimeDateTimeOffset(
                                    petition.StpePetitionStatusDateAssocMember, colleagueTimeZone) ?? new DateTimeOffset();
                                stuPetition.DateTimeChanged = dateTimeChanged;
                                stuPetition.Comment = stuPetitionCmnts;
                                stuPetition.ReasonCode = petition.StpePetitionReasonCodeAssocMember;
                                stuPetition.TermCode = studentPetitionData.StpeTerm;
                                stuPetition.SetBy = petition.StpePetitionStatusSetByAssocMember;
                                stuPetition.UpdatedBy = studentPetitionData.StudentPetitionsChgopr;
                                stuPetition.StartDate = studentPetitionData.StpeStartDate;
                                stuPetition.EndDate = studentPetitionData.StpeEndDate;
                                studentPetition = stuPetition;
                            }
                            catch (Exception ex)
                            {
                                LogDataError("StudentPetitions", studentPetitionData.Recordkey, studentPetitionData, ex);
                            }
                        }
                        else
                        {
                            // StudentPetition not found. Throw KeyNotFound exception.
                            throw new KeyNotFoundException("No student petition found for this student petition Id and section Id.");
                        }
                    }
                }
            }
            if (studentPetition == null)
            {
                throw new KeyNotFoundException("Item not found.");
            }
            return studentPetition;
        }
    }
}
