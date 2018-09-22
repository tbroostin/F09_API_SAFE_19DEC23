// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestAccountingCodesRepository
    {
        List<AccountingCode> accountingCodes;
        public List<AccountingCode> Get()
        {
            Populate();
            return accountingCodes;
        }

        private void Populate()
        {
            if (accountingCodes == null) accountingCodes = new List<AccountingCode>();

            accountingCodes.Add(new AccountingCode("4b7568dd-d4e5-41f6-9ac7-5da29bfda07a", "111", "Pam's 111 Charge"));
            accountingCodes.Add(new AccountingCode("16cbe147-c987-4d84-9de0-26875366f892", "222", "Student Activity Fee"));
            accountingCodes.Add(new AccountingCode("a142d78a-b472-45de-8a4b-953258976a0b", "333", "Add Fee"));
            accountingCodes.Add(new AccountingCode("f3691b68-a4ea-44d7-84c6-ed0e0d18924b", "444", "Application Fee"));
            accountingCodes.Add(new AccountingCode("e46d2a2a-31c1-430a-85d4-a1c1b02ae92f", "555", "Athletic Fee"));
        }

    }
}