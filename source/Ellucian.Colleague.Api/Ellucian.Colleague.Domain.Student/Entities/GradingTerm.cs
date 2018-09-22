// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Domain.Entities;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Term open to faculty grading
    /// </summary>
    [Serializable]
    public class GradingTerm : CodeItem
    {
        public GradingTerm(string code, string desc)
            : base(code, desc)
        {

        }
    }
}
