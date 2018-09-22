// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestSourceContextRepository
    {
        private readonly string[,] sourceContexts =
        {
            //GUID   CODE   DESCRIPTION                                          
            {"b3bf4128-259a-4623-8731-f519c0ae933c", "REMARKS", "Remarks Source Codes"},
            {"85f52e2a-a1d5-46d7-a921-165744e5f9e7", "TESTS", "Tests Source Codes"},
            {"5d2c5279-9ea2-4dfd-b817-54c23631e02d", "ADDRESSES", "Address Change Src Codes"},
            {"5d2c5279-9ea2-4dfd-b817-54c23631e02d", "APPLICATION.SOURCES", "Application Source Codes"}

        };

        public IEnumerable<SourceContext> GetSourceContexts()
        {
            var sourceContextList = new List<SourceContext>();

            // There are 3 fields for each SourceContext in the array
            var items = sourceContexts.Length/3;

            for (int x = 0; x < items; x++)
            {
                sourceContextList.Add(
                    new SourceContext(
                        sourceContexts[x, 0], sourceContexts[x, 1], sourceContexts[x, 2]
                        ));
            }
            return sourceContextList;
        }
    }
}