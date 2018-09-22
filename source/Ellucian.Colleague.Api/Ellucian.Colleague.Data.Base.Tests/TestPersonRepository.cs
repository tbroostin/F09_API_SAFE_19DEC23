// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public static class TestPersonRepository
    {
        private static Collection<Person> _persons = new Collection<Person>();
        public static Collection<Person> Persons
        {
            get
            {
                if (_persons.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _persons;
            }
        }

        private static void GenerateDataContracts()
        {
            var personsData = GetPersonsData();
            for (int i = 0; i < personsData.Length / 4; i++)
            {
                string id = personsData[i, 0].Trim();
                string lastName = personsData[i, 1].Trim();               
                string firstName = personsData[i, 2].Trim();
                string privacyFlag = personsData[i, 3].Trim();

                _persons.Add(new Person()
                {
                    Recordkey = id,
                    LastName = lastName,
                    FirstName = firstName,
                    PrivacyFlag = privacyFlag
                });
            }
        }

        private static string[,] GetPersonsData()
        {
            string[,] personsTable = {
                                                    {"0000016", "Ayres",     "John",    "" },
                                                    {"0000030", "Langdon",   "Diana",   "" },
                                                    {"0003316", "Reuitmann", "David",   "" },
                                                    {"0000894", "Student",   "Johnny",  "" },
                                                    {"0000895", "Student",   "Johnny",  "" },
                                                    {"1234567", "Person",    "Private", "S"},
                                                    {"0003315", "Jon",    "Doe", "S"}
                                                };
            return personsTable;
        }
    }
}
