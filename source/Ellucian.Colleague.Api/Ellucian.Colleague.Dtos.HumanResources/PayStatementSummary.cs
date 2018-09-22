/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Pay statement summary construct for use in data transfer
    /// </summary>
    public class PayStatementSummary
    {
        /// <summary>
        /// Pay Statement Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Date the employee was paid
        /// </summary> 
        public DateTime PayDate { get; set; }
    }
}
