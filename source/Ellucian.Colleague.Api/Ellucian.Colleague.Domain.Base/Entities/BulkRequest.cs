using System;
// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class BulkRequest
    {
        public string ResourceName { get; set; }

        /// <summary>
        /// Header version representation of HEDM schema
        /// </summary>
        public string Representation { get; set; }

        /// <summary>
        /// Tracking ID used by the requesting system
        /// </summary>
        public string RequestorTrackingId { get; set; }

        /// <summary>
        /// Job Number for requesting system to check with
        /// </summary>
        public string JobNumber { get; set; }

        /// <summary>
        /// Job Status
        /// </summary>
        public BulkRequestStatus Status { get; set; }

        /// <summary>
        /// ApplicationId of the application in the ethos tenant
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Message from source system about bulk load request
        /// </summary>
        public string Message { get; set; }

    }
}
