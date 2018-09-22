// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IPreferredSectionService
    {
        Task<PreferredSectionsResponse> GetAsync(string studentId);
        Task<IEnumerable<PreferredSectionMessage>> UpdateAsync(string studentId, IEnumerable<PreferredSection> preferredSections);
        Task<IEnumerable<PreferredSectionMessage>> DeleteAsync(string studentId, string sectionId);
    }
}
