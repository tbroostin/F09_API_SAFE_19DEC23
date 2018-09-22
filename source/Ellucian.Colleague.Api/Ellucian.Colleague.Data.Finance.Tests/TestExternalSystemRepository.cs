using System.Collections.Generic;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public class TestExternalSystemsRepository
    {
        private static ApplValcodes _ExternalSystems;

        public static ApplValcodes ExternalSystems
        {
            get
            {
                if (_ExternalSystems == null)
                {
                    GenerateDataContracts();
                }
                return _ExternalSystems;
            }
        }

        private static void GenerateDataContracts()
        {
            int rowSize;
            string[,] inputData = GetExternalSystems(out rowSize);

            _ExternalSystems = new ApplValcodes
            {
                Recordkey = "RES.LIFE.EXTL.ID.SOURCE",
                ValApplication = "ST",
                ValCodeLength = 10,
                ValNoMod = "N",
                ValPurpose = GetComments()
            };
            _ExternalSystems.ValsEntityAssociation = new List<ApplValcodesVals>();

            for (int i = 0; i < inputData.Length / rowSize; i++)
            {
                string internalCode = inputData[i, 0].Trim();
                string externalDesc = inputData[i, 1].Trim();
                string minimumCode = inputData[i, 2].Trim();
                string actionCode1 = inputData[i, 3].Trim();
                string actionCode2 = inputData[i, 4].Trim();
                string actionCode3 = inputData[i, 5].Trim();
                string actionCode4 = inputData[i, 6].Trim();

                var entry = new ApplValcodesVals(minimumCode, externalDesc, actionCode1, internalCode, actionCode2,
                    actionCode3, actionCode4);
                _ExternalSystems.ValsEntityAssociation.Add(entry);
            }
        }

        static string[,] GetExternalSystems(out int rowSize)
        {
            string[,] ExternalSystems =
            {  // Internal External representation      Min Code   Action 1      Action 2      Action 3      Action 4
                { "HD ", "Housing Director", "HD ", "          ", "          ", "          ", "          " },
                { "RMS", "RMS             ", "RMS", "          ", "          ", "          ", "          " },
                { "SR ", "StarRez         ", "SR ", "BK        ", "          ", "          ", "          " },
            };

            rowSize = 7;
            return ExternalSystems;
        }

        static string GetComments()
        {
            string[] comments =
            {
                "When Colleague is integrated with an external Residence Life",
                "system, some residence life records in Colleague may store a",
                "unique ID of the entity in an external system. For example, this",
                "may be the case with the ROOM.ASSIGNMENT and",
                "MEAL.PLAN.ASSIGNMENT entities.",
                "Along with the external ID, the record also stores a source code",
                "that identifies the external Residence Life system. Should a",
                "Colleague entity ever contain records from different external",
                "systems, the combination of the external ID and source code",
                "makes a unique identifier in the Colleague system.",
                "This table is client maintainable, but is pre-populated with",
                "values for some common external Residence Life systems at the",
                "time this validation table was delivered."
            };

            return string.Join(DmiString.sVM, comments);
        }
    }
}
