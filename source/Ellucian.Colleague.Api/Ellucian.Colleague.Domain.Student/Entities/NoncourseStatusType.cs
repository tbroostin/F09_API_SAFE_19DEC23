// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Indicates the test status type. If a test is retaken, sometimes only the most recent score or the highest score is considered 'accepted' and the
    /// other scores for the same test are just notational.
    /// </summary>
    [Serializable]
    public enum NoncourseStatusType
    {
        None, Accepted, Notational, Withdrawn
    }
}
