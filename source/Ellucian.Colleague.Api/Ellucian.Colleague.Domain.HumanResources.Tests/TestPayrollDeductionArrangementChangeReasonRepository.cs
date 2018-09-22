// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests
{
    public class TestPayrollDeductionArrangementChangeReasonRepository
    {
        private string[,] payrollDeductionArrangementChangeReasons = {
                                            //GUID   CODE   DESCRIPTION
                                            {"625c69ff-280b-4ed3-9474-662a43616a8a", "MAR", "Marriage"}, 
                                            {"bfea651b-8e27-4fcd-abe3-04573443c04c", "BOC", "Birth of Child"},
                                            {"9ae3a175-1dfd-4937-b97b-3c9ad596e023", "SJC", "Spouse Job Change"},
                                            {"e9e6837f-2c51-431b-9069-4ac4c0da3041", "DIV", "Divorce"},
                                            {"80779c4f-b2ac-4ad4-a970-ca5699d9891f", "ADP", "Adoption"},
                                            {"ae21110e-991e-405e-9d8b-47eeff210a2d", "DEA", "Death"}
                                      };

        public IEnumerable<PayrollDeductionArrangementChangeReason> GetPayrollDeductionArrangementChangeReasons()
        {
            var payrollDeductionArrangementChangeReasonList = new List<PayrollDeductionArrangementChangeReason>();

            // There are 3 fields for each employment classification in the array
            var items = payrollDeductionArrangementChangeReasons.Length / 3;

            for (int x = 0; x < items; x++)
            {
                payrollDeductionArrangementChangeReasonList.Add(new PayrollDeductionArrangementChangeReason(payrollDeductionArrangementChangeReasons[x, 0], payrollDeductionArrangementChangeReasons[x, 1], payrollDeductionArrangementChangeReasons[x, 2]));
            }
            return payrollDeductionArrangementChangeReasonList;
        }
    }
}
