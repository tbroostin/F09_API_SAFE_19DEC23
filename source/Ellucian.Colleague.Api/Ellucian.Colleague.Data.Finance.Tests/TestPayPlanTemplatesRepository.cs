// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestPayPlanTemplatesRepository
    {
        private static Collection<PayPlanTemplates> _payPlanTemplates = new Collection<PayPlanTemplates>();
        public static Collection<PayPlanTemplates> PayPlanTemplates 
        { 
            get 
            {
                if (_payPlanTemplates.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _payPlanTemplates; 
            }
        }

        /// <summary>
        /// Performs data setup for payment plan templates to be used in tests
        /// </summary>
        private static void GenerateDataContracts()
        {
            string[,] paymentPlanTemplatesData = GetPaymentPlanTemplatesData();
            int paymentPlanTemplatesCount = paymentPlanTemplatesData.Length / 29;
            for (int i = 0; i < paymentPlanTemplatesCount; i++)
            {
                // Parse out the data
                string id = paymentPlanTemplatesData[i, 0].Trim();
                string isActive = paymentPlanTemplatesData[i, 1].Trim();
                string description = paymentPlanTemplatesData[i, 2].Trim();
                string frequency = paymentPlanTemplatesData[i, 3].Trim();
                int? numberOfPayments = !string.IsNullOrEmpty(paymentPlanTemplatesData[i, 4].Trim()) ? Int32.Parse(paymentPlanTemplatesData[i, 4]) : (int?)null;
                decimal minPlanAmount = Decimal.Parse(paymentPlanTemplatesData[i, 5].Trim());
                decimal? maxPlanAmount = !string.IsNullOrEmpty(paymentPlanTemplatesData[i, 6].Trim()) ? Decimal.Parse(paymentPlanTemplatesData[i, 6]) : (decimal?)null;
                string inclSetupFee = paymentPlanTemplatesData[i, 7].Trim();
                string calcOnPlanBal = paymentPlanTemplatesData[i, 8].Trim();
                string subtractFa = paymentPlanTemplatesData[i, 9].Trim();
                string autoCalcAmt = paymentPlanTemplatesData[i, 10].Trim();
                string autoModify = paymentPlanTemplatesData[i, 11].Trim();
                string modMethod = paymentPlanTemplatesData[i, 12].Trim();
                decimal? setupAmount = !string.IsNullOrEmpty(paymentPlanTemplatesData[i, 13].Trim()) ? Decimal.Parse(paymentPlanTemplatesData[i, 13]) : (decimal?)null;
                decimal? setupPercent = !string.IsNullOrEmpty(paymentPlanTemplatesData[i, 14].Trim()) ? Decimal.Parse(paymentPlanTemplatesData[i, 14]) : (decimal?)null;
                string setupCode = paymentPlanTemplatesData[i, 15].Trim();
                decimal? downPayPercent = !string.IsNullOrEmpty(paymentPlanTemplatesData[i, 16].Trim()) ? Decimal.Parse(paymentPlanTemplatesData[i, 16]) : (decimal?)null;
                int? downPayDays = !string.IsNullOrEmpty(paymentPlanTemplatesData[i, 17].Trim()) ? Int32.Parse(paymentPlanTemplatesData[i, 17]) : (int?)null;
                int? graceDays = !string.IsNullOrEmpty(paymentPlanTemplatesData[i, 18].Trim()) ? Int32.Parse(paymentPlanTemplatesData[i, 18]) : (int?)null;
                decimal? lateAmount = !string.IsNullOrEmpty(paymentPlanTemplatesData[i, 19].Trim()) ? Decimal.Parse(paymentPlanTemplatesData[i, 19]) : (decimal?)null;
                string lateAmountCode = paymentPlanTemplatesData[i, 20].Trim();
                decimal? latePercent = !string.IsNullOrEmpty(paymentPlanTemplatesData[i, 21].Trim()) ? Decimal.Parse(paymentPlanTemplatesData[i, 21]) : (decimal?)null;
                string latePercentCode = paymentPlanTemplatesData[i, 22].Trim();
                string termsDocumentId = paymentPlanTemplatesData[i, 23].Trim();
                List<string> allowedArTypes = string.IsNullOrEmpty(paymentPlanTemplatesData[i, 24].Trim()) ? new List<string>() : paymentPlanTemplatesData[i, 24].Trim().Split(';').ToList();
                List<string> invExclRules = string.IsNullOrEmpty(paymentPlanTemplatesData[i, 25].Trim()) ? new List<string>() : paymentPlanTemplatesData[i, 25].Trim().Split(';').ToList();
                List<string> inclArCodes = string.IsNullOrEmpty(paymentPlanTemplatesData[i, 26].Trim()) ? new List<string>() : paymentPlanTemplatesData[i, 26].Trim().Split(';').ToList();
                List<string> exclArCodes = string.IsNullOrEmpty(paymentPlanTemplatesData[i, 27].Trim()) ? new List<string>() : paymentPlanTemplatesData[i, 27].Trim().Split(';').ToList();
                string customFreqSubr = paymentPlanTemplatesData[i, 28].Trim();

                PayPlanTemplates payPlanTemplate = new PayPlanTemplates()
                {
                    Recordkey = id,
                    PptActiveFlag = isActive,
                    PptDescription = description,
                    PptFrequency = frequency,
                    PptNoPayments = numberOfPayments,
                    PptMinPlanAmt = minPlanAmount,
                    PptMaxPlanAmt = maxPlanAmount,
                    PptPrepaySetupFlag = inclSetupFee,
                    PptCalcOnBalFlag = calcOnPlanBal,
                    PptSubtractAnticipatedFa = subtractFa,
                    PptCalcAmtFlag = autoCalcAmt,
                    PptRebillModifyFlag = autoModify,
                    PptModifyMethod = modMethod,
                    PptPayFrequencySubr = customFreqSubr,
                    PptAllowedArTypes = (id == "REGONLY") ? new List<string>() { "01", "02"} : null,
                    PptIncludeArCodes = (id == "BIWEEKLY") ? new List<string>() { "TUIFT", "TUIPT", "MATFE", "TECFE"} : null,
                    PptExcludeArCodes = (id == "REGONLY") ? new List<string>() { "LAWFE" } : null,
                    PptInvoiceExclRules = (id == "CUSTOM") ? new List<string>() { "REGONLY" } : null,
                    PptTermsAndConditionsDoc = (id == "DEFAULT" || id == "CUSTOM2" ) ? "PPTANDC" : null
                };

                _payPlanTemplates.Add(payPlanTemplate);
            }
        }

        /// <summary>
        /// Gets payment plan template raw data
        /// </summary>
        /// <returns>String array of raw payment plan template data</returns>
        private static string[,] GetPaymentPlanTemplatesData()
        {
            string[,] paymentPlanTemplatesTable = {
                                                // ID          Active   Description              Frequency  NumOfPmts  Min Plan Amt  Max Plan Amt  InclSetupFee  CalcOnPlanBal  SubtractFA  AutoCalcAmt  AutoModify  ModMethod  SetupAmt  SetupPct  SUARC  DownPayPct  DownPayDays  GraceDays  LateAmount LateAmtCode  LatePct LatePctCode  TermsDoc      AllowedArTypes  InvExclRules  InclArCodes     ExclArCodes  Subrt
                                                { "DEFAULT ", "Y",     "Default Payment Plan",  "W",           "3",      "   5.00",   "        ",      "Y",          "N",          " ",         "Y",        "Y",        "E",    "60.00",  "10.00", "PLSU",  "10.00",      "10",       "3",      " 5.00",    "PFLAT",   "     ",  "     ",   "SPECIAL   ", "           ",  "           ", "           ", "PARK;LIBR",  " " },
                                                { "BIWEEKLY", "Y",     "Biweekly Plan       ",  "B",           "5",      " 100.00",   "10000.00",      "N",          "N",          "N",         "N",        "N",        "D",    "     ",  "     ", "PLSU",  "     ",      "30",       " ",      "10.00",    "PFLAT",   "     ",  "     ",   "          ", "           ",  "           ", "           ", "         ",  " " },
                                                { "MONTHLY",  "Y",     "Monthly Plan        ",  "M",           "4",      "1000.00",   "        ",      "N",          "N",          "N",         "N",        "N",        "D",    "25.00",  "     ", "PLSU",  "33.33",      "  ",       "0",      "20.00",    "PFLAT",   " 8.00",  "PPCT ",   "PLANT&C   ", "01;02      ",  "           ", "           ", "         ",  " " },
                                                { "REGONLY ", "N",     "Reg Charges Only    ",  "W",           " ",      "   0.00",   " 5000.00",      "Y",          "N",          "Y",         "N",        "Y",        "R",    "     ",  " 5.00", "PLSU",  "     ",      "  ",       " ",      "50.00",    "PFLAT",   "     ",  "     ",   "          ", "           ",  "REGONLY    ", "TUIFT;TUIPT", "         ",  " " },
                                                { "CUSTOM  ", "Y",     "Custom Frequency    ",  "C",           " ",      "   0.00",   "        ",      "N",          "Y",          "Y",         "Y",        "N",        "E",    "     ",  "     ", "PLSU",  "     ",      "  ",       " ",      "     ",    "     ",   "18.00",  "PPCT ",   "          ", "           ",  "RULE1;RULE2", "           ", "         ",  "S.SAMPLE.PLAN.FREQ.SUBR" },
                                                { "CUSTOM2 ", "Y",     "Custom Frequency 2  ",  "C",           " ",      "   0.00",   "        ",      "N",          "Y",          "Y",         "Y",        "Y",        "E",    "     ",  "     ", "PLSU",  "     ",      "  ",       " ",      "     ",    "     ",   "18.00",  "PPCT ",   "PLANT&C   ", "           ",  "RULE1;RULE2", "           ", "         ",  "S.SAMPLE.PLAN.FREQ.SUBR" }
         
                                                  };
            return paymentPlanTemplatesTable;
        }
    }
}
