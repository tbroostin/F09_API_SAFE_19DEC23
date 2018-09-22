//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class FacultySpecialStatuses: GuidCodeItem
    {
        /// <summary>
        /// Constructor for FacultySpecialStatuses
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="code">code</param>
        /// <param name="description">description</param>
        public FacultySpecialStatuses(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
