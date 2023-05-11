// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using slf4net;
using Ellucian.Web.Cache;
using System.Threading.Tasks;
using Ellucian.Data.Colleague;
using Ellucian.Web.Dependency;
using System.Collections.Generic;
using Ellucian.Web.Http.Configuration;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Data.Student.DataContracts;
using System.Linq;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentRecordsReleaseRepository : BaseColleagueRepository, IStudentRecordsReleaseRepository
    {
        private string colleagueTimeZone;

        public StudentRecordsReleaseRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = 0;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// Get student records release information for a given student asynchronously
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>A collection of <see cref="StudentRecordsReleaseInfo">StudentRecordsReleaseInfo</see> object.</returns>
        public async Task<IEnumerable<StudentRecordsReleaseInfo>> GetStudentRecordsReleaseInfoAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("StudentId", "studentId cannot be null or empty.");
            }
            try
            {
                var criteria = "WITH SREL.STUDENT EQ '" + studentId + "'";
                Collection<StudentReleases> studentRecordsReleaseData = await DataReader.BulkReadRecordAsync<StudentReleases>(criteria);
                var studentRecordsReleaseInfoEntity = BuildStudentRecordsRelease(studentId, studentRecordsReleaseData);
                return studentRecordsReleaseInfoEntity;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving student records release configuration information for student {0}", studentId);
                throw;
            }
            catch (Exception ex)
            {
                string error = string.Format("Exception occurred while trying to retrieve student records release configuration information for student {0}", studentId);
                logger.Error(ex, error);
                throw new ApplicationException(error);
            }
        }

        /// <summary>
        /// Add a student Records Release Information for student
        /// </summary>
        /// <param name="StudentRecordsRelease">The StudentRecordsReleaseInfo to create</param>
        /// <returns>Added StudentRecordsReleaseInfo</returns>
        public async Task<StudentRecordsReleaseInfo> AddStudentRecordsReleaseInfoAsync(StudentRecordsReleaseInfo studentRecordsRelease)
        {
            StudentRecordsReleaseInfo newStudentRecordsReleaseInfo = new StudentRecordsReleaseInfo();


            if (studentRecordsRelease == null)
            {
                throw new ArgumentNullException("studentRecordsRelease", "Must provide the add StudentRecordsRelease input to add a new record.");
            }

            AddStudentReleaseRecordRequest newRequest = new AddStudentReleaseRecordRequest()
            {
                InStudentId = studentRecordsRelease.StudentId,
                InRelFirstName = studentRecordsRelease.FirstName,
                InRelLastName = studentRecordsRelease.LastName,
                InRelPin = studentRecordsRelease.PIN,
                InRelRelationType = studentRecordsRelease.RelationType,
                InRelAccessGiven = studentRecordsRelease.AccessAreas,
                InRelStartDate = studentRecordsRelease.StartDate,
                InRelEndDate = studentRecordsRelease.EndDate
            };

            AddStudentReleaseRecordResponse createResponse = null;
            try
            {
                createResponse = await transactionInvoker.ExecuteAsync<AddStudentReleaseRecordRequest, AddStudentReleaseRecordResponse>(newRequest);
               
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while adding student records release information");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred during addStudentRecordsRelease transaction execution.");
                throw new ApplicationException("Error occurred during Add StudentRecordsRelease creation.");
            }
            if (createResponse == null)
            {
                logger.Error("Null response returned by add StudentRecordsRelease transaction.");
                throw new ApplicationException("Null response returned by add StudentRecordsRelease creation.");
            }
            if (createResponse.OutErrorMessages != null && createResponse.OutErrorMessages.Count > 0)
            {
                 foreach (var message in createResponse.OutErrorMessages)
                {
                    if (message.Contains("locked"))
                    {
                        logger.Error("Adding a new student records release information with id " + studentRecordsRelease.StudentId + " is locked");
                        throw new RecordLockException("Adding a new student records release information with id" + studentRecordsRelease.StudentId + " is locked");
                    }
                    else
                    {
                        string error = "An error occurred while trying to add StudentRecordsRelease information";
                        logger.Error(error);
                        throw new ApplicationException(error);
                    }
                }
            }
            newStudentRecordsReleaseInfo = await GetStudentRecordsReleaseInfoByIdAsync(createResponse.OutStudentRecordsReleaseId);
            return newStudentRecordsReleaseInfo;
        }

        /// <summary>
        /// Delete a student records release information for a student
        /// </summary>
        /// <param name="studentReleaseId">Student Release Id</param>
        /// <returns>StudentRecordsReleaseInfo object</returns>
        public async Task<StudentRecordsReleaseInfo> DeleteStudentRecordsReleaseInfoAsync(string studentReleaseId)
        {
            if (string.IsNullOrEmpty(studentReleaseId))
            {
                throw new ArgumentNullException("studentReleaseId", "Must provide the student release id to delete student records release info.");
            }

            DeleteStudentReleaseRecordRequest deleteRequest = new DeleteStudentReleaseRecordRequest()
            {
                InStudentReleasesId = studentReleaseId
            };

            DeleteStudentReleaseRecordResponse deleteResponse = null;
            try
            {
                deleteResponse = await transactionInvoker.ExecuteAsync<DeleteStudentReleaseRecordRequest, DeleteStudentReleaseRecordResponse>(deleteRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while deleting student records release info");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred during deleteStudentReleaseRecord transaction execution.");
                throw new ApplicationException("Error occurred during student records release info deletion.");
            }
            if (deleteResponse == null)
            {
                logger.Error("Null response returned by delete student records release info transaction.");
                throw new ApplicationException("Null response returned by student records release info deletion.");
            }
            if (deleteResponse.OutErrorMessages != null && deleteResponse.OutErrorMessages.Count > 0)
            {
                foreach (var message in deleteResponse.OutErrorMessages)
                {
                    if (message.Contains("locked"))
                    {
                        logger.Error("Deleting the student records release information with id " + studentReleaseId + " is locked");
                        throw new RecordLockException("Deleting the student records release information with id " + studentReleaseId + " is locked");
                    }
                    else
                    {
                        string error = "An error occurred while trying to delete student records release info";
                        logger.Error(error);
                        throw new ApplicationException(error);
                    }
                }
            }
            return await GetStudentRecordsReleaseInfoByIdAsync(studentReleaseId);
        }

        /// Get the student records release deny access information
        /// </summary>
        /// <returns>The StudentRecordsReleaseDenyAccess entity</returns>
        public async Task<StudentRecordsReleaseDenyAccess> GetStudentRecordsReleaseDenyAccessAsync(string studentId)
        {

            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("StudentId", "studentId cannot be null or empty.");
            }
            var studentRecordsReleaseDenyAccessInformation = new StudentRecordsReleaseDenyAccess();

            // if the entry doesn't exist in StuRelDenyAccess entity then we are explicitly settting studentReleaseDenyAccess as false
            StuRelDenyAccess studentReleaseDenyAccess = await DataReader.ReadRecordAsync<StuRelDenyAccess>("STU.REL.DENY.ACCESS", studentId );
            if (studentReleaseDenyAccess == null)
            {
                studentRecordsReleaseDenyAccessInformation.DenyAccessToAll = false;
            }
            else
            {
                studentRecordsReleaseDenyAccessInformation.DenyAccessToAll = (!string.IsNullOrEmpty(studentReleaseDenyAccess.StuRelDenyAccessAll) && (studentReleaseDenyAccess.StuRelDenyAccessAll.ToUpper() == "Y")) ? true : false;
            }
            return studentRecordsReleaseDenyAccessInformation;

        }

        /// <summary>
        /// Deny access to student records release information 
        /// </summary>
        /// <param name="studentRecordsRelDenyAccess">The student records release deny access information for denying access to student records release</param>
        /// <returns>A collection of updated<see cref="StudentRecordsReleaseInfo">StudentRecordsReleaseInfo</see>objects</returns>
        public async Task<IEnumerable<StudentRecordsReleaseInfo>> DenyStudentRecordsReleaseAccessAsync(DenyStudentRecordsReleaseAccessInformation studentRecordsRelDenyAccess)
        {

            if (studentRecordsRelDenyAccess == null)
            {
                throw new ArgumentNullException("studentRecordsRelDenyAccess", "Must provide the student records release deny access input to deny access.");
            }
            StudentRecordsReleaseDenyAccessAllRequest denyAccessRequest = new StudentRecordsReleaseDenyAccessAllRequest()
            {
                InStudentId = studentRecordsRelDenyAccess.StudentId,
                InDenyAccessAll = studentRecordsRelDenyAccess.DenyAccessToAll ? "Y": "N",

            };
            StudentRecordsReleaseDenyAccessAllResponse denyAccessResponse = null;
            try
            {
                denyAccessResponse = await transactionInvoker.ExecuteAsync<StudentRecordsReleaseDenyAccessAllRequest, StudentRecordsReleaseDenyAccessAllResponse>(denyAccessRequest);
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while denying access to student records release information");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred during denying access to student records release transaction execution.");
                throw new ApplicationException("Error occurred during student records release denying access");
            }
            if (denyAccessResponse == null)
            {
                logger.Error("Null response returned by denying access to student records release transaction.");
                throw new ApplicationException("Null response returned by student records release denying access");
            }

            if (denyAccessResponse.OutErrorMessages != null && denyAccessResponse.OutErrorMessages.Count > 0)
            {
                foreach (var message in denyAccessResponse.OutErrorMessages)
                {
                    if (message.Contains("locked"))
                    {
                        logger.Error("Denying access to student records release information with id  " + studentRecordsRelDenyAccess.StudentId + " is locked");
                        throw new RecordLockException("Denying access to student records release information with id  " + studentRecordsRelDenyAccess.StudentId + " is locked");
                    }
                    else
                    {
                        string error = "An error occurred while trying to deny access to student records release information";
                        logger.Error(error);
                        throw new ApplicationException(error);
                    }
                }
            }
            var updatedStudentRecordsReleaseInfo = await GetStudentRecordsReleaseInfoAsync(studentRecordsRelDenyAccess.StudentId);
            return updatedStudentRecordsReleaseInfo;
        }
        /// <summary>
        /// Build student records release
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="studentRecordsReleaseData">studentRecordsRelease Data</param>
        /// <returns>StudentRecordsReleaseInfo</returns>
        private List<StudentRecordsReleaseInfo> BuildStudentRecordsRelease(string studentId, Collection<StudentReleases> studentRecordsReleaseData)
        {
            List<StudentRecordsReleaseInfo> studentsRecordsRelease = new List<StudentRecordsReleaseInfo>();
            if (studentRecordsReleaseData != null && studentRecordsReleaseData.Any())
            {
                foreach (StudentReleases studentRecordsRelease in studentRecordsReleaseData)
                {
                    if (studentId != studentRecordsRelease.SrelStudent)
                    {
                        logger.Error("Student records release information retrieved is not for the provided studentId {0}", studentId);
                    }
                    else
                    {
                        try
                        {
                            var studentRecordsReleaseInfo = new StudentRecordsReleaseInfo();
                            studentRecordsReleaseInfo.Id = studentRecordsRelease.Recordkey;
                            studentRecordsReleaseInfo.StudentId = studentRecordsRelease.SrelStudent;
                            studentRecordsReleaseInfo.FirstName = studentRecordsRelease.SrelFirstName;
                            studentRecordsReleaseInfo.LastName = studentRecordsRelease.SrelLastName;
                            studentRecordsReleaseInfo.PIN = studentRecordsRelease.SrelPin;
                            studentRecordsReleaseInfo.RelationType = studentRecordsRelease.SrelRelationship;
                            studentRecordsReleaseInfo.AccessAreas = studentRecordsRelease.SrelAccessGiven;
                            studentRecordsReleaseInfo.StartDate = studentRecordsRelease.SrelStartDate;
                            studentRecordsReleaseInfo.EndDate = studentRecordsRelease.SrelEndDate;
                            studentsRecordsRelease.Add(studentRecordsReleaseInfo);
                        }
                        catch (Exception ex)
                        {
                            LogDataError("StudentRecordsRelease", studentRecordsRelease.Recordkey, studentRecordsRelease, ex);
                        }
                    }
                }
            }
            return studentsRecordsRelease;
        }


        /// <summary>
        /// Update a student Records Release Information for student
        /// </summary>
        /// <param name="StudentRecordsRelease">The StudentRecordsReleaseInfo to Update</param>
        /// <returns>StudentRecordsReleaseInfo object which was just updated</returns>
        public async Task<StudentRecordsReleaseInfo> UpdateStudentRecordsReleaseInfoAsync(StudentRecordsReleaseInfo studentRecordsRelease)
        {
            StudentRecordsReleaseInfo updateStudentRecordsReleaseInfo = new StudentRecordsReleaseInfo();
            updateStudentRecordsReleaseInfo = studentRecordsRelease;
            if (studentRecordsRelease == null)
            {
                throw new ArgumentNullException("studentRecordsRelease", "Must provide the  StudentRecordsRelease input to update record.");
            }

            UpdateStudentReleaseRecordsRequest updateRequest = new UpdateStudentReleaseRecordsRequest()
            {
                InStuRecRelId = studentRecordsRelease.Id,
                InNewRelPin = studentRecordsRelease.PIN,
                InNewRelAccessGiven = studentRecordsRelease.AccessAreas,
                InNewRelEndDate = studentRecordsRelease.EndDate
            };

            UpdateStudentReleaseRecordsResponse updateResponse = null;
            try
            {
                updateResponse = await transactionInvoker.ExecuteAsync<UpdateStudentReleaseRecordsRequest, UpdateStudentReleaseRecordsResponse>(updateRequest);

            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while updating student records release information");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred during updateStudentRecordsRelease transaction execution.");
                throw new ApplicationException("Error occurred during UpdateStudentRecordsRelease execution.");
            }
            if (updateResponse == null)
            {
                logger.Error("Null response returned by UpdateStudentRecordsRelease transaction.");
                throw new ApplicationException("Null response returned by UpdateStudentRecordsRelease execution.");
            }
            if (updateResponse.OutErrorMessages != null && updateResponse.OutErrorMessages.Count > 0)
            {
                foreach (var message in updateResponse.OutErrorMessages) 
                {
                    if (message.Contains("locked"))
                    {
                        logger.Error("Updating student records release information with id  " + studentRecordsRelease.StudentId + " is locked");
                        throw new RecordLockException("Updating student records release information with id  " + studentRecordsRelease.StudentId + " is locked");
                    }
                    else
                    {
                        string error = "An error occurred while trying to update StudentRecordsRelease information";
                        logger.Error(error);
                        throw new ColleagueWebApiException(error);
                    }               
                }                               
            }
            //return the entity that was just updated
            return await GetStudentRecordsReleaseInfoByIdAsync(updateResponse.OutStuRelId);
        }

        /// <summary>
        /// Get student records release information for a given student records release id asynchronously
        /// </summary>
        /// <param name="studentId">Student Records Release Id</param>
        /// <returns><see cref="StudentRecordsReleaseInfo">StudentRecordsReleaseInfo</see> object.</returns>
        public async Task<StudentRecordsReleaseInfo> GetStudentRecordsReleaseInfoByIdAsync(string studentRecordsReleaseId)
        {
            if (string.IsNullOrEmpty(studentRecordsReleaseId))
            {
                throw new ArgumentNullException("studentRecordsReleaseId", "studentRecordsReleaseId cannot be null or empty.");
            }
            try
            {
                StudentReleases studentRecordsReleaseData = await DataReader.ReadRecordAsync<StudentReleases>(studentRecordsReleaseId);
                var studentRecordsReleaseInfo = new StudentRecordsReleaseInfo();
                studentRecordsReleaseInfo.Id = studentRecordsReleaseData.Recordkey;
                studentRecordsReleaseInfo.StudentId = studentRecordsReleaseData.SrelStudent;
                studentRecordsReleaseInfo.FirstName = studentRecordsReleaseData.SrelFirstName;
                studentRecordsReleaseInfo.LastName = studentRecordsReleaseData.SrelLastName;
                studentRecordsReleaseInfo.PIN = studentRecordsReleaseData.SrelPin;
                studentRecordsReleaseInfo.RelationType = studentRecordsReleaseData.SrelRelationship;
                studentRecordsReleaseInfo.AccessAreas = studentRecordsReleaseData.SrelAccessGiven;
                studentRecordsReleaseInfo.StartDate = studentRecordsReleaseData.SrelStartDate;
                studentRecordsReleaseInfo.EndDate = studentRecordsReleaseData.SrelEndDate;
                return studentRecordsReleaseInfo;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                logger.Error(csee, "Colleague session expired while retrieving student records release configuration information for student records release id {0}", studentRecordsReleaseId);
                throw;
            }
            catch (Exception ex)
            {
                string error = string.Format("Exception occurred while trying to retrieve student records release configuration information for student records release id {0}", studentRecordsReleaseId);
                logger.Error(ex, error);
                throw new ApplicationException(error);
            }
        }
    }
}
