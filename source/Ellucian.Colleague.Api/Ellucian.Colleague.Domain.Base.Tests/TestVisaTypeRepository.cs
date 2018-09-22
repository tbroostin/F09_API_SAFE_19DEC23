// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestVisaTypeRepository
    {
        private string[,] visaType =
        {
            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "H1", "H1Visa", "immigrant"},
            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "H2", "H2Visa", "immigrant"},
            {"b769e6a9-da86-47a9-ab21-b17198880439", "C1", "C1Visa", "nonimmigrant"},
            {"e297656e-8d50-4c63-a2dd-0fcfc46647c4", "C2", "C2Visa", "nonimmigrant"},
            {"8d0e291e-7246-4067-aff1-47ff6adc0392", "B1", "B1Visa", "nonimmigrant"}
        };

        public IEnumerable<VisaTypeGuidItem> GetVisaTypes()
        {
            var visaTypeList = new List<VisaTypeGuidItem>();

            // There are 3 fields for each visa type in the array
            var items = visaType.Length / 4;

            for (int x = 0; x < items; x++)
            {
                visaTypeList.Add(
                    new VisaTypeGuidItem(
                        visaType[x, 0], visaType[x, 1], visaType[x, 2],
                        ConvertVisaTypeCategoryCodeToVisaTypeCategory(visaType[x, 3]) 
                    ));
            }
            return visaTypeList;
        }

        private VisaTypeCategory ConvertVisaTypeCategoryCodeToVisaTypeCategory(string code)
        {
            if (string.IsNullOrEmpty(code))
                return VisaTypeCategory.Immigrant;

            if (string.Equals(code, "immigrant", StringComparison.OrdinalIgnoreCase))
                return VisaTypeCategory.Immigrant;

            if (string.Equals(code, "nonimmigrant", StringComparison.OrdinalIgnoreCase))
                return VisaTypeCategory.NonImmigrant;

            return VisaTypeCategory.Immigrant;
        }
    }
}
