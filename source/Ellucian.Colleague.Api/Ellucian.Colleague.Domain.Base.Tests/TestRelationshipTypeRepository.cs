// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestRelationshipTypeRepository
    {
        private string[,] relationshipTypes = {
                                            //CODE  DESCRIPTION        INVERSE CODE
                                            {"AFF","Affiliated","AFF"},
                                            {"C", "Child","P"},
                                            {"CC","Clearinghouse Client","CH"},
                                            {"CH","Clearinghouse","CC"},
                                            {"CO","Companion","CO"},
                                            {"CON","Contractor","CON"},
                                            {"CSB","Chld/Prnt","SBP"},
                                            {"CT", "Cousin","CT"},
                                            {"CZ", "Contact","CZ"},
                                            {"E",  "Executor","E"},
                                            {"EMP","Employment","EMP"},
                                            {"EST","Estate","EST"},
                                            {"F",  "Friend","F"},
                                            {"FME","Former Employer (retired)",""},
                                            {"FMP","Former Employment","FMP"},
                                            {"FND","Foundation","FP"},
                                            {"FP", "Parent Organization","FND"},
                                            {"GC", "Grandchild","GP"},
                                            {"GGC","Great-Grand-Child","GGP"},
                                            {"GGP","Great-Grand-Parent","GGC"},
                                            {"GP", "Grandparent","GC"},
                                            {"GU", "Guardian","WA"},
                                            {"HS", "Half-Sibling","HS"},
                                            {"IC", "In-law/Chld","IP"},      
                                            {"IP", "In-laws","IC"},
                                            {"IS", "In-law/Sib","IS"},
                                            {"LS", "Late Spouse","W"},
                                            {"OCN","Organization Contact","OCN"},
                                            {"OS", "Other Kind of Spouse","OS"},
                                            {"OWB","Owned By","OWN"},
                                            {"OWN","Owner","OWB"},
                                            {"P",  "Parent","C"},
                                            {"PAR","Father",""},
                                            {"RET","Retired",""},
                                            {"S",  "Spouse","S"},
                                            {"SB", "Sibling","SB"},
                                            {"SBP","Sib/Prnt","CSB"},
                                            {"SC", "Stepchild","SP"},
                                            {"SP", "Step-Parent","SC"},
                                            {"STS","Step-Sibling","STS"},
                                            {"UN", "Unknown","UN"},
                                            {"W",  "Widow(er)","LS"},
                                            {"WA", "Ward","GU"},
                                            {"XS", "Ex Spouse","XS"},
                                      };

        public IEnumerable<RelationshipType> Get()
        {
            var RelationshipTypeList = new List<RelationshipType>();

            // There are 3 fields for each RelationshipType in the array
            var items = relationshipTypes.Length / 3;

            for (int x = 0; x < items; x++)
            {
                RelationshipTypeList.Add(new RelationshipType(relationshipTypes[x, 0], relationshipTypes[x, 1], relationshipTypes[x, 2]));
            }
            return RelationshipTypeList;
        }
    }
}