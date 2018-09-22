using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestIpcRegApprovalsRepository
    {
        private static Collection<IpcRegApprovals> _ipcRegApprovals = new Collection<IpcRegApprovals>();
        public static Collection<IpcRegApprovals> IpcRegApprovals
        {
            get
            {
                if (_ipcRegApprovals.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _ipcRegApprovals;
            }
        }

        private static void GenerateDataContracts()
        {
            string[,] ipcRegApprovalsSectionData = GetIpcRegApprovalsSectionData();
            int ipcRegApprovalsSectionCount = ipcRegApprovalsSectionData.Length / 2;
            Dictionary<string, List<string>> sectionDict = new Dictionary<string, List<string>>();
            List<string> sectionList = new List<string>();
            for (int i = 0; i < ipcRegApprovalsSectionCount; i++)
            {
                string key = ipcRegApprovalsSectionData[i, 0].Trim();
                string sectionId = ipcRegApprovalsSectionData[i, 1].Trim();
                if (sectionDict.TryGetValue(key, out sectionList))
                {
                    sectionDict[key].Add(sectionId);
                }
                else
                {
                    sectionDict.Add(key, new List<string>() { sectionId });
                }
            }

            string[,] ipcRegApprovalsInvoiceData = GetIpcRegApprovalsInvoiceData();
            int ipcRegApprovalsInvoiceCount = ipcRegApprovalsInvoiceData.Length / 2;
            Dictionary<string, List<string>> invoiceDict = new Dictionary<string, List<string>>();
            List<string> invoiceList = new List<string>();
            for (int i = 0; i < ipcRegApprovalsInvoiceCount; i++)
            {
                string key = ipcRegApprovalsInvoiceData[i, 0].Trim();
                string invoiceId = ipcRegApprovalsInvoiceData[i, 1].Trim();
                if (invoiceDict.TryGetValue(key, out invoiceList))
                {
                    invoiceDict[key].Add(invoiceId);
                }
                else
                {
                    invoiceDict.Add(key, new List<string>() { invoiceId });
                }
            }

            string[,] ipcRegApprovalsData = GetIpcRegApprovalsData();
            int regApprovalsCount = ipcRegApprovalsData.Length / 7;
            for (int i = 0; i < regApprovalsCount; i++)
            {
                string id = ipcRegApprovalsData[i, 0].Trim();
                string studentId = ipcRegApprovalsData[i, 1].Trim();
                string regId = ipcRegApprovalsData[i, 2].Trim();
                string termsRespId = ipcRegApprovalsData[i, 3].Trim();
                string ackDocId = ipcRegApprovalsData[i, 4].Trim();
                DateTime ackDate = DateTime.Parse(ipcRegApprovalsData[i, 5].Trim());
                DateTime ackTime = DateTime.Parse(ipcRegApprovalsData[i, 6].Trim());

                _ipcRegApprovals.Add(new IpcRegApprovals()
                {
                    Recordkey = id,
                    IpcraRegistration = regId,
                    IpcraTermsResponse = termsRespId,
                    IpcraAckDocument = ackDocId,
                    IpcraAckDate = ackDate,
                    IpcraAckTime = ackTime
                });
            }

            if (_ipcRegApprovals.Count > 0)
            {
                foreach (var ipcra in _ipcRegApprovals)
                {
                    ipcra.IpcraCourseSections = sectionDict[ipcra.Recordkey];
                    ipcra.IpcraArInvoices = invoiceDict[ipcra.Recordkey];
                }
            }
        }
        private static string[,] GetIpcRegApprovalsData()
        {
            string[,] ipcRegApprovalsData = {
                                                // ID   Student ID, IPC.REG, TERMS RESP, ACK DOC, ACK DATE, ACK TIME
                                                    {"1","0003900","2","1","1","08/21/2013","17:16:48"},
                                                    {"100","0003901","2","100","1","09/11/2013","09:43:45"},
                                                    {"1000","0005526","44","1000","34","11/22/2013","12:19:33"},
                                                    {"1001","0005527","44","1001","34","11/22/2013","12:19:33"},
                                                    {"1002","0005528","44","1002","34","11/22/2013","12:19:33"},
                                                    {"1003","0005529","44","1003","34","11/22/2013","12:19:33"},
                                                    {"1004","0005530","44","1004","34","11/22/2013","12:19:33"},
                                                    {"1005","0005531","44","1005","34","11/22/2013","12:19:33"},
                                                    {"1006","0005532","44","1006","34","11/22/2013","12:19:33"},
                                                    {"1007","0005533","44","1007","34","11/22/2013","12:19:33"},
                                                    {"1008","0005534","44","1008","34","11/22/2013","12:19:33"},
                                                    {"1009","0005535","44","1009","34","11/22/2013","12:19:33"}
                                            };
            return ipcRegApprovalsData;
        }

        private static string[,] GetIpcRegApprovalsSectionData()
        {
            string[,] ipcRegApprovalsSectionData = {// ID, Section ID
                                                    {"1","14875"},
                                                    {"1","15066"},
                                                    {"1","16768"},
                                                    {"1","16896"},
                                                    {"100","15003"},
                                                    {"100","16896"},
                                                    {"100","16768"},
                                                    {"1000","14952"},
                                                    {"1000","16823"},
                                                    {"1001","14952"},
                                                    {"1001","16823"},
                                                    {"1002","14952"},
                                                    {"1002","16823"},
                                                    {"1003","14952"},
                                                    {"1003","16823"},
                                                    {"1004","14952"},
                                                    {"1004","16823"},
                                                    {"1005","14952"},
                                                    {"1005","16823"},
                                                    {"1006","14952"},
                                                    {"1006","16823"},
                                                    {"1007","14952"},
                                                    {"1007","16823"},
                                                    {"1008","14952"},
                                                    {"1008","16823"},
                                                    {"1009","14952"},
                                                    {"1009","16823"}
                                        };
            return ipcRegApprovalsSectionData;
        }

        private static string[,] GetIpcRegApprovalsInvoiceData()
        {
            string[,] ipcRegApprovalsInvoiceData = { // ID   InvId
                                                    {"1","9579"},
                                                    {"100","8561"},
                                                    {"1000","9699"},
                                                    {"1000","9767"},
                                                    {"1001","9699"},
                                                    {"1001","9767"},
                                                    {"1002","9699"},
                                                    {"1002","9767"},
                                                    {"1003","9699"},
                                                    {"1003","9767"},
                                                    {"1004","9699"},
                                                    {"1004","9767"},
                                                    {"1005","9699"},
                                                    {"1005","9767"},
                                                    {"1006","9699"},
                                                    {"1006","9767"},
                                                    {"1007","9699"},
                                                    {"1007","9767"},
                                                    {"1008","9699"},
                                                    {"1008","9767"},
                                                    {"1009","9699"},
                                                    {"1009","9767"}
                                               };
            return ipcRegApprovalsInvoiceData;
        }
    }
}
