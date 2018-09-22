// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Configuration for Emergency Information options
    /// </summary>
    public class EmergencyInformationConfiguration2
    {
        /// <summary>
        /// Hide health conditions on emergency information
        /// </summary>
        public bool HideHealthConditions { get; set; }

        /// <summary>
        /// Hide other information (including insurance and hospital) on emergency information
        /// </summary>
        public bool HideOtherInformation { get; set; }

        /// <summary>
        /// Require contact details to save emergency information
        /// </summary>
        public bool RequireContact { get; set; }

        /// <summary>
        /// Allow users to explictly opt out of providing emergency information
        /// </summary>
        public bool AllowOptOut { get; set; }
    }
}
