// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonVisaRequest
    {
        public string PersonId { get; set; }
        public string VisaType { get; set; }
        public string VisaNo { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public DateTime? EntryDate { get; set; }
        public string StrGuid { get; set; }
        public string Status { get; set; }

        public PersonVisaRequest(string id, string personId)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id is required");
            }
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId is required");
            }
            StrGuid = id;
            PersonId = personId;
        }
    }
}
