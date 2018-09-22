// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// "MAXIMUM 5 100,200 LEVEL HOURS"
    /// </summary>
    [Serializable]
    public class MaxCreditAtLevels : MaxAtLevels
    {
        private readonly decimal _MaxCredit;
        public decimal MaxCredit { get { return _MaxCredit; } }

        public MaxCreditAtLevels(decimal max, ICollection<string> levels)
            : base(levels)
        {
            _MaxCredit = max;
        }

    }
}
