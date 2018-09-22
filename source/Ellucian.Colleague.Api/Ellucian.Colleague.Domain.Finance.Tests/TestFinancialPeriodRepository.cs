using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestFinancialPeriodRepository
    {
        private static List<FinancialPeriod> _financialPeriods = new List<FinancialPeriod>();
        public static List<FinancialPeriod> FinancialPeriods
        {
            get
            {
                if (_financialPeriods.Count == 0)
                {
                    GenerateEntities();
                }
                return _financialPeriods;
            }
        }

        private static void GenerateEntities()
        {
            _financialPeriods.Add(new FinancialPeriod(Base.Entities.PeriodType.Past, null, DateTime.Today.AddDays(-31)));
            _financialPeriods.Add(new FinancialPeriod(Base.Entities.PeriodType.Current, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(30)));
            _financialPeriods.Add(new FinancialPeriod(Base.Entities.PeriodType.Future, DateTime.Today.AddDays(31), null));
        }
    }
}
