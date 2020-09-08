// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Vendor and commodity code association entity
    /// </summary>
    [Serializable]
    public class VendorCommodity
    {
        public string Id { get; private set; }        
        public decimal? StdPrice { get; set; }
        public DateTime? StdPriceDate { get; set; }

        /// <summary>
        /// VendorCommodity constructor
        /// </summary>
        /// <param name="id">id</param>        
        public VendorCommodity(string id)            
        {
            this.Id = id;
        }
    }
}
