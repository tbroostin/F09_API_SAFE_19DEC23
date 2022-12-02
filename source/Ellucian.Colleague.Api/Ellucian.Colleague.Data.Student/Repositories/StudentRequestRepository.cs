// Copyright 2016-2022 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Http.Exceptions;
using slf4net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Ellucian.Colleague.Domain.Student.Entities.Transcripts;
using Ellucian.Data.Colleague.Exceptions;

namespace Ellucian.Colleague.Data.Student.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentRequestRepository : BaseColleagueRepository, IStudentRequestRepository
    {
        private string colleagueTimeZone;

        public StudentRequestRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Not cached
            CacheTimeout = 0;
            colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }


        /// <summary>
        /// Creates a student transcript request or enrollment verification request
        /// </summary>
        /// <param name="request">The request object to add. It could be either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see></param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
        public async Task<StudentRequest> CreateStudentRequestAsync(StudentRequest request)
        {
            StudentRequest createdRequest = null;
            if (request == null)
            {
                throw new ArgumentNullException("student request", "student request object to create must be supplied");
            }
            CreateStudentRequestRequest studentRequest = new CreateStudentRequestRequest();
            studentRequest.StudentId = request.StudentId;
            studentRequest.RecipientName = request.RecipientName;
            studentRequest.NumberOfCopies = (!request.NumberOfCopies.HasValue || (request.NumberOfCopies.HasValue && request.NumberOfCopies.Value <= 0)) ? 1 : request.NumberOfCopies.Value;
            studentRequest.RequestHoldCode = request.HoldRequest;
            studentRequest.RecipientAddressLines = request.MailToAddressLines;
            studentRequest.RecipientCity = request.MailToCity;
            studentRequest.RecipientCountryCode = request.MailToCountry;
            studentRequest.RecipientPostalCode = request.MailToPostalCode;
            studentRequest.RecipientState = request.MailToState;
            studentRequest.Comments = SeparateOnLineBreaks(request.Comments);
            studentRequest.RequestType = request is StudentTranscriptRequest ? "T" : "E";
            studentRequest.TranscriptGrouping = request is StudentTranscriptRequest ? (request as StudentTranscriptRequest).TranscriptGrouping : string.Empty;

            CreateStudentRequestResponse createResponse = null;
            try
            {
                createResponse = await transactionInvoker.ExecuteAsync<CreateStudentRequestRequest, CreateStudentRequestResponse>(studentRequest);
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session got expired while creating student transcript request.";
                logger.Error(ce, message);
                throw;
            }
            catch
            {
                logger.Error("Error occurred during CreateStudentRequest transaction execution.");
                throw new ColleagueWebApiException("Error occurred during CreateStudentRequest transaction execution.");
            }
            if (createResponse == null)
            {
                logger.Error("Null response returned by create request transaction.");
                throw new ColleagueWebApiException("Null response returned by create request transaction.");
            }
            if (createResponse != null && createResponse.ErrorOccurred)
            {
                string errorText = string.Format("Following error occured during CreateStudentRequest transaction execution {0}", createResponse.ErrorMessage);
                logger.Error(errorText);
                throw new ColleagueWebApiException(errorText);
            }
            if (string.IsNullOrEmpty(createResponse.StudentRequestLogsId))
            {
                logger.Error("Null strudent request log id returned.");
                throw new ColleagueWebApiException("Null strudent request log id returned.");
            }
            if (createResponse != null && !createResponse.ErrorOccurred && !string.IsNullOrEmpty(createResponse.StudentRequestLogsId))
            {

                try
                {
                    // Update appears successful and we have a returned ID - Get the new request from Colleague. 
                    createdRequest = await GetAsync(createResponse.StudentRequestLogsId);
                    if (createdRequest.StudentId != request.StudentId)
                    {
                        string errorText = string.Format("studentId retrieved is not the same as was requested for Student request creation for RequestLogId: {0}", createResponse.StudentRequestLogsId);
                        logger.Error(errorText);
                        throw new ColleagueWebApiException(errorText);
                    }
                }
                catch (KeyNotFoundException)
                {
                    logger.Error("Could not retrieve the newly created request specified by id " + createResponse.StudentRequestLogsId);
                    throw new ColleagueWebApiException();
                }
                catch (Exception)
                {
                    logger.Error("Error occurred while retrieving newly added request using id " + createResponse.StudentRequestLogsId);
                    throw;
                }

            }
            return createdRequest;
        }



        /// <summary>
        /// Get the student transcript request or enrollment verification request
        /// </summary>
        /// <param name="requestId">Id of a student request log</param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
        public async Task<StudentRequest> GetAsync(string requestId)
        {

            //  throw new NotImplementedException();
            if (string.IsNullOrEmpty(requestId))
            {
                throw new ArgumentNullException("requestId", "Request Id for student request logs must be provided.");
            }
            var requestRecord = await DataReader.ReadRecordAsync<Ellucian.Colleague.Data.Student.StudentRequestLogs>(requestId);

            if (requestRecord == null)
            {
                logger.Error("StudentRequest record not found for request Id " + requestId);
                throw new KeyNotFoundException();
            }

            //find the type of requset record and accordingly return appropriate object
            try
            {
                StudentRequest studentrequest = GenerateRequest(requestRecord);
                return studentrequest;

            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = "Colleague session got expired while retrieving the student transcript request.";
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception occured while retrieving the student transcript request.");
                throw;
            }
        }

        /// <summary>
        /// private method to find polymorphic behaviour of object and return appropriate student request(either transcript request or enrollment verification request) object
        /// </summary>
        /// <param name="requestRecord">StudentRequestLogs entity <see cref="Ellucian.Colleague.Data.Student.StudentRequestLogs"/></param>
        /// <returns>Either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see>is returned</returns>
        private StudentRequest GenerateRequest(Ellucian.Colleague.Data.Student.StudentRequestLogs requestRecord)
        {
            StudentRequest studentRequest = null;
            if ("T".Equals(requestRecord.StrlType.ToUpper()))
            {
                string transcriptGrouping = requestRecord.StrlTranscriptGroupings != null && requestRecord.StrlTranscriptGroupings.Any() ? requestRecord.StrlTranscriptGroupings[0] : string.Empty;
                studentRequest = new StudentTranscriptRequest(requestRecord.StrlStudent, requestRecord.StrlRecipientName, transcriptGrouping);
            }
            else if ("E".Equals(requestRecord.StrlType.ToUpper()))
            {
                studentRequest = new StudentEnrollmentRequest(requestRecord.StrlStudent, requestRecord.StrlRecipientName);
            }
            if (studentRequest != null)
            {
                //map other fields
                studentRequest.Id = requestRecord.Recordkey;
                studentRequest.Comments = !string.IsNullOrEmpty(requestRecord.StrlComments) ? requestRecord.StrlComments.Replace(DmiString._VM, '\n') : string.Empty;
                studentRequest.HoldRequest = requestRecord.StrlStuRequestLogHolds;
                studentRequest.NumberOfCopies = requestRecord.StrlCopies;
                studentRequest.MailToAddressLines = requestRecord.StrlAddress;
                studentRequest.MailToCity = requestRecord.StrlCity;
                studentRequest.MailToCountry = requestRecord.StrlCountry;
                studentRequest.MailToPostalCode = requestRecord.StrlZip;
                studentRequest.MailToState = requestRecord.StrlState;
                studentRequest.InvoiceNumber = requestRecord.StrlInvoice;
                studentRequest.RequestDate = requestRecord.StrlDate;
                studentRequest.CompletedDate = requestRecord.StrlPrintDate;
            }
            return studentRequest;
        }

        /// <summary>
        /// find newline characters and convert into collection of strings
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        private List<string> SeparateOnLineBreaks(string contents)
        {
            List<string> lines = new List<string>();
            if (contents == null)
            {
                return null;
            }
            //this regular expression capture pattern that occurs before any newline or return characters  in <captureLine> group.
            //newline characters are searched for \n or \r or \n\r combinations
            //hence if there are 3 lines then regualar expression will have 3 matches found such as each match will have group called catureLine
            //that would contain text appeared before newline or return characters.
            Regex rx = new Regex(@"(?<captureLine>[^\n|\r|\n\r]*)[\n|\r|\n\r]?", RegexOptions.Singleline);
            MatchCollection matches = rx.Matches(contents);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    string lineContents = match.Groups["captureLine"].Value;
                    lines.Add(lineContents);
                }
            }
            return lines;
        }

        /// <summary>
        /// Returns the full list of Student Requests for given student Id asynchronously.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <returns>A list of student requests for a student that are either transcript request <see cref="Dtos.Student.StudentTranscriptRequest">Student Transcript Request</see>or enrollment verification request<see cref="Dtos.Student.StuentEnrollmentRequest">EnrollmentVerification Request</see></returns>
        public async Task<List<StudentRequest>> GetStudentRequestsAsync(string studentId)
        {
            List<StudentRequest> studentRequestEntityList = new List<StudentRequest>();
            if (string.IsNullOrEmpty(studentId))
            {
                logger.Error("You must provide the student Id");
                throw new ArgumentNullException("studentId", "You must provide the student Id");
            }
            try
            {
                string criteria = "STRL.STUDENT.INDEX EQ '" + studentId + "'";
                Collection<StudentRequestLogs> studentRequestLogs = await DataReader.BulkReadRecordAsync<Ellucian.Colleague.Data.Student.StudentRequestLogs>(criteria);

                //map contract to entities
                if (studentRequestLogs != null && studentRequestLogs.Any())
                {
                    foreach (var request in studentRequestLogs)
                    {
                        StudentRequest requestEntity = null;
                        try
                        {
                            requestEntity = GenerateRequest(request);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, string.Format("An error occurred building student request data for record {0}", request.Recordkey));
                        }
                        if (requestEntity != null)
                        {
                            studentRequestEntityList.Add(requestEntity);
                        }
                    }
                }

                return studentRequestEntityList;
            }
            catch (ColleagueSessionExpiredException ce)
            {
                string message = string.Format("Colleague session got expired  while retrieving student transcript requests for student {0}", studentId);
                logger.Error(ce, message);
                throw;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unable to access student request log records for student Id {0}", studentId);
                logger.Error(ex, errorMessage);
                throw;
            }
        }


        /// <summary>
        /// Returns the student request fee for given student Id and request Id asynchronously.
        /// </summary>
        /// <param name="studentId">Student Id</param>
        /// <param name="requestId">Request  Id</param>
        /// <returns> <see cref="StudentRequestFee"/>The requested student request fee object</returns>
        public async Task<StudentRequestFee> GetStudentRequestFeeAsync(string studentId, string requestId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                logger.Error("You must provide the student Id");
                throw new ArgumentNullException("studentId", "You must provide the student Id");
            }
            if (string.IsNullOrEmpty(requestId))
            {
                logger.Error("You must provide the request Id");
                throw new ArgumentNullException("requestId", "You must provide the request Id");
            }

            GetStudentRequestFeeRequest getApplicationFeeRequest = new GetStudentRequestFeeRequest();
            getApplicationFeeRequest.StudentId = studentId;
            getApplicationFeeRequest.StudentRequestLogsId = requestId;
            try
            {
                // Call the Colleague Transaction used to calculate the fee and the distribution
                GetStudentRequestFeeResponse getApplicationFeeResponse = await transactionInvoker.ExecuteAsync<Ellucian.Colleague.Data.Student.Transactions.GetStudentRequestFeeRequest, Ellucian.Colleague.Data.Student.Transactions.GetStudentRequestFeeResponse>(getApplicationFeeRequest);
                // Take results and create the student request fee entity.
                StudentRequestFee studentRequestFee = new StudentRequestFee(studentId, requestId, getApplicationFeeResponse.Fee, getApplicationFeeResponse.DistributionCode);
                return studentRequestFee;
            }
            catch (ColleagueSessionExpiredException csee)
            {
                var message = string.Format("Colleague session expired while retrieving student request fee for student Id {0} and request Id {1} ", studentId, requestId);
                logger.Error(csee, message);
                throw;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Unable to determine student request fee for student Id {0} and request Id {1} ", studentId, requestId);
                logger.Error(ex, errorMessage);
                throw;
            }
        }
    }
}
