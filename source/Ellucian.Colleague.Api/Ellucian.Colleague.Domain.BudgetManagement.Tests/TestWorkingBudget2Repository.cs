// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.BudgetManagement.DataContracts;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.BudgetManagement.Tests
{
    public class TestWorkingBudget2Repository : IBudgetDevelopmentRepository
    {
        public TestGeneralLedgerAccountRepository testGlAcccountRepository;
        public GeneralLedgerAccountStructure glAccountStructure;

        WorkingBudget2 workingBudget2 = new WorkingBudget2();

        BudgetLineItem budgetLineItem51111 = new BudgetLineItem("11_00_01_00_33333_51111")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-51111",
            BaseBudgetAmount = 1111,
            WorkingAmount = 51111,
            BudgetComparables = new List<BudgetComparable>()
                        {
                            new BudgetComparable("C1")
                            {
                                ComparableAmount = 111,
                            },
                            new BudgetComparable("C2")
                            {
                                ComparableAmount = 112,
                            },
                            new BudgetComparable("C3")
                            {
                                ComparableAmount = 113,
                            },
                            new BudgetComparable("C4")
                            {
                                ComparableAmount = 114,
                            },
                            new BudgetComparable("C5")
                            {
                                ComparableAmount = 115,
                            }
                        },
            BudgetOfficer = new BudgetOfficer("1111")
            {
                BudgetOfficerLogin = "LOGINFOR0000001",
                BudgetOfficerName = "Name for 0000001"
            }
        };
        BudgetLineItem budgetLineItem52222 = new BudgetLineItem("11_00_01_00_33333_52222")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-52222",
            BudgetAccountDescription = "Department 33333 : Object 52222",
            BaseBudgetAmount = 0,
            WorkingAmount = 52222,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 211,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 212,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 213,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 214,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 215,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("1111")
            {
                BudgetOfficerLogin = "LOGINFOR0000001",
                BudgetOfficerName = "Name for 0000001"
            }
        };
        BudgetLineItem budgetLineItem53333 = new BudgetLineItem("11_00_01_00_33333_53333")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-53333",
            BudgetAccountDescription = "Department 33333 : Object 53333",
            BaseBudgetAmount = 1111,
            WorkingAmount = 53333,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 311,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 312,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 313,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 314,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 315,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("1111")
            {
                BudgetOfficerLogin = "LOGINFOR0000001",
                BudgetOfficerName = "Name for 0000001"
            }
        };
        BudgetLineItem budgetLineItem54005 = new BudgetLineItem("11_00_01_00_33333_54005")
        {
            FormattedBudgetAccountId = "11-00-11_00_01_00_33333_54005",
            BudgetAccountDescription = "Department 33333 : Object 54005",
            BaseBudgetAmount = 2100,
            WorkingAmount = 54005,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 411,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 412,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 413,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 414,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 415,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("2100")
            {
                BudgetOfficerLogin = "LOGINFOR0000002",
                BudgetOfficerName = "Name for 0000002"
            }
        };
        BudgetLineItem budgetLineItem54006 = new BudgetLineItem("11_00_01_00_33333_54006")
        {
            FormattedBudgetAccountId = "11-00-11_00_01_00_33333_54006",
            BudgetAccountDescription = "Department 33333 : Object 54006",
            BaseBudgetAmount = 2100,
            WorkingAmount = 54006,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 511,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 512,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 513,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 514,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 515,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("2100")
            {
                BudgetOfficerLogin = "LOGINFOR0000002",
                BudgetOfficerName = "Name for 0000002"
            }
        };
        BudgetLineItem budgetLineItem54011 = new BudgetLineItem("11_00_01_00_33333_54011")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-54011",
            BudgetAccountDescription = "Department 33333 : Object 54011",
            BaseBudgetAmount = 2100,
            WorkingAmount = 54011,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 611,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 612,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 313,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 614,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 615,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("2100")
            {
                BudgetOfficerLogin = "LOGINFOR0000002",
                BudgetOfficerName = "Name for 0000002"
            }
        };
        BudgetLineItem budgetLineItem44030 = new BudgetLineItem("11_00_01_00_33333_44030")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-44030",
            BudgetAccountDescription = "Department 33333 : Object 44030",
            BaseBudgetAmount = -2100,
            WorkingAmount = -44030,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = -711,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = -712,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = -713,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = -714,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = -715,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("2100")
            {
                BudgetOfficerLogin = "LOGINFOR0000002",
                BudgetOfficerName = "Name for 0000002"
            }
        };
        BudgetLineItem budgetLineItem54400 = new BudgetLineItem("11_00_01_00_33333_54400")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-54400",
            BudgetAccountDescription = "Department 33333 : Object 54400",
            BaseBudgetAmount = 2100,
            WorkingAmount = 54400,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 811,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 812,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 813,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 814,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 815,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("2100")
            {
                BudgetOfficerLogin = "LOGINFOR0000002",
                BudgetOfficerName = "Name for 0000002"
            }
        };
        BudgetLineItem budgetLineItem54A95 = new BudgetLineItem("11_00_01_00_33333_54A95")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-54A95",
            BudgetAccountDescription = "Department 33333 : Object 54A95",
            BaseBudgetAmount = 1111,
            WorkingAmount = 54995,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 911,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 912,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 913,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 914,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 915,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("3100")
            {
                BudgetOfficerLogin = "LOGINFOR0003100",
                BudgetOfficerName = "Name for 0003100"
            }
        };
        BudgetLineItem budgetLineItem54N70 = new BudgetLineItem("11_00_01_00_33333_54N70")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-54N70",
            BudgetAccountDescription = "Department 33333 : Object 54N70",
            BaseBudgetAmount = 1111,
            WorkingAmount = 54770,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 1011,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 1012,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 1013,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 4014,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 1015,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("3100")
            {
                BudgetOfficerLogin = "LOGINFOR0000001",
                BudgetOfficerName = "Name for 0000001"
            }
        };
        BudgetLineItem budgetLineItem54X35 = new BudgetLineItem("11_00_01_00_33333_54X35")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-54X35",
            BudgetAccountDescription = "Department 33333 : Object 54X35",
            BaseBudgetAmount = 3200,
            WorkingAmount = 54335,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 1021,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 1022,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 1023,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 1024,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 1025,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("3200")
            {
                BudgetOfficerLogin = "LOGINFOR0003200",
                BudgetOfficerName = "Name for 0003200"
            }
        };
        BudgetLineItem budgetLineItem55200 = new BudgetLineItem("11_00_01_00_33333_55200")
        {
            FormattedBudgetAccountId = "11-00-01-00-33333-55200",
            BudgetAccountDescription = "Department 33333 : Object 55200",
            BaseBudgetAmount = 3200,
            WorkingAmount = 55200,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 1031,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 1032,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 1033,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 0,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 0,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("3200")
            {
                BudgetOfficerLogin = "LOGINFOR0003200",
                BudgetOfficerName = "Name for 0003200"
            }
        };
        BudgetLineItem budgetLineItem454005 = new BudgetLineItem("11_00_01_00_44444_54005")
        {
            FormattedBudgetAccountId = "11-00-01-00-44444-54005",
            BudgetAccountDescription = "Department 44444 : Object 54005",
            BaseBudgetAmount = 3300,
            WorkingAmount = 54005,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 1041,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 1042,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 1043,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 1044,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 1045,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("3300")
            {
                BudgetOfficerLogin = "LOGINFOR0003300",
                BudgetOfficerName = "Name for 0003300"
            }
        };
        BudgetLineItem budgetLineItem454006 = new BudgetLineItem("11_00_01_00_44444_54006")
        {
            FormattedBudgetAccountId = "11-00-01-00-44444-54006",
            BudgetAccountDescription = "Department 44444 : Object 54006",
            BaseBudgetAmount = 3300,
            WorkingAmount = 54006,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 1051,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 1052,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 1053,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 1054,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 1055,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("3300")
            {
                BudgetOfficerLogin = "LOGINFOR0003300",
                BudgetOfficerName = "Name for 0003300"
            }
        };
        BudgetLineItem budgetLineItem454011 = new BudgetLineItem("11_00_01_00_44444_54011")
        {
            FormattedBudgetAccountId = "11-00-01-00-44444-54011",
            BudgetAccountDescription = "Department 44444 : Object 54011",
            BaseBudgetAmount = 3400,
            WorkingAmount = 54011,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 0,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 0,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 0,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 0,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 0,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("3300")
            {
                BudgetOfficerLogin = "LOGINFOR0003300",
                BudgetOfficerName = "Name for 0003300"
            }
        };
        BudgetLineItem budgetLineItem444030 = new BudgetLineItem("11_00_01_00_44444_44030")
        {
            FormattedBudgetAccountId = "11-00-01-00-44444-54011",
            BudgetAccountDescription = "Department 44444 : Object 44030",
            BaseBudgetAmount = -3400,
            WorkingAmount = -44030,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = -1071,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 0,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = -1073,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = -1074,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 0,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("3400")
            {
                BudgetOfficerLogin = "LOGINFOR0003400",
                BudgetOfficerName = "Name for 0003400"
            }
        };
        BudgetLineItem budgetLineItem454400 = new BudgetLineItem("11_00_01_00_44444_54400")
        {
            FormattedBudgetAccountId = "11-00-01-00-44444-54400",
            BudgetAccountDescription = "Department 44444 : Object 54400",
            BaseBudgetAmount = 3400,
            WorkingAmount = 454400,
            BudgetComparables = new List<BudgetComparable>()
                            {
                                new BudgetComparable("C1")
                                {
                                    ComparableAmount = 1082,
                                },
                                new BudgetComparable("C2")
                                {
                                    ComparableAmount = 0,
                                },
                                new BudgetComparable("C3")
                                {
                                    ComparableAmount = 0,
                                },
                                new BudgetComparable("C4")
                                {
                                    ComparableAmount = 1085,
                                },
                                new BudgetComparable("C5")
                                {
                                    ComparableAmount = 0,
                                }
                            },
            BudgetOfficer = new BudgetOfficer("3400")
            {
                BudgetOfficerLogin = "LOGINFOR0003400",
                BudgetOfficerName = "Name for 0003400"
            }
        };

        SubtotalLineItem subtotalLine4 = new SubtotalLineItem()
        {
            SubtotalType = "BO",
            SubtotalName = "Budget Officer",
            SubtotalOrder = 3,
            SubtotalValue = "1111",
            SubtotalDescription = "Opers name for login 1",
            SubtotalWorkingAmount = 156666,
            SubtotalBaseBudgetAmount = 2222,
            SubtotalBudgetComparables = new List<BudgetComparable>()
            {
                new BudgetComparable("C1")
                {
                    ComparableAmount = 633
                },
                new BudgetComparable("C2")
                {
                    ComparableAmount = 636
                },
                 new BudgetComparable("C3")
                {
                    ComparableAmount = 639
                },
                new BudgetComparable("C4")
                {
                    ComparableAmount = 642
                },
                 new BudgetComparable("C5")
                {
                    ComparableAmount = 645
                },
            }
        };
        SubtotalLineItem subtotalLine10 = new SubtotalLineItem()
        {
            SubtotalType = "BO",
            SubtotalName = "Budget Officer",
            SubtotalOrder = 3,
            SubtotalValue = "2100",
            SubtotalDescription = "Opers name for login 2",
            SubtotalWorkingAmount = 172392,
            SubtotalBaseBudgetAmount = 10500,
            SubtotalBudgetComparables = new List<BudgetComparable>()
            {
                new BudgetComparable("C1")
                {
                    ComparableAmount = 633
                },
                new BudgetComparable("C2")
                {
                    ComparableAmount = 636
                },
                 new BudgetComparable("C3")
                {
                    ComparableAmount = 639
                },
                new BudgetComparable("C4")
                {
                    ComparableAmount = 642
                },
                 new BudgetComparable("C5")
                {
                    ComparableAmount = 645
                },
            }
        };
        SubtotalLineItem subtotalLine13 = new SubtotalLineItem()
        {
            SubtotalType = "BO",
            SubtotalName = "Budget Officer",
            SubtotalOrder = 3,
            SubtotalValue = "3100",
            SubtotalDescription = "Opers name for login 2",
            SubtotalWorkingAmount = 109765,
            SubtotalBaseBudgetAmount = 2222,
            SubtotalBudgetComparables = new List<BudgetComparable>()
            {
                new BudgetComparable("C1")
                {
                    ComparableAmount = 633
                },
                new BudgetComparable("C2")
                {
                    ComparableAmount = 636
                },
                 new BudgetComparable("C3")
                {
                    ComparableAmount = 639
                },
                new BudgetComparable("C4")
                {
                    ComparableAmount = 642
                },
                 new BudgetComparable("C5")
                {
                    ComparableAmount = 645
                },
            }
        };
        SubtotalLineItem subtotalLine16 = new SubtotalLineItem()
        {
            SubtotalType = "BO",
            SubtotalName = "Budget Officer",
            SubtotalOrder = 3,
            SubtotalValue = "3200",
            SubtotalDescription = "Opers name for login 2",
            SubtotalWorkingAmount = 109535,
            SubtotalBaseBudgetAmount = 6400,
            SubtotalBudgetComparables = new List<BudgetComparable>()
            {
                new BudgetComparable("C1")
                {
                    ComparableAmount = 633
                },
                new BudgetComparable("C2")
                {
                    ComparableAmount = 636
                },
                 new BudgetComparable("C3")
                {
                    ComparableAmount = 639
                },
                new BudgetComparable("C4")
                {
                    ComparableAmount = 642
                },
                 new BudgetComparable("C5")
                {
                    ComparableAmount = 645
                },
            }
        };
        SubtotalLineItem subtotalLine17 = new SubtotalLineItem()
        {
            SubtotalType = "GL",
            SubtotalName = "UNIT",
            SubtotalOrder = 2,
            SubtotalValue = "33333",
            SubtotalDescription = null,
            SubtotalWorkingAmount = 548358,
            SubtotalBaseBudgetAmount = 21344,
            SubtotalBudgetComparables = new List<BudgetComparable>()
            {
                new BudgetComparable("C1")
                {
                    ComparableAmount = 633
                },
                new BudgetComparable("C2")
                {
                    ComparableAmount = 636
                },
                 new BudgetComparable("C3")
                {
                    ComparableAmount = 639
                },
                new BudgetComparable("C4")
                {
                    ComparableAmount = 642
                },
                 new BudgetComparable("C5")
                {
                    ComparableAmount = 645
                },
            }
        };
        SubtotalLineItem subtotalLine20 = new SubtotalLineItem()
        {
            SubtotalType = "BO",
            SubtotalName = "Budget Officer",
            SubtotalOrder = 3,
            SubtotalValue = "3300",
            SubtotalDescription = "Opers name for login 2",
            SubtotalWorkingAmount = 108011,
            SubtotalBaseBudgetAmount = 6600,
            SubtotalBudgetComparables = new List<BudgetComparable>()
            {
                new BudgetComparable("C1")
                {
                    ComparableAmount = 633
                },
                new BudgetComparable("C2")
                {
                    ComparableAmount = 636
                },
                 new BudgetComparable("C3")
                {
                    ComparableAmount = 639
                },
                new BudgetComparable("C4")
                {
                    ComparableAmount = 642
                },
                 new BudgetComparable("C5")
                {
                    ComparableAmount = 645
                },
            }
        };
        SubtotalLineItem subtotalLine24 = new SubtotalLineItem()
        {
            SubtotalType = "BO",
            SubtotalName = "Budget Officer",
            SubtotalOrder = 3,
            SubtotalValue = "3400",
            SubtotalDescription = "Opers name for login 2",
            SubtotalWorkingAmount = 464381,
            SubtotalBaseBudgetAmount = 3400,
            SubtotalBudgetComparables = new List<BudgetComparable>()
            {
                new BudgetComparable("C1")
                {
                    ComparableAmount = 633
                },
                new BudgetComparable("C2")
                {
                    ComparableAmount = 636
                },
                 new BudgetComparable("C3")
                {
                    ComparableAmount = 639
                },
                new BudgetComparable("C4")
                {
                    ComparableAmount = 642
                },
                 new BudgetComparable("C5")
                {
                    ComparableAmount = 645
                },
            }
        };
        SubtotalLineItem subtotalLine25 = new SubtotalLineItem()
        {
            SubtotalType = "GL",
            SubtotalName = "UNIT",
            SubtotalOrder = 2,
            SubtotalValue = "44444",
            SubtotalDescription = null,
            SubtotalWorkingAmount = 572392,
            SubtotalBaseBudgetAmount = 16800,
            SubtotalBudgetComparables = new List<BudgetComparable>()
            {
                new BudgetComparable("C1")
                {
                    ComparableAmount = 633
                },
                new BudgetComparable("C2")
                {
                    ComparableAmount = 636
                },
                 new BudgetComparable("C3")
                {
                    ComparableAmount = 639
                },
                new BudgetComparable("C4")
                {
                    ComparableAmount = 642
                },
                 new BudgetComparable("C5")
                {
                    ComparableAmount = 645
                },
            }
        };
        SubtotalLineItem subtotalLine26 = new SubtotalLineItem()
        {
            SubtotalType = "GL",
            SubtotalName = "FUND",
            SubtotalOrder = 1,
            SubtotalValue = "11",
            SubtotalDescription = null,
            SubtotalWorkingAmount = 1120750,
            SubtotalBaseBudgetAmount = 38144,
            SubtotalBudgetComparables = new List<BudgetComparable>()
            {
                new BudgetComparable("C1")
                {
                    ComparableAmount = 633
                },
                new BudgetComparable("C2")
                {
                    ComparableAmount = 636
                },
                 new BudgetComparable("C3")
                {
                    ComparableAmount = 639
                },
                new BudgetComparable("C4")
                {
                    ComparableAmount = 642
                },
                 new BudgetComparable("C5")
                {
                    ComparableAmount = 645
                },
            }
        };


        // -------------------------------
        //           DATA CONTRACTS 
        // -------------------------------


        // Create the parameter and budget records.
        public BudgetDevDefaults BudgetDevDefaultsContract = new BudgetDevDefaults()
        {
            Recordkey = "BUDGET.DEV.DEFAULTS",
            BudDevBudget = "FY2021"
        };

        public Data.BudgetManagement.DataContracts.Budget BudgetContract = new Data.BudgetManagement.DataContracts.Budget()
        {
            Recordkey = "FY2021",
            BuTitle = "Working Budget",
            BuStatus = "W",
            BuBaseYear = "2021",
            BuFinalDate = DateTime.Now.AddDays(300),
            BuComp1Year = 2015,
            BuComp1Heading = "2015 Actual",
            BuComp2Year = 2016,
            BuComp2Heading = "2016 Org Bu",
            BuComp3Year = 2017,
            BuComp3Heading = "2017 Adj Bu",
            BuComp4Year = 2018,
            BuComp4Heading = "2018 Enc Ac",
            BuComp5Year = 2019,
            BuComp5Heading = "2019 All Bu"
        };

        public Collection<BudResp> budRespRecords = new Collection<BudResp>()
                {
                    new BudResp()
                    {
                        Recordkey = "OB",
                        BrDesc = "OB Description"
                    },
                    new BudResp()
                    {
                        Recordkey = "OB_ENROL",
                        BrDesc = "OB_ENROL Description"
                    },
                    new BudResp()
                    {
                        Recordkey = "OB_ENROL_FA",
                        BrDesc = "OB_ENROL_FA Description"
                    },
                    new BudResp()
                    {
                        Recordkey = "OB_ENROL_REV",
                        BrDesc = "OB_ENROL_REV Description"
                    },
                    new BudResp()
                    {
                        Recordkey = "OB_FIN",
                        BrDesc = "OB_FIN Description"
                    },
                    new BudResp()
                    {
                        Recordkey = "OB_FIN_ACCT",
                        BrDesc = "OB_FIN_ACCT Description"
                    },
                    new BudResp()
                                {
                        Recordkey = "OB_FIN_IT",
                        BrDesc = "OB_FIN_IT Description"
                    },
                };

        public Collection<BudWork> budWorkRecords = new Collection<BudWork>()
                {
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_51111",
                        BwTitle = "Department 33333 : Object 51111",
                        BwExpenseAcct = "11_00_01_00_33333_51111",
                        BwLineAmt = 51111,
                        BwComp1Amount = 111,
                        BwComp2Amount = 112,
                        BwComp3Amount = 113,
                        BwComp4Amount = 114,
                        BwComp5Amount = 115,
                        BwControlLink = "OB",
                        BwOfcrLink = "1111",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 1111
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_52222",
                        BwTitle = "Department 33333 : Object 52222",
                        BwExpenseAcct = "11_00_01_00_33333_52222",
                        BwLineAmt = 52222,
                        BwComp1Amount = 211,
                        BwComp2Amount = 212,
                        BwComp3Amount = 213,
                        BwComp4Amount = 214,
                        BwComp5Amount = 215,
                        BwControlLink = "OB",
                        BwOfcrLink = "1111",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = null
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_53333",
                        BwTitle = "Department 33333 : Object 53333",
                        BwExpenseAcct = "11_00_01_00_33333_53333",
                        BwLineAmt = 53333,
                        BwComp1Amount = 311,
                        BwComp2Amount = 312,
                        BwComp3Amount = 313,
                        BwComp4Amount = 314,
                        BwComp5Amount = 315,
                        BwControlLink = "OB",
                        BwOfcrLink = "1111",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 1111
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_54005",
                        BwTitle = "Department 33333 : Object 54005",
                        BwExpenseAcct = "11_00_01_00_33333_54005",
                        BwLineAmt = 54005,
                        BwComp1Amount = 411,
                        BwComp2Amount = 412,
                        BwComp3Amount = 413,
                        BwComp4Amount = 414,
                        BwComp5Amount = 415,
                        BwControlLink = "OB_ENROL",
                        BwOfcrLink = "2100",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 2100
                            }
                        },
                        BwNotes = new List<string>() {"First line of the justification notes", "second line of the justification notes.", "", "This is the fourth line because the third one is empty." }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_54006",
                        BwTitle = "Department 33333 : Object 54006",
                        BwExpenseAcct = "11_00_01_00_33333_54006",
                        BwLineAmt = 54006,
                        BwComp1Amount = 511,
                        BwComp2Amount = 512,
                        BwComp3Amount = 513,
                        BwComp4Amount = 514,
                        BwComp5Amount = 515,
                        BwControlLink = "OB_ENROL",
                        BwOfcrLink = "2100",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 2100
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_54011",
                        BwTitle = "Department 33333 : Object 54011",
                        BwExpenseAcct = "11_00_01_00_33333_54011",
                        BwLineAmt = 54011,
                        BwComp1Amount = 611,
                        BwComp2Amount = 612,
                        BwComp3Amount = 613,
                        BwComp4Amount = 614,
                        BwComp5Amount = 615,
                        BwControlLink = "OB_ENROL",
                        BwOfcrLink = "2100",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 2100
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_44030",
                        BwTitle = "Department 33333 : Object 44030",
                        BwExpenseAcct = "11_00_01_00_33333_44030",
                        BwLineAmt = -44030,
                        BwComp1Amount = -711,
                        BwComp2Amount = -712,
                        BwComp3Amount = -713,
                        BwComp4Amount = -714,
                        BwComp5Amount = -715,
                        BwControlLink = "OB_ENROL",
                        BwOfcrLink = "2100",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = -2100
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_54400",
                        BwTitle = "Department 33333 : Object 54400",
                        BwExpenseAcct = "11_00_01_00_33333_54400",
                        BwLineAmt = 54400,
                        BwComp1Amount = 811,
                        BwComp2Amount = 812,
                        BwComp3Amount = 813,
                        BwComp4Amount = 814,
                        BwComp5Amount = 815,
                        BwControlLink = "OB_ENROL",
                        BwOfcrLink = "2100",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 2100
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_54A95",
                        BwTitle = "Department 33333 : Object 54A95",
                        BwExpenseAcct = "11_00_01_00_33333_54A95",
                        BwLineAmt = 54995,
                        BwComp1Amount = 911,
                        BwComp2Amount = 912,
                        BwComp3Amount = 913,
                        BwComp4Amount = 914,
                        BwComp5Amount = 915,
                        BwControlLink = "OB_ENROL_FA",
                        BwOfcrLink = "3100",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 1111
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_54N70",
                        BwTitle = "Department 33333 : Object 54N70",
                        BwExpenseAcct = "11_00_01_00_33333_54N70",
                        BwLineAmt = 54770,
                        BwComp1Amount = 1011,
                        BwComp2Amount = 1012,
                        BwComp3Amount = 1013,
                        BwComp4Amount = 1014,
                        BwComp5Amount = 1015,
                        BwControlLink = "OB_ENROL_FA",
                        BwOfcrLink = "3100",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 1111
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_54X35",
                        BwTitle = "Department 33333 : Object 54X35",
                        BwExpenseAcct = "11_00_01_00_33333_54X35",
                        BwLineAmt = 54335,
                        BwComp1Amount = 1021,
                        BwComp2Amount = 1022,
                        BwComp3Amount = 1023,
                        BwComp4Amount = 1024,
                        BwComp5Amount = 1025,
                        BwControlLink = "OB_ENROL_REV",
                        BwOfcrLink = "3200",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 3200
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_33333_55200",
                        BwTitle = "Department 33333 : Object 55200",
                        BwExpenseAcct = "11_00_01_00_33333_55200",
                        BwLineAmt = 55200,
                        BwComp1Amount = 1031,
                        BwComp2Amount = 1032,
                        BwComp3Amount = 1033,
                        BwComp4Amount = 0,
                        BwComp5Amount = 0,
                        BwControlLink = "OB_ENROL_REV",
                        BwOfcrLink = "3200",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 3200
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_44444_54005",
                        BwTitle = "Department 44444 : Object 54005",
                        BwExpenseAcct = "11_00_01_00_44444_54005",
                        BwLineAmt = 54005,
                        BwComp1Amount = 1041,
                        BwComp2Amount = 1042,
                        BwComp3Amount = 1043,
                        BwComp4Amount = 1044,
                        BwComp5Amount = 1045,
                        BwControlLink = "OB_FIN_ACCT",
                        BwOfcrLink = "3300",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 3300
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_44444_54006",
                        BwTitle = "Department 44444 : Object 54006",
                        BwExpenseAcct = "11_00_01_00_44444_54006",
                        BwLineAmt = 54006,
                        BwComp1Amount = 1051,
                        BwComp2Amount = 1052,
                        BwComp3Amount = 1053,
                        BwComp4Amount = 1054,
                        BwComp5Amount = 1055,
                        BwControlLink = "OB_FIN_ACCT",
                        BwOfcrLink = "3300",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 3300
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_44444_54011",
                        BwTitle = "Department 44444 : Object 54011",
                        BwExpenseAcct = "11_00_01_00_44444_54011",
                        BwLineAmt = 54011,
                        BwComp1Amount = 0,
                        BwComp2Amount = 0,
                        BwComp3Amount = 0,
                        BwComp4Amount = 0,
                        BwComp5Amount = 0,
                        BwControlLink = "OB_FIN_IT",
                        BwOfcrLink = "3400",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 3400
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_44444_44030",
                        BwTitle = "Department 44444 : Object 44030",
                        BwExpenseAcct = "11_00_01_00_44444_44030",
                        BwLineAmt = -44030,
                        BwComp1Amount = -1071,
                        BwComp2Amount = 0,
                        BwComp3Amount = -1073,
                        BwComp4Amount = -1074,
                        BwComp5Amount = 0,
                        BwControlLink = "OB_FIN_IT",
                        BwOfcrLink = "3400",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = -3400
                            }
                        }
                    },
                    new BudWork()
                    {
                        Recordkey = "11_00_01_00_44444_54400",
                        BwTitle = "Department 44444 : Object 54400",
                        BwExpenseAcct = "11_00_01_00_44444_54400",
                        BwLineAmt = 454400,
                        BwComp1Amount = 0,
                        BwComp2Amount = 1082,
                        BwComp3Amount = 0,
                        BwComp4Amount = 0,
                        BwComp5Amount = 1085,
                        BwControlLink = "OB_FIN_IT",
                        BwOfcrLink = "3400",
                        BwfreezeEntityAssociation = new List<BudWorkBwfreeze>
                        {
                            new BudWorkBwfreeze
                            {
                                BwVersionAssocMember = "BASE",
                                BwVlineAmtAssocMember = 3400
                            }
                        }
                    }
                };

        public Collection<BudOctl> budOctlRecords = new Collection<BudOctl>()
                {
                    new BudOctl()
                    {
                        Recordkey = "1111",
                        BoBcId = new List<string>() { "OB" }
                    },
                    new BudOctl()
                    {
                        Recordkey = "2100",
                        BoBcId = new List<string>() { "OB_ENROL" }
                    },
                    new BudOctl()
                    {
                        Recordkey = "3100",
                        BoBcId = new List<string>() { "OB_ENROL_FA" }
                    },
                    new BudOctl()
                    {
                        Recordkey = "3200",
                        BoBcId = new List<string>() { "OB_ENROL_REV" }
                    },
                    new BudOctl()
                    {
                        Recordkey = "2200",
                        BoBcId = new List<string>() { "OB_FIN" }
                    },
                    new BudOctl()
                    {
                        Recordkey = "3300",
                        BoBcId = new List<string>() { "OB_FIN_ACCT" }
                    },
                    new BudOctl()
                    {
                        Recordkey = "3400",
                        BoBcId = new List<string>() { "OB_FIN_IT" }
                    }
                };

        public Collection<BudCtrl> budCtrlRecords = new Collection<BudCtrl>()
                {
                    new BudCtrl()
                    {
                        Recordkey = "OB",
                        BcBofId = "1111",
                        BcSup = "",
                        BcAuthDate = DateTime.Now.AddDays(200),
                        BcSub = new List<string>() { "OB_ENROL", "OB_FIN" },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_33333_51111", "11_00_01_00_33333_52222", "11_00_01_00_33333_53333" }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_ENROL",
                        BcBofId = "2100",
                        BcSup = "OB",
                        BcAuthDate = DateTime.Now.AddDays(100),
                        BcSub = new List<string>() { "OB_ENROL_FA", "OB_ENROL_REV" },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_33333_54005", "11_00_01_00_33333_54006", "11_00_01_00_33333_54011", "11_00_01_00_33333_44030", "11_00_01_00_33333_54400" }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_ENROL_FA",
                        BcBofId = "3100",
                        BcSup = "OB_ENROL",
                        BcAuthDate = DateTime.Now.AddDays(50),
                        BcSub = new List<string>() { },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_33333_54A95", "11_00_01_00_33333_54N70" }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_ENROL_REV",
                        BcBofId = "3200",
                        BcSup = "OB_ENROL",
                        BcAuthDate = DateTime.Now.AddDays(60),
                        BcSub = new List<string>() { },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_33333_54X35", "11_00_01_00_33333_55200" }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_FIN",
                        BcBofId = "2200",
                        BcSup = "OB",
                        BcAuthDate = DateTime.Now.AddDays(-3),
                        BcSub = new List<string>() { "OB_FIN_ACCT", "OB_FIN_IT" },
                        BcWorkLineNo = new List<string>() { }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_FIN_ACCT",
                        BcBofId = "3300",
                        BcSup = "OB_FIN",
                        BcAuthDate = DateTime.Now.AddDays(-1),
                        BcSub = new List<string>() { },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_44444_54005", "11_00_01_00_44444_54006"}
                    },
                    new BudCtrl()
                                {
                        Recordkey = "OB_FIN_IT",
                        BcBofId = "3400",
                        BcSup = "OB_FIN",
                        BcAuthDate = DateTime.Now.AddDays(-2),
                        BcSub = new List<string>() { },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_44444_54011", "11_00_01_00_44444_44030", "11_00_01_00_44444_54400" }
                    },
                };

        public Collection<BudCtrl> budCtrlRecordsWithOfcrIds = new Collection<BudCtrl>()
                {
                    new BudCtrl()
                    {
                        Recordkey = "OB",
                        BcBofId = "1111",
                        BcSub = new List<string>() { "OB_ENROL", "OB_FIN" },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_33333_51111", "11_00_01_00_33333_52222", "11_00_01_00_33333_53333" }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_ENROL",
                        BcBofId = "2100",
                        BcSub = new List<string>() { "OB_ENROL_FA", "OB_ENROL_REV" },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_33333_54005", "11_00_01_00_33333_54006", "11_00_01_00_33333_54011", "11_00_01_00_33333_44030", "11_00_01_00_33333_54400" }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_ENROL_FA",
                        BcBofId = "1111",
                        BcSub = new List<string>() { },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_33333_54A95", "11_00_01_00_33333_54N70" }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_ENROL_REV",
                        BcBofId = "3200",
                        BcSub = new List<string>() { },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_33333_54X35", "11_00_01_00_33333_55200" }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_FIN",
                        BcBofId = "1111",
                        BcSub = new List<string>() { "OB_FIN_ACCT", "OB_FIN_IT" },
                        BcWorkLineNo = new List<string>() { }
                    },
                    new BudCtrl()
                    {
                        Recordkey = "OB_FIN_ACCT",
                        BcBofId = "3300",
                        BcSub = new List<string>() { },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_44444_54005", "11_00_01_00_44444_54006"}
                    },
                    new BudCtrl()
                                {
                        Recordkey = "OB_FIN_IT",
                        BcBofId = "3400",
                        BcSub = new List<string>() { },
                        BcWorkLineNo = new List<string>() { "11_00_01_00_44444_54011", "11_00_01_00_44444_44030", "11_00_01_00_44444_54400" }
                    },
                };

        public Collection<BudOfcr> budOfcrRecords = new Collection<BudOfcr>()
                {
                    new BudOfcr()
                    {
                        Recordkey = "1111",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2018",
                                BoLoginAssocMember =  "LOGINFORID0000009",
                                BoPeopleIdAssocMember = "0000009",
                                BoBudgetAssocMember =  "FY2018"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000001",
                                BoLoginAssocMember =  "LOGINFORID0000001",
                                BoBudgetAssocMember =  "FY2021"
                            }
                        }
                    },
                    new BudOfcr()
                    {
                        Recordkey = "2100",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2018",
                                BoLoginAssocMember =  "LOGINFORID0000009",
                                BoPeopleIdAssocMember = "0000009",
                                BoBudgetAssocMember =  "FY2018"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2019",
                                BoLoginAssocMember =  "LOGINFORID0000009",
                                BoPeopleIdAssocMember = "0000009",
                                BoBudgetAssocMember =  "FY2019"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2020",
                                BoLoginAssocMember =  "LOGINFORID0000009",
                                BoPeopleIdAssocMember = "0000009",
                                BoBudgetAssocMember =  "FY2020"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember =  "FY2021"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoPeopleIdAssocMember = "0000002",
                                BoBudgetAssocMember =  "FY2022"
                            }
                        }
                    },
                    new BudOfcr()
                    {
                        Recordkey = "3100",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2018",
                                BoLoginAssocMember =  "LOGINFORID0000001",
                                BoPeopleIdAssocMember = "0000001",
                                BoBudgetAssocMember = "FY2018" ,
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember =  "FY2021"
                            },
                        }
                    },
                    new BudOfcr()
                    {
                        Recordkey = "3200",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2018",
                                BoLoginAssocMember =  "LOGINFORID0000001",
                                BoPeopleIdAssocMember = "0000001",
                                BoBudgetAssocMember = "FY2018" ,
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember =  "FY2021"
                            },
                        }
                    },
                    new BudOfcr()
                    {
                        Recordkey = "2200",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2018",
                                BoLoginAssocMember =  "LOGINFORID0000001",
                                BoPeopleIdAssocMember = "0000001",
                                BoBudgetAssocMember = "FY2018" ,
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoLoginAssocMember =  "LOGINFORID0000001",
                                BoPeopleIdAssocMember = "0000001",
                                BoBudgetAssocMember = "FY2019" ,
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember =  "FY2021"
                            },
                        }
                    },
                    new BudOfcr()
                    {
                        Recordkey = "3300",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember = "FY2020"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember = "FY2021"
                            },
                        }

                    },
                    new BudOfcr()
                    {
                        Recordkey = "3400",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember = "FY2021"
                            }
                        }
                    }
                };

        public Collection<BudOfcr> budOfcrRecordsForFy2021_0000001 = new Collection<BudOfcr>()
                {
                    new BudOfcr()
                    {
                        Recordkey = "1111",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2018",
                                BoLoginAssocMember =  "LOGINFORID0000009",
                                BoPeopleIdAssocMember = "0000009",
                                BoBudgetAssocMember =  "FY2018"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000001",
                                BoLoginAssocMember =  "LOGINFORID0000001",
                                BoBudgetAssocMember =  "FY2021"
                            }
                        }
                    },
                    new BudOfcr()
                    {
                        Recordkey = "2100",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2018",
                                BoLoginAssocMember =  "LOGINFORID0000009",
                                BoPeopleIdAssocMember = "0000009",
                                BoBudgetAssocMember =  "FY2018"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2019",
                                BoLoginAssocMember =  "LOGINFORID0000009",
                                BoPeopleIdAssocMember = "0000009",
                                BoBudgetAssocMember =  "FY2019"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2020",
                                BoLoginAssocMember =  "LOGINFORID0000009",
                                BoPeopleIdAssocMember = "0000009",
                                BoBudgetAssocMember =  "FY2020"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember =  "FY2021"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoPeopleIdAssocMember = "0000002",
                                BoBudgetAssocMember =  "FY2022"
                            }
                        }
                    },
                    new BudOfcr()
                    {
                        Recordkey = "3200",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "2018",
                                BoLoginAssocMember =  "LOGINFORID0000001",
                                BoPeopleIdAssocMember = "0000001",
                                BoBudgetAssocMember = "FY2018" ,
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember =  "FY2021"
                            },
                        }
                    },
                    new BudOfcr()
                    {
                        Recordkey = "3300",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember = "FY2020"
                            },
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember = "FY2021"
                            },
                        }

                    },
                    new BudOfcr()
                    {
                        Recordkey = "3400",
                        OfcrinfoEntityAssociation = new List<BudOfcrOfcrinfo>
                        {
                            new BudOfcrOfcrinfo
                            {
                                BoLedgersAssocMember = "",
                                BoPeopleIdAssocMember = "0000002",
                                BoLoginAssocMember =  "LOGINFORID0000002",
                                BoBudgetAssocMember = "FY2021"
                            }
                        }
                    }
                };

        public async Task<WorkingBudget2> GetBudgetDevelopmentWorkingBudget2Async(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> buDevComparables, WorkingBudgetQueryCriteria criteria, string currentUserPersonId, GeneralLedgerAccountStructure glAccountStructure, int startPosition, int recordCount)
        {
            return await Task.Run(async () =>
            {
                await BuildWorkingBudget2();
                workingBudget2.TotalLineItems = 17;
                return workingBudget2;
            });
        }

        public async Task<WorkingBudget2> GetBudgetDevelopmentREVENUEWorkingBudget2Async(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> buDevComparables, WorkingBudgetQueryCriteria criteria, string currentUserPersonId, GeneralLedgerAccountStructure glAccountStructure, int startPosition, int recordCount)
        {
            return await Task.Run(async () =>
            {
                await BuildWorkingREVENUEBudget2();
                workingBudget2.TotalLineItems = 2;
                return workingBudget2;
            });
        }
        public async Task<WorkingBudget2> GetBudgetDevelopmentAllLineItemsWorkingBudget2Async()
        {
            return await Task.Run(async () =>
            {
                await BuildWorkingBudget2();
                workingBudget2.TotalLineItems = 17;
                return workingBudget2;
            });
        }

        public async Task<string> PopulateDescription(string glAccountId)
        {
            testGlAcccountRepository = new TestGeneralLedgerAccountRepository();
            glAccountStructure = new GeneralLedgerAccountStructure();
            glAccountStructure.SetMajorComponentStartPositions(new List<string>() { "1", "4", "7", "10", "13", "19" });

            var description = await testGlAcccountRepository.GetGlAccountDescriptionsAsync(new List<string>() { glAccountId }, glAccountStructure);

            var glAccountDescription = "";
            if (!string.IsNullOrEmpty(glAccountId))
            {
                description.TryGetValue(glAccountId, out glAccountDescription);
            }

            return glAccountDescription;
        }

        public async Task<WorkingBudget2> BuildWorkingBudget2()
        {
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(1) { BudgetLineItem = budgetLineItem51111 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(2) { BudgetLineItem = budgetLineItem52222 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(3) { BudgetLineItem = budgetLineItem53333 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(4) { SubtotalLineItem = subtotalLine4 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(5) { BudgetLineItem = budgetLineItem54005 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(6) { BudgetLineItem = budgetLineItem54006 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(7) { BudgetLineItem = budgetLineItem54011 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(8) { BudgetLineItem = budgetLineItem44030 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(9) { BudgetLineItem = budgetLineItem54400 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(10) { SubtotalLineItem = subtotalLine10 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(11) { BudgetLineItem = budgetLineItem54A95 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(12) { BudgetLineItem = budgetLineItem54N70 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(13) { SubtotalLineItem = subtotalLine13 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(14) { BudgetLineItem = budgetLineItem54X35 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(15) { BudgetLineItem = budgetLineItem55200 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(16) { SubtotalLineItem = subtotalLine16 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(17) { SubtotalLineItem = subtotalLine17 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(18) { BudgetLineItem = budgetLineItem454005 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(19) { BudgetLineItem = budgetLineItem454006 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(20) { SubtotalLineItem = subtotalLine20 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(21) { BudgetLineItem = budgetLineItem454011 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(22) { BudgetLineItem = budgetLineItem444030 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(23) { BudgetLineItem = budgetLineItem454400 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(24) { SubtotalLineItem = subtotalLine24 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(25) { SubtotalLineItem = subtotalLine25 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(26) { SubtotalLineItem = subtotalLine26 });

            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            foreach (var bli in budgetLineItems)
            {
                bli.BudgetAccountDescription = await PopulateDescription(bli.BudgetAccountId);
            }
            return workingBudget2;
        }

        public async Task<WorkingBudget2> BuildWorkingREVENUEBudget2()
        {
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(8) { BudgetLineItem = budgetLineItem44030 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(22) { BudgetLineItem = budgetLineItem444030 });

            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            foreach (var bli in budgetLineItems)
            {
                bli.BudgetAccountDescription = await PopulateDescription(bli.BudgetAccountId);
            }
            return workingBudget2;
        }

        public async Task<WorkingBudget2> BuildWorkingThreeLineItemsBudget2()
        {
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(21) { BudgetLineItem = budgetLineItem454011 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(22) { BudgetLineItem = budgetLineItem444030 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(23) { BudgetLineItem = budgetLineItem454400 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(24) { SubtotalLineItem = subtotalLine24 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(25) { SubtotalLineItem = subtotalLine25 });
            workingBudget2.AddLineItem(new BudgetManagement.Entities.LineItem(26) { SubtotalLineItem = subtotalLine26 });

            var budgetLineItems = workingBudget2.LineItems.Where(li => li.BudgetLineItem != null).Select(li => li.BudgetLineItem);
            foreach (var bli in budgetLineItems)
            {
                bli.BudgetAccountDescription = await PopulateDescription(bli.BudgetAccountId);
            }
            return workingBudget2;
        }

        Task<WorkingBudget> IBudgetDevelopmentRepository.GetBudgetDevelopmentWorkingBudgetAsync(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> budgetConfigurationComparables, WorkingBudgetQueryCriteria criteria, string currentUserPersonId, IList<string> majorComponentStartPosition, int startPosition, int recordCount)
        {
            throw new NotImplementedException();
        }

        Task<List<BudgetLineItem>> IBudgetDevelopmentRepository.GetBudgetDevelopmentBudgetLineItemsAsync(string workingBudgetId, string currentUserPersonId, List<string> budgetAccountIds)
        {
            throw new NotImplementedException();
        }

        Task<List<BudgetLineItem>> IBudgetDevelopmentRepository.UpdateBudgetDevelopmentBudgetLineItemsAsync(string currentUserPersonId, string workingBudgetId, List<string> budgetAccountIds, List<long?> newBudgetAmounts, List<string> justificationNotes)
        {
            throw new NotImplementedException();
        }

        Task<List<BudgetOfficer>> IBudgetDevelopmentRepository.GetBudgetDevelopmentBudgetOfficersAsync(string workingBudgetId, string currentUserPersonId)
        {
            throw new NotImplementedException();
        }

        Task<List<BudgetReportingUnit>> IBudgetDevelopmentRepository.GetBudgetDevelopmentReportingUnitsAsync(string workingBudgetId, string currentUserPersonId)
        {
            throw new NotImplementedException();
        }

    }
}