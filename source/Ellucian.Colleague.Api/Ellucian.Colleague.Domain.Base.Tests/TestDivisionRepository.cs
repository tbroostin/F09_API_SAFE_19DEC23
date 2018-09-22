// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestDivisionRepository
    {
        private readonly string[,] _divisions =
        {
            //GUID                                    ID      DESC							SCHOOL							                                           
            {"50052c84-9f25-4f08-bd13-48e2a2ec4f49", "AHLTH", "Division of Allied Health", "APSC"},
            {"2a2e2906-d46b-4698-80f6-af87b8083c64", "ART", "Division of Art", "ARSC"},
            {"b36dbff6-6a2e-43f4-ad80-b7c214de799b", "BUSIN", "Division of Business", "COE"},
            {"ecd21389-7f9f-4b35-a701-1a50fa6f1bce", "EDUCA", "Division of Education", "ARSC"},
            {"b7c2a84f-315f-4b10-94d1-07d56215159d", "HUMAN", "Division of Humanities", "ARSC"},
            {"ca7d1b7d-ab81-4f9f-8f3b-83f4c9031f89", "INDUS", "Division of Industrial Technology", "APSC"},
            {"79882da8-39b5-4731-b624-de0047657986", "SCMAT", "Division of Science & Mathematics", "ARSC"},
            {"42402fbe-9263-4080-b21a-f1eeef6df54f", "SOCSC", "Division of Social Science", "ARSC"},
            {"9f4cf9ad-104a-4f14-94d9-b6185a7b1f57", "THIRD", "Third Division", "HKNCK"},
            {"dcc027c3-1f00-4e22-bb70-87a5ee2d90e9", "TRAIN", "Division of Professional Training", "APSC"}
        };

        public IEnumerable<Division> GetDivisions()
        {
            var division = new List<Division>();

            // There are 4 fields for each division in the array
            var items = _divisions.Length/4;

            for (var x = 0; x < items; x++)
            {
                var guid = Guid.NewGuid().ToString();
                division.Add(new Division(_divisions[x, 0], _divisions[x, 1], _divisions[x, 2]) { SchoolCode = _divisions[x, 3], InstitutionId = "0000043" });
            }
            return division;
        }
    }
}