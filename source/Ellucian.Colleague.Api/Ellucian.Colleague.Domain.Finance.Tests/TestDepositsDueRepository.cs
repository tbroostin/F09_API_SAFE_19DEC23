// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestDepositsDueRepository
    {
        private static List<DepositDue> _depositsDue = new List<DepositDue>();
        public static List<DepositDue> DepositsDue
        {
            get
            {
                if (_depositsDue.Count == 0)
                {
                    GenerateEntities();
                }
                return _depositsDue;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var record in TestArDepositsDueRepository.ArDepositsDue)
            {
                _depositsDue.Add(new DepositDue(record.Recordkey, record.ArddPersonId, record.ArddAmount.GetValueOrDefault(),
                    record.ArddDepositType, record.ArddDueDate.GetValueOrDefault())
                    {
                        TermId = record.ArddTerm
                    });
            }
        }
    }
}
