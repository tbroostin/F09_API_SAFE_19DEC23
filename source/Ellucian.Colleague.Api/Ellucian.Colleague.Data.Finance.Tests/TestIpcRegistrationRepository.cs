// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestIpcRegistrationRepository
    {
        private static Collection<IpcRegistration> _ipcRegistrations = new Collection<IpcRegistration>();
        public static Collection<IpcRegistration> IpcRegistrations
        {
            get
            {
                if (_ipcRegistrations.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _ipcRegistrations;
            }
        }

        private static void GenerateDataContracts()
        {
            string[,] ipcRegistrationActiveSectionData = GetIpcRegistrationActiveSectionData();
            int ipcRegApprovalsSectionCount = ipcRegistrationActiveSectionData.Length / 2;
            Dictionary<string, List<string>> activeSectionDict = new Dictionary<string, List<string>>();
            List<string> activeSectionList = new List<string>();
            for (int i = 0; i < ipcRegApprovalsSectionCount; i++)
            {
                string key = ipcRegistrationActiveSectionData[i, 0].Trim();
                string sectionId = ipcRegistrationActiveSectionData[i, 1].Trim();
                if (activeSectionDict.TryGetValue(key, out activeSectionList))
                {
                    activeSectionDict[key].Add(sectionId);
                }
                else
                {
                    activeSectionDict.Add(key, new List<string>() { sectionId });
                }
            }

            string[,] ipcRegistrationInvoiceData = GetIpcRegistrationInvoiceData();
            int ipcRegistrationInvoiceCount = ipcRegistrationInvoiceData.Length / 2;
            Dictionary<string, List<string>> invoiceDict = new Dictionary<string, List<string>>();
            List<string> invoiceList = new List<string>();
            for (int i = 0; i < ipcRegistrationInvoiceCount; i++)
            {
                string key = ipcRegistrationInvoiceData[i, 0].Trim();
                string invoiceId = ipcRegistrationInvoiceData[i, 1].Trim();
                if (invoiceDict.TryGetValue(key, out invoiceList))
                {
                    invoiceDict[key].Add(invoiceId);
                }
                else
                {
                    invoiceDict.Add(key, new List<string>() { invoiceId });
                }
            }

            string[,] ipcPaymentData = GetIpcrPaymentsData();
            int ipcrPaymentCount = ipcPaymentData.Length / 2;
            Dictionary<string, List<string>> paymentDict = new Dictionary<string, List<string>>();
            List<string> paymentList = new List<string>();
            for (int i = 0; i < ipcrPaymentCount; i++)
            {
                string key = ipcPaymentData[i, 0].Trim();
                string paymentId = ipcPaymentData[i, 1].Trim();
                if (paymentDict.TryGetValue(key, out paymentList))
                {
                    paymentDict[key].Add(paymentId);
                }
                else
                {
                    paymentDict.Add(key, new List<string>() { paymentId });
                }
            }

            string[,] ipcRegApprovalsData = GetIpcRegistrationData();
            int regApprovalsCount = ipcRegApprovalsData.Length / 5;
            for (int i = 0; i < regApprovalsCount; i++)
            {
                string id = ipcRegApprovalsData[i, 0].Trim();
                string studentId = ipcRegApprovalsData[i, 1].Trim();
                string termId = ipcRegApprovalsData[i, 2].Trim();
                string status = ipcRegApprovalsData[i, 3].Trim();
                string planId = ipcRegApprovalsData[i, 4].Trim();

                _ipcRegistrations.Add(new IpcRegistration()
                {
                    Recordkey = id,
                    IpcrStudent = studentId,
                    IpcrTerm = termId,
                    IpcrPayStatus = status,
                    IpcrArPayPlan = planId
                });
            }

            if (_ipcRegistrations.Count > 0)
            {
                foreach (var ipcr in _ipcRegistrations)
                {
                    if (activeSectionDict.ContainsKey(ipcr.Recordkey))
                    {
                        ipcr.IpcrActiveScs = activeSectionDict[ipcr.Recordkey];
                    }
                    if (invoiceDict.ContainsKey(ipcr.Recordkey))
                    {
                        ipcr.IpcrRegInvoices = invoiceDict[ipcr.Recordkey];
                    }
                    if (paymentDict.ContainsKey(ipcr.Recordkey))
                    {
                        ipcr.IpcrPayments = paymentDict[ipcr.Recordkey];
                    }
                }
            }
        }
        private static string[,] GetIpcRegistrationData()
        {
            string[,] ipcRegistrationData = {
                                                // ID   Student ID  TermID  Status  PlanId
                                                    {"10","0005141","2013/FA","COMPLETE"," "},
                                                    {"100","0010906","2014/FA","ACCEPT","1704"},
                                                    {"101","0003895","2014/FA","COMPLETE","1621"},
                                                    {"102","0005635","2014/FA","NEW"," "},
                                                    {"103","0003905","2014/FA","NEW"," "},
                                                    {"104","0003963","2014/FA","COMPLETE","1798"},
                                                    {"105","0004758","2014/FA","NEW"," "},
                                                    {"106","0005295","2014/FA","COMPLETE"," "},
                                                    {"11","0005134","2013/FA","NEW"," "},
                                                    {"12","0000304","2013/FA","NEW"," "},
                                                    {"13","0005200","2013/FA","ACCEPT"," "},
                                                    {"14","0003905","2013/FA","NEW"," "},
                                                    {"15","0004033","2013/FA","NEW"," "},
                                                    {"18","0005239","2014/SP","NEW"," "},
                                                    {"2","0003900","2013/FA","COMPLETE"," "},
                                                    {"44","0003939","2014/FA","ERROR"," "},
                                            };
            return ipcRegistrationData;
        }

        private static string[,] GetIpcRegistrationActiveSectionData()
        {
            string[,] ipcRegistrationActiveSectionData = {// ID, Section ID
                                            {"100","13509"},
                                            {"100","13510"},
                                            {"100","13511"},
                                            {"100","13512"},
                                            {"101","13623"},
                                            {"101","13624"},
                                            {"102","13632"},
                                            {"103","14336"},
                                            {"103","14337"},
                                            {"105","15009"},
                                            {"105","14818"},
                                            {"11","11715"},
                                            {"12","11717"},
                                            {"13","11721"},
                                            {"13","11722"},
                                            {"13","11758"},
                                            {"14","11736"},
                                            {"15","11740"},
                                            {"18","11831"},
                                            {"18","11833"},
                                            {"2","11661"},
                                            {"2","11660"},
                                            {"2","11745"},
                                            {"2","11755"},
                                            {"44","12221"},
                                        };
            return ipcRegistrationActiveSectionData;
        }

        private static string[,] GetIpcRegistrationInvoiceData()
        {
            string[,] ipcRegistrationInvoiceData = { // ID   InvId
                                                    {"100","12590"},
                                                    {"101","12798"},
                                                    {"102","12837"},
                                                    {"103","12980"},
                                                    {"105","13701"},
                                                    {"11","9676"},
                                                    {"12","8720"},
                                                    {"13","8985"},
                                                    {"13","8993"},
                                                    {"14","8941"},
                                                    {"15","9944"},
                                                    {"18","9145"},
                                                    {"2","9579"},
                                                    {"44","9759"},
                                               };
            return ipcRegistrationInvoiceData;
        }

        private static string[,] GetIpcrPaymentsData()
        {
            string[,] ipcrPaymentsData = { // ID   PmtId
                                            {"100","12590"},
                                            {"101","12798"},
                                            {"102","12837"},
                                            {"103","12980"},
                                            {"105","13701"},
                                            {"11","9676"},
                                            {"12","8720"},
                                            {"13","8985"},
                                            {"13","8993"},
                                            {"14","8941"},
                                            {"15","9944"},
                                            {"18","9145"},
                                            {"2","9579"},
                                            {"44","9759"}
                                        };
            return ipcrPaymentsData;
        }
        

    }
}
