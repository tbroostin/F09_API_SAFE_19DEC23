// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    /// <summary>
    /// Class for Purchasing Defaults
    /// </summary>
    [Serializable]
    public class PurchasingDefaults
    {
        /// <summary>
        /// Default value of ShipToCode 
        /// </summary>
        public string DefaultShipToCode { get; set; }

    }
}
