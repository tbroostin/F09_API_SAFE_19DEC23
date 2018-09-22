// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;


namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestIdentityDocumentTypeRepository
    {
        private readonly string[,] _identityDocumentTypes = {
                                            //GUID   CODE   DESCRIPTION   SP3
                                           
                                            {"4236641d-5c29-4884-9a17-530820ec9d29", "PASSPORT", "Passport Document", "passport"},
                                            {"a3fe5df5-91ff-49e0-b418-cd047461594a", "LICENSE", "Driver's License", "photoId"},
                                            {"693d90bb-2db5-4255-aa00-68e1ca8f51cc", "BIRTHCERT", "Birth Certificate", "birthCertificate"},
                                            {"a3fe5df5-91ff-49e0-b418-cd047461594a", "PASS", "Passport", "passport"},
                                           
                                      };

        public IEnumerable<IdentityDocumentType> Get()
        {
            var identityDocumentTypeList = new List<IdentityDocumentType>();

            // There are 4 fields for each idenity document tpye in the array
            var items = _identityDocumentTypes.Length / 4;

            for (int x = 0; x < items; x++)
            {
                var newType = new IdentityDocumentTypeCategory();
                switch (_identityDocumentTypes[x,1])
                {
                    
                    case "PASSPORT":
                        newType = IdentityDocumentTypeCategory.Passport;
                        break;
                    case "LICENSE":
                        newType = IdentityDocumentTypeCategory.PhotoId;
                        break;
                    case "OTHER":
                        newType = IdentityDocumentTypeCategory.Other;
                        break;
                    default:
                        newType = IdentityDocumentTypeCategory.Other;
                        break;
                }
                identityDocumentTypeList.Add(new IdentityDocumentType(_identityDocumentTypes[x, 0], _identityDocumentTypes[x, 1], _identityDocumentTypes[x, 2], newType));
            }
            return identityDocumentTypeList;
        }
    }
}