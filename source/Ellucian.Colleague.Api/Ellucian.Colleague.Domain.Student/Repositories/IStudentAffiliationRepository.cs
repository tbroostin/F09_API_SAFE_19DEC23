// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IStudentAffiliationRepository
    {
        Task<IEnumerable<StudentAffiliation>> GetStudentAffiliationsByStudentIdsAsync(IEnumerable<string> studentIds, Term termData, string affiliationCode);
    }
}
