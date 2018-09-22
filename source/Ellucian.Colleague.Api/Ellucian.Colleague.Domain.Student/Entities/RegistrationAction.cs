// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Possible actions of a SectionRegistration object
    /// </summary>
    [Serializable]
    public enum RegistrationAction
    {
        Add,
        PassFail,
        Audit,
        Drop,
        Waitlist,
        RemoveFromWaitlist
    }
}
