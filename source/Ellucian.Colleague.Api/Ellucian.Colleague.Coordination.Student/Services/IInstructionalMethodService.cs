// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Instructional Method services
    /// </summary>
    public interface IInstructionalMethodService
    {
        IEnumerable<Ellucian.Colleague.Dtos.InstructionalMethod> GetInstructionalMethods();
    }
}
