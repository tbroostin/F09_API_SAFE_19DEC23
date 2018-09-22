// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Identifies a related process for a work task
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WorkTaskProcess
    {
        /// <summary>
        /// Used when there is no or an unknown Work Task Process
        /// </summary>
        None,
        /// <summary>
        /// Time Approval Work Task Process
        /// </summary>
        TimeApproval,
        /// <summary>
        /// Leave Request Approval Work Task Process
        /// </summary>
        LeaveRequestApproval
    }
}
