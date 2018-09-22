// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public class TestAccountPeriodRepository
    {
        public static List<AccountPeriod> PcfAccountPeriods
        {
            get
            {
                return new List<Domain.Finance.Entities.AccountActivity.AccountPeriod>()
                {
                    new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                    {
                        AssociatedPeriods = new List<string>() { "2013/FA", "2014/SP", "2014/S1"},
                        Balance = 10000m,
                        Description = "Past",
                        Id = FinanceTimeframeCodes.PastPeriod,
                        EndDate = DateTime.Today.AddDays(-30),
                        StartDate = null
                    },
                    new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                    {
                        AssociatedPeriods = new List<string>() { "2014/FA", "2015/SP", "2015/S1"},
                        Balance = 5000m,
                        Description = "Current",
                        Id = FinanceTimeframeCodes.CurrentPeriod,
                        EndDate = DateTime.Today.AddDays(30),
                        StartDate = DateTime.Today.AddDays(-29)
                    },
                    new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                    {
                        AssociatedPeriods = new List<string>() { "2015/FA", "2016/SP", "2016/S1"},
                        Balance = 1000m,
                        Description = "Future",
                        Id = FinanceTimeframeCodes.FuturePeriod,
                        EndDate = null,
                        StartDate = DateTime.Today.AddDays(31)
                    }
                };
            }
        }

        public static List<AccountPeriod> TermAccountPeriods
        {
            get
            {
                return new List<Domain.Finance.Entities.AccountActivity.AccountPeriod>()
                {
                    new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                    {
                        AssociatedPeriods = null,
                        Balance = 10000m,
                        Description = "2014 Spring Term",
                        Id = "2014/SP",
                        EndDate = DateTime.Today.AddDays(-30),
                        StartDate = null
                    },
                    new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                    {
                        AssociatedPeriods = null,
                        Balance = 5000m,
                        Description = "2015 Spring Term",
                        Id = "2015/SP",
                        EndDate = DateTime.Today.AddDays(30),
                        StartDate = DateTime.Today.AddDays(-29)
                    },
                    new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                    {
                        AssociatedPeriods = null,
                        Balance = 1000m,
                        Description = "2015 Fall Term",
                        Id = "2015/FA",
                        EndDate = null,
                        StartDate = DateTime.Today.AddDays(31)
                    }
                };
            }
        }

        public static AccountPeriod NonTermAccountPeriod
        {
            get
            {
                return new Domain.Finance.Entities.AccountActivity.AccountPeriod()
                {
                    AssociatedPeriods = null,
                    Balance = 10000m,
                    Description = "Other",
                    Id = FinanceTimeframeCodes.NonTerm,
                    EndDate = null,
                    StartDate = null
                };
            }
        }
    }
}
