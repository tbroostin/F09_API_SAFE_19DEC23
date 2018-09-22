// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Configuration for Emergency Information options
    /// </summary>
    public class EmergencyInformationConfiguration
    {
        /// <summary>
        /// Hide health conditions on emergency information
        /// </summary>
        public bool HideHealthConditions { get; set; }

        /// <summary>
        /// Require contact details to confirm emergency information
        /// </summary>
        public bool RequireContactToConfirm { get; set; }

        /// <summary>
        /// Allow users to explictly opt out of providing emergency information
        /// </summary>
        public bool AllowOptOut { get; set; }
    }
}
