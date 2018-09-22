// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestLocationTypeRepository
    {
        private string[,] locationTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "H", "Home/Permanent", "home", "home",}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "CR", "Main Office/Headquarters", "school", "main"},
                                            {"61336621-71e9-49df-ade3-65cee449ccfb", "B", "Business/Employment", "business", "business"},
                                            {"7fc96ae3-fe58-4f99-91aa-75f9e8a36a62", "V", "Vacation", "vacation", ""},
                                            {"e67855a0-f5c9-4aa8-a387-9797a6eaacae", "COB", "Correction", "billing", "pobox"},
                                            {"5914cb0d-ec01-43f5-b1a2-faa73b9a963b", "AL", "Alternate/Seasonal", "shipping", "support"},
                                            {"ec2b0f56-65ef-4752-814e-e73a6f034298", "FO", "Foreign", "mailing", "regional"},
                                            {"e63f0368-4bad-4b49-8fed-36bf79j551d4", "WB", "Web-obtained", "", "branch"},
                                            {"w53g4371-2cfd-2h48-0fgd-85vx27b551d4", "CB", "Campus Box", "", ""},
                                            {"ec2b0f56-65ef-4752-814e-e73a6f034298", "FO2", "Foreign", "", "regional"},
                                            {"5914cb0d-ec01-43f5-b1a2-faa73b9a963b", "AL2", "Alternate/Seasonal", "", "support"},
                                            {"e67855a0-f5c9-4aa8-a387-9797a6eaacae", "COB2", "Correction", "", "pobox"},
                                            {"61336621-71e9-49df-ade3-65cee449ccfb", "B2", "Business/Employment", "", "business"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "CR2", "Main Office/Headquarters", "", "main"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "H2", "Home/Permanent", "", "home",}
                                          };

        public IEnumerable<LocationTypeItem> Get()
        {
            var locationTypeList = new List<LocationTypeItem>();

            // There are 3 fields for each location type in the array
            var items = locationTypes.Length / 5;

            for (int x = 0; x < items; x++)
            {
                var entType = new EntityType();
                // person code is defined, always select entity type of person
                if (!string.IsNullOrEmpty(locationTypes[x, 3]))
                {
                    entType = EntityType.Person;
                }
                else
                {
                    // Both person and organization code is not defined so use person
                    if (string.IsNullOrEmpty(locationTypes[x, 4]))
                    {
                        entType = EntityType.Person;
                    }
                    // only organization code is defined so use organization entity type
                    else
                    {
                        entType = EntityType.Organization;
                    }
                }

                var perLocType = new PersonLocationType();
                switch (locationTypes[x, 3])
                {
                    case "billing":
                        perLocType = PersonLocationType.Billing;
                        break;
                    case "business":
                        perLocType = PersonLocationType.Business;
                        break;
                    case "home":
                        perLocType = PersonLocationType.Home;
                        break;
                    case "mailing":
                        perLocType = PersonLocationType.Mailing;
                        break;
                    case "vacation":
                        perLocType = PersonLocationType.Vacation;
                        break;
                    case "school":
                        perLocType = PersonLocationType.School;
                        break;
                    case "shipping":
                        perLocType = PersonLocationType.Shipping;
                        break;
                    default:
                        perLocType = PersonLocationType.Other;
                        break;
                }

                var orgLocType = new OrganizationLocationType();
                switch (locationTypes[x, 4])
                {
                    case "branch":
                        orgLocType = OrganizationLocationType.Branch;
                        break;
                    case "business":
                        orgLocType = OrganizationLocationType.Business;
                        break;
                    case "main":
                        orgLocType = OrganizationLocationType.Main;
                        break;
                    case "support":
                        orgLocType = OrganizationLocationType.Support;
                        break;
                    case "pobox":
                        orgLocType = OrganizationLocationType.Pobox;
                        break;
                    case "regional":
                        orgLocType = OrganizationLocationType.Region;
                        break;
                    default:
                        orgLocType = OrganizationLocationType.Other;
                        break;
                }
                locationTypeList.Add(new LocationTypeItem(locationTypes[x, 0], locationTypes[x, 1], locationTypes[x, 2], entType, perLocType, orgLocType));
            }
            return locationTypeList;
        }
    }
}