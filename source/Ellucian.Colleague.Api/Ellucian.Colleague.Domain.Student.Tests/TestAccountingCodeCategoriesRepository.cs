﻿// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestAccountingCodeCategoriesRepository
    {
        List<ArCategory> accountingCodeCategories;
        public List<ArCategory> Get()
        {
            Populate();
            return accountingCodeCategories;
        }

        private void Populate()
        {
            if (accountingCodeCategories == null) accountingCodeCategories = new List<ArCategory>();

            accountingCodeCategories.Add(new ArCategory("4b7568dd-d4e5-41f6-9ac7-5da29bfda07a", "111", "Pam's 111 Charge"));
            accountingCodeCategories.Add(new ArCategory("16cbe147-c987-4d84-9de0-26875366f892", "222", "Student Activity Fee"));
            accountingCodeCategories.Add(new ArCategory("a142d78a-b472-45de-8a4b-953258976a0b", "333", "Add Fee"));
            accountingCodeCategories.Add(new ArCategory("f3691b68-a4ea-44d7-84c6-ed0e0d18924b", "444", "Application Fee"));
            accountingCodeCategories.Add(new ArCategory("e46d2a2a-31c1-430a-85d4-a1c1b02ae92f", "555", "Athletic Fee"));
        }

    }
}