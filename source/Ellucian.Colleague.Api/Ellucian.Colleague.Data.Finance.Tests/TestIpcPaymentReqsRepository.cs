using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestIpcPaymentReqsRepository
    {
        private static Collection<IpcPaymentReqs> _ipcPaymentReqs = new Collection<IpcPaymentReqs>();
        public static Collection<IpcPaymentReqs> IpcPaymentReqs
        {
            get
            {
                if (_ipcPaymentReqs.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _ipcPaymentReqs;
            }
        }

        private static void GenerateDataContracts()
        {
            string[,] deferralOptionsData = GetDeferralOptionsData();
            int deferralOptionsCount = deferralOptionsData.Length / 4;
            Dictionary<string, List<IpcPaymentReqsIpcpDeferrals>> deferralOptionDict = new Dictionary<string, List<IpcPaymentReqsIpcpDeferrals>>();
            List<IpcPaymentReqsIpcpDeferrals> defList = new List<IpcPaymentReqsIpcpDeferrals>();
            for (int i = 0; i < deferralOptionsCount; i++)
            {
                string key = deferralOptionsData[i, 0].Trim();
                DateTime effStart = DateTime.Parse(deferralOptionsData[i, 1]);
                DateTime? effEnd = !String.IsNullOrEmpty(deferralOptionsData[i, 2].Trim()) ? DateTime.Parse(deferralOptionsData[i, 2]) : DateTime.MaxValue;
                decimal defPct = Decimal.Parse(deferralOptionsData[i, 3]);
                if (deferralOptionDict.TryGetValue(key, out defList))
                {
                    deferralOptionDict[key].Add(new IpcPaymentReqsIpcpDeferrals(effStart, effEnd, defPct));
                }
                else
                {
                    deferralOptionDict.Add(key, new List<IpcPaymentReqsIpcpDeferrals>() { new IpcPaymentReqsIpcpDeferrals(effStart, effEnd, defPct) });
                }
            }

            string[,] paymentPlanOptionsData = GetPaymentPlanOptionsData();
            int paymentPlanOptionsCount = paymentPlanOptionsData.Length / 5;
            Dictionary<string, List<IpcPaymentReqsIpcpPayPlans>> paymentPlanOptionDict = new Dictionary<string, List<IpcPaymentReqsIpcpPayPlans>>();
            List<IpcPaymentReqsIpcpPayPlans> ppList = new List<IpcPaymentReqsIpcpPayPlans>();
            for (int i = 0; i < paymentPlanOptionsCount; i++)
            {
                string key = paymentPlanOptionsData[i, 0].Trim();
                DateTime effStart = DateTime.Parse(paymentPlanOptionsData[i, 1]);
                DateTime? effEnd = !String.IsNullOrEmpty(paymentPlanOptionsData[i, 2].Trim()) ? DateTime.Parse(paymentPlanOptionsData[i, 2]) : DateTime.MaxValue;
                string templateId = paymentPlanOptionsData[i, 3];
                DateTime? planStartDate = !String.IsNullOrEmpty(paymentPlanOptionsData[i, 4].Trim()) ? DateTime.Parse(paymentPlanOptionsData[i, 4]) : DateTime.Today;
                if (paymentPlanOptionDict.TryGetValue(key, out ppList))
                {
                    paymentPlanOptionDict[key].Add(new IpcPaymentReqsIpcpPayPlans(effStart, effEnd, templateId, planStartDate));
                }
                else
                {
                    paymentPlanOptionDict.Add(key, new List<IpcPaymentReqsIpcpPayPlans>() { new IpcPaymentReqsIpcpPayPlans(effStart, effEnd, templateId, planStartDate) });
                }
            }

            string[,] paymentRequirementsData = GetPaymentRequirementsData();
            int paymentRequirementsCount = paymentRequirementsData.Length / 4;
            for (int i = 0; i < paymentRequirementsCount; i++)
            {
                string id = paymentRequirementsData[i, 0].Trim();
                string term = paymentRequirementsData[i, 1].Trim();
                string rule = paymentRequirementsData[i, 2].Trim();
                int order = Int32.Parse(paymentRequirementsData[i, 3]);

                _ipcPaymentReqs.Add(new IpcPaymentReqs()
                {
                    Recordkey = id,
                    IpcpTerm = term,
                    IpcpEligibilityRule = rule,
                    IpcpRuleEvalOrder = order,
                    IpcpDeferralsEntityAssociation = deferralOptionDict[id],
                    IpcpPayPlansEntityAssociation = paymentPlanOptionDict[id],
                });
            }
        }
        private static string[,] GetPaymentRequirementsData()
        {
            string[,] paymentRequirementsData = {
                                                // ID    Term      Rule ID  Eval Order
                                                { "106","2014/SP","   ",   "0" },
                                                { "108","2014/SP","WMK",   "1" },
                                                { "109","2013/FA","   ",   "0" },
                                                { "110","2013/FA","WMK",   "1" },
                                                { "111","2014/SP","NRA",   "2" },
                                            };
            return paymentRequirementsData;
        }

        private static string[,] GetDeferralOptionsData()
        {
            string[,] deferralOptionsData = {//Pmt Req   Eff Start     Eff End       Pct
                                            { "106",    "01/01/2014", "01/31/2014", "100" },
                                            { "106",    "02/01/2014", "02/28/2014", "75"  },
                                            { "106",    "03/01/2014", "          ", "0"   },
                                            { "108",    "03/01/2014", "04/30/2014", "75"  },
                                            { "108",    "05/01/2014", "          ", "50"  },
                                            { "109",    "09/01/2013", "09/19/2013", "100" },
                                            { "109",    "09/20/2013", "09/30/2013", "80"  },
                                            { "109",    "10/01/2013", "10/14/2013", "50"  },
                                            { "109",    "10/15/2013", "          ", "0"   },
                                            { "110",    "08/01/2013", "          ", "0"   },
                                            { "111",    "01/01/2014", "02/01/2014", "80"  }
                                        };
            return deferralOptionsData;
        }

        private static string[,] GetPaymentPlanOptionsData()
        {
            string[,] paymentPlanOptionsData = { // Pmt Req  Eff Start     Eff End       Template Id  Plan Start Date
                                                { "106",    "01/01/2014", "01/31/2014", "TEMPLATE1", "02/01/2014"  },
                                                { "106",    "02/01/2014", "02/28/2014", "TEMPLATE2", "03/01/2014"  },
                                                { "108",    "03/01/2014", "04/30/2014", "TEMPLATE1", "05/01/2014"  },
                                                { "108",    "05/01/2014", "          ", "TEMPLATE2", "05/31/2014"  },
                                                { "109",    "09/01/2013", "09/19/2013", "TEMPLATE1", "09/20/2013"  },
                                                { "109",    "09/20/2013", "09/30/2013", "TEMPLATE2", "10/01/2013"  },
                                                { "109",    "10/01/2013", "10/14/2013", "TEMPLATE3", "10/15/2013"  },
                                                { "109",    "10/15/2013", "          ", "TEMPLATE4", "10/16/2013"  },
                                                { "110",    "09/15/2013", "          ", "         ", "          "  },
                                                { "111",    "01/01/2014", "02/01/2014", "TEMPLATE1", "02/14/2014"  }
                                               };
            return paymentPlanOptionsData;
        }
    }
}
