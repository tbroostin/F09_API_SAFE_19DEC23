// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IPreferredSectionRepository
    {
        Task<PreferredSectionsResponse> GetAsync(string studentId);
        Task<IEnumerable<PreferredSectionMessage>> UpdateAsync(string studentId, List<PreferredSection> sections);
        Task<IEnumerable<PreferredSectionMessage>> DeleteAsync(string studentId, string sectionId);
    }
}
