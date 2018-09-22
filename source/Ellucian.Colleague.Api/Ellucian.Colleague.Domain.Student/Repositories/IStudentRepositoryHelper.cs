// Copyright 2014 Ellucian Company L.P. and its acffiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentRepositoryHelper
    {
        CurriculumConfiguration CurriculumConfiguration { get; }
        IEnumerable<InstructionalMethod> InstructionalMethods { get; }
        IEnumerable<SectionStatusCode> SectionStatusCodes { get; }
    }
}
