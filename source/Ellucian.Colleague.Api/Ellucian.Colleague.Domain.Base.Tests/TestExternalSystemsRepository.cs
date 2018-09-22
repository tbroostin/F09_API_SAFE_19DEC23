using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Domain.Base.Tests
{
    public class TestExternalSystemsRepository
    {
        private static ApplValcodes _externalSystems;

        public static ApplValcodes ExternalSystems
        {
            get
            {
                if (_externalSystems == null)
                {
                    GenerateDataContracts();
                }

                return _externalSystems;
            }
        }

        static void GenerateDataContracts()
        {
            int rowSize;
            string[,] inputData = GetExternalSystems(out rowSize);

            _externalSystems = new ApplValcodes
            {
                Recordkey = "RES.LIFE.EXTL.ID.SOURCE",
                ValApplication = "ST",
                ValCodeLength = 4,
                ValNoMod = "N",
                ValPurpose = GetComments()
            };
            _externalSystems.ValsEntityAssociation = new List<ApplValcodesVals>();

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
                _externalSystems.ValsEntityAssociation.Add(entry);
            }
        }

        static string[,] GetExternalSystems(out int rowSize)
        {
            string[,] externalSystems =
            {  // Internal External representation      Min Code   Action 1      Action 2      Action 3      Action 4
                { "HD  ", "Housing Director         ", "HD  ", "          ", "          ", "          ", "          " },
                { "RMS ", "Residential Mgmt System  ", "RMS ", "          ", "          ", "          ", "          " },
                { "SR  ", "StarRez                  ", "SR  ", "          ", "          ", "          ", "          " }
            };

            rowSize = 7;
            return externalSystems;
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
                " ",
                "Along with the external ID, the record also stores a source code",
                "that identifies the external Residence Life system. Should a",
                "Colleague entity ever contain records from different external",
                "systems, the combination of the external ID and source code",
                "makes a unique identifier in the Colleague system."
            };

            return string.Join(DmiString.sVM, comments);
        }
    }
}
