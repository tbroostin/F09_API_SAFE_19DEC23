// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestRoomTypesRepository
    {
        private string[,] roomTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "110", "Lecture Hall"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "111", "Classroom"},
                                            {"76d8aa11-dbe6-4a49-a1c4-5gad39e232e9", "212", "Computer Lab"},
                                      };


        public IEnumerable<RoomTypes> Get()
        {
            var roomTypesList = new List<RoomTypes>();

            // There are 4 fields for each type in the array
            var items = roomTypes.Length / 4;

            for (int x = 0; x < items; x++)
            {
                roomTypesList.Add(new RoomTypes(roomTypes[x, 0], roomTypes[x, 1], roomTypes[x, 2], RoomType.Classroom));
            }
            return roomTypesList;
        }

        public IEnumerable<RoomTypes> GetRoomTypes()
        {
            var roomTypesList = new List<RoomTypes>();

            var values = Enum.GetValues(typeof(RoomType)).Cast<Ellucian.Colleague.Domain.Base.Entities.RoomType>();
            foreach (var value in values)
            {
                // There are 4 fields for each type in the array
                var items = roomTypes.Length / 4;

                for (int x = 0; x < items; x++)
                {
                    roomTypesList.Add(new RoomTypes(roomTypes[x, 0], roomTypes[x, 1], roomTypes[x, 2], value));
                    roomTypesList.Add(new RoomTypes(roomTypes[x, 0], roomTypes[x, 1], roomTypes[x, 2], null));
                }
            }
            return roomTypesList;
        }
    }
}