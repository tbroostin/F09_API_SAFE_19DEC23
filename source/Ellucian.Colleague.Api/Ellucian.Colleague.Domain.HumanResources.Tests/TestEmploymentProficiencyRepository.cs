// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestEmploymentProficiencyRepository
    {
        private readonly string[,] _employmentProficiencies = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "BA", "Description"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "CA", "Description"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "CC", "Description"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "EP", "Description"}
                                      };

        public IEnumerable<EmploymentProficiency> GetEmploymentProficiencies()
        {
            var empProficiencyList = new List<EmploymentProficiency>();

            // There are 3 fields for each employment proficiency in the array
            var items = _employmentProficiencies.Length / 3;

            for (var x = 0; x < items; x++)
            {
                empProficiencyList.Add(new EmploymentProficiency(_employmentProficiencies[x, 0], _employmentProficiencies[x, 1], _employmentProficiencies[x, 2]));
            }
            return empProficiencyList;
        }
    }
}