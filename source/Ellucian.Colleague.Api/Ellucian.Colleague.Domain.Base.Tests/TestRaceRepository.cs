// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestRaceRepository
    {
        private string[,] races = {
                                            //GUID   CODE   DESCRIPTION   SP1
                                            {"87ec6f69-9b16-4ed5-8954-59067f0318ec", "AN", "American/Alaska Native", "1"}, 
                                            {"4236641d-5c29-4884-9a17-530820ec9d29", "AS", "Asian", "2"},
                                            {"eb2e3bed-3bfc-43b6-9305-ac1da21c2f33", "BL", "Black or African American", "3"},
                                            {"a3fe5df5-91ff-49e0-b418-cd047461594a", "HP", "Hawaiian/Pacific Islander", "4"}, 
                                            {"b5315454-36b7-412e-9c9b-557a9af013bc", "WH", "White", "5"}
                                      };

        public IEnumerable<Race> Get()
        {
            var raceList = new List<Race>();

            // There are 3 fields for each race in the array
            var items = races.Length / 4;

            for (int x = 0; x < items; x++)
            {
                var newType = new RaceType();
                switch (races[x,3])
                {
                    case "1":
                         newType = RaceType.AmericanIndian;
                         break;
                    case "2":
                        newType = RaceType.Asian;
                        break;
                    case "3":
                        newType = RaceType.Black;
                        break;
                    case "4":
                        newType = RaceType.PacificIslander;
                        break;
                    default:
                        newType = RaceType.White;
                        break;
                }
                raceList.Add(new Race(races[x, 0], races[x, 1], races[x, 2], newType));
            }
            return raceList;
        }
    }
}