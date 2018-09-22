// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestInterestsRepository
    {
        private string[,] interests = {
                                            //GUID   CODE   DESCRIPTION
                                            {"6ae3a175-1dfd-4937-b97b-3c9ad596e024", "ART", "Art", "AR"}, 
                                            {"41d8aa32-dbe6-4a49-a1c4-2cad39e232e5", "BASE", "Baseball", "AT"},
                                            {"c769e6a9-da86-47a9-ab21-b17198880436", "TD", "Tri Delta", "F"}, 
                                            {"e297656e-8d50-4c63-a2dd-0fcfc46647c5", "YB", "Year Book", "J"}, 
                                            {"7d0e291e-7246-4067-aff1-47ff6adc0391", "BASS", "Bass", "M"}, 
                                            {"c91bbee8-88d1-4063-86e2-e7cb1865b45b", "GOV", "Goverment", "ST"}, 
                                            {"9eaca2e7-fb59-44b6-be64-ce9e2ad73e82", "AT", "Astronomy Club", "SC"}
                                            };

        public IEnumerable<Interest> GetInterests()
        {
            var interestList = new List<Interest>();

            // There are 4 fields for each interest in the array
            var items = interests.Length / 4;

            for (int x = 0; x < items; x++)
            {
                interestList.Add(
                    new Interest(
                        interests[x, 0], interests[x, 1], interests[x, 2], interests[x, 3]
                     ));
            }
            return interestList;
        }
    }
}