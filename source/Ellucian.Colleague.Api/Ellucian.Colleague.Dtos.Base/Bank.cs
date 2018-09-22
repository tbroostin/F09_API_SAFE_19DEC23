/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// A bank
    /// </summary>
    public class Bank
    {
        /// <summary>
        /// The id of a bank, either the routing number for a US Bank or 
        /// the institution Id and the branch transit number separated by a hyphen for a Canadian Bank, i.e 123-12345
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the bank
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The RoutingId of the bank. If a US Bank, this will be the same as the ID attribute. If Canadian, this attribute
        /// will be null;
        /// </summary>
        public string RoutingId { get; set; }

        /// <summary>
        /// The InstitutionId of the Canadian Bank. If a US bank, this attribute will be null
        /// </summary>
        public string InstitutionId { get; set; }

        /// <summary>
        /// The BranchTransitNumber of the Canadian Bank. If a US Bank, this attribute will be null
        /// </summary>
        public string BranchTransitNumber { get; set; }
    }
}
