/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// An EarningsTypeGroupItem object contains information about an entry in an
    /// earnings type group. An earnings type group contains one or more earnings types
    /// and a description that can override the default earnings type description. An
    /// earnings type group is associated to a position pay record.
    /// </summary>
    public class EarningsTypeGroupItem
    {
        /// <summary>
        /// The id of the earnings type of this group entry
        /// </summary>
        public string EarningsTypeId { get; set; }

        /// <summary>
        /// A custom description of this entry in the group
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The id of the EarningsTypeGroup to which this entry belongs.
        /// </summary>
        public string EarningsTypeGroupId { get; set; }
    }
}