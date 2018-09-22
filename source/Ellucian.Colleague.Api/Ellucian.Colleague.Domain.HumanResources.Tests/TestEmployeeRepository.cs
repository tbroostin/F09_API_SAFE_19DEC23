// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestEmployeeRepository
    {
        private string[,] employees = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "e9e6837f-2c51-431b-9069-4ac4c0da3041"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "9ae3a175-1dfd-4937-b97b-3c9ad596e023"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "e9e6837f-2c51-431b-9069-4ac4c0da3041"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "bfea651b-8e27-4fcd-abe3-04573443c04c"},
                                            {"00000000-0000-0000-0000-000000000000", "bfea651b-8e27-4fcd-abe3-04573443c04c"}
                                      };

        public IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.Employee> GetEmployees()
        {
            var employeeList = new List<Ellucian.Colleague.Domain.HumanResources.Entities.Employee>();

            // There are 3 fields for each job change reason in the array
            var items = employees.Length / 2;

            for (int x = 0; x < items; x++)
            {
                employeeList.Add(new Ellucian.Colleague.Domain.HumanResources.Entities.Employee(employees[x, 0], employees[x, 1]));
            }
            return employeeList;
        }

        public async Task<IEnumerable<string>> GetEmployeeKeysAsync()
        {
            return await Task.FromResult(new List<string>() { "123" });
        }
    }
}
