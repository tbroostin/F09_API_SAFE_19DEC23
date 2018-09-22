// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestRemarkCodeRepository
    {
        private string[,] remarkCodes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"m330e686-7692-4012-8da5-b1b5d44389b4", "FA", "Family"}, 
                                            {"trdde9d0-b81d-4e59-850c-b439221c1e81", "FF", "Friend od Family"},
                                            {"d96e2e5b-e45b-4828-b2dc-d98964066b5b", "FR", "Financial Reports"}, 
                                            {"f00bc578-c3d7-4764-b338-86026b034846", "ST", "Staff"}
                                            };

        public IEnumerable<RemarkCode> GetRemarkCode()
        {
            var remarkCodeList = new List<RemarkCode>();

            var items = remarkCodes.Length / 3;

            for (int x = 0; x < items; x++)
            {
                remarkCodeList.Add(
                    new RemarkCode(
                        remarkCodes[x, 0], remarkCodes[x, 1], remarkCodes[x, 2]
                     ));
            }
            return remarkCodeList;
        }
    }
}