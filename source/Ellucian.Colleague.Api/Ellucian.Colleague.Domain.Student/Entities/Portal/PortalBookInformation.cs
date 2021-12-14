// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.Portal
{
    /// <summary>
    /// course section book information
    /// </summary>
    [Serializable]
    public class PortalBookInformation
    {
        /// <summary>
        /// details about the book 
        /// </summary>
        public string Information { get; private set; }

        /// <summary>
        /// cost of the book
        /// </summary>
        public decimal Cost { get; private set; }

        public PortalBookInformation(string information, decimal cost)
        {
            Information = information;
            Cost = cost;
        }
    }
}
