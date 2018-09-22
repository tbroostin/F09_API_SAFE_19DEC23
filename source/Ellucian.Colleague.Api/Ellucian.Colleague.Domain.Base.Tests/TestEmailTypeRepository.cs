// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestEmailTypeRepository
    {
        private string[,] emailTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "PRI", "Primary", "personal"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "WEB", "Web Obtained", "sales"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "CR", "Main Office/Headquarters", "school"},
                                            {"61336621-71e9-49df-ade3-65cee449ccfb", "B", "Business/Employment", "business"},
                                            {"7fc96ae3-fe58-4f99-91aa-75f9e8a36a62", "V", "Vacation", "general"},
                                            {"e67855a0-f5c9-4aa8-a387-9797a6eaacae", "COB", "Correction", "billing"},
                                            {"5914cb0d-ec01-43f5-b1a2-faa73b9a963b", "AL", "Alternate/Seasonal", "legal"},
                                            {"ec2b0f56-65ef-4752-814e-e73a6f034298", "FO", "Foreign", "hr"},
                                            {"e63f0368-4bad-4b49-8fed-36bf79j551d4", "WB", "Web-obtained", "media"},
                                            {"w53g4371-2cfd-2h48-0fgd-85vx27b551d4", "CB", "Campus Box", "parent"},
                                            {"5914cb0d-ec01-43f5-b1a2-faa73b9a963b", "AL2", "Alternate/Seasonal", "support"},
                                            {"61336621-71e9-49df-ade3-65cee449ccfb", "B2", "Business/Employment", "matchingGifts"},
                                            {"3cf4t676-2aed-5626-b67c-3r6ab796e024", "H3", "Home/Permanent", ""},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "H2", "Home/Permanent", "family"}
                                         
                                      };

        public IEnumerable<EmailType> Get()
        {
            var emailTypeList = new List<EmailType>();

            // There are 3 fields for each email type in the array
            var items = emailTypes.Length / 4;

            for (int x = 0; x < items; x++)
            {
                 var emailTypeCategory = new EmailTypeCategory();
                 switch (emailTypes[x, 3])
                 {
                     case "personal":
                         emailTypeCategory = EmailTypeCategory.Personal;
                         break;
                     case "billing":
                         emailTypeCategory = EmailTypeCategory.Billing;
                         break;
                     case "business":
                         emailTypeCategory = EmailTypeCategory.Business;
                         break;
                     case "general":
                         emailTypeCategory = EmailTypeCategory.General;
                         break;
                     case "hr":
                         emailTypeCategory = EmailTypeCategory.HR;
                         break;
                     case "legal":
                         emailTypeCategory = EmailTypeCategory.Legal;
                         break;
                     case "school":
                         emailTypeCategory = EmailTypeCategory.School;
                         break;
                     case "media":
                         emailTypeCategory = EmailTypeCategory.Media;
                         break;
                     case "parent":
                         emailTypeCategory = EmailTypeCategory.Parent;
                         break;
                     case "family":
                         emailTypeCategory = EmailTypeCategory.Family;
                         break;
                     case "sales":
                         emailTypeCategory = EmailTypeCategory.Sales;
                         break;
                     case "support":
                         emailTypeCategory = EmailTypeCategory.Support;
                         break;
                     case "matchingGifts":
                         emailTypeCategory = EmailTypeCategory.MatchingGifts;
                         break;
                     default:
                         emailTypeCategory = EmailTypeCategory.Other;
                         break;
                 }
                emailTypeList.Add(new EmailType(emailTypes[x, 0], emailTypes[x, 1], emailTypes[x, 2], emailTypeCategory));
            }
            return emailTypeList;
        }
    }
}