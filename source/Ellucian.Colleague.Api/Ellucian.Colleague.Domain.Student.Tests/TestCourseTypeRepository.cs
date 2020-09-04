// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestCourseTypeRepository
    {
                private string[,] courseTypes = {
                                            //GUID                                    CODE    DESCRIPTION       SPECIAL_PROCESSING_2   SPECIAL_PROCESSING_1
                                            {"f660ba1c-e9fa-48a9-9948-a11bbd60ef03", "HONOR", "Honors",               "Y",                  "I"}, 
                                            {"36ab7a63-3444-49d8-b629-8094ced00f89", "STND",  "Standard",              "",                  ""},
                                            {"ca680ded-6f30-4482-a19c-126ab477c0a9", "COOP",  "Coop Work Experience", "N",                  ""},
                                            {"d792f3cf-1c20-4aae-a10a-ddd99f3b244f", "REMED", "Remedial",             "",                   ""}, 
                                            {"e89907ec-b9a5-4f71-8532-baa79009783c", "VOC",   "Vocational",           "",                   ""},
                                            {"b262328f-3e75-4a71-b0a6-932dd6da8dcf", "WR",    "Writing-Intensive",    "O",                  ""}
                                      };


        public IEnumerable<CourseType> Get()
        {
            var crsTypes = new List<CourseType>();

            // There are 4 fields for each courseType in the array
            var items = courseTypes.Length / 5;

            for (int x = 0; x < items; x++)
            {
                var ct = new CourseType(courseTypes[x, 0], courseTypes[x, 1], courseTypes[x, 2], courseTypes[x, 3] != "N");
                ct.Categorization = courseTypes[x, 4];
                crsTypes.Add(ct);
            }
            return crsTypes;
        }
    }
}