// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    [Serializable]
    public class OrganizationContact
    {

        public OrganizationContact(string guid, string id)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new ArgumentNullException("guid", "Vendor contacts guid can not be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id", "Vendor contacts id can not be null or empty.");
            }
            this.Guid = guid;
            this.Id = id;
        }
        public string Id { get; private set; }

        /// <summary>
        /// GUID for the remark; not required, but cannot be changed once assigned.
        /// </summary>
        public string Guid { get; private set; }
        public string ContactAddress { get; set; }
        public string VendorId { get; set; }
        public string ContactPersonGuid { get; set; }
        public string RelationshipType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ContactPreferedName { get; set; }
        public List<ContactPhoneInfo> PhoneInfos { get; set; }
        public string ContactPersonId { get; set; }
    }
}