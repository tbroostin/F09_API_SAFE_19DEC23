// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestAddressChangeSourceRepository
    {
        private string[,] addressChangeSources = {
                                            //GUID   CODE   DESCRIPTION
                                            {"b831e686-7692-4012-8da5-b1b5d44389b4", "AP", "Application"}, 
                                            {"fdd3e9d0-b81d-4e59-850c-b439221c1e81", "CO", "Correction"},
                                            {"896f2e5b-e45b-4828-b2dc-d98964066b5b", "FM", "Change of Address Form"}, 
                                            {"1f0cc578-c3d7-4764-b338-86026b034846", "IP", "In Person"}
                                            };

        public IEnumerable<AddressChangeSource> GetAddressChangeSource()
        {
            var addressChangeSourceList = new List<AddressChangeSource>();

            var items = addressChangeSources.Length / 3;

            for (int x = 0; x < items; x++)
            {
                addressChangeSourceList.Add(
                    new AddressChangeSource(
                        addressChangeSources[x, 0], addressChangeSources[x, 1], addressChangeSources[x, 2]
                     ));
            }
            return addressChangeSourceList;
        }
    }
}