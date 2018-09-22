using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public static class TestApprDocResponsesRepository
    {
        private static Collection<ApprDocResponses> _apprDocResponses = new Collection<ApprDocResponses>();
        public static Collection<ApprDocResponses> ApprDocResponses
        {
            get
            {
                if (_apprDocResponses.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _apprDocResponses;
            }
        }

        private static void GenerateDataContracts()
        {
            var approvalDocumentsData = GetApprovalDocumentsData();
            for (int i = 0; i < approvalDocumentsData.Length / 6; i++)
            {
                DateTime outDate, outTime;
                string id = approvalDocumentsData[i, 0].Trim();
                string personId = approvalDocumentsData[i, 1].Trim();
                string documentId = approvalDocumentsData[i, 2].Trim();
                DateTime apprDate = DateTime.TryParse(approvalDocumentsData[i, 3].Trim(), out outDate) ? outDate : default(DateTime);
                DateTime apprTime = DateTime.TryParse(approvalDocumentsData[i, 4].Trim(), out outTime) ? outTime : default(DateTime);
                string userId = approvalDocumentsData[i, 5].Trim();

                _apprDocResponses.Add(new ApprDocResponses()
                    {
                        Recordkey = id,
                        ApdrPersonId = personId,
                        ApdrApprovalDocument = documentId,
                        ApdrApproved = "Y",
                        ApdrDate = apprDate,
                        ApdrTime = apprTime,
                        ApdrUserid = userId
                    });
            }
        }

        private static string[,] GetApprovalDocumentsData()
        {
            string[,] approvalDocumentsTable = {
                                                    {"1", "", "1", "2014-01-15", "14:32:48", "jsmith1"},
                                                    {"2", "", "1", "", "", ""},
                                                    {"3", "", "1", "", "", ""},
                                                    {"4", "", "2", "", "", ""},
                                                    {"5", "", "3", "", "", ""},
                                                    {"6", "", "3", "", "", ""},
                                                    {"7", "", "3", "", "", ""}
                                                };
            return approvalDocumentsTable;
        }
    }
}
