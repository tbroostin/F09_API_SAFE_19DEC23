// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests
{
    public class TestAdministrativeInstructionalMethodRepository
    {
        private string[,] administrativeInstructionalMethods = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "LEC", "Lecture", "D8CED21A-F220-4F79-9544-706E13B51972"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "LAB", "Laboratory", "705F052C-7B63-492D-A7CA-5769CE003274"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "OL", "Online Learning", "67B0664B-0650-4C88-ACC6-FB0C689CB519"}
                                      };

        public IEnumerable<AdministrativeInstructionalMethod> GetAdministrativeInstructionalMethods()
        {
            var administrativeInstructionalMethodList = new List<AdministrativeInstructionalMethod>();

            // There are 4 fields for each instructional method in the array
            var items = administrativeInstructionalMethods.Length / 4;

            for (int x = 0; x < items; x++)
            {
                administrativeInstructionalMethodList.Add(new AdministrativeInstructionalMethod(administrativeInstructionalMethods[x, 0], administrativeInstructionalMethods[x, 1], administrativeInstructionalMethods[x, 2], administrativeInstructionalMethods[x, 3]));
            }
            return administrativeInstructionalMethodList;
        }
    }
}
