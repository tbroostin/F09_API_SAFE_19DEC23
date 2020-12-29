// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Exceptions;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentWaiverRepository : BaseColleagueRepository, IStudentWaiverRepository
    {
        private string colleagueTimeZone;

        public StudentWaiverRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Not cached
            CacheTimeout = 0;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        /// <summary>
        /// Get all Waivers created for the given section.
        /// </summary>
        /// <param name="sectionId">ID of the section</param>
        /// <returns>List of waiver objects found for this section</returns>
        public async Task<List<StudentWaiver>> GetSectionWaiversAsync(string sectionId)
        {
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException(sectionId);
            }
            else
            {
                var waivers =await GetOrAddToCacheAsync<List<StudentWaiver>>("SectionWaivers" + sectionId,
                async () =>
                {
                    try
                    {
                        string criteria = "SRWV.SECTION EQ '" + sectionId + "'";
                        Collection<Ellucian.Colleague.Data.Student.DataContracts.StudentReqWaivers> reqWaiverData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StudentReqWaivers>(criteria);
                        return BuildWaivers(reqWaiverData);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = "Error selecting and building waivers";
                        logger.Error(errorMessage);
                        logger.Error(ex.ToString());
                        throw new Exception(errorMessage);
                    }
                }, CacheTimeout);
                return waivers;
            }
        }

        /// <summary>
        /// Returns the waiver requested by waiver ID
        /// </summary>
        /// <param name="waiverId">Id of a waiver</param>
        /// <returns>Waiver domain object</returns>
        public async Task<StudentWaiver> GetAsync(string waiverId)
        {
            if (string.IsNullOrEmpty(waiverId))
            {
                throw new ArgumentNullException("waiverId", "Waiver Id must be provided.");
            }
            var waiverRecord = await DataReader.ReadRecordAsync<StudentReqWaivers>(waiverId);

            if (waiverRecord == null)
            {
                logger.Error("StudentReqWaivers record not found for waiver Id " + waiverId);
                throw new KeyNotFoundException();
            }

            return BuildWaivers(new Collection<StudentReqWaivers>() { waiverRecord }).First();
        }

        // Private method to build Waiver objects using the data read from Colleague.
        private List<StudentWaiver> BuildWaivers(Collection<Student.DataContracts.StudentReqWaivers> reqWaiverData)
        {
            List<StudentWaiver> waivers = new List<StudentWaiver>();

            if (reqWaiverData != null && reqWaiverData.Count() > 0)
            {
                foreach (var waiverItem in reqWaiverData)
                {
                    try
                    {
                        // We need to convert value marks to new line characters because we want to maintain any formatting
                        // (line-to-line) that the user may have entered.
                        string comments = (string.IsNullOrEmpty(waiverItem.SrwvComments)) ? null : waiverItem.SrwvComments.Replace(Convert.ToChar(DynamicArray.VM), '\n');
                        var waiver = new StudentWaiver(waiverItem.Recordkey,
                                                waiverItem.SrwvStudent,
                                                waiverItem.SrwvCourse,
                                                waiverItem.SrwvSection,
                                                waiverItem.SrwvReason,
                                                comments,
                                                waiverItem.SrwvTerm,
                                                waiverItem.SrwvStartDate,
                                                waiverItem.SrwvEndDate);
                        waiver.AuthorizedBy = waiverItem.SrwvWaiverPersonId;
                        waiver.ChangedBy = waiverItem.StudentReqWaiversChgopr;
                        waiver.IsRevoked = waiverItem.SrwvRevokedFlag == "Y" || waiverItem.SrwvRevokedFlag == "y" ? true : false;
                        if (waiverItem.StudentReqWaiversChgdate.HasValue && waiverItem.StudentReqWaiversChgtime.HasValue)
                        {
                            var dateLastChanged = new DateTime(waiverItem.StudentReqWaiversChgdate.Value.Year,
                                                               waiverItem.StudentReqWaiversChgdate.Value.Month,
                                                               waiverItem.StudentReqWaiversChgdate.Value.Day,
                                                               waiverItem.StudentReqWaiversChgtime.Value.Hour,
                                                               waiverItem.StudentReqWaiversChgtime.Value.Minute,
                                                               waiverItem.StudentReqWaiversChgtime.Value.Second);
                            waiver.DateTimeChanged = waiverItem.StudentReqWaiversChgtime.ToPointInTimeDateTimeOffset(waiverItem.StudentReqWaiversChgdate, colleagueTimeZone).Value;
                        }
                        // Load list of specific requisite waivers
                        if (waiverItem.SrwvReqCoursesEntityAssociation != null && waiverItem.SrwvReqCoursesEntityAssociation.Count() > 0)
                        {
                            foreach (var requisite in waiverItem.SrwvReqCoursesEntityAssociation)
                            {
                                try
                                {
                                    WaiverStatus status;
                                    switch (requisite.SrwvWaiveReqmtFlagAssocMember.ToUpper())
                                    {
                                        case "Y":
                                            status = WaiverStatus.Waived;
                                            break;
                                        case "N":
                                            status = WaiverStatus.Denied;
                                            break;
                                        default:
                                            status = WaiverStatus.NotSelected;
                                            break;
                                    }
                                    RequisiteWaiver reqWaiver = new RequisiteWaiver(requisite.SrwvAcadReqmtsAssocMember, status);
                                    waiver.AddRequisiteWaiver(reqWaiver);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, "Waiver recordkey " + waiverItem.Recordkey + " Error creating or adding RequisiteWaiver for academic credit id " + requisite.SrwvAcadReqmtsAssocMember);
                                }
                            }
                        }
                        waivers.Add(waiver);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error creating Waiver object for STUDENT.REQUISITE.WAIVER with ID " + waiverItem.Recordkey);
                    }
                }
            }

            return waivers;
        }

        /// <summary>
        /// Add a new section waiver to Colleague for a student
        /// </summary>
        /// <param name="waiver">Waiver object to create in Colleague</param>
        /// <returns>The new Waiver</returns>
        public async Task<StudentWaiver> CreateSectionWaiverAsync(StudentWaiver waiver)
        {
            if (waiver == null)
            {
                throw new ArgumentNullException("waiver", "Waiver object to create must be supplied");
            }
            CreateStudentReqWaiverRequest createRequest = new CreateStudentReqWaiverRequest();

            // We may have line break characters in the data. Split them out and add each line separately
            // to preserve any line-to-line formatting the user entered. Note that these characters could be
            // \n or \r\n (two variations of a new line character) or \r (a carriage return). We will change
            // any of the new line or carriage returns to the same thing, and then split the string on that.
            char newLineCharacter = '\n';
            string alternateNewLineCharacter = "\r\n";
            string carriageReturnCharacter = "\r";
            string commentText = waiver.Comment.Replace(alternateNewLineCharacter, newLineCharacter.ToString());
            commentText = commentText.Replace(carriageReturnCharacter, newLineCharacter.ToString());
            var commentLines = commentText.Split(newLineCharacter);
            foreach (var line in commentLines)
            {
                createRequest.AlComment.Add(line);
            }

            createRequest.AReasonCode = waiver.ReasonCode;
            createRequest.ASectionId = waiver.SectionId;
            createRequest.AStudentId = waiver.StudentId;
            createRequest.RequirementGroup = new List<RequirementGroup>();
            foreach (var item in waiver.RequisiteWaivers)
            {
                var reqGroupItem = new RequirementGroup()
                {
                    AlAcadReqmtIds = item.RequisiteId,
                    AlWaiveReqmtFlag = (item.Status == WaiverStatus.Denied) ? "N" : (item.Status == WaiverStatus.Waived ? "Y" : "")
                };
                createRequest.RequirementGroup.Add(reqGroupItem);
            }

            CreateStudentReqWaiverResponse createResponse = null;
            try
            {
                createResponse = await transactionInvoker.ExecuteAsync<CreateStudentReqWaiverRequest, CreateStudentReqWaiverResponse>(createRequest);
            }
            catch
            {
                logger.Error("Error occurred during CreateStudentReqWaiver transaction execution.");
                throw new Exception();
            }
            if (createResponse != null && createResponse.AErrorOccurred != "1" && !string.IsNullOrEmpty(createResponse.AStudentReqWaiversId))
            {
                StudentWaiver createdWaiver = null;
                try
                {
                    // Update appears successful and we have a returned ID - Get the new waiver from Colleague. Verify the section and the Student
                    createdWaiver = await GetAsync(createResponse.AStudentReqWaiversId);
                    if (createdWaiver.StudentId != waiver.StudentId || createdWaiver.SectionId != waiver.SectionId)
                    {
                        logger.Error("Student req waiver for student " + waiver.StudentId + " section " + waiver.SectionId + " appeared successful but new waiver could not be retrieved");
                        throw new Exception();
                    }
                }
                catch (KeyNotFoundException)
                {
                    logger.Error("Could not retrieve the newly created waiver specified by id " + createResponse.AStudentReqWaiversId);
                    throw new Exception();
                }
                catch (Exception)
                {
                    logger.Error("Error occurred while retrieving newly added waiver using id " + createResponse.AStudentReqWaiversId);
                    throw;
                }
                return createdWaiver;
            }
            // Update failed, return an appropriate exception
            if (createResponse == null)
            {
                logger.Error("Null response returned by create waiver transaction.");
                throw new Exception();
            }

            if (!string.IsNullOrEmpty(createResponse.AExistingId))
            {
                logger.Error("Student " + waiver.StudentId + " already has a waiver for section " + waiver.SectionId + ". Cannot create a new one.");
                throw new ExistingSectionWaiverException("Existing waiver found", createResponse.AExistingId);
            }
            else
            {
                // For all other errors:
                // - Invalid data in request
                // - transaction appeared successful but no waiver id returned
                logger.Error("Error creating a waiver for Student " + waiver.StudentId + " Section " + waiver.SectionId + ". Transaction Message: " + createResponse.AMsg);
                throw new Exception();
            }
        }

        /// <summary>
        /// Get all Waivers created for the given section.
        /// </summary>
        /// <param name="studentId">ID of the student</param>
        /// <returns>List of waiver objects found for this section</returns>
        public async Task<List<StudentWaiver>> GetStudentWaiversAsync(string studentId)
        {
             if (string.IsNullOrEmpty(studentId))
             {
                  var message = "Student Id must be provided";
                  logger.Info(message);
                  throw new ArgumentNullException(message);
             }
                    try
                    {
                        string criteria = "SRWV.STUDENT EQ '" + studentId + "'";
                        Collection<Ellucian.Colleague.Data.Student.DataContracts.StudentReqWaivers> reqWaiverData = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.DataContracts.StudentReqWaivers>(criteria);
                        return BuildWaivers(reqWaiverData);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = "Error selecting and building waivers";
                        logger.Error(ex,errorMessage);
                        throw ;
                    }
            }
    }
}
