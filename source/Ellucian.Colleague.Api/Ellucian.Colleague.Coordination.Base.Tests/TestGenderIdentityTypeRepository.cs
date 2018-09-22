using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestGenderIdentityTypeRepository
    {
        private string[,] genderIdentityTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "ALT", "Alternate Gender Identity"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "FEM", "Female"},
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "MAL", "Male"},
                                            {"61336621-71e9-49df-ade3-65cee449ccfb", "NCO", "Non-conforming"},
                                            {"7fc96ae3-fe58-4f99-91aa-75f9e8a36a62", "TFM", "Transexual (F/M)"},
                                            {"e67855a0-f5c9-4aa8-a387-9797a6eaacae", "TMF", "Transexual (M/F)"}

                                      };

        public IEnumerable<GenderIdentityType> Get()
        {
            var genderIdentityTypeList = new List<GenderIdentityType>();

            // There are 3 fields for each gender identity type in the array
            var items = genderIdentityTypes.Length / 3;

            for (int x = 0; x < items; x++)
            {

                genderIdentityTypeList.Add(new GenderIdentityType(genderIdentityTypes[x, 0], genderIdentityTypes[x, 1], genderIdentityTypes[x, 2]));

            }
            return genderIdentityTypeList;
        }
    }
}