// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// IpedsInstitution helps translate OPE ids into the Name of the institution.
    /// </summary>
    public class IpedsInstitution
    {
        /// <summary>
        /// Unique identification number of the institution as known to IPEDS dataset
        /// </summary>
        public string UnitId { get; set; }

        /// <summary>
        /// Institution name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Office of Postsecondary Education (OPE) ID Number
        /// </summary>        
        public string OpeId { get; set; }
    }
}
