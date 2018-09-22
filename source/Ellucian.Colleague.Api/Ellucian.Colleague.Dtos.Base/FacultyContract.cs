/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    ///     
    /// </summary>
    public class FacultyContract
    {
        /// <summary>
        /// Id for a given Faculty Contract
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Description for a given Faculty Contract
        /// </summary>
        public string ContractDescription { get; set; }

        /// <summary>
        /// Number for the Faculty Contract
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Type of Contract
        /// </summary>
        public string ContractType { get; set; }

        /// <summary>
        /// Start Date of the Contract (Can be null)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End Date of the Contract (Can be null)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// ID for a given LoadPeriod
        /// </summary>
        public string LoadPeriodId { get; set; }

        /// <summary>
        /// The Intended Total Load for a given Load Period
        /// </summary>
        public string IntendedTotalLoad { get; set; }

        /// <summary>
        /// Total Value for a given load period
        /// </summary>
        public string TotalValue { get; set; }
        
        /// <summary>
        /// List of positions for a contract 
        /// </summary>
        public List<FacultyContractPosition> FacultyContractPositions { get; set; }
    }
}
