/*Copyright 2015-2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests
{
    public class TestStudentChecklistRepository : IStudentChecklistRepository
    {

        public class YsStudentRecord
        {
            public string awardYear;
            public List<ChecklistItemsRecord> checklistItems;
        }

        public class ChecklistItemsRecord
        {
            public string checklistItem;
            public string displayAction;            
        }

        public List<YsStudentRecord> ysStudentRecords = new List<YsStudentRecord>()
        {
            new YsStudentRecord()
            {
               awardYear = "2012",
               checklistItems = new List<ChecklistItemsRecord>()
               {
                  new ChecklistItemsRecord()
                  {
                      checklistItem = "FAFSA",
                      displayAction = "Q"
                  },
                  new ChecklistItemsRecord()
                  {
                      checklistItem = "PROFILE",
                      displayAction = "R"
                  }
               }
            },
            
            new YsStudentRecord()
            {
               awardYear = "2013",
               checklistItems = new List<ChecklistItemsRecord>()
               {
                  new ChecklistItemsRecord()
                  {
                      checklistItem = "FAFSA",
                      displayAction = "Q"
                  },
                  new ChecklistItemsRecord()
                  {
                      checklistItem = "PROFILE",
                      displayAction = "R"
                  },
                  new ChecklistItemsRecord()
                  {
                      checklistItem = "ACCAWDPKG",
                      displayAction = "Q"
                  }
               }
            },
            
            new YsStudentRecord()
            {
               awardYear = "2014",
               checklistItems = new List<ChecklistItemsRecord>()
               {
                  new ChecklistItemsRecord()
                  {
                      checklistItem = "FAFSA",
                      displayAction = "Q"
                  },
                  new ChecklistItemsRecord()
                  {
                      checklistItem = "APPLRVW",
                      displayAction = ""
                  }
               }
            },
            new YsStudentRecord()
            {
                awardYear = "2015",
                checklistItems = new List<ChecklistItemsRecord>()
                {
                    new ChecklistItemsRecord()
                    {
                        checklistItem = "FAFSA",
                        displayAction = "R"
                    },
                    new ChecklistItemsRecord()
                    {
                        checklistItem = "CMPLREQDOC",
                        displayAction = "S"
                    },
                    new ChecklistItemsRecord()
                    {
                        checklistItem = "SIGNAWDLTR",
                        displayAction = "Q"
                    },
                }
            },
            new YsStudentRecord(){
                awardYear = "2016"
            }
        };

        public class CreateTransactionRequestRecord
        {
            public string studentId;
            public string awardYear;
            public List<ChecklistItemsRecord> checklistItems;
        }    

        public void createChecklistHelper(CreateTransactionRequestRecord request)
        {
            var ysRecord = ysStudentRecords.FirstOrDefault(ys => ys.awardYear == request.awardYear);
            if (ysRecord == null)
            {
                ysRecord = new YsStudentRecord()
                {
                    awardYear = request.awardYear
                };
                ysStudentRecords.Add(ysRecord);
            }
            ysRecord.checklistItems = request.checklistItems;
        }

        public class CreateTransactionResponseRecord
        {
            public string errorMessage;
        }

        public CreateTransactionResponseRecord createTransactionResponseData = new CreateTransactionResponseRecord()
        {
            errorMessage = ""
        };        
        
        public Task<StudentFinancialAidChecklist> GetStudentChecklistAsync(string studentId, string year)
        {
            var ysRecord = ysStudentRecords.FirstOrDefault(y => y.awardYear == year);
            if (ysRecord == null || ysRecord.checklistItems == null || !ysRecord.checklistItems.Any())
            {
                throw new KeyNotFoundException(string.Format("No checklist for student {0} for year {1}", studentId, year));
            }
            var studentChecklist = new StudentFinancialAidChecklist(studentId, year)
                {
                    ChecklistItems = ysRecord.checklistItems.Select(c => 
                        new StudentChecklistItem(c.checklistItem, TranslateDisplayAction(c.displayAction))).ToList()
                };

            return Task.FromResult(studentChecklist);
        }

        private ChecklistItemControlStatus TranslateDisplayAction(string displayAction)
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


        public Task<StudentFinancialAidChecklist> CreateStudentChecklistAsync(StudentFinancialAidChecklist checklist)
        {
            var ysRecord = ysStudentRecords.FirstOrDefault(ys => ys.awardYear == checklist.AwardYear);
            if (ysRecord == null)
            {
                ysRecord = new YsStudentRecord()
                    {
                        awardYear = checklist.AwardYear
                    };  
                ysStudentRecords.Add(ysRecord);
            }
            ysRecord.checklistItems = checklist.ChecklistItems.Select(c =>
                    new ChecklistItemsRecord()
                    {
                        checklistItem = c.Code,
                        displayAction = c.ControlStatusCode,
                    }).ToList();


            return GetStudentChecklistAsync(checklist.StudentId, checklist.AwardYear);
        }

        public Task<IEnumerable<StudentFinancialAidChecklist>> GetStudentChecklistsAsync(string studentId, IEnumerable<string> years)
        {
            var studentChecklistList = new List<StudentFinancialAidChecklist>();
            foreach (var year in years)
            {
                try
                {
                    studentChecklistList.Add(GetStudentChecklistAsync(studentId, year).Result);
                }
                catch(Exception)
                {

                }

            }
            return Task.FromResult(studentChecklistList.AsEnumerable());
        }
    }
}
