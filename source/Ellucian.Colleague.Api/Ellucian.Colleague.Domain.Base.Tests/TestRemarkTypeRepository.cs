// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestRemarkTypeRepository
    {
        private string[,] remarkTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"a830e686-7692-4012-8da5-b1b5d44389b4", "BU", "Business Info"}, 
                                            {"eddde9d0-b81d-4e59-850c-b439221c1e81", "PE", "Personal"},
                                            {"796e2e5b-e45b-4828-b2dc-d98964066b5b", "IN", "Involvement Info"}, 
                                            {"0f0bc578-c3d7-4764-b338-86026b034846", "FN", "Funding Info"}
                                        };

        public IEnumerable<RemarkType> GetRemarkType()
        {
            var remarkTypeList = new List<RemarkType>();

            // There are 3 fields for each interest type in the array
            var items = remarkTypes.Length / 3;

            for (int x = 0; x < items; x++)
            {
                remarkTypeList.Add(
                    new RemarkType(
                        remarkTypes[x, 0], remarkTypes[x, 1], remarkTypes[x, 2]
                     ));
            }
            return remarkTypeList;
        }
    }
}