// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

using Ellucian.Web.Security;


namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories
{
    /// <summary>
    /// Define a GL user factory.
    /// </summary>
    public abstract class GeneralLedgerCurrentUser
    {
        public class UserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000001",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "Budget.Adjustor", "BLANKET.PURCHASE.ORDER.VIEWER", "JOURNAL.ENTRY.VIEWER", "PURCHASE.ORDER.VIEWER", "RECURRING.VOUCHER.VIEWER", "REQUISITION.VIEWER", "VOUCHER.VIEWER", "UPDATE.BLANKET.PURCHASE.ORDERS", "DELETE.REQUISITION", "VOUCHER.CREATER" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class TaxInformationUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "1",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "VIEW.W2", "VIEW.1095C", "VIEW.1098", "VIEW.T4", "VIEW.T4A", "VIEW.T2202A", "VIEW.EMPLOYEE.W2", "VIEW.EMPLOYEE.1095C", "VIEW.STUDENT.1098", "VIEW.EMPLOYEE.T4", "VIEW.RECIPIENT.T4A", "VIEW.STUDENT.T2202A" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class UserFactorySubset : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000001",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "VIEW.REQUISITIONS", "DELETE.REQUISITIONS", "UPDATE.REQUISITIONS", "VIEW.ACCOUNTING.STRINGS", "DELETE.REQUISITIONS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
              
        public class UserFactoryNone : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000002",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "DELETE.REQUISITION" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class UserFactoryNonExistant : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "9999999",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class UserFactoryAll : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "Budget.Adjustor" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class BudgetAdjustmentViewUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000099",
                        ControlId = "123",
                        Name = "John BA View",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "John can only view budget adjustment",
                        Roles = new List<string>() { "Budget.Adjustor.View" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class BudgetAdjustmentUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Johnny Budget Adjustor",
                        Roles = new List<string>() { "CREATE.UPDATE.BUDGET.ADJUSTMENT", "VIEW.BUDGET.ADJUSTMENT", "DELETE.BUDGET.ADJUSTMENT" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class BudgetAdjustmentApproverUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Johnny Budget Adjustment Approver",
                        Roles = new List<string>() { "VIEW.BUD.ADJ.PENDING.APPR" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class AccountFundsAvailableUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "VIEW.ACCOUNT.FUNDS.AVAILABLE", "VIEW.AP.INVOICES", "UPDATE.AP.INVOICES", "VIEW.FISCAL.PERIODS.INTG"},
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class BudgetUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "VIEW.BUDGET.PHASES", "VIEW.BUDGET.CODES", "VIEW.BUDGET.PHASE.LINE.ITEMS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
        public class PurchaseOrderUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentPO",
                        Roles = new List<string>() { "VIEW.PURCHASE.ORDERS", "UPDATE.PURCHASE.ORDERS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class GeneralLedgerUserAllAccounts : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000033",
                        ControlId = "123",
                        Name = "Andy",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "VIEW.ACCOUNT.FUNDS.AVAILABLE", "VIEW.AP.INVOICES", "UPDATE.AP.INVOICES", "VIEW.LEDGER.ACTIVITIES" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class GeneralLedgerTransactionsUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0004319",
                        ControlId = "123",
                        Name = "Andy",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "CREATE.GL.POSTINGS", "CREATE.JOURNAL.ENTRIES", "CREATE.BUDGET.ENTRIES", "CREATE.ENCUMBRANCE.ENTRIES" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class PaymentTransactionsUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0004319",
                        ControlId = "123",
                        Name = "Andy",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "PaymentTransactionUser",
                        Roles = new List<string>() { "VIEW.PAYMENT.TRANSACTIONS", "UPDATE.PAYMENT.TRANSACTIONS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class GrantsUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentPO",
                        Roles = new List<string>() { "VIEW.GRANTS"},
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class ProcurementReceiptsUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000004",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentPO",
                        Roles = new List<string>() { "VIEW.PROCUREMENT.RECEIPTS", "CREATE.PROCUREMENT.RECEIPT" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class FixedAssetsUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000028",
                        ControlId = "123",
                        Name = "Bernard",
                        SecurityToken = "954",
                        SessionTimeout = 30,
                        UserName = "FixAssetsUser",
                        Roles = new List<string>() { "VIEW.FIXED.ASSETS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class ReceiveProcurementUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000143",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "StudentPO",
                        Roles = new List<string>() { "UPDATE.RECEIVING"},
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class DocumentApprovalUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0003884",
                        ControlId = "123",
                        Name = "Gary",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "DocumentApprover",
                        Roles = new List<string>() { "View Document Approval" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class InitiatorProcurementUser : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        PersonId = "0000143",
                        ControlId = "123",
                        Name = "Johnny",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "ProcurementUser",
                        Roles = new List<string>() { "CREATE.UPDATE.DOC" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
