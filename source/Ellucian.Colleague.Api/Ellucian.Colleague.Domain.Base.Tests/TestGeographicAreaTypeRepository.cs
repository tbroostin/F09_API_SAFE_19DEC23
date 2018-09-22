// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestGeographicAreaTypeRepository
    {
        private string[,] geographicAreaTypes = {
                                            //GUID   CODE   DESCRIPTION   SP1
                                            {"87ec6f69-9b16-4ed5-8954-59067f0318ec", "GOV", "governmental"}, 
                                            {"4236641d-5c29-4884-9a17-530820ec9d29", "POST", "postal"},
                                            {"eb2e3bed-3bfc-43b6-9305-ac1da21c2f33", "FUND", "fundraising"},
                                            {"a3fe5df5-91ff-49e0-b418-cd047461594a", "REC", "recruitment"}
                                      };

        public IEnumerable<GeographicAreaType> Get()
        {
            var geographicAreaTypeList = new List<GeographicAreaType>();

            // There are 3 fields for each citizenship status in the array
            var items = geographicAreaTypes.Length / 3;

            for (int x = 0; x < items; x++)
            {
                var newType = new GeographicAreaTypeCategory();
                switch (geographicAreaTypes[x,1])
                {
                    case "GOV":
                         newType = GeographicAreaTypeCategory.Governmental;
                         break;
                    case "POST":
                        newType = GeographicAreaTypeCategory.Postal;
                        break;
                    case "FUND":
                        newType = GeographicAreaTypeCategory.Fundraising;
                        break;
                    default:
                        newType = GeographicAreaTypeCategory.Recruitment;
                        break;
                }
                geographicAreaTypeList.Add(new GeographicAreaType(geographicAreaTypes[x, 0], geographicAreaTypes[x, 1], geographicAreaTypes[x, 2], newType));
            }
            return geographicAreaTypeList;
        }
    }
}