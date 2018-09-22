// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Configuration for Emergency Information options
    /// </summary>
    [Serializable]
    public class EmergencyInformationConfiguration
    {
        /// <summary>
        /// Hide health conditions on emergency information
        /// </summary>
        public bool HideHealthConditions { get; private set; }

        /// <summary>
        /// Hide other information (including insurance and hospital) on emergency information
        /// </summary>
        public bool HideOtherInformation { get; private set; }

        /// <summary>
        /// Require contact details to confirm emergency information
        /// </summary>
        public bool RequireContact { get; private set; }

        /// <summary>
        /// Allow users to explictly opt out of providing emergency information
        /// </summary>
        public bool AllowOptOut { get; private set; }

        /// <summary>
        /// Constructor for EmergencyInformationConfiguration
        /// </summary>
        /// <param name="hideHealthConditions"></param>
        /// <param name="hideOtherInformation"></param>
        /// <param name="requireContact"></param>
        /// <param name="allowOptOut"></param>
        public EmergencyInformationConfiguration(bool hideHealthConditions, bool hideOtherInformation, bool requireContact, bool allowOptOut)
        {
            HideHealthConditions = hideHealthConditions;
            HideOtherInformation = hideOtherInformation;
            RequireContact = requireContact;
            AllowOptOut = allowOptOut;
        }
    }
}
