// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Class defines the CorrespondenceRequest domain
    /// </summary>
    [Serializable]
    public class CorrespondenceRequest
    {
        private readonly string _PersonId;
        private readonly string _Code;

        /// <summary>
        /// Person Id of the CorrespondenceRequest
        /// </summary>
        public string PersonId { get { return _PersonId; } }

        /// <summary>
        /// Correspondence Request Code identifying the CorrespondenceRequest
        /// </summary>
        public string Code { get { return _Code; } }

        /// <summary>
        /// Correspondence Request Instance specifying student specific information
        /// about the correspondence request
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// The status of the CorrespondenceRequest. Initialized to Incomplete
        /// </summary>
        public CorrespondenceRequestStatus Status { get; set; }

        /// <summary>
        /// The date that the status of this CorrespondenceRequest was updated.
        /// </summary>
        public DateTime? StatusDate { get; set; }

        /// <summary>
        /// The DueDate, if any, of the CorrespondenceRequest. Initialized to null.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// The correspondence request's assign date. Needed to match up to correct item in the database along with Code and person.
        /// </summary>
        public DateTime? AssignDate { get; set; }

        /// <summary>
        /// Correspondence Request status description
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Constructor for a CorrespondenceRequest
        /// </summary>
        /// <param name="personId">The personId of the CorrespondenceRequest</param>
        /// <param name="correspondenceRequestCode">The Correspondence Request Code identifying the CorrespondenceRequest. This code will come from a list of correspondence codes.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the arguments are null or empty.</exception>
        public CorrespondenceRequest(string personId, string correspondenceRequestCode)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(correspondenceRequestCode))
            {
                throw new ArgumentNullException("documentCode");
            }

            _PersonId = personId;
            _Code = correspondenceRequestCode;

            Status = CorrespondenceRequestStatus.Incomplete;
        }
    }
}
