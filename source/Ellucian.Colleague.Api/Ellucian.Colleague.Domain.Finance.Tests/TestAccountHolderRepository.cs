// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Base.Tests;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestAccountHolderRepository
    {
        private static List<AccountHolder> _accountHolders = new List<AccountHolder>();
        public static List<AccountHolder> AccountHolders
        {
            get
            {
                if (_accountHolders.Count == 0)
                {
                    GenerateEntities();
                }
                return _accountHolders;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var item in TestPersonRepository.Persons)
            {
                var ah = new AccountHolder(item.Recordkey, item.LastName, item.PrivacyFlag)
                {
                    FirstName = item.FirstName
                };
                ah.AddDepositsDue(TestDepositsDueRepository.DepositsDue.Where(dd => dd.PersonId == item.Recordkey));
                _accountHolders.Add(ah);
            }
        }
    }
}
