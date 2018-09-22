// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Finance
{
    [Serializable]
    public static class FinancePermissionCodes
    {
        /// <summary>
        /// Permission to view account activity of anyone.
        /// </summary>
        public const string ViewStudentAccountActivity = "VIEW.STUDENT.ACCOUNT.ACTIVITY";
        /// <summary>
        /// Permission to create an Accounts Receivable invoice
        /// </summary>
        public const string CreateArInvoices = "CREATE.AR.INVOICES";
        /// <summary>
        /// Permission to create a receipt
        /// </summary>
        public const string CreateReceipts = "CREATE.RECEIPTS";
    }
}
