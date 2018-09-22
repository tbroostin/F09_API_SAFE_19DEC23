// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Test at the institution such as ACT, SAT, LSAT, etc.
    /// </summary>
    [Serializable]
    public class Test :CodeItem
    {

        /// <summary>
        /// Type of Test such as Placement, etc.
        /// </summary>
        public TestType Type { get; set; }
        /// <summary>
        /// Maximum score that this test can have.
        /// </summary>
        public int? MaximumScore { get; set; }
        /// <summary>
        /// If this is a primary test and has sub-tests associated to it, then a list of sub-tests is stored.
        /// </summary>
        public List<string> SubTestsCodes { get; set; }

        public Test(string code, string description)
            : base(code, description)
           
        {
            SubTestsCodes = new List<string>();
        }
     
    }

}

