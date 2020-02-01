// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.BudgetManagement.DataContracts;
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.BudgetManagement.Tests
{
    public class TestBudgetDevelopmentRepository : IBudgetDevelopmentRepository
    {
        public TestGeneralLedgerAccountRepository testGlAcccountRepository;
        public GeneralLedgerAccountStructure glAccountStructure;

        WorkingBudget workingBudget1111 = new WorkingBudget()
        {
            BudgetLineItems = new System.Collections.Generic.List<BudgetLineItem>()
            {
                new BudgetLineItem("11_00_01_00_33333_51111")
                    {
                        FormattedBudgetAccountId = "11-00-01-00-33333-51111",
                        BaseBudgetAmount = 51111,
                        WorkingAmount = 51111,
                        BudgetComparables = new List<BudgetComparable>()
                        {
                            new BudgetComparable("C1")
                            {
                                ComparableAmount = 111,
                            },
                            new BudgetComparable("C2")
                            {
                                ComparableAmount = 222,
                            },
                            new BudgetComparable("C3")
                            {
                                ComparableAmount = 333,
                            },
                            new BudgetComparable("C4")
                            {
                                ComparableAmount = 444,
                            },
                            new BudgetComparable("C5")
                            {
                                ComparableAmount = 555,
                            }
                        },
                        BudgetOfficer = new BudgetOfficer("1111")
                        {
                            BudgetOfficerLogin = "LOGINFOR0000001",
                            BudgetOfficerName = "Name for 0000001"
                        }
                    },
                new BudgetLineItem("11_00_01_00_33333_52222")
                    {
                        FormattedBudgetAccountId = "11-00-01-00-33333-52222",
                        BudgetAccountDescription = "Department 33333 : Object 52222",
                        BaseBudgetAmount = 52222,
                        WorkingAmount = 52222,
                        BudgetComparables = new List<BudgetComparable>()
                        {
                            new BudgetComparable("C1")
                            {
                                ComparableAmount = 111,
                            },
                            new BudgetComparable("C2")
                            {
                                ComparableAmount = 222,
                            },
                            new BudgetComparable("C3")
                            {
                                ComparableAmount = 333,
                            },
                            new BudgetComparable("C4")
                            {
                                ComparableAmount = 444,
                            },
                            new BudgetComparable("C5")
                            {
                                ComparableAmount = 555,
                            }
                        },
                        BudgetOfficer = new BudgetOfficer("1111")
                        {
                            BudgetOfficerLogin = "LOGINFOR0000001",
                            BudgetOfficerName = "Name for 0000001"
                        }
                    },
                new BudgetLineItem("11_00_01_00_33333_53333")
                    {
                        FormattedBudgetAccountId = "11-00-01-00-33333-53333",
                        BudgetAccountDescription = "Department 33333 : Object 53333",
                        BaseBudgetAmount = 53333,
                        WorkingAmount = 53333,
                        BudgetComparables = new List<BudgetComparable>()
                        {
                            new BudgetComparable("C1")
                            {
                                ComparableAmount = 111,
                            },
                            new BudgetComparable("C2")
                            {
                                ComparableAmount = 222,
                            },
                            new BudgetComparable("C3")
                            {
                                ComparableAmount = 333,
                            },
                            new BudgetComparable("C4")
                            {
                                ComparableAmount = 444,
                            },
                            new BudgetComparable("C5")
                            {
                                ComparableAmount = 555,
                            }
                        },
                        BudgetOfficer = new BudgetOfficer("1111")
                        {
                            BudgetOfficerLogin = "LOGINFOR0000001",
                            BudgetOfficerName = "Name for 0000001"
                        }
                    },
                new BudgetLineItem("11_00_01_00_33333_54A95")
                    {
                        FormattedBudgetAccountId = "11-00-01-00-33333-54A95",
                        BudgetAccountDescription = "Department 33333 : Object 54A95",
                        BaseBudgetAmount = 54995,
                        WorkingAmount = 54995,
                        BudgetComparables = new List<BudgetComparable>()
                        {
                            new BudgetComparable("C1")
                            {
                                ComparableAmount = 111,
                            },
                            new BudgetComparable("C2")
                            {
                                ComparableAmount = 222,
                            },
                            new BudgetComparable("C3")
                            {
                                ComparableAmount = 333,
                            },
                            new BudgetComparable("C4")
                            {
                                ComparableAmount = 444,
                            },
                            new BudgetComparable("C5")
                            {
                                ComparableAmount = 555,
                            }
                        },
                        BudgetOfficer = new BudgetOfficer("1111")
                        {
                            BudgetOfficerLogin = "LOGINFOR0000001",
                            BudgetOfficerName = "Name for 0000001"
                        }
                    },
                new BudgetLineItem("11_00_01_00_33333_54N70")
                    {
                        FormattedBudgetAccountId = "11-00-01-00-33333-54N70",
                        BudgetAccountDescription = "Department 33333 : Object 54N70",
                        BaseBudgetAmount = 54775,
                        WorkingAmount = 54775,
                        BudgetComparables = new List<BudgetComparable>()
                        {
                            new BudgetComparable("C1")
                            {
                                ComparableAmount = 111,
                            },
                            new BudgetComparable("C2")
                            {
                                ComparableAmount = 222,
                            },
                            new BudgetComparable("C3")
                            {
                                ComparableAmount = 333,
                            },
                            new BudgetComparable("C4")
                            {
                                ComparableAmount = 444,
                            },
                            new BudgetComparable("C5")
                            {
                                ComparableAmount = 555,
                            }
                        },
                        BudgetOfficer = new BudgetOfficer("1111")
                        {
                            BudgetOfficerLogin = "LOGINFOR0000001",
                            BudgetOfficerName = "Name for 0000001"
                        }
                    }
            },
            TotalLineItems = 5
        };

        WorkingBudget workingBudget2222 = new WorkingBudget()
        {
            BudgetLineItems = new System.Collections.Generic.List<BudgetLineItem>()
            {
                new BudgetLineItem("11_00_01_00_33333_41111")
                {
                    FormattedBudgetAccountId = "11-00-01-00-33333-41111",
                    BaseBudgetAmount = 41111,
                    WorkingAmount = 41111,
                    BudgetComparables = new List<BudgetComparable>()
                    {
                        new BudgetComparable("C1")
                        {
                            ComparableAmount = 111,
                        },
                        new BudgetComparable("C2")
                        {
                            ComparableAmount = 222,
                        },
                        new BudgetComparable("C3")
                        {
                            ComparableAmount = 333,
                        },
                        new BudgetComparable("C4")
                        {
                            ComparableAmount = 444,
                        },
                        new BudgetComparable("C5")
                        {
                            ComparableAmount = 555,
                        }
                    },
                    BudgetOfficer = new BudgetOfficer("2222")
                    {
                        BudgetOfficerLogin = "LOGINFOR0000002",
                        BudgetOfficerName = "Name for 0000002"
                    }
                },
                new BudgetLineItem("11_00_01_00_33333_44N70")
                {
                    FormattedBudgetAccountId = "11-00-01-00-33333-44N70",
                    BudgetAccountDescription = "Department 33333 : Object 44N70",
                    BaseBudgetAmount = 44770,
                    WorkingAmount = 44775,
                    BudgetComparables = new List<BudgetComparable>()
                    {
                        new BudgetComparable("C1")
                        {
                            ComparableAmount = 111,
                        },
                        new BudgetComparable("C2")
                        {
                            ComparableAmount = 222,
                        },
                        new BudgetComparable("C3")
                        {
                            ComparableAmount = 333,
                        },
                        new BudgetComparable("C4")
                        {
                            ComparableAmount = 444,
                        },
                        new BudgetComparable("C5")
                        {
                            ComparableAmount = 555,
                        }
                    },
                    BudgetOfficer = new BudgetOfficer("2222")
                    {
                        BudgetOfficerLogin = "LOGINFOR0000002",
                        BudgetOfficerName = "Name for 0000002"
                    }
                }
            }
        };

        List<BudgetLineItem> lineItems1 = new List<BudgetLineItem>
        {
            new BudgetLineItem("11_00_02_00_20002_51000")
            {
                FormattedBudgetAccountId = "11-00-02-00-20002-51000",
                WorkingAmount = 3000
            }
        };

        WorkingBudget2 workingBudget2 = new WorkingBudget2();

        List<BudgetLineItem> lineItems = new List<BudgetLineItem>()
        {
            new BudgetLineItem("11_00_02_00_20002_51000")
            {
               FormattedBudgetAccountId = "",
               WorkingAmount = 5000
            }
        };

        List<BudgetOfficer> budgetOfficers = new List<BudgetOfficer>()
        {
            new BudgetOfficer("2100")
            {
               BudgetOfficerLogin = "GTT",
               BudgetOfficerName =  "Gary Thorne"

            }
        };

        List<BudgetReportingUnit> reportingUnits = new List<BudgetReportingUnit>()
        {
            new BudgetReportingUnit("OB_FIN")
            {
                Description = "Description for OB_FIN"

            },
            new BudgetReportingUnit("OB_FIN_ACCT")
            {
                Description = "Description for OB_FIN_ACCT"

            },
            new BudgetReportingUnit("OB_FIN_IT")
            {
                Description = "Description for OB_FIN_IT"

            }
        };

        public BudgetConfiguration budgetConfiguration = new BudgetConfiguration("FY2021")
        {
            BudgetTitle = "Working Budget",
            BudgetStatus = BudgetStatus.Working
        };

        IEnumerable<BudgetConfigurationComparable> buDevComparables = new List<BudgetConfigurationComparable>()
        {
            new BudgetConfigurationComparable()
            {
            SequenceNumber = 1,
            ComparableId = "C1",
            ComparableYear = "2015",
            ComparableHeader = "Actuals"
        },
               new BudgetConfigurationComparable()
        {
            SequenceNumber = 2,
            ComparableId = "C2",
            ComparableYear = "2016",
            ComparableHeader = "Original Budget"
        },

         new BudgetConfigurationComparable()
        {
            SequenceNumber = 3,
            ComparableId = "C3",
            ComparableYear = "2017",
            ComparableHeader = "Adjusted Budget"
        },

         new BudgetConfigurationComparable()
        {
            SequenceNumber = 4,
            ComparableId = "C4",
            ComparableYear = "2018",
            ComparableHeader = "Encumbered Actuals"
        },

        new BudgetConfigurationComparable()
        {
            SequenceNumber = 5,
            ComparableId = "C5",
            ComparableYear = "2019",
            ComparableHeader = "Allocated Budget"
        }
        };

        public List<string> budgetAccountIds = new List<string>()
        {
            "11_00_01_00_33333_51111",
            "11_00_01_00_33333_52222"
        };

        public List<long?> budgetAmounts = new List<long?>()
        {
            50000,
            80000
        };

        public List<string> justificationNotes = new List<string>()
        {
            "note one.",
            "note two."
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
                BwTitle = "Department 33333 : Object 554005",
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
                BwLineAmt = 44030,
                BwComp1Amount = 711,
                BwComp2Amount = 712,
                BwComp3Amount = 713,
                BwComp4Amount = 714,
                BwComp5Amount = 715,
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
                BwLineAmt = 54200,
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
                BwLineAmt = 44030,
                BwComp1Amount = 1071,
                BwComp2Amount = 0,
                BwComp3Amount = 1073,
                BwComp4Amount = 1074,
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
                Recordkey = "11_00_01_00_44444_54400",
                BwTitle = "Department 44444 : Object 54400",
                BwExpenseAcct = "11_00_01_00_44444_54400",
                BwLineAmt = 54011,
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
                BoBcId = new List<string>() { "OB", "OB_ENROL_FA" }
            },
            new BudOctl()
            {
                Recordkey = "2100",
                BoBcId = new List<string>() { "OB_ENROL" }
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
                BcBofId = "1111",
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

        public async Task<WorkingBudget> GetBudgetDevelopmentWorkingBudgetAsync(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> buDevComparables, WorkingBudgetQueryCriteria criteria, string currentUserPersonId, IList<string> majorComponentStartPosition, int startPosition, int recordCount)
        {
            return await Task.Run(async () =>
            {

                foreach (var lineItem in workingBudget1111.BudgetLineItems)
                {
                    lineItem.BudgetAccountDescription = await PopulateDescription(lineItem.BudgetAccountId);
                }
                workingBudget1111.TotalLineItems = 17;
                return workingBudget1111;
            });
        }

        public async Task<WorkingBudget> GetBudgetDevelopmentRevenueWorkingBudgetAsync(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> buDevComparables, WorkingBudgetQueryCriteria criteria, string currentUserPersonId, IList<string> majorComponentStartPosition, int startPosition, int recordCount)
        {
            return await Task.Run(async () =>
            {

                foreach (var lineItem in workingBudget2222.BudgetLineItems)
                {
                    lineItem.BudgetAccountDescription = await PopulateDescription(lineItem.BudgetAccountId);
                }
                workingBudget1111.TotalLineItems = 17;
                return workingBudget2222;
            });
        }

        public async Task<WorkingBudget> GetBudgetDevelopmentWorkingBudgetOnlyOneLineAsync(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> buDevComparables, WorkingBudgetQueryCriteria criteria, string currentUserPersonId, IList<string> majorComponentStartPosition, int startPosition, int recordCount)
        {
            return await Task.Run(async () =>
            {
                workingBudget1111.BudgetLineItems.RemoveAll(x => true);
                workingBudget1111.AddBudgetLineItem(new BudgetLineItem("11_00_01_00_33333_51111")
                {
                    FormattedBudgetAccountId = "11-00-01-00-33333-51111",
                    BaseBudgetAmount = 51111,
                    WorkingAmount = 51111,
                    BudgetComparables = new List<BudgetComparable>()
                        {
                            new BudgetComparable("C1")
                            {
                                ComparableAmount = 111,
                            },
                            new BudgetComparable("C2")
                            {
                                ComparableAmount = 222,
                            },
                            new BudgetComparable("C3")
                            {
                                ComparableAmount = 333,
                            },
                            new BudgetComparable("C4")
                            {
                                ComparableAmount = 444,
                            },
                            new BudgetComparable("C5")
                            {
                                ComparableAmount = 555,
                            }
                        }

                });
                workingBudget1111.TotalLineItems = 1;
                foreach (var lineItem in workingBudget1111.BudgetLineItems)
                {
                    lineItem.BudgetAccountDescription = await PopulateDescription(lineItem.BudgetAccountId);
                }
                return workingBudget1111;
            });
        }

        public async Task<WorkingBudget> GetBudgetDevelopmentWorkingBudgetNoC1C5Async(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> buDevComparables, WorkingBudgetQueryCriteria criteria, string currentUserPersonId, IList<string> majorComponentStartPosition, int startPosition, int recordCount)
        {
            return await Task.Run(async () =>
            {
                workingBudget1111.BudgetLineItems.RemoveAll(x => true);
                workingBudget1111.AddBudgetLineItem(new BudgetLineItem("11_00_01_00_33333_51111")
                {
                    FormattedBudgetAccountId = "11-00-01-00-33333-51111",
                    BaseBudgetAmount = 51111,
                    WorkingAmount = 51111
                });
                workingBudget1111.TotalLineItems = 1;
                foreach (var lineItem in workingBudget1111.BudgetLineItems)
                {
                    lineItem.BudgetAccountDescription = await PopulateDescription(lineItem.BudgetAccountId);
                }

                return workingBudget1111;
            });
        }

        public async Task<List<BudgetLineItem>> GetBudgetDevelopmentBudgetLineItemsAsync(string workingBudgetId, string currentUserPersonId, List<string> budgetAccountId)
        {
            return await Task.Run(() =>
            {
                return lineItems1;
            });
        }

        public async Task<List<BudgetLineItem>> UpdateBudgetDevelopmentBudgetLineItemsAsync(string currentUserPersonId, string workingBudgetId, List<string> budgetAccountIds, List<long?> newBudgetAmounts, List<string> justificationNotes)
        {
            return await Task.Run(() =>
            {
                return lineItems;
            });
        }

        public async Task<List<BudgetOfficer>> GetBudgetDevelopmentBudgetOfficersAsync(string workingBudgetId, string currentUserPersonId)
        {
            return await Task.Run(() =>
            {
                return budgetOfficers;
            });
        }

        public async Task<List<BudgetReportingUnit>> GetBudgetDevelopmentReportingUnitsAsync(string workingBudgetId, string currentUserPersonId)
        {
            return await Task.Run(() =>
            {
                return reportingUnits;
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

        Task<WorkingBudget2> IBudgetDevelopmentRepository.GetBudgetDevelopmentWorkingBudget2Async(string workingBudgetId, ReadOnlyCollection<BudgetConfigurationComparable> buDevComparables, WorkingBudgetQueryCriteria criteria, string currentUserPersonId, GeneralLedgerAccountStructure glAccountStructure, int startPosition, int recordCount)
        {
            throw new NotImplementedException();
        }
    }
}