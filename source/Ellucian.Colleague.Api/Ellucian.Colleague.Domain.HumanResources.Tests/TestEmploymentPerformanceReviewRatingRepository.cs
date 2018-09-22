// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestEmploymentPerformanceReviewRatingRepository
    {
            private readonly string[,] _employmentPerformanceReviewRatings = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "BA", "Description"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "CA", "Description"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "CC", "Description"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "EP", "Description"}
                                      };

            public IEnumerable<EmploymentPerformanceReviewRating> GetEmploymentPerformanceReviewRatings()
            {
                var empPerformanceList = new List<EmploymentPerformanceReviewRating>();

                // There are 3 fields for each employment performance review rating in the array
                var items = _employmentPerformanceReviewRatings.Length / 3;

                for (var x = 0; x < items; x++)
                {
                    empPerformanceList.Add(new EmploymentPerformanceReviewRating(_employmentPerformanceReviewRatings[x, 0], _employmentPerformanceReviewRatings[x, 1], _employmentPerformanceReviewRatings[x, 2]));
                }
                return empPerformanceList;
            }
    }
}