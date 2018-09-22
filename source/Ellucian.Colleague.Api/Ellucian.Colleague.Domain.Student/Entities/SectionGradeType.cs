// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section grade type codes
    /// </summary>
    [Serializable]
    public class SectionGradeType : GuidCodeItem
    {
        /// <summary>
        /// Constructor for SectionGradeType
        /// </summary>
        /// <param name="guid">Section grade type GUID</param>
        /// <param name="code">Section grade type code</param>
        /// <param name="description">Section grade type description</param>
        public SectionGradeType(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
