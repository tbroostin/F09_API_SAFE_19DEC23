// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class ComponentTest
    {
        public string Test { get; set; }
        public decimal? Score { get; set; }
        public int? Percentile { get; set; }

        public ComponentTest(string componentTest, decimal? componentScore, int? componentPercentile)
        {
            if (string.IsNullOrEmpty(componentTest))
            {
                throw new ArgumentNullException("componentTest");
            }
            Test = componentTest;
            Score = componentScore;
            Percentile = componentPercentile;
        }
    }
}
