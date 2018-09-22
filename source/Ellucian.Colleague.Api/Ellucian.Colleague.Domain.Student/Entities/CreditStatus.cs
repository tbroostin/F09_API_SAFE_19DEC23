// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// The various states of an AcademicCredit.
    /// </summary>
    [Serializable]
    public enum CreditStatus
    {
        New, Add, Dropped, Withdrawn, Deleted, Cancelled, TransferOrNonCourse, Preliminary, Unknown
    }
}
