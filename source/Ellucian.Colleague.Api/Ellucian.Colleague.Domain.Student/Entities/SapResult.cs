//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Sapresults
    /// </summary>
    [Serializable]
    public class SapResult
    {
       
        /// <summary>
        /// Sap Results.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="id"></param>
        public SapResult(string guid, string id, string status)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new ArgumentNullException(string.Format("Guid is required. Id:{0}", id));
            }
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("Id is required.");
            }
            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentNullException(string.Format("Status is required. Guid:{0}", guid));
            }
           
            RecordGuid = guid;
            RecordKey = id;
            SapStatus = status;
        }

        public string PersonId { get; set; }
        public string RecordGuid { get; private set; }
        public string RecordKey { get; private set; }
        public string SapStatus { get; private set; }
        public DateTime? OvrResultsAddDate { get; set; }
        public string SapTypeId { get; set; }
        public string SaprEvalPdEndTerm { get; set; }
        public DateTime? SaprEvalPdEndDate { get; set; }
        public string SaprCalcThruTerm { get; set; }
        public DateTime? SaprCalcThruDate { get; set; }
    }
}