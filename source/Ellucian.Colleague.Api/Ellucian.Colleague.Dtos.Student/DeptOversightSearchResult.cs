// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// This Entity will hold the details that needs to be displayed for Departmental Oversight person
    /// </summary>
    public class DeptOversightSearchResult
    {
        /// <summary>
        /// Id of the Faculty
        /// </summary>
        public string FacultyId { get; set; }

        /// <summary>
        /// Department for which this detail holds good for departmental oversight person
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Sections for which this details holds good for departmental oversight person
        /// </summary>
        public List<string> SectionIds { get; set; }

    }
}
