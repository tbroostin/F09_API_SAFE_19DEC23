using System.Collections.Generic;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public class TestInvoiceTypesRepository
    {
        private static ApplValcodes _invoiceTypes;

        public static ApplValcodes InvoiceTypes
        {
            get
            {
                if (_invoiceTypes == null)
                {
                    GenerateDataContracts();
                }
                return _invoiceTypes;
            }
        }

        private static void GenerateDataContracts()
        {
            int rowSize;
            string[,] inputData = GetInvoiceTypes(out rowSize);

            _invoiceTypes = new ApplValcodes
            {
                Recordkey = "INVOICE.TYPES",
                ValApplication = "ST",
                ValCodeLength = 5,
                ValNoMod = "N",
                ValPurpose = GetComments()
            };
            _invoiceTypes.ValsEntityAssociation = new List<ApplValcodesVals>();

            for (int i = 0; i < inputData.Length/rowSize; i++)
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
                _invoiceTypes.ValsEntityAssociation.Add(entry);
            }
        }

        static string[,] GetInvoiceTypes(out int rowSize)
        {
            string[,] invoiceTypes =
            {  // Internal External representation      Min Code   Action 1      Action 2      Action 3      Action 4
                { "PROMO", "Promotional item sale    ", "PROMO", "          ", "          ", "          ", "          " },
                { "SGVT ", "Student govt mtg expense ", "SGVT ", "          ", "          ", "          ", "          " },
                { "BK   ", "Bookstore                ", "B    ", "BK        ", "          ", "          ", "          " },
                { "EXTRL", "External Res. Life System", "EXTRL", "          ", "          ", "          ", "          " }
            };

            rowSize = 7;
            return invoiceTypes;
        }

        static string GetComments()
        {
            string[] comments =
            {
                "These are types that can be assigned to recurring and",
                "regular invoices.  They would be used if an invoice",
                "was to be categorized by some other mechanism than the",
                "AR code.  These types are not used by the system."
            };

            return string.Join(DmiString.sVM, comments);
        }
    }
}
