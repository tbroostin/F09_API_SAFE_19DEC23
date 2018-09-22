// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestRoomWingsRepository
    {
        private readonly string[,] _roomWings =
        {
            //GUID                                  CODE   DESCRIPTION
            {"84b9c4eb-22fa-4467-94b0-1966015e9953", "N", "North"},
            {"ed50ddc8-0e88-40dd-bfa4-6097b4269e0e", "S", "South"},
            {"cabf6adb-3fee-4185-a2dc-75e6c5e995ff", "E", "East"},
            {"0eac944f-f9f2-42cc-97ad-61f6d56b619f", "W", "West"}
        };

        public IEnumerable<RoomWing> Get()
        {
            var roomWingList = new List<RoomWing>();

            // There are 3 fields for each type in the array
            var items = _roomWings.Length/3;

            for (var x = 0; x < items; x++)
            {
                roomWingList.Add(new RoomWing(_roomWings[x, 0], _roomWings[x, 1], _roomWings[x, 2]));
            }
            return roomWingList;
        }
    }
}