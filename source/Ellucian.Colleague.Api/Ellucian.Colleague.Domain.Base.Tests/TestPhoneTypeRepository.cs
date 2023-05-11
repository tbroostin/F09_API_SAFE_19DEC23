// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPhoneTypeRepository
    {
        private string[,] phoneTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "H", "Home/Permanent", "home", "N"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "CR", "Main Office/Headquarters", "school", "N"},
                                            {"61336621-71e9-49df-ade3-65cee449ccfb", "B", "Business/Employment", "business", "N"},
                                            {"7fc96ae3-fe58-4f99-91aa-75f9e8a36a62", "V", "Vacation", "vacation", "N"},
                                            {"e67855a0-f5c9-4aa8-a387-9797a6eaacae", "COB", "Correction", "billing", "N"},
                                            {"5914cb0d-ec01-43f5-b1a2-faa73b9a963b", "AL", "Alternate/Seasonal", "mobile", "Y"},
                                            {"ec2b0f56-65ef-4752-814e-e73a6f034298", "FO", "Foreign", "fax", "N"},
                                            {"e63f0368-4bad-4b49-8fed-36bf79j551d4", "WB", "Web-obtained", "branch", "Y"},
                                            {"w53g4371-2cfd-2h48-0fgd-85vx27b551d4", "CB", "Campus Box", "parent", "N"},
                                            {"ec2b0f56-65ef-4752-814e-e73a6f034298", "FO2", "Foreign", "regional", "N"},
                                            {"5914cb0d-ec01-43f5-b1a2-faa73b9a963b", "AL2", "Alternate/Seasonal", "support", "N"},
                                            {"e67855a0-f5c9-4aa8-a387-9797a6eaacae", "COB2", "Correction", "tdd", "N"},
                                            {"61336621-71e9-49df-ade3-65cee449ccfb", "B2", "Business/Employment", "matching", "N"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "CR2", "Main Office/Headquarters", "main", "N"},
                                            {"3cf4t676-2aed-5626-b67c-3r6ab796e024", "H3", "Home/Permanent", "", "N"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "H2", "Home/Permanent", "family", "N"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "H2", "Home/Permanent", "pager", "N"}
                                          };

        public IEnumerable<PhoneType> Get()
        {
            var phoneTypeList = new List<PhoneType>();

            // There are 4 fields for each phone type in the array
            var items = phoneTypes.Length / 5;

            for (int x = 0; x < items; x++)
            {
                var phoneTypeCategory = new PhoneTypeCategory();
                switch (phoneTypes[x, 3])
                {
                    case "billing":
                        phoneTypeCategory = PhoneTypeCategory.Billing;
                        break;
                    case "business":
                        phoneTypeCategory = PhoneTypeCategory.Business;
                        break;
                    case "home":
                        phoneTypeCategory = PhoneTypeCategory.Home;
                        break;
                    case "fax":
                        phoneTypeCategory = PhoneTypeCategory.Fax;
                        break;
                    case "vacation":
                        phoneTypeCategory = PhoneTypeCategory.Vacation;
                        break;
                    case "school":
                        phoneTypeCategory = PhoneTypeCategory.School;
                        break;
                    case "mobile":
                        phoneTypeCategory = PhoneTypeCategory.Mobile;
                        break;
                    case "pager":
                        phoneTypeCategory = PhoneTypeCategory.Pager;
                        break;
                    case "parent":
                        phoneTypeCategory = PhoneTypeCategory.Parent;
                        break;
                    case "family":
                        phoneTypeCategory = PhoneTypeCategory.Family;
                        break;
                    case "branch":
                        phoneTypeCategory = PhoneTypeCategory.Branch;
                        break;
                    case "main":
                        phoneTypeCategory = PhoneTypeCategory.Main;
                        break;
                    case "support":
                        phoneTypeCategory = PhoneTypeCategory.Support;
                        break;
                    case "tdd":
                        phoneTypeCategory = PhoneTypeCategory.TDD;
                        break;
                    case "regional":
                        phoneTypeCategory = PhoneTypeCategory.Region;
                        break;
                    case "matching":
                        phoneTypeCategory = PhoneTypeCategory.MatchingGifts;
                        break;
                    default:
                        phoneTypeCategory = PhoneTypeCategory.Other;
                        break;
                }
                var isPersonal = phoneTypes[x, 4] == "Y" ? true : false;
                phoneTypeList.Add(new PhoneType(phoneTypes[x, 0], phoneTypes[x, 1], phoneTypes[x, 2], phoneTypeCategory, isPersonal));
            }
            return phoneTypeList;

            //private string[,] phoneTypes = {
            //                                    //GUID   CODE   DESCRIPTION
            //                                    {"69d3987d-a1da-4c32-a7ce-edb9b6c9c8b5", "HO", "Home / Permanent"}, 
            //                                    {"ce7e23e2-5c56-49f8-a851-5275f64c5e51", "CP", "Cell Phone"},
            //                                    {"9b00ef0a-312c-49ad-a6a4-c91131747a23", "BU","Business"},
            //                                    {"def010fd-1db0-4ce1-a23b-557693b318a4", "MA","Main"},
            //                                    {"c6f19434-98c0-4760-a82b-d3c02d524e28", "BR","Branch"},
            //                                    {"720e7edf-21c1-4929-b891-93bc2509ba62", "RE","Region"},
            //                                    {"a841f0fa-c101-4173-a5c9-380d3ae3c8cf", "SU","Support"},
            //                                    {"5b110bf2-a836-4e0a-b115-21e8686cce53", "OT","Other"},
            //                                    {"a24fea4b-5af1-4224-879b-b4c0b2427a8f", "TDD","TDD"},
            //                                    {"b4aabf53-1bf0-4ade-84d7-b33a331644dc", "F","FAX"},
            //                               };

            //public IEnumerable<PhoneTypeItem> Get()
            //{
            //    var phoneTypeList = new List<PhoneTypeItem>();

            //    // There are 3 fields for each phone type in the array
            //    var items = phoneTypes.Length / 3;

            //    for (int x = 0; x < items; x++)
            //    {
            //        phoneTypeList.Add(new PhoneTypeItem(phoneTypes[x, 0], phoneTypes[x, 1], phoneTypes[x, 2], EntityType.Person, PersonPhoneType.Home, OrganizationPhoneType.Main));
            //    }
            //    return phoneTypeList;
            //}


            //public IEnumerable<PhoneTypeItem> GetOrganizationPhoneTypes()
            //{
            //    var phoneTypeList = new List<PhoneTypeItem>();

            //    var values = Enum.GetValues(typeof(OrganizationPhoneType)).Cast<Ellucian.Colleague.Domain.Base.Entities.OrganizationPhoneType>();
            //    foreach (var value in values)
            //    {

            //        // There are 3 fields for each phone type in the array
            //        var items = phoneTypes.Length / 3;

            //        for (int x = 0; x < items; x++)
            //        {
            //            phoneTypeList.Add(new PhoneTypeItem(phoneTypes[x, 0], phoneTypes[x, 1], phoneTypes[x, 2], EntityType.Organization, PersonPhoneType.Home, value));
            //        }
            //    }
            //    return phoneTypeList;
            //}


            //public IEnumerable<PhoneTypeItem> GetPersonPhoneType()
            //{
            //    var phoneTypeList = new List<PhoneTypeItem>();

            //    var values = Enum.GetValues(typeof(PersonPhoneType)).Cast<Ellucian.Colleague.Domain.Base.Entities.PersonPhoneType>();
            //    foreach (var value in values)
            //    {

            //        // There are 3 fields for each phone type in the array
            //        var items = phoneTypes.Length / 3;

            //        for (int x = 0; x < items; x++)
            //        {
            //            phoneTypeList.Add(new PhoneTypeItem(phoneTypes[x, 0], phoneTypes[x, 1], phoneTypes[x, 2], EntityType.Person, value, OrganizationPhoneType.Main));
            //        }
            //    }
            //    return phoneTypeList;
            //}
        }
    }
}