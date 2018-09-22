// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Entities;
using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    [Serializable]
    public class RoomCharacteristicPreference
    {
        public string RoomCharacteristic { get; set; }
        public string RoomCharacteristicRequired { get; set; }
    }
}
