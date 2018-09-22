// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Academic Level services
    /// </summary>
    public interface IAcademicLevelService
    {
        IEnumerable<Ellucian.Colleague.Dtos.AcademicLevel> GetAcademicLevels();
    }
}
