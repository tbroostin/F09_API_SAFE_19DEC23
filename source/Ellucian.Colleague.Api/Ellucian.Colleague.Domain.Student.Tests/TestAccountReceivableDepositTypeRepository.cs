// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestAccountReceivableDepositTypeRepository
    {
        List<AccountReceivableDepositType> accountReceivableDepositType;
        public List<AccountReceivableDepositType> Get()
        {
            Populate();
            return accountReceivableDepositType;
        }

        private void Populate()
        {
            if (accountReceivableDepositType == null) accountReceivableDepositType = new List<AccountReceivableDepositType>();

            accountReceivableDepositType.Add(new AccountReceivableDepositType("4b7568dd-d4e5-41f6-9ac7-5da29bfda07a", "01", "Student Receivable"));
            accountReceivableDepositType.Add(new AccountReceivableDepositType("16cbe147-c987-4d84-9de0-26875366f892", "GTT", "Project Receivables"));
            accountReceivableDepositType.Add(new AccountReceivableDepositType("a142d78a-b472-45de-8a4b-953258976a0b", "PJ", "Pam's Ar Type"));
            accountReceivableDepositType.Add(new AccountReceivableDepositType("f3691b68-a4ea-44d7-84c6-ed0e0d18924b", "PJN", "Pam's Additional Ar Type"));
            accountReceivableDepositType.Add(new AccountReceivableDepositType("e46d2a2a-31c1-430a-85d4-a1c1b02ae92f", "02", "Continuing Ed Receivable"));
        }

    }
}