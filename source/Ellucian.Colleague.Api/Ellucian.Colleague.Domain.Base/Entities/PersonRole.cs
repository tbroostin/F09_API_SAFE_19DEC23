// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonRole
    {
        /// <summary>
        /// Role code defining the person's language
        /// </summary>
        public PersonRoleType RoleType { get; private set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public PersonRole(PersonRoleType roleType, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(roleType.ToString()))
            {
                throw new ArgumentNullException("roleType", "Role type must be specified");
            }
            RoleType = roleType;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
