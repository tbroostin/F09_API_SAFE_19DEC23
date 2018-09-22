// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RoommatePreference
    {
        public string RoommateId { get; set; }
        public string RoommateRequired { get; set; }
    }
}
