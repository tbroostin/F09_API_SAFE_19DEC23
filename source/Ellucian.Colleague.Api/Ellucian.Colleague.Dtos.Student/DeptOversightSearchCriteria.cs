// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains INCOMING Departmental Oversight search/filter request
    /// Search criteria must either contain an SectionKeyword or FacultyKeyword a  value of at least 2 characters. It cannot contain both.
    /// </summary>
    public class DeptOversightSearchCriteria
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public DeptOversightSearchCriteria()
        {

        }

        /// <summary>
        /// Used when requesting a search of sections by faculty name or Id. [last, first middle] or [first middle last] or [last] or [Id] expected.
        /// Must contain at least 2 characters when provided.
        /// </summary>
        public string FacultyKeyword { get; set; }

        /// <summary>
        /// Used when requesting a search of sections by section name.
        /// </summary>
        public string SectionKeyword { get; set; }
       

    }
}
