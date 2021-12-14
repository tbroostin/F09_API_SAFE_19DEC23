//Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Repository class gets StudentDocuments from the colleague database
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentDocumentRepository : BaseColleagueRepository, IStudentDocumentRepository
    {
        /// <summary>
        /// Dependency Injection constructor for the StudentDocumentRepository
        /// </summary>
        /// <param name="cacheProvider"></param>
        /// <param name="transactionFactory"></param>
        /// <param name="logger"></param>
        public StudentDocumentRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }
       
        /// <summary>
        /// Get all of a student's financial aid documents across all award years.
        /// </summary>
        /// <param name="studentId">The Id of the student for whom to get documents</param>
        /// <returns>A list of StudentDocument objects</returns>
        /// <exception cref="ArgumentNullException">Thrown if the studentId argument is null or empty</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no MAILING record exists for the given studentId </exception>        
        public async Task<IEnumerable<StudentDocument>> GetDocumentsAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            var studentDocumentList = new List<StudentDocument>();

            var mailingData = await DataReader.ReadRecordAsync<Mailing>(studentId);
            if (mailingData == null)
            {
                throw new KeyNotFoundException("Student has no MAILING record");
            }

            if (mailingData.ChCorrEntityAssociation.Count() == 0 && mailingData.MailingCurrentCrcCode == null)
            {
                return studentDocumentList;
            }

            if (mailingData.ChCorrEntityAssociation.Count() > 0)
            {
                foreach (var chCorr in mailingData.ChCorrEntityAssociation)
                {
                    try
                    {
                        var studentDoc = CreateStudentDocument(
                                         studentId,
                                         chCorr.MailingCorrReceivedAssocMember,
                                         chCorr.MailingCorrRecvdActDtAssocMember,
                                         chCorr.MailingCorrReceivedDateAssocMember,
                                         chCorr.MailingCorrRecvdInstanceAssocMember,
                                         chCorr.MailingCorrRecvdStatusAssocMember,
                                         chCorr.MailingCorrRecvdAsgnDtAssocMember);

                        studentDocumentList.Add(studentDoc);

                    }
                    catch (Exception e)
                    {
                        logger.Info(e, e.Message);
                    }
                }
            }

            if (mailingData.MailingCurrentCrcCode != null && mailingData.MailingCurrentCrcCode.Count() > 0)
            {
                var coreqIds = mailingData.MailingCurrentCrcCode.Where(code => !string.IsNullOrEmpty(code)).Select(code => string.Format("{0}*{1}", studentId, code));
                var coreqRecords = await DataReader.BulkReadRecordAsync<Coreq>(coreqIds.ToArray());

                foreach (var coreq in coreqRecords)
                {
                    foreach (var request in coreq.CoreqRequestsEntityAssociation)
                    {
                        //see if we have a document with same code in studentDocumentList
                        var matchingDocument = studentDocumentList.FirstOrDefault(sd => sd.Code == request.CoreqCcCodeAssocMember);

                        //Create a new document only if there is a document in the studentDocumentList with same code but different date and/or instance;
                        //or there is no document with such a code
                        if ((matchingDocument != null && (matchingDocument.StatusDate != request.CoreqCcDateAssocMember || matchingDocument.Instance != request.CoreqCcInstanceAssocMember)) || matchingDocument == null)
                        {
                            try
                            {
                                var studentDoc = CreateStudentDocument(
                                                    studentId,
                                                    request.CoreqCcCodeAssocMember,
                                                    request.CoreqCcExpActDtAssocMember,
                                                    request.CoreqCcDateAssocMember,
                                                    request.CoreqCcInstanceAssocMember,
                                                    request.CoreqCcStatusAssocMember,
                                                    request.CoreqCcAssignDtAssocMember);
                                studentDocumentList.Add(studentDoc);
                            }
                            catch (Exception e)
                            {
                                logger.Info(e, e.Message);
                            }
                        }
                    }
                }

            }

            return studentDocumentList;
        }
       
        /// <summary>
        /// Creates a student document entity and assigns its properties
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="code">document code</param>
        /// <param name="dueDate">document due date</param>
        /// <param name="statusDate">document status date</param>
        /// <param name="instanceDescription">document instance description</param>
        /// <param name="statusCode">document status code</param>        
        /// <returns>StudentDocument entity</returns>
        private StudentDocument CreateStudentDocument(string studentId, string code, DateTime? dueDate, DateTime? statusDate, string instanceDescription, string statusCode, DateTime? assignDate)
        {
            var studentDocument = new StudentDocument(studentId, code);
            studentDocument.DueDate = dueDate;
            studentDocument.StatusDate = statusDate;
            studentDocument.Instance = instanceDescription;
            studentDocument.AssignDate = assignDate;

            if (statusCode == null) statusCode = string.Empty;
            var documentStatuses = GetDocumentStatuses();
            var statusCodeObject =  (documentStatuses != null && documentStatuses.ValsEntityAssociation != null) ? documentStatuses.ValsEntityAssociation.FirstOrDefault(
                v => v.ValInternalCodeAssocMember != null ? v.ValInternalCodeAssocMember.ToUpper() == statusCode.ToUpper() : false) : null;

            if (statusCodeObject == null)
            {
                logger.Info(string.Format("{0} is not a valid status code in CORR.STATUSES", statusCode));
                studentDocument.Status = DocumentStatus.Incomplete;
                studentDocument.StatusDescription = null;
            }
            else
            {
                switch (statusCodeObject.ValActionCode1AssocMember)
                {
                    case "0":
                        studentDocument.Status = DocumentStatus.Waived;                        
                        break;
                    case "1":
                        studentDocument.Status = DocumentStatus.Received;                        
                        break;
                    default:
                        studentDocument.Status = DocumentStatus.Incomplete;                        
                        break;
                }

                studentDocument.StatusDescription = statusCodeObject.ValExternalRepresentationAssocMember;
            }
            
            return studentDocument;
        }        

        /// <summary>
        /// Helper method to get document statuses
        /// </summary>
        /// <returns>valcode values for document statuses</returns>
        private ApplValcodes GetDocumentStatuses()
        {
            return GetOrAddToCache<ApplValcodes>("DocumentStatuses",
                () =>
                {
                    var statusTable = DataReader.ReadRecord<ApplValcodes>("CORE.VALCODES", "CORR.STATUSES");
                    if (statusTable == null)
                    {
                        var message = "Unable to get CORE->CORR.STATUSES valcode table";
                        logger.Error(message);
                        throw new Exception(message);
                    }
                    return statusTable;
                }, Level1CacheTimeoutValue);
        }
    }
}
