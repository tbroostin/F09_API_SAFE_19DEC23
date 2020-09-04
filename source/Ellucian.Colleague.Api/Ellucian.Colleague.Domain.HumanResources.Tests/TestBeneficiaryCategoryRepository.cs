// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestBeneficiaryCategoryRepository
    {
        private string[,] beneficiaryCategories = {
                                            //CODE   DESCRIPTION PROCESSINGCODE
                                            {"CO", "Co-Owner", ""},
                                            {"ALT", "Alternate Owner", ""},
                                            {"BENEF", "Beneficiary", "P"},
                                      };

        public IEnumerable<BeneficiaryCategory> GetBeneficiaryCategoriesAsync()
        {
            var beneficiaryCategoryList = new List<BeneficiaryCategory>();

            // There are 3 fields for each beneficiary type in the array
            var items = beneficiaryCategories.Length / 3;

            for (int x = 0; x < items; x++)
            {
                beneficiaryCategoryList.Add(new BeneficiaryCategory(beneficiaryCategories[x, 0], beneficiaryCategories[x, 1], beneficiaryCategories[x, 2]));
            }
            return beneficiaryCategoryList;
        }
    }
}

