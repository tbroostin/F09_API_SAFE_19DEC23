/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// PositionFundingSource object describes where money comes from to pay a position
    /// or a person position
    /// </summary>
    public class PositionFundingSource
    { 
        /// <summary>
        /// The Id of the FundingSource
        /// </summary>
        public string FundingSourceId { get; set; }

        /// <summary>
        /// A sort order used to group funding sources.        
        /// </summary>
        public int FundingOrder { get; set; }

        /// <summary>
        /// The Id of the Project this funding source is assigned to. Could be null indicating there is no project
        /// </summary>
        public string ProjectId { get; set; }

    }
}
