// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestArPayPlansRepository
    {
        private static Collection<ArPayPlans> _arPayPlans = new Collection<ArPayPlans>();
        public static Collection<ArPayPlans> ArPayPlans
        {
            get
            {
                if (_arPayPlans.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _arPayPlans;
            }
        }

        /// <summary>
        /// Performs data setup for payment plans to be used in tests
        /// </summary>
        private static void GenerateDataContracts()
        {
            // Make sure we start with a blank slate
            _arPayPlans.Clear();
            GenerateArPayPlans();
        }


        private static void GenerateArPayPlans()
        {
            string[,] planStatusData = GetPaymentPlanStatusesData();
            int statusCount = planStatusData.Length / 3;
            Dictionary<string, List<ArPayPlansArplStatuses>> statusDictionary = new Dictionary<string, List<ArPayPlansArplStatuses>>();
            for (int i = 0; i < statusCount; i++)
            {
                // Parse out the data
                string planId = planStatusData[i, 0].Trim();
                string status = planStatusData[i, 1].Trim();
                DateTime date = DateTime.Parse(planStatusData[i, 2].Trim());
                if (statusDictionary.ContainsKey(planId))
                {
                    statusDictionary[planId].Add(new ArPayPlansArplStatuses() { ArplStatusAssocMember = status, ArplStatusDateAssocMember = date });
                }
                else
                {
                    statusDictionary.Add(planId, new List<ArPayPlansArplStatuses>() { new ArPayPlansArplStatuses() { ArplStatusAssocMember = status, ArplStatusDateAssocMember = date } });
                }
            }


            string[,] paymentPlansData = GetPaymentPlansData();
            var arPayPlanItems = TestArPayPlanItemsRepository.ArPayPlanItems;
            var arPayInvoiceItems = TestArPayInvoiceItemsRepository.ArPayInvoiceItems;

            int paymentPlansCount = paymentPlansData.Length / 16;
            for (int i = 0; i < paymentPlansCount; i++)
            {
                // Parse out the data
                string id = paymentPlansData[i, 0].Trim();
                string templateId = paymentPlansData[i, 1].Trim();
                string personId = paymentPlansData[i, 2].Trim();
                string receivableTypeCode = paymentPlansData[i, 3].Trim();
                string termId = paymentPlansData[i, 4].Trim();
                decimal originalAmount = Decimal.Parse(paymentPlansData[i, 5].Trim());
                DateTime firstDueDate = DateTime.Parse(paymentPlansData[i, 6].Trim());
                decimal currentAmount = Decimal.Parse(paymentPlansData[i, 7].Trim());
                string frequency = paymentPlansData[i, 8].Trim();
                int? numberOfPayments = !String.IsNullOrEmpty(paymentPlansData[i, 9].Trim()) ? Int32.Parse(paymentPlansData[i, 9]) : (int?)null;
                decimal? setupChargeAmount = !String.IsNullOrEmpty(paymentPlansData[i, 10].Trim()) ? Decimal.Parse(paymentPlansData[i, 10]) : (decimal?)null;
                decimal? setupChargePercent = !String.IsNullOrEmpty(paymentPlansData[i, 11].Trim()) ? Decimal.Parse(paymentPlansData[i, 11]) : (decimal?)null;
                decimal? downPaymentPercent = !String.IsNullOrEmpty(paymentPlansData[i, 12].Trim()) ? Decimal.Parse(paymentPlansData[i, 12]) : (decimal?)null;
                int? graceDays = !String.IsNullOrEmpty(paymentPlansData[i, 13].Trim()) ? Int32.Parse(paymentPlansData[i, 13]) : (int?)null;
                decimal? lateChargeAmount = !String.IsNullOrEmpty(paymentPlansData[i, 14].Trim()) ? Decimal.Parse(paymentPlansData[i, 14]) : (decimal?)null;
                decimal? lateChargePercent = !String.IsNullOrEmpty(paymentPlansData[i, 15].Trim()) ? Decimal.Parse(paymentPlansData[i, 15]) : (decimal?)null;

                var planItems = arPayPlanItems.Where(x => x.ArpliPayPlan == id).Select(x => x.Recordkey).ToList();
                var planCharges = arPayInvoiceItems.Where(x => x.Recordkey.Split('*')[0] == id).Select(x => x.Recordkey.Split('*')[1]).ToList();
                var planStatuses = statusDictionary[id];

                ArPayPlans payPlan = new ArPayPlans()
                {
                    Recordkey = id,
                    ArplPayPlanTemplate = templateId,
                    ArplPersonId = personId,
                    ArplArType = receivableTypeCode,
                    ArplTerm = termId,
                    ArplOrigAmt = originalAmount,
                    ArplFirstDueDate = firstDueDate,
                    ArplAmt = currentAmount,
                    ArplFrequency = frequency,
                    ArplNoPayments = numberOfPayments,
                    ArplChargeAmt = setupChargeAmount,
                    ArplChargePct = setupChargePercent,
                    ArplDownPayPct = downPaymentPercent,
                    ArplGraceNoDays = graceDays,
                    ArplLateChargeAmt = lateChargeAmount,
                    ArplPayPlanItems = planItems,
                    ArplInvoiceItems = planCharges,
                    ArplStatusesEntityAssociation = planStatuses
                };

                _arPayPlans.Add(payPlan);
            }
        }

        /// <summary>
        /// Gets payment plan raw data
        /// </summary>
        /// <returns>String array of raw payment plan data</returns>
        private static string[,] GetPaymentPlansData()
        {
            string[,] paymentPlansTable = {
                                            // ID     TemplateId  PersonId RecType TermId     OrigAmt  First Due Date  CurrAmt  Freq  NoP SetupAmt  SetupPct DownPay  Grace  LateAmt  LatePct
                                            { "   1", "MONTHLY", "0003500", "01", "2013/SP", "8080.00", "01/19/2022", "6790.00", "M", "4", "25.00", "10.00", "20.00",   "0", "20.00", " 8.00" },
                                            { "1006", "DEFAULT", "0010456", "01", "2024/SP", " 550.00", "04/01/2024", " 550.00", "W", "3", "60.00", "     ", "10.00",   "3", " 5.00", "     " },
                                            { "1111", "DEFAULT", "0000895", "01", "2014/FA", "1000.00", "12/31/2014", "1000.00", "Y", "4", "50.00", "     ", "10.00",   "0", " 5.00", "     " },
                                            { "2222", "DEFAULT", "0003315", "01", "2014/FA", "1000.00", "11/14/2014", "   0.00", "B", "4", "50.00", "     ", "10.00",   "0", " 5.00", "     " },
                                            { "3333", "DEFAULT", "0003315", "01", "2014/FA", "1000.00", "11/14/2014", "1000.00", "B", "4", "50.00", "     ", "10.00",   "0", " 5.00", "     " },
                                            { "4444", "DEFAULT", "0003315", "01", "2014/FA", "1000.00", "11/14/2014", "   0.00", "X", "4", "50.00", "     ", "10.00",   "0", " 5.00", "     " },
                                            { "5555", "DEFAULT", "0003315", "01", "2014/FA", "1000.00", "11/14/2014", "1000.00", " ", "4", "50.00", "     ", "10.00",   "0", " 5.00", "     " },
                                            { "6666", "DEFAULT", "0003315", "01", "2014/FA", "1000.00", "11/14/2014", "   0.00", "B", "4", "50.00", "     ", "10.00",   "0", " 5.00", "     " }
                                          };
            return paymentPlansTable;
        }

        /// <summary>
        /// Gets payment plan status raw data
        /// </summary>
        /// <returns>String array of raw payment plan status data</returns>
        private static string[,] GetPaymentPlanStatusesData()
        {
            string[,] payPlanStatusesData = {   //PlanID, Sts, Date
                                                { "   1", "O", "10/31/2001" },
                                                { "1006", "C", "02/28/2014" },
                                                { "1006", "O", "02/14/2014" },
                                                { "1111", "O", "12/14/2014" },
                                                { "2222", "P", "11/14/2014" },
                                                { "3333", "X", "11/14/2014" },
                                                { "4444", "P", "11/14/2014" },
                                                { "5555", "P", "11/14/2014" },
                                                { "6666", " ", "11/14/2014" }
                                            };
            return payPlanStatusesData;
        }
    }
}
