// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestInterestTypesRepository
    {
        private string[,] interestTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "AR", "Arts"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "AT", "Athletics"},
                                            {"b769e6a9-da86-47a9-ab21-b17198880439", "F", "Fraternal Organization"}, 
                                            {"e297656e-8d50-4c63-a2dd-0fcfc46647c4", "J", "Journalism"}, 
                                            {"8d0e291e-7246-4067-aff1-47ff6adc0392", "M", "Music"}, 
                                            {"b91bbee8-88d1-4063-86e2-e7cb1865b45a", "ST", "Student Government"}, 
                                            {"4eaca2e7-fb59-44b6-be64-ce9e2ad73e81", "SC", "Science"}
                                            };

        public IEnumerable<InterestType> GetInterestTypes()
        {
            var interestTypeList = new List<InterestType>();

            // There are 3 fields for each interest type in the array
            var items = interestTypes.Length / 3;

            for (int x = 0; x < items; x++)
            {
                interestTypeList.Add(
                    new InterestType(
                        interestTypes[x, 0], interestTypes[x, 1], interestTypes[x, 2]
                     ));
            }
            return interestTypeList;
        }
    }
}