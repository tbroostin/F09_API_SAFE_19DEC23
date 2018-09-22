using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public static class TestApprovalDocumentsRepository
    {
        private static Collection<ApprovalDocuments> _apprDocResponses = new Collection<ApprovalDocuments>();
        public static Collection<ApprovalDocuments> ApprDocResponses
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
            var approvalDocumentsText = GetApprovalDocumentsText();
            var approvalDocumentsList = new List<TextList>();
            for (int i = 0; i < approvalDocumentsText.Length / 2; i++)
            {
                string id = approvalDocumentsText[i,0].Trim();
                string text = approvalDocumentsText[i,1].Trim();
                approvalDocumentsList.Add(new TextList(id, text));
            }

            var approvalDocumentsData = GetApprovalDocumentsData();
            for (int i = 0; i < approvalDocumentsData.Length / 2; i++)
            {
                string id = approvalDocumentsData[i, 0].Trim();
                string personId = approvalDocumentsData[i, 1].Trim();
                string text = string.Join("\r\n", approvalDocumentsList.Where(x => x.Id == id).Select(x => x.Text)).Trim();
                _apprDocResponses.Add(new ApprovalDocuments()
                {
                    Recordkey = id,
                    ApdPersonId = personId,
                    ApdText = text
                });
            }
        }

        private static string[,] GetApprovalDocumentsData()
        {
            string[,] approvalDocumentsTable = {
                                                    {"1", ""},
                                                    {"2", ""},
                                                    {"3", ""},
                                                    {"4", ""},
                                                    {"5", ""},
                                                    {"6", ""},
                                                    {"7", ""}
                                                };
            return approvalDocumentsTable;
        }

        private static string[,] GetApprovalDocumentsText()
        {
            string[,] approvalDocumentsText = {
                                                   {"1", "Line 1 of text."},
                                                   {"1", "Line 2 of text."},
                                                   {"1", ""},
                                                   {"1", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""},
                                                   {"", ""}
                                               };
            return approvalDocumentsText;
        }

        private class TextList
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public TextList(string id, string text)
            {
                Id = id;
                Text = text;
            }
        }
    }
}
