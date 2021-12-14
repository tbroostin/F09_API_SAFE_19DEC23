// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    /// <summary>
    /// This class represents a set of vouchers
    /// </summary>
    public class TestAccountsPayableInvoicesRepository : IVoucherRepository
    {
        private List<Voucher> vouchers = new List<Voucher>();
        #region Define all data for a voucher

        private string[,] vouchersArray = {
            //  0       1           2                                               3       4           5           6               7               8                   9               10              11              12              13                  14          15          16                  17
            //  ID      Vendor ID   Vendor Name                                     Status  AP Type     Amount      Date            Due Date        Maintenance Date    Invoice Number  Invoice Date    Check Number    Check Date      Comments            PO ID       BPO ID      RCV Schedule ID     Currency Code   
            {   "1",    "0001234",  "Susty Corporation",                            "N",    "AP",       "800.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has approvers and next approvers
            {   "2",    "0001234",  "Susty Corporation",                            "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "",        "",         "",                 "CAD"    },  // Has approvers and next approvers
            {   "3",    "0001234",  "Susty Corporation",                            "O",    "AP",       "221.15",   "1/2/2015",     "1/10/2015",    "1/4/2015",         "null",         "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Null Invoice Number
            {   "4",    "0001234",  "Susty Corporation",                            "P",    "AP",       "221.15",   "1/2/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "12*345",       "1/8/2015",     "Just comments.",   "12",      "",         "",                 ""       },  // No approvers
            {   "11",   "",         "Susty Corporation for the Severly Wealthy",    "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has a vendor name, no vendor ID
            {   "13",   "0000002",  "null",                                         "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has only vendor ID, CTX long name
            {   "14",   "0001234",  "Susty Corporation",                            "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has a vendor ID and name
            {   "15",   "",         "null",                                         "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has neither vendor ID or name
            {   "16",   "0001234",  "Susty Corporation",                            "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "",        "",         "65",               ""       },  // Originated from RC Voucher
            {   "17",   "0001234",  "Susty Corporation",                            "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "",        "",         "",                 ""       },  // Not Approved voucher
            {   "18",   "0001234",  "Susty Corporation",                            "R",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "01*0000123",   "1/10/2015",    "Just comments.",   "",        "",         "",                 ""       },  // Reconciled voucher
            {   "19",   "0001234",  "Susty Corporation",                            "V",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "",        "",         "",                 ""       },  // Voided voucher
            {   "20",   "",         "CANCELLED BEFORE CREATION",                    "P",    "AP",         "1.00",   "1/1/2015",              "",            "",       "CANCELLED",      "1/9/2015",     "",             "",                           "",   "",        "",         "",                 ""       },  // Cancelled voucher
            {   "21",   "0001234",  "Susty Corporation",                            "N",    "AP",       "800.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "",      "21",         "",                 ""       },  // Originated from BPO
            {   "22",   "0001234",  "Susty Corporation",                            "N",    "AP",       "800.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "",      "21",         "",                 ""       },  // GL security: Possible access - all line items visisible, but some GL numbers masked
            {   "23",   "0001234",  "Susty Corporation",                            "N",    "AP",       "800.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "",      "21",         "",                 ""       },  // GL security: Possible access - only one line item is visible, some GL numbers masked
            {   "24",   "0001234",  "Susty Corporation",                            "N",    "AP",       "800.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "",      "21",         "",                 ""       },  // GL security: Possible access - zero line items returned
            {   "25",   "",         "Susty Corporation",                            "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has a multi-line vendor name, no vendor ID
            {   "26",   "0000002",  "null",                                         "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has only vendor ID, CTX multi-line name
            {   "27",   "0000002",  "whitespace",                                   "N",    "AP",       "400.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has only vendor ID, blank name
            {   "28",   "0001234",  "Susty Corporation",                            "N",    "AP",       "100.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has limited access to line item distributions
            {   "29",   "0001234",  "Susty Corporation",                            "N",    "AP",       "100.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },
            {   "30",   "0001234",  "Susty Corporation",                            "N",    "AP",       "100.00",   "1/1/2015",     "1/10/2015",    "1/4/2015",         "IN12345",      "1/9/2015",     "",             "",             "Just comments.",   "12",      "",         "",                 ""       },  // Has no line items
        };

        private string[,] approversArray = {
            //  0           1                       2               3
            //  ID          Name                    Date            Document ID
            {   "0000001",  "Andy Kleehammer",      "1/1/2015",     "1" },
            {   "0000002",  "Gary Thorne",          "1/3/2015",     "1" },
            {   "0000003",  "Teresa Longerbeam",    "null",         "1" },

            {   "0000002",  "Gary Thorne",          "1/3/2015",     "28" },
            {   "0000003",  "Teresa Longerbeam",    "1/4/2015",     "28" },
        };

        private string[,] lineItemsArray = {
            //  0       1               2                       3           4           5               6               7               8                       9               10          11              12                  13
            //  ID      Voucher ID      Description             Quantity    Price       Unit of Issue   Vendor Part     Extended Price  Expected Delivery Date  Invoice Number  Tax Form    Tax Form Code   Tax Form Location   Comments
            {   "1",    "1",            "Outlet covers",        "14",       "101.55",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "2",    "1",            "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "null",                 "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},  // Null expected delivery date
            
            {   "3",    "2",            "Stuff",                "1",        "450.55",   "thing",        "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},

            {   "4",    "22",           "Outlet covers",        "14",       "101.55",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "5",    "22",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},

            {   "6",    "23",           "Outlet covers",        "14",       "101.55",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "7",    "23",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},

            {   "8",    "24",           "Outlet covers",        "14",       "101.55",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "9",    "24",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},

            {   "10",   "28",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "29",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "17",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "20",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "3",            "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "4",            "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "18",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "11",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "13",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "14",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "15",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "16",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "21",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "25",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "26",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "27",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "30",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."},
            {   "10",   "19",           "Outlets",              "14",       "208.09",   "rock",         "VP",           "543.21",       "1/3/2015",             "IN9876",       "TaxForm",  "TaxFormCode",  "TFLocation",       "Install the covers."}
        };

        // The LineItemGlDistribution domain entity into which this data is mapped only contains a single local amount field, but the foreign amount property in the
        // data contract that the voucher repository uses to calculate the voucher total amount needs to be represented in the VoucherRepositoryTests, where the data
        // contracts are being populated from the domain entities. 
        private string[,] glDistributionArray = {
            //  0                           1                           2               3                   4                           5                   6           7               8
            //  GL Account                  Voucher Line Item ID        Project ID      Project Number      Project Line Item ID        Project Item Code   Quantity    Local Amount    Masked
            {   "11_10_00_01_20601_51000",  "1",                        "10",           "AJK-100",          "50",                       "AJK1",             "43",       "75.00",        "false" },
            {   "11_10_00_01_20601_51001",  "1",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "100.00",       "false" },
            {   "11_10_00_01_20601_52001",  "1",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "125.00",       "false" },

            {   "11_10_00_01_20601_51000",  "2",                        "10",           "AJK-100",          "50",                       "AJK1",             "43",       "175.00",       "false" },
            {   "11_10_00_01_20601_51001",  "2",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "125.00",       "false" },

            {   "11_10_00_01_20601_51001",  "3",                        "11",           "AJK-200",          "60",                       "AJK2",             "6",        "250.00",       "false" }, // data contract foreign amount of 150.00 set in VoucherRepositoryTests
            {   "11_10_00_01_20601_51002",  "3",                        "12",           "AJK-300",          "70",                       "AJK3",             "6",        "150.00",       "false" }, // data contract foreign amount of 100.00 set in VoucherRepositoryTests
            {   "11_10_00_01_20601_51003",  "3",                        "13",           "AJK-400",          "80",                       "AJK4",             "6",        "100.00",       "false" }, // data contract foreign amount of  50.00 set in VoucherRepositoryTests

            {   "11_10_00_01_20601_51000",  "4",                        "10",           "AJK-100",          "50",                       "AJK1",             "43",       "75.00",        "false" },
            {   "11_10_00_01_20601_51001",  "4",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "100.00",       "false" },
            {   "11_10_00_01_20601_52001",  "4",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "125.00",       "false" },

            {   "11_10_00_01_20601_51000",  "5",                        "10",           "AJK-100",          "50",                       "AJK1",             "43",       "175.00",       "false" },
            {   "11_10_00_01_20601_51001",  "5",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "125.00",       "false" },

            {   "11_10_00_01_20601_51000",  "6",                        "10",           "AJK-100",          "50",                       "AJK1",             "43",       "75.00",        "false" },
            {   "11_10_00_01_20601_51001",  "6",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "100.00",       "false" },
            {   "11_10_00_01_20601_52001",  "6",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "125.00",       "false" },

            {   "11_10_00_01_20601_51000",  "7",                        "10",           "AJK-100",          "50",                       "AJK1",             "43",       "175.00",       "false" },
            {   "11_10_00_01_20601_51001",  "7",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "125.00",       "false" },

            {   "11_10_00_01_20601_51000",  "8",                        "10",           "AJK-100",          "50",                       "AJK1",             "43",       "75.00",        "false" },
            {   "11_10_00_01_20601_51001",  "8",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "100.00",       "false" },
            {   "11_10_00_01_20601_52001",  "8",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "125.00",       "false" },

            {   "11_10_00_01_20601_51000",  "9",                        "10",           "AJK-100",          "50",                       "AJK1",             "43",       "175.00",       "false" },
            {   "11_10_00_01_20601_51001",  "9",                        "11",           "AJK-200",          "60",                       "AJK2",             "10",       "125.00",       "false" },

            {   "11_10_00_01_20601_51000",  "10",                       "10",           "AJK-100",          "50",                       "AJK1",             "43",       "50.00",        "true"  },
            {   "11_10_00_01_20601_51001",  "10",                       "11",           "AJK-200",          "60",                       "AJK2",             "10",       "50.00",        "false" },
        };

        private string[,] taxArray = {
            //  0       1           2
            //  Code    Amount      Line Item ID
            {   "VA",   "60.00",    "1" }, // Voucher 1
            {   "MD",   "40.00",    "1" }, // Voucher 1
            {   "VA",   "25.00",    "2" }, // Voucher 1
            {   "MD",   "75.00",    "2" }, // Voucher 1

            {   "BC",   "30.00",    "3" }, // Voucher 2 - data contract local amount of 15.00 set in VoucherRepositoryTests
            {   "NV",   "50.00",    "3" }, // Voucher 2 - data contract local amount of 25.00 set in VoucherRepositoryTests
            {   "AL",   "20.00",    "3" }, // Voucher 2 - data contract local amount of 10.00 set in VoucherRepositoryTests

            {   "VA",   "60.00",    "4" }, // Voucher 22
            {   "MD",   "40.00",    "4" }, // Voucher 22
            {   "VA",   "25.00",    "5" }, // Voucher 22
            {   "MD",   "75.00",    "5" }, // Voucher 22

            {   "VA",   "60.00",    "6" }, // Voucher 23
            {   "MD",   "40.00",    "6" }, // Voucher 23
            {   "VA",   "25.00",    "7" }, // Voucher 23
            {   "MD",   "75.00",    "7" }, // Voucher 23

            {   "VA",   "60.00",    "8" }, // Voucher 24
            {   "MD",   "40.00",    "8" }, // Voucher 24
            {   "VA",   "25.00",    "9" }, // Voucher 24
            {   "MD",   "75.00",    "9" }, // Voucher 24

            {   "VA",   "25.00",    "10" }, // Voucher 28
            {   "MD",   "75.00",    "10" }, // Voucher 28
        };
        #endregion

        public TestAccountsPayableInvoicesRepository()
        {
            Populate();

            // Add a domain entity to the requisitions list that has one GL account masked.
            var voucher = new Voucher("999", DateTime.Now, VoucherStatus.InProgress, "Susty Corporation");
            voucher.InvoiceDate = DateTime.Now;
            voucher.InvoiceNumber = "IN12345";

            var lineItem = new LineItem("999", "Line item #1", 10m, 100m, 125m);
            lineItem.AddGlDistribution(new LineItemGlDistribution("11_10_00_01_20601_51001", 10m, 100m) { GlAccountDescription = "GL description #1", Masked = false, ProjectId = "29", ProjectLineItemCode = "AJK1", ProjectLineItemId = "12", ProjectNumber = "AJK-123" });
            lineItem.AddGlDistribution(new LineItemGlDistribution("11_10_00_01_20601_51002", 150m, 1050m) { GlAccountDescription = "GL description #2", Masked = true, ProjectId = "291", ProjectLineItemCode = "AJK21", ProjectLineItemId = "112", ProjectNumber = "AJK-1263" });
            voucher.AddLineItem(lineItem);

            vouchers.Add(voucher);
        }

        public async Task<Voucher> GetVoucherAsync(string voucherId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts, int versionNumber)
        {
            var voucher = await Task.Run(() => vouchers.FirstOrDefault(x => x.Id == voucherId));
            return voucher;
        }

        private void Populate()
        {
            #region Populate vouchers
            // Loop through the vouchers array and create voucher domain entities.
            string voucherId,
                vendorId,
                vendorName,
                invoiceNumber,
                checkNumber,
                comments,
                poId,
                bpoId,
                rcvId,
                currencyCode,
                apType;

            decimal voucherAmount;

            DateTime voucherDate;

            DateTime? dueDate,
                maintenanceDate,
                checkDate,
                invoiceDate;

            VoucherStatus status;
            for (var i = 0; i < vouchersArray.GetLength(0); i++)
            {
                voucherId = vouchersArray[i, 0];
                vendorId = vouchersArray[i, 1];
                vendorName = vouchersArray[i, 2];

                switch (vouchersArray[i, 3])
                {
                    case "N":
                        status = VoucherStatus.NotApproved;
                        break;
                    case "O":
                        status = VoucherStatus.Outstanding;
                        break;
                    case "P":
                        status = VoucherStatus.Paid;
                        break;
                    case "R":
                        status = VoucherStatus.Reconciled;
                        break;
                    case "U":
                        status = VoucherStatus.InProgress;
                        break;
                    case "V":
                        status = VoucherStatus.Voided;
                        break;
                    case "X":
                        status = VoucherStatus.Cancelled;
                        break;
                    default:
                        throw new Exception("Invalid status specified in TestVoucherRepository.");
                }

                apType = vouchersArray[i, 4];

                voucherAmount = Convert.ToDecimal(vouchersArray[i, 5]);

                voucherDate = Convert.ToDateTime(vouchersArray[i, 6]);

                dueDate = null;
                if (vouchersArray[i, 7] != "")
                {
                    dueDate = Convert.ToDateTime(vouchersArray[i, 7]);
                }

                maintenanceDate = null;
                if (vouchersArray[i, 8] != "")
                {
                    maintenanceDate = Convert.ToDateTime(vouchersArray[i, 8]);
                }

                invoiceNumber = vouchersArray[i, 9];

                invoiceDate = Convert.ToDateTime(vouchersArray[i, 10]);

                checkNumber = null;
                if (vouchersArray[i, 11] != "")
                {
                    checkNumber = vouchersArray[i, 11];
                }

                checkDate = null;
                if (vouchersArray[i, 12] != "")
                {
                    checkDate = Convert.ToDateTime(vouchersArray[i, 12]);
                }

                comments = vouchersArray[i, 13];
                poId = vouchersArray[i, 14];
                bpoId = vouchersArray[i, 15];
                rcvId = vouchersArray[i, 16];
                currencyCode = vouchersArray[i, 17];

                var voucher = new Voucher(voucherId, voucherDate, status, vendorName);
                voucher.InvoiceNumber = invoiceNumber;
                voucher.InvoiceDate = invoiceDate;
                voucher.VendorId = vendorId;
                voucher.ApType = apType;
                voucher.Amount = voucherAmount;
                voucher.DueDate = dueDate;
                voucher.MaintenanceDate = maintenanceDate;
                voucher.CheckNumber = checkNumber;
                voucher.CheckDate = checkDate;
                voucher.Comments = comments;
                voucher.PurchaseOrderId = poId;

                if (string.IsNullOrEmpty(voucher.PurchaseOrderId))
                {
                    voucher.BlanketPurchaseOrderId = bpoId;
                }

                if (string.IsNullOrEmpty(voucher.PurchaseOrderId) && string.IsNullOrEmpty(voucher.BlanketPurchaseOrderId))
                {
                    voucher.RecurringVoucherId = rcvId;
                }

                voucher.CurrencyCode = currencyCode;
                vouchers.Add(voucher);
            }
            #endregion

            #region Populate approvers
            string approverId,
                approvalName,
                approvalVoucherId;
            DateTime? approvalDate;
            for (var i = 0; i < approversArray.GetLength(0); i++)
            {
                approverId = approversArray[i, 0];
                approvalName = approversArray[i, 1];

                if (approversArray[i, 2] == "null")
                {
                    approvalDate = null;
                }
                else
                {
                    approvalDate = Convert.ToDateTime(approversArray[i, 2]);
                }
                approvalVoucherId = approversArray[i, 3];
                var approverDomainEntity = new Approver(approverId);
                approverDomainEntity.SetApprovalName(approvalName);
                approverDomainEntity.ApprovalDate = approvalDate;

                foreach (var voucher in vouchers)
                {
                    if (voucher.Id == approvalVoucherId)
                    {
                        voucher.AddApprover(approverDomainEntity);
                    }
                }
            }
            #endregion

            #region Populate line items
            string lineItemId,
                lineItemVoucherId,
                description,
                unitOfIssue,
                vendorPart,
                lineItemInvoiceNumber,
                taxForm,
                taxFormCode,
                taxFormLocation,
                lineItemComments;

            decimal quantity,
                price,
                extendedPrice;

            DateTime? expectedDeliveryDate;
            for (var i = 0; i < lineItemsArray.GetLength(0); i++)
            {
                lineItemId = lineItemsArray[i, 0];
                lineItemVoucherId = lineItemsArray[i, 1];
                description = lineItemsArray[i, 2];
                quantity = Convert.ToDecimal(lineItemsArray[i, 3]);
                price = Convert.ToDecimal(lineItemsArray[i, 4]);

                unitOfIssue = lineItemsArray[i, 5];
                vendorPart = lineItemsArray[i, 6];
                extendedPrice = Convert.ToDecimal(lineItemsArray[i, 7]);

                if (lineItemsArray[i, 8] == "null")
                {
                    expectedDeliveryDate = null;
                }
                else
                {
                    expectedDeliveryDate = Convert.ToDateTime(lineItemsArray[i, 8]);
                }

                lineItemInvoiceNumber = lineItemsArray[i, 9];
                taxForm = lineItemsArray[i, 10];
                taxFormCode = lineItemsArray[i, 11];
                taxFormLocation = lineItemsArray[i, 12];
                lineItemComments = lineItemsArray[i, 13];

                var lineItem = new LineItem(lineItemId, description, quantity, price, extendedPrice);
                lineItem.UnitOfIssue = unitOfIssue;
                lineItem.VendorPart = vendorPart;
                lineItem.ExpectedDeliveryDate = expectedDeliveryDate;
                lineItem.InvoiceNumber = lineItemInvoiceNumber;
                lineItem.TaxForm = taxForm;
                lineItem.TaxFormCode = taxFormCode;
                lineItem.TaxFormLocation = taxFormLocation;
                lineItem.Comments = lineItemComments;

                foreach (var voucher in vouchers)
                {
                    if (voucher.Id == lineItemVoucherId)
                    {
                        voucher.AddLineItem(lineItem);
                    }
                }
            }
            #endregion

            #region Populate GL Distributions
            string glAccount,
                glDistrLineItemId,
                projectId,
                projectNumber,
                projectLineItemId,
                projectItemCode,
                masked;

            decimal glDistrQuantity,
                glDistrAmount;

            for (var i = 0; i < glDistributionArray.GetLength(0); i++)
            {
                glAccount = glDistributionArray[i, 0];
                glDistrLineItemId = glDistributionArray[i, 1];
                projectId = glDistributionArray[i, 2];
                projectNumber = glDistributionArray[i, 3];
                projectLineItemId = glDistributionArray[i, 4];
                projectItemCode = glDistributionArray[i, 5];
                glDistrQuantity = Convert.ToDecimal(glDistributionArray[i, 6]);
                glDistrAmount = Convert.ToDecimal(glDistributionArray[i, 7]);
                masked = glDistributionArray[i, 8];

                var lineItemGlDistribution = new LineItemGlDistribution(glAccount, glDistrQuantity, glDistrAmount);
                lineItemGlDistribution.ProjectId = projectId;
                lineItemGlDistribution.ProjectNumber = projectNumber;
                lineItemGlDistribution.ProjectLineItemId = projectLineItemId;
                lineItemGlDistribution.ProjectLineItemCode = projectItemCode;
                lineItemGlDistribution.Masked = Convert.ToBoolean(masked);

                foreach (var voucher in vouchers)
                {
                    foreach (var lineItem in voucher.LineItems)
                    {
                        if (lineItem.Id == glDistrLineItemId)
                        {
                            lineItem.AddGlDistribution(lineItemGlDistribution);
                        }
                    }
                }
            }
            #endregion

            #region Populate tax distributions
            string code,
                taxDistrLineItemId;

            decimal taxDistrAmount;
            for (var i = 0; i < taxArray.GetLength(0); i++)
            {
                code = taxArray[i, 0];
                taxDistrAmount = Convert.ToDecimal(taxArray[i, 1]);
                taxDistrLineItemId = taxArray[i, 2];

                var lineItemTax = new LineItemTax(code, taxDistrAmount);

                foreach (var voucher in vouchers)
                {
                    foreach (var lineItem in voucher.LineItems)
                    {
                        if (lineItem.Id == taxDistrLineItemId)
                        {
                            lineItem.AddTax(lineItemTax);
                        }
                    }
                }
            }
            #endregion
        }

        public Task<IEnumerable<VoucherSummary>> GetVoucherSummariesByPersonIdAsync(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<VoucherCreateUpdateResponse> CreateVoucherAsync(VoucherCreateUpdateRequest createUpdateRequest)
        {
            throw new NotImplementedException();
        }

        public Task<VendorsVoucherSearchResult> GetReimbursePersonAddressForVoucherAsync(string personId)
        {
            throw new NotImplementedException();
        }

        public Task<VoucherCreateUpdateResponse> UpdateVoucherAsync(VoucherCreateUpdateRequest createUpdateRequest, Voucher originalVoucher)
        {
            throw new NotImplementedException();
        }

        Task<VoucherVoidResponse> IVoucherRepository.VoidVoucherAsync(VoucherVoidRequest voucherVoidRequest)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Voucher>> GetVouchersByVendorAndInvoiceNoAsync(string vendorId, string invoiceNo)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<VoucherSummary>> QueryVoucherSummariesAsync(ProcurementDocumentFilterCriteria criteria)
        {
            throw new NotImplementedException();
        }
    }
}
