// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestLocationRepository
    {
        private readonly string[,] _locations =
        {
            //   GUID                                   CODE    DESCRIPTION         NW Latitude  NW Longitude   SE Latitude  
            //   SE Longitude   Visible/Mobile  Building  HideInSelfServiceCourseSearch
            {
                "b0eba383-5acf-4050-949d-8bb7a17c5012", "MAIN", "Main Campus",      "77.123456", "-114.987654", "77.123450",
                "-114.987652", "Y", "EIN", "", ""
            },
            {
                "b2cb62b5-936f-4456-b29e-e49242f70e5c", "SC",   "South Campus",     "77.000456", "-114.000654", "77.000450",
                "-114.000652", "Y", "EIN", "", ""
            },
            {
                "51dcb8a4-6d47-4429-9769-d11a0725d3f6", "NW",   "NorthWest Campus", "77.111456", "-114.111654", "77.111450",
                "-114.111652", "Y", "EIN", "", ""
            },
            {
                "b886c618-fd24-49e0-ac5a-c300d4554a39", "SAT",   "Satellite Campus", "",           "",           "", 
                "",            "N", "EIN", "N", ""
            },
            {
                "b886c625-fd24-49e0-ac5a-c300d4554a41", "DT",   "Dalton-Tierney Campus", "77.234567","-114.678543","77.234560",
                "-114.678541", "N", "EIN", "Y", "99"
            }

        };


        public IEnumerable<Location> Get()
        {
            var locs = new List<Location>();
            var items = _locations.Length/10;

            for (var x = 0; x < items; x++)
            {
                decimal d1;
                decimal.TryParse(_locations[x, 3], out d1);
                decimal d2;
                decimal.TryParse(_locations[x, 4], out d2);
                decimal d3;
                decimal.TryParse(_locations[x, 5], out d3);
                decimal d4;
                decimal.TryParse(_locations[x, 6], out d4);
                bool hideInSelfServiceCourseSearch = _locations[x, 9].ToUpperInvariant() == "Y";
                int tempI1;
                int? i1 = int.TryParse(_locations[x, 10], out tempI1) ? tempI1 : (int?)null;

                locs.Add(new Location(_locations[x, 0], _locations[x, 1], _locations[x, 2], d1, d2, d3, d4, _locations[x, 7],
                    new List<string>() { _locations[x, 8] }, hideInSelfServiceCourseSearch)
                { SortOrder = i1 });
            }
            return locs;
        }
    }
}