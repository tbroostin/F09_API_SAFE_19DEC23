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
    public class FacultyContractTypes : GuidCodeItem
    {
        /// <summary>
        /// FacultyContractTypes
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="code"></param>
        /// <param name="description"></param>
        public FacultyContractTypes(string guid, string code, string description)
            : base(guid, code, description)
        {

        }
    }
}
