/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Timestamp data object
    /// </summary>
    public class Timestamp
    {
        /// <summary>
        /// Identity of record add operator (read only)
        /// </summary>
        public string AddOperator { get; set; }
        /// <summary>
        /// Identity of record change operator (read only)
        /// </summary>
        public string ChangeOperator { get; set; } 
        /// <summary>
        /// DateTimeOffset of record add (read only)
        /// </summary>
        public DateTimeOffset AddDateTime { get; set; }
        /// <summary>
        /// DateTimeOffset of record change (read only)
        /// </summary>
        public DateTimeOffset ChangeDateTime { get; set; }
    }
}
