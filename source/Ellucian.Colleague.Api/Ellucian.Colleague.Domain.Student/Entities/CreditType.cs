// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    ///// <summary>
    ///// The different types of academic credits.
    ///// </summary>
    [Serializable]
    public enum CreditType
    {
        Institutional, Transfer, ContinuingEducation, Exchange, Other, None
    }
}
