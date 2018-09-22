// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestStudentStatementSummaryRepository
    {
        private static StudentStatementSummary _studentStatementSummary;
        public static StudentStatementSummary StudentStatementSummary()
        {
            GenerateEntities();
            return _studentStatementSummary;
        }

        private static void GenerateEntities()
        {
            List<ActivityTermItem> chargeItems = new List<ActivityTermItem>()
                {
                    new ActivityTermItem() { Amount = 100, Description = "Charge 1", Id = "1", TermId = FinanceTimeframeCodes.PastPeriod },
                    new ActivityTermItem() { Amount = 200, Description = "Charge 2", Id = "2", TermId = FinanceTimeframeCodes.PastPeriod },
                    new ActivityTermItem() { Amount = 300, Description = "Charge 3", Id = "3", TermId = FinanceTimeframeCodes.PastPeriod },
                    new ActivityTermItem() { Amount = 400, Description = "Charge 4", Id = "4", TermId = FinanceTimeframeCodes.PastPeriod },
                };
            List<ActivityTermItem> nonChargeItems = new List<ActivityTermItem>()
                {
                    new ActivityTermItem() { Amount = 500, Description = "Non-Charge 5", Id = "5", TermId = FinanceTimeframeCodes.PastPeriod },
                    new ActivityTermItem() { Amount = 600, Description = "Non-Charge 6", Id = "6", TermId = FinanceTimeframeCodes.PastPeriod },
                    new ActivityTermItem() { Amount = 700, Description = "Non-Charge 7", Id = "7", TermId = FinanceTimeframeCodes.PastPeriod },
                    new ActivityTermItem() { Amount = 800, Description = "Non-Charge 8", Id = "8", TermId = FinanceTimeframeCodes.PastPeriod },
                };
            _studentStatementSummary = new Domain.Finance.Entities.StudentStatementSummary(chargeItems, nonChargeItems, 0, 0)            
            {
                SummaryDateRange = string.Format(" (before {0}", DateTime.Today.ToShortDateString()),
                TimeframeDescription = "Past"
            };
        }
    }
}
