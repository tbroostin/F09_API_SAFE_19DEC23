// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class SectionRegistrationStatusItem : GuidCodeItem
    {
        /// <summary>
        /// The course status
        /// </summary>
        public SectionRegistrationStatus Status { get; set; }

        public CourseTransferStatusesCategory Category { get; set; }

        public SectionRegistrationStatusItem(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
