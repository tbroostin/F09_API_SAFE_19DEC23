// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// This Entity will hold the details that needs to be displayed for Departmental Oversight person
    /// </summary>
    [Serializable]
   public class DeptOversightSearchResult
    {
        /// <summary>
        /// Id of the Faculty
        /// </summary>
        public string FacultyId { get; set; }

        /// <summary>
        /// Department for which this detail holds good in this department
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// List of all Section Ids for this faculty in this department
        /// </summary>
        public List<string> SectionIds { get; set; }

        public DeptOversightSearchResult()
        {
            SectionIds = new List<string>();
        }

    }

   
}
