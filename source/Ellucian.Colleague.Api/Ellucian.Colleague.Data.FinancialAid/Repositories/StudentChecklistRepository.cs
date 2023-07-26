/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Transactions;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{

    /// <summary>
    /// Creates StudentChecklistItems from database records
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentChecklistRepository : BaseColleagueRepository, IStudentChecklistRepository
    {
        /// <summary>
        /// Constructor for the StudentChecklist Repository
        /// </summary>
        /// <param name="cacheProvider">cacheProvider</param>
        /// <param name="transactionFactory">transactionFactory</param>
        /// <param name="logger">logger</param>
        public StudentChecklistRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get a single StudentFinancialAidChecklist object
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get a checklist</param>
        /// <param name="year">The award year for which to get a checklist</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Thrown if student has no checklist for the year</exception>
        public async Task<StudentFinancialAidChecklist> GetStudentChecklistAsync(string studentId, string year)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(year))
            {
                throw new ArgumentNullException(year);
            }

            var ysAcyrFile = "YS." + year;
            var ysRecord = await DataReader.ReadRecordAsync<YsAcyr>(ysAcyrFile, studentId);

            if (ysRecord == null || ysRecord.ChecklistItemsEntityAssociation == null || !ysRecord.ChecklistItemsEntityAssociation.Any())
            {
                var message = string.Format("Student {0} has no checklist items for year {1}", studentId, year);
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var studentChecklist = new StudentFinancialAidChecklist(studentId, year);

            foreach (var ysChecklistEntity in ysRecord.ChecklistItemsEntityAssociation)
            {
                try
                {
                    var checklistItem = new StudentChecklistItem(ysChecklistEntity.YsChecklistItemsAssocMember, TranslateDisplayActionToControlStatus(ysChecklistEntity.YsDisplayActionAssocMember));
                    studentChecklist.ChecklistItems.Add(checklistItem);
                }
                catch (Exception e)
                {
                    LogDataError("ChecklistItemsEntityAssociation", ysChecklistEntity.YsChecklistItemsAssocMember, ysChecklistEntity, e, string.Format("Error creating checklist item for student {0}, awardYear {1}", studentId, year));
                    LogDataError(ysAcyrFile, studentId, ysRecord, e, "Showing YS record for previous checklist item error");
                }

            }

            return studentChecklist;
        }

        /// <summary>
        /// Helper method to translate a db display action to a ChecklistItemControlStatus. 
        /// Default display action is CompletionRequired
        /// </summary>
        /// <param name="displayAction">display action to translate</param>
        /// <returns></returns>
        private ChecklistItemControlStatus TranslateDisplayActionToControlStatus(string displayAction)
        {
            if (displayAction == null) displayAction = "";
            switch (displayAction.ToUpper())
            {
                case "R":
                    return ChecklistItemControlStatus.RemovedFromChecklist;

                case "S":
                    return ChecklistItemControlStatus.CompletionRequiredLater;

                case "Q":
                    return ChecklistItemControlStatus.CompletionRequired;

                default:
                    return ChecklistItemControlStatus.CompletionRequired;
            }
        }

        /// <summary>
        /// Create a StudentChecklist in the database
        /// </summary>
        /// <param name="checklist">The checklist to create</param>
        /// <returns>The created checklist</returns>
        public async Task<StudentFinancialAidChecklist> CreateStudentChecklistAsync(StudentFinancialAidChecklist checklist)
        {
            if (checklist == null)
            {
                throw new ArgumentNullException("checklist");
            }
            if (checklist.ChecklistItems == null || !checklist.ChecklistItems.Any())
            {
                var message = "StudentChecklistItems are required to create a StudentChecklist";
                logger.Error(message);
                throw new ArgumentException(message);
            }

            var createStudentChecklistRequest = new CreateStudentChecklistRequest()
            {
                StudentId = checklist.StudentId,
                Year = checklist.AwardYear,
                Items = checklist.ChecklistItems.Select(c => new Items() { ChecklistItems = c.Code, DisplayActions = c.ControlStatusCode }).ToList()
            };

            var response = await transactionInvoker.ExecuteAsync<CreateStudentChecklistRequest, CreateStudentChecklistResponse>(createStudentChecklistRequest);
            if (response == null)
            {
                var message = "Error getting CreateStudentChecklist transaction response from Colleague";
                logger.Error(message);
                throw new ApplicationException(message);
            }

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                var message = string.Format("Error creating checklist for {0}. Error from CTX: {1}", checklist.ToString(), response.ErrorMessage);
                logger.Error(message);
                throw new ApplicationException(message);
            }

            return await GetStudentChecklistAsync(checklist.StudentId, checklist.AwardYear);
        }

        /// <summary>
        /// Get a list of StudentFinancialAidChecklist objects for the given years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get checklists</param>
        /// <param name="years">The years for which to get checklists. If a student doesn't have a checklist 
        /// for one of the years, no checklist for that year will be returned</param>
        /// <returns></returns>
        public async Task<IEnumerable<StudentFinancialAidChecklist>> GetStudentChecklistsAsync(string studentId, IEnumerable<string> years)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (years == null)
            {
                throw new ArgumentNullException("years");
            }

            var studentChecklistList = new List<StudentFinancialAidChecklist>();
            foreach (var year in years)
            {
                try
                {
                    studentChecklistList.Add(await GetStudentChecklistAsync(studentId, year));
                }
                catch (Exception e)
                {
                    logger.Debug(e, string.Format("Unable to get student checklist for studentId {0}, awardYear {1}", studentId, year));
                }
            }

            return studentChecklistList;
        }
        /// <summary>
        /// Gets or sets a student's CS.HOUSING.CODE for a given year
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="awardYear"></param>
        /// <param name="housingCode"></param>
        /// <param name="retrieveOption"></param>
        /// <returns></returns>
        public async Task<string> GetSetHousingOptionAsync(string studentId, string awardYear, string housingCode, string retrieveOption)
        {
            try
            {
                var housingCtxRequest = new InteractWithHousingRequest()
                {
                    StudentId = studentId,
                    AwardYear = awardYear,
                    HousingCode = housingCode,
                    GetSet = retrieveOption
                };

                var response = await transactionInvoker.ExecuteAsync<InteractWithHousingRequest, InteractWithHousingResponse>(housingCtxRequest);
                if (response.ErrorMessages != "")
                {
                    //Log error messages but return anyway since a null housing code will be provided in case of error
                    logger.Error(response.ErrorMessages);
                    throw new ColleagueException(response.ErrorMessages);
                }
                return response.HousingCode;
            }
            catch (ColleagueException ce)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Unable to {0} housing option for {1}*{2}", retrieveOption, studentId, awardYear));
                return null;
            }

        }
    }
}
