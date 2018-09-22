// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Section Registration status
    /// </summary>
    [Serializable]
    public class SectionRegistrationStatus
    {
        public RegistrationStatus RegistrationStatus { get; set; }
        public RegistrationStatusReason SectionRegistrationStatusReason { get; set; }

        public SectionRegistrationStatus()
        {

        }
    }
}