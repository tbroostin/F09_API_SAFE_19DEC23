// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Configuration information for Colleague integration with Pilot. 
    /// </summary>
    [Serializable]
    public class PilotConfiguration
    {      
        /// <summary>
        /// Colleague user defined phone types to determine primary phone number for Pilot student
        /// </summary>
        public List<string> PrimaryPhoneTypes { get; set; }

        /// <summary>
        /// Colleague user defined phone types to determine phone number for Pilot student for text messaging
        /// </summary>
        public List<string> SmsPhoneTypes { get; set; }
        
        /// <summary>
        /// Constructor for PilotConfiguration
        /// </summary>
        public PilotConfiguration()
        {           
            PrimaryPhoneTypes = new List<string>();
            SmsPhoneTypes = new List<string>();
        }
    }
}
