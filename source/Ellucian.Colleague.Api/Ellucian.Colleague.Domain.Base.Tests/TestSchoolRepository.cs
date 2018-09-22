// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestSchoolRepository
    {
        private readonly string[,] _schools =
        {
            //GUID                                    ID      DESC						INSTITUTIONS_ID							                                           
            {"62052c84-9f25-4f08-bd13-48e2a2ec4f49", "APSC", "School of Applied Sciences", "0000001"},
            {"3m2e2906-d46b-4698-80f6-af87b8083c64", "ARSC", "School of Arts & Sciences", "ARSC"},
            {"e96dbff6-6a2e-43f4-ad80-b7c214de799b", "COE", "Coe School Of Outdoor Educ.", "COE"},
            {"pud21389-7f9f-4b35-a701-1a50fa6f1bce", "HKNCK", "School of Hard Knocks", "ARSC"},
            {"e4c2a84f-315f-4b10-94d1-07d56215159d", "LAW", "School of Law", "ARSC"}
            
        };

        public IEnumerable<School> GetSchools()
        {
            var school = new List<School>();

            // There are 4 fields for each school in the array
            var items = _schools.Length/4;

            for (var x = 0; x < items; x++)
            {
                var guid = Guid.NewGuid().ToString();
                school.Add(new School(_schools[x, 0], _schools[x, 1], _schools[x, 2]) { InstitutionId = _schools[x, 3] });
            }
            return school;
        }
    }
}