// Copyright 2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Contains INCOMING Departmental Oversight details request
    /// Search criteria must contain a list of Ids and a SectionId.
    /// </summary>
    public class DeptOversightDetailsCriteria
    {
        /// <summary>
        /// Default parameter-less constructor
        /// </summary>
        public DeptOversightDetailsCriteria()
        {

        }

        /// <summary>
        /// Contains list of Ids having either of departmental overisight Ids or faculty Ids or both.
        /// </summary>
        public IEnumerable<string> Ids { get; set; }

        /// <summary>
        /// Contains sectionId.
        /// </summary>
        public string SectionId { get; set; }
       

    }
}
