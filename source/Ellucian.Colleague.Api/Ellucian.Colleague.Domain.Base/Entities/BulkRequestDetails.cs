using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class BulkRequestDetails
    {
        /// <summary>
        /// Resource name of HEDM schema
        /// </summary>
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
        public string Status { get; set; }

        /// <summary>
        /// ApplicationId of the application in the ethos tenant
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Id of the Tenant in Ethos
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Total count of resources in job
        /// </summary>
        public string XTotalCount { get; set; }

        /// <summary>
        /// Collection of <see cref="BulkRequestGetError"> errors</see>
        /// </summary>
        public IEnumerable<BulkRequestGetError> Errors { get; set; }

        /// <summary>
        /// Collection of <see cref="BulkRequestProcessingStep"> errors</see>
        /// </summary>
        public IEnumerable<BulkRequestProcessingStep> ProcessingSteps { get; set; }

    }

    /// <summary>
    /// Error object for 
    /// </summary>
    [Serializable]
    public class BulkRequestGetError
    {
        /// <summary>
        /// Error Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Id for the resource the error is related to
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// error message
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// Bulk load processing step
    /// </summary>
    [Serializable]
    public class BulkRequestProcessingStep
    {
        /// <summary>
        /// Count of records in this step
        /// </summary>
        public string Count { get; set; }

        /// <summary>
        /// elapsed time for the batch process
        /// </summary>
        public string ElapsedTime { get; set; }

        /// <summary>
        /// job number of this step
        /// </summary>
        public string JobNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Seq { get; set; }

        /// <summary>
        /// start time of this step
        /// </summary>
        public string StartTime { get; set; }

        /// <summary>
        /// status of this step
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// step name
        /// </summary>
        public string Step { get; set; }
    }
}
