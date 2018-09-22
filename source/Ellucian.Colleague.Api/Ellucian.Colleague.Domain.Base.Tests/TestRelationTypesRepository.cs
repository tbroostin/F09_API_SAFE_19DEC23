// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestRelationTypesRepository
    {
        private string[,] relationTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "C", "Child", "N"}, 
                                            {"31d8aa32-dbe6-4a49-a1c4-2cad39e232e4", "P", "Parent", "N"},
                                            {"76d8aa11-dbe6-4a49-a1c4-5gad39e232e9", "A", "Affiliated", "Y"},
                                      };


        public IEnumerable<RelationType> GetRelationTypes()
        {
            var relationTypesList = new List<RelationType>();

            // There are 4 fields for each type in the array
            var items = relationTypes.Length / 4;

            for (int x = 0; x < items; x++)
            {
                relationTypesList.Add(new RelationType(relationTypes[x, 0], relationTypes[x, 1], relationTypes[x, 2], 
                    relationTypes[x, 3], PersonalRelationshipType.Parent, PersonalRelationshipType.Father, 
                    PersonalRelationshipType.Mother, "P"));
            }
            return relationTypesList;
        }

    }
}