// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestAddressRepository
    {
        public string[,] _addressData = {
                                       {"0011304", "052 ", "PO Box 14428", "PO Box 14428", "", "", "8001", "AU", "Australia", "", "d44135f9-0924-45d4-8b91-be9531aa7773", "", "Home", "HO"},
                                       {"0000304", "101 ", "65498 Ft. Belvoir Hwy;Mount Vernon;Alexandria, VA 21348", "65498 Ft. Belvoir Hwy;Mount Vernon", "Alexandria", "VA", "21348", "US", "United States of America", "Father of our Country", "d44134f9-0924-45d4-8b91-be9531aa7773", "FFX", "Home", "HO"},
                                       {"0000304", "102 ", "", "235 Beacon Hill Dr.", "Boston", "MA", "03549", "", "", "", "081ae7a2-f7b3-45f4-808b-a35f50c5c418", "", "Home", "HO"},
                                       {"0000304", "103 ", "1 Champs d'Elyssie;U.S. Embassy;Paris;FRANCE", "1 Champs d'Elyssie", "Paris", "", "", "FR", "France", "Ambassador to France", "da5905c9-d607-4788-996a-f0f2567b0bd4", "", "Mailing", "MA"},
                                       {"0000404", "104", "", "1812 Dolly Madison Dr.", "Arlington", "VA", "22146", "", "", "", "ec9da88c-b14a-4a8e-a9d0-4760b31816aa", "", "Home", "HO"},
                                       {"0000404", "105", "", "1787 Constitution Ave.", "Franklin", "VA", "34567", "US", "", "", "9482c660-cbe1-4c4a-9ee1-10818a7c7f27", "", "Home", "HO"},
                                       {"0000504", "106", "", "1600 Pennsylvania Ave.;The White House", "Washington", "DC", "12345", "US", "", "POTUS", "ebdd0871-54aa-4237-8a66-ddb7cbb15753", "", "Home", "HO"},
                                       {"0000504", "107", "", "7413 Clifton Quarry Dr.", "Clifton", "VA", "20121", "", "", "", "2ec57bef-8a13-4a6a-8a79-ea99d062fd27", "", "Mailing", "MA"},
                                       {"9999999", "108 ", null, null, null, null, null, null, null, null, "d43bbf09-bbdc-4b17-86cc-4a183b1ec6d6", "", "Home", "HO"},
                                       {"9999998", "109 ", "", "", "", "", "", "", "", "", "3ba6b4ba-8668-42e0-a4aa-319410aff7cb", "", "Home", "HO"}
                                   };

        public IEnumerable<Address> GetAddressData()
        {
            string[,] recordData = _addressData;

            int recordCount = recordData.Length / 13;
            var results = new List<Address>();
            for (int i = 0; i < recordCount; i++)
            {
                Address response = new Address();
                string key = recordData[i, 0].TrimEnd();
                string addressId = (recordData[i, 1] == null) ? String.Empty : recordData[i, 1].TrimEnd();
                List<string> label = (recordData[i, 2] == null) ? null : recordData[i, 2].TrimEnd().Split(';').ToList<string>();
                List<string> lines = (recordData[i, 3] == null) ? null : recordData[i, 3].TrimEnd().Split(';').ToList<string>();
                string city = (recordData[i, 4] == null) ? null : recordData[i, 4].TrimEnd();
                string state = (recordData[i, 5] == null) ? null : recordData[i, 5].TrimEnd();
                string zip = (recordData[i, 6] == null) ? null : recordData[i, 6].TrimEnd();
                string country = (recordData[i, 7] == null) ? null : recordData[i, 7].TrimEnd();
                string countryDesc = (recordData[i, 8] == null) ? null : recordData[i, 8].TrimEnd();
                string modifier = (recordData[i, 9] == null) ? null : recordData[i, 9].TrimEnd();
                string guid = (recordData[i, 10] == null) ? new Guid().ToString() : recordData[i, 10].TrimEnd();
                string county = (recordData[i, 11] == null) ? null : recordData[i, 11].TrimEnd();
                string type = recordData[i, 12];
                string typeCode = recordData[i, 13];

                response.Guid = guid;
                response.AddressLines = lines;
                response.City = city;
                response.State = state;
                response.PostalCode = zip;
                response.Country = country;
                response.County = county;
                response.Type = type;
                response.TypeCode = typeCode;
                results.Add(response);
            }
            return results;
        }

        public IEnumerable<Address> GetAddressDataWithNullId()
        {
            string[,] recordData = _addressData;

            int recordCount = recordData.Length / 12;
            List<Address> results = this.GetAddressData().ToList();
            var addrWithNoGuid = new List<Address>() 
            {
                new Address()
                {
                    City = "Boston",
                    State = "MA",
                    PostalCode = "02135",
                    Country = "",
                    Type = "Home",
                    TypeCode = "HO"
                }
            
            };
            results.Add(addrWithNoGuid.First());
            return results;
        }
    }
}