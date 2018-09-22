// Copyright 2015-16 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestAddressTypeRepository
    {
        private string[,] addressTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "H", "Home/Permanent", "home"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "CR", "Main Office/Headquarters", "school"},
                                            {"61336621-71e9-49df-ade3-65cee449ccfb", "B", "Business/Employment", "business"},
                                            {"7fc96ae3-fe58-4f99-91aa-75f9e8a36a62", "V", "Vacation", "vacation"},
                                            {"e67855a0-f5c9-4aa8-a387-9797a6eaacae", "COB", "Correction", "billing"},
                                            {"5914cb0d-ec01-43f5-b1a2-faa73b9a963b", "AL", "Alternate/Seasonal", "shipping"},
                                            {"ec2b0f56-65ef-4752-814e-e73a6f034298", "FO", "Foreign", "mailing"},
                                            {"e63f0368-4bad-4b49-8fed-36bf79j551d4", "WB", "Web-obtained", "branch"},
                                            {"w53g4371-2cfd-2h48-0fgd-85vx27b551d4", "CB", "Campus Box", "parent"},
                                            {"ec2b0f56-65ef-4752-814e-e73a6f034298", "FO2", "Foreign", "regional"},
                                            {"5914cb0d-ec01-43f5-b1a2-faa73b9a963b", "AL2", "Alternate/Seasonal", "support"},
                                            {"e67855a0-f5c9-4aa8-a387-9797a6eaacae", "COB2", "Correction", "pobox"},
                                            {"61336621-71e9-49df-ade3-65cee449ccfb", "B2", "Business/Employment", "matchingGifts"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "CR2", "Main Office/Headquarters", "main"},
                                            {"3cf4t676-2aed-5626-b67c-3r6ab796e024", "H3", "Home/Permanent", ""},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "H2", "Home/Permanent", "family"}
                                          };

        public IEnumerable<AddressType2> Get()
        {
            var addressTypeList = new List<AddressType2>();

            // There are 4 fields for each address type in the array
            var items = addressTypes.Length / 4;

            for (int x = 0; x < items; x++)
            {
                var addTypeCategory = new AddressTypeCategory();
                switch (addressTypes[x, 3])
                {
                    case "billing":
                        addTypeCategory = AddressTypeCategory.Billing;
                        break;
                    case "business":
                        addTypeCategory = AddressTypeCategory.Business;
                        break;
                    case "home":
                        addTypeCategory = AddressTypeCategory.Home;
                        break;
                    case "mailing":
                        addTypeCategory = AddressTypeCategory.Mailing;
                        break;
                    case "vacation":
                        addTypeCategory = AddressTypeCategory.Vacation;
                        break;
                    case "school":
                        addTypeCategory = AddressTypeCategory.School;
                        break;
                    case "shipping":
                        addTypeCategory = AddressTypeCategory.Shipping;
                        break;
                    case "parent":
                        addTypeCategory = AddressTypeCategory.Parent;
                        break;
                    case "family":
                        addTypeCategory = AddressTypeCategory.Family;
                        break;
                    case "branch":
                        addTypeCategory = AddressTypeCategory.Branch;
                        break;
                    case "main":
                        addTypeCategory = AddressTypeCategory.Main;
                        break;
                    case "support":
                        addTypeCategory = AddressTypeCategory.Support;
                        break;
                    case "pobox":
                        addTypeCategory = AddressTypeCategory.Pobox;
                        break;
                    case "regional":
                        addTypeCategory = AddressTypeCategory.Region;
                        break;
                    case "matchingGifts":
                        addTypeCategory = AddressTypeCategory.MatchingGifts;
                        break;
                    default:
                        addTypeCategory = AddressTypeCategory.Other;
                        break;
                }
                addressTypeList.Add(new AddressType2(addressTypes[x, 0], addressTypes[x, 1], addressTypes[x, 2], addTypeCategory));
            }
            return addressTypeList;
        }
    }
}