// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestPersonNameTypeRepository
    {
        private string[,] personNameTypes = {
                                            //GUID   CODE   DESCRIPTION
                                            {"69d3987d-a1da-4c32-a7ce-edb9b6c9c8b5", "BIRTH", "Person's Birth Name", "birth"}, 
                                            {"ce7e23e2-5c56-49f8-a851-5275f64c5e51", "LEGAL", "Person's Legal Name", "legal"},
                                            {"9b00ef0a-312c-49ad-a6a4-c91131747a23", "PREFERRED","Person's Preferred Name", "personal"},
                                            {"def010fd-1db0-4ce1-a23b-557693b318a4", "NICKNAME","Person's Nickname", "personal"},
                                            {"c6f19434-98c0-4760-a82b-d3c02d524e28", "HISTORY","Person's Previous Name", "personal"},
                                            
                                       };

        public IEnumerable<PersonNameTypeItem> Get()
        {
            var personNameTypeList = new List<PersonNameTypeItem>();

            // There are 4 fields for each person name type in the array
            var items = personNameTypes.Length / 4;

            for (int x = 0; x < items; x++)
            {
                switch (personNameTypes[x, 3])
                {
                    case "birth":
                        personNameTypeList.Add(new PersonNameTypeItem(personNameTypes[x, 0], personNameTypes[x, 1], personNameTypes[x, 2], PersonNameType.Birth)); break;
                    case "legal":
                        personNameTypeList.Add(new PersonNameTypeItem(personNameTypes[x, 0], personNameTypes[x, 1], personNameTypes[x, 2], PersonNameType.Legal)); break;
                    default:
                        personNameTypeList.Add(new PersonNameTypeItem(personNameTypes[x, 0], personNameTypes[x, 1], personNameTypes[x, 2], PersonNameType.Personal)); break;
                }
            }
            return personNameTypeList;
        }
    }
}