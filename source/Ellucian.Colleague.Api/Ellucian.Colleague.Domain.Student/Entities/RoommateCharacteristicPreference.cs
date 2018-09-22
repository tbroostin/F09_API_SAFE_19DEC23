// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RoommateCharacteristicPreference
    {
        public string RoommateCharacteristic { get; set; }
        public string RoommateCharacteristicRequired { get; set; }
    }
}
