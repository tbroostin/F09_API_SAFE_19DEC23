// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Indicates whether a requisite must be completed prior to, concurrent with, or either
    /// </summary>
    [Serializable]
    public enum RequisiteCompletionOrder
    {
        Concurrent, Previous, PreviousOrConcurrent
    }
}
