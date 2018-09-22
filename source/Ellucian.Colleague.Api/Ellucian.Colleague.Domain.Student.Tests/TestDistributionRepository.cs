// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestDistributionRepository
    {
        List<Distribution2> distribution;
        public List<Distribution2> Get()
        {
            Populate();
            return distribution;
        }

        private void Populate()
        {
            if (distribution == null) distribution = new List<Distribution2>();

            distribution.Add(new Distribution2("4b7568dd-d4e5-41f6-9ac7-5da29bfda07a", "01", "Student Receivable"));
            distribution.Add(new Distribution2("16cbe147-c987-4d84-9de0-26875366f892", "GTT", "Project Receivables"));
            distribution.Add(new Distribution2("a142d78a-b472-45de-8a4b-953258976a0b", "PJ", "Pam's Ar Type"));
            distribution.Add(new Distribution2("f3691b68-a4ea-44d7-84c6-ed0e0d18924b", "PJN", "Pam's Additional Ar Type"));
            distribution.Add(new Distribution2("e46d2a2a-31c1-430a-85d4-a1c1b02ae92f", "02", "Continuing Ed Receivable"));
        }

    }
}