/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using slf4net;

namespace Ellucian.Colleague.Data.FinancialAid.Repositories
{
    /// <summary>
    /// Creates StudentChecklistItems from database records
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class StudentChecklistItemRepository : BaseColleagueRepository, IStudentChecklistItemRepository
    {
        /// <summary>
        /// Constructor for the StudentChecklistItem Repository
        /// </summary>
        /// <param name="cacheProvider">cacheProvider</param>
        /// <param name="transactionFactory">transactionFactory</param>
        /// <param name="logger">logger</param>
        public StudentChecklistItemRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger)
            : base(cacheProvider, transactionFactory, logger)
        {

        }

        /// <summary>
        /// Get StudentChecklistItems for the given award years
        /// </summary>
        /// <param name="studentId">The Colleague PERSON id of the student for whom to get checklist items</param>
        /// <param name="studentAwardYears">The StudentAwardYears for which to get items</param>
        /// <returns>A list of StudentChecklistItems objects for the given student id and award years</returns>
        public IEnumerable<StudentChecklistItem> GetStudentChecklist(string studentId, IEnumerable<StudentAwardYear> studentAwardYears)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (studentAwardYears == null || studentAwardYears.Count() == 0)
            {
                logger.Info(string.Format("Cannot get checklist items for student {0} with no studentAwardYears", studentId));
                return new List<StudentChecklistItem>();
            }

            var studentChecklistItems = new List<StudentChecklistItem>();
            foreach (var studentAwardYear in studentAwardYears)
            {
                var ysAcyrFile = "YS." + studentAwardYear.Code;
                var ysRecord = DataReader.ReadRecord<YsAcyr>(ysAcyrFile, studentId);
                if (ysRecord != null && ysRecord.ChecklistItemsEntityAssociation != null)
                {
                    foreach (var ysChecklistEntity in ysRecord.ChecklistItemsEntityAssociation)
                    {
                        var checklistRecord = new StudentChecklistItem(ysChecklistEntity.YsChecklistItemsAssocMember);

                        if (ysChecklistEntity.YsDisplayActionAssocMember != null)
                        {
                            switch (ysChecklistEntity.YsDisplayActionAssocMember.ToUpper())
                            {
                                case "R":
                                    checklistRecord.ControlStatus = ChecklistItemControlStatus.RemovedFromChecklist;
                                    break;
                                case "S":
                                    checklistRecord.ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater;
                                    break;
                                case "Q":
                                    checklistRecord.ControlStatus = ChecklistItemControlStatus.CompletionRequired;
                                    break;
                                default:
                                    checklistRecord.ControlStatus = ChecklistItemControlStatus.CompletionRequired;
                                    break;
                            }
                        }
                        studentChecklistItems.Add(checklistRecord);
                    }
                }
            }

            return studentChecklistItems;
        }
    }
}
