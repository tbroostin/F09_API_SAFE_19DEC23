/*Copyright 2022 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Contains parameters from HRSS Defaults entity
    /// </summary>
    
    [Serializable]
    public class HRSSConfiguration
    {
        /// <summary>
        /// Display hierarchy name set in the HRSS form 
        /// </summary>
        public string HrssDisplayNameHierarchy;

        public HRSSConfiguration()
        {
            HrssDisplayNameHierarchy = string.Empty;
        }
    }
}
