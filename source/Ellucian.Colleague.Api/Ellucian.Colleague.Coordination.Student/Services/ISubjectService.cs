// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Subject services
    /// </summary>
    public interface ISubjectService
    {
        IEnumerable<Ellucian.Colleague.Dtos.Subject> GetSubjects();
    }
}
