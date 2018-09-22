using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Transactions;

namespace Ellucian.Colleague.Data.Base.Tests
{
    public static class TestElfTranslateTablesRepository
    {
        private static Collection<ElfTranslateTables> _elfTranslateTables = new Collection<ElfTranslateTables>();
        public static Collection<ElfTranslateTables> ElfTranslateTables
        {
            get
            {
                if (_elfTranslateTables.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _elfTranslateTables;
            }
        }

        /// <summary>
        /// Performs data setup for international parameters to be used in tests
        /// </summary>
        private static void GenerateDataContracts()
        {
            string[,] _elfTranslateTablesData = GetElfTranslateTablesData();
            int elfTableCount = _elfTranslateTablesData.Length / 4;
            for (int i = 0; i < elfTableCount; i++)
            {
                // Parse out the data
                string id = _elfTranslateTablesData[i, 0].Trim();
                string description = _elfTranslateTablesData[i, 1].Trim();
                string origCode = _elfTranslateTablesData[i, 2].Trim();
                string newCode = _elfTranslateTablesData[i, 3].Trim();

                ElfTranslateTables table = new ElfTranslateTables()
                {
                    Recordkey = id,
                    ElftDesc = description,
                    ElftOrigCodeField = origCode,
                    ElftNewCodeField = newCode
                };
                table.ElftblEntityAssociation = new List<ElfTranslateTablesElftbl>();

                if (table.Recordkey == "LDM.SUBJ.DEPT")
                {
                    string[,] _elfMappingData = GetElfTranslateTablesMappingData();
                    int elfMappingCount = _elfMappingData.Length / 6;
                    for (int j = 0; j < elfMappingCount; j++)
                    {
                        string originalCode = _elfMappingData[j, 0].Trim();
                        string newMapCode = _elfMappingData[j, 1].Trim();
                        string actionCode1 = _elfMappingData[j, 2].Trim();
                        string actionCode2 = _elfMappingData[j, 3].Trim();
                        string actionCode3 = _elfMappingData[j, 4].Trim();
                        string actionCode4 = _elfMappingData[j, 5].Trim();
                        table.ElftblEntityAssociation.Add(new ElfTranslateTablesElftbl()
                        {
                            ElftOrigCodesAssocMember = originalCode,
                            ElftNewCodesAssocMember = newMapCode,
                            ElftActionCodes1AssocMember = actionCode1,
                            ElftActionCodes2AssocMember= actionCode2,
                            ElftActionCodes3AssocMember = actionCode3,
                            ElftActionCodes4AssocMember = actionCode4
                        });
                    }
                }
                _elfTranslateTables.Add(table);
            }
        }

        /// <summary>
        /// Gets ELF table raw data
        /// </summary>
        /// <returns>String array of ELF table data</returns>
        private static string[,] GetElfTranslateTablesData()
        {
            string[,] elfTranslateTablesData =   {   //ID                  Description                      Orig Code      New Code
                                                    {"LDM.SUBJ.DEPT",     "LDM/Colleague Subject Mapping", "CRS.SUBJECT", "CRS.DEPTS " },
                                                    {"LDM.COURSE.LEVELS", "LDM Course Levels            ", "           ", "CRS.LEVELS"}
                                             };
            return elfTranslateTablesData;
        }

        /// <summary>
        /// Gets ELF mapping raw data
        /// </summary>
        /// <returns>String array of ELF mapping data</returns>
        private static string[,] GetElfTranslateTablesMappingData()
        {
            string[,] elfTranslateTablesMappingData =  {
                                                       { "ACCT", "BUSN", " ", " ", " ", " " },
                                                       { "MATH", "MATH", " ", " ", " ", " "}
                                                   };
            return elfTranslateTablesMappingData;
        }
    }
}
