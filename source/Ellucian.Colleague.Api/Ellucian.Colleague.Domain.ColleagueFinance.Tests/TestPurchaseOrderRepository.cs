// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    /// <summary>
    /// This class represents a set of purchase orders
    /// </summary>
    public class TestPurchaseOrderRepository : IPurchaseOrderRepository
    {
        private List<PurchaseOrder> purchaseOrders = new List<PurchaseOrder>();
        private List<PurchaseOrderSummary> purchaseOrdersSummaryList = new List<PurchaseOrderSummary>();
        private PurchaseOrderCreateUpdateResponse purchaseOrderCreateUpdateResponse = new PurchaseOrderCreateUpdateResponse();

        #region Define all data for a purchase order

        private string[,] purchaseOrdersArray = {
            //  0       1         2                                    3       4            5         6              7                8                   9               10             11                     12                   13                    14         15                       16                17 
            //  ID    Vendor ID   Vendor Name                          Status  AP Type      Amount    Date           Delivery Date    Maintenance Date    PO Number       Status Date    Initiator              Requestor            Comments              Ship To    Internal Comments        Currency Code     Guid
            {  "1",   "0009876",  "Ellucian Consulting, Inc.",         "I",    "AP",    "2,330.00",   "1/11/2015",   "1/30/2015",     "1/21/2015",        "P0001111",     "1/21/2015",   "One Initiator",       "Mary Requestor",    "Just comments 1",     "MC",      "Internal Comments 1",   "",              "d43c02c1-ce35-4090-a30b-4e4735670ccd"      }, 
            {  "2",   "0009876",  "Ellucian Consulting, Inc.",         "N",    "AP",      "222.22",   "2/3/2015",    "2/23/2015",     "2/13/2015",        "P0002222",     "2/13/2015",   "Two Initiator",       "null",              "Just comments 2",    "null",    "Internal Comments 2",   "CAN" ,           "775a0cd7-5c31-4dea-b899-fe7075109ff7"   }, 
            {  "3",   "0009876",  "Ellucian Consulting, Inc.",         "O",    "AP",      "333.33",   "2/4/2015",    "2/24/2015",     "2/14/2015",        "P0003333",     "2/14/2015",   "Three Initiator",     "Mary Requestor",    "Just comments 3",    "null",    "Internal Comments 3",   "",               "58e55c9e-c501-4377-8cc0-15e28bf75f7c"     }, 
            {  "4",   "0009876",  "Ellucian Consulting, Inc.",         "U",    "AP",      "444.44",   "2/5/2015",    "2/25/2015",     "2/15/2015",        "P0004444",     "2/25/2015",   "Four Initiator",      "null",              "Just comments 4",    "null",    "Internal Comments 4",   "",               "38682c97-9a31-4930-a499-60f12773db82"  },
            {  "5",   "",         "Ellucian Consulting",               "U",    "AP",      "555.55",   "2/6/2015",    "2/26/2015",     "2/16/2015",        "null",         "2/26/2015",   "",                    "",                  "Just comments 5",    "MC",      "Internal Comments 5",   "",               "e048c9f7-fac0-4bcb-bf2b-432b97c3eb45" }, 
            {  "6",   "",         "Ellucian's Consulting Associates",  "B",    "AP",      "666.66",   "2/7/2015",    "2/27/2015",     "3/17/2015",        "P0006666",     "2/27/2015",   "",                    "",                  "Just comments 6",    "null",    "Internal Comments 6",   "",               "813fa105-f3a5-4dbd-83e1-64ecaf3f94f2"  }, 
            {  "7",   "0001111",  "null",                              "A",    "AP",    "7,777.77",   "3/1/2015",    "3/31/2015",     "3/11/2015",        "P0007777",     "3/31/2015",   "Seven Initiator",     "Mary Requestor",    "Just comments 7",    "null",    "Internal Comments 7",   "",               "f361e3e7-df96-46f0-960d-c7b4634c6a36" }, 
            {  "8",   "0002222",  "null",                              "I",    "AP",    "8,888.88",   "3/2/2015",    "3/22/2015",     "3/12/2015",        "P0008888",     "3/22/2015",   "",                    "",                  "Just comments 8",    "MC",      "Internal Comments 8",   "",               "9c8d26f8-07aa-4ab9-a240-bd4df7abc754"},
            {  "9",   "0005432",  "Offices Supplies Unlimited",        "R",    "AP",    "9,999.99",   "3/3/2015",    "3/23/2015",     "3/13/2015",        "P0009999",     "3/23/2015",   "Nine Initiator",      "Mary Requestor",    "Just comments 9",    "null",    "Internal Comments 9",   "",               "957b64fa-7322-44ae-9870-3e671cdc5095"}, 
            { "10",   "0005432",  "null",                              "C",    "AP",    "1,234.56",   "3/4/2015",    "3/24/2015",     "3/14/2015",        "P0010001",     "3/24/2015",   "Ten Initiator",       "Mary Requestor",    "Just comments 10",   "null",    "Internal Comments 10",  "",               "e408fc0b-947e-488e-81c3-023423a4b6af" },
            { "11",   "0005432",  "Offices Supplies Unlimited",        "P",    "AP",      "876.54",   "3/5/2015",    "3/25/2015",     "3/15/2015",        "P0010101",     "3/25/2015",   "Eleven Initiator",    "null",              "Just comments 11",   "null",    "Internal Comments 11",  "",               "f168fe0c-7741-4282-8812-e22ad8348411" },  
            { "12",   "0005432",  "Offices Supplies Unlimited",        "V",    "AP",          null,   "1/1/2015",           null,      "1/1/2015",        "P0010102",     "1/1/2015",    "",                    "",                  "",                   "null",    "",                      "",               "0c075b2b-9f11-4fec-a91b-3dff36de283e"  }, 
            { "13",   "",         "null",                              "U",    "AP",          null,   "1/1/2015",           null,      "1/1/2015",        "P0010103",     "1/1/2015",    "",                    "",                  "",                   "null",    "",                      "",               "14c16db2-6dbc-4f58-a656-59cd6bd239b4" }, 
            { "31",   "0009876",  "Office Supplies",                  "O",    "AP",      "800.00",   "3/31/2015",    "4/15/2015",     "3/31/2015",        "P0003131",      "3/31/2015",   "",                    "",                  "Just Comments 31",   "null",    "Internal Comments 31",  "",              "ed5c6e34-4e8f-4ab1-8c04-d1ef9a6f86d3"  },   // GL security: Possible access - all line items visisible, but some GL numbers masked
            { "32",   "0009876",  "Office Supplies",                  "O",    "AP",      "800.00",   "3/31/2015",    "4/15/2015",     "3/31/2015",        "P0003132",      "3/31/2015",   "",                    "",                  "Just Comments 32",   "null",    "Internal Comments 32",  "",              "7f67ce94-c474-4139-83b6-b77a656ce80b" },   // GL security: Possible access - only one line item is visible, some GL numbers masked
            { "33",   "0009876",  "Office Supplies",                  "O",    "AP",      "800.00",   "3/31/2015",    "4/15/2015",     "3/31/2015",        "P0003133",      "3/31/2015",   "",                    "",                  "Just Comments 33",   "null",    "Internal Comments 33",  "",              "7bdcd6cc-4e0c-4096-8c8d-e2159318f816"},   // GL security: Possible access - zero line items returned
                                                };

        private string[,] approversArray = {
            //  0           1                       2               3
            //  ID          Name                    Date            PO ID
            {   "0000001",  "Andy Kleehammer",      "1/1/2015",     "1" },
            {   "0000002",  "Gary Thorne",          "1/3/2015",     "1" },
            {   "0000003",  "Teresa Longerbeam",    "null",         "1" },
        };

        private string[,] requisitionsArray = {
            //  0          1             2              
            //  ID         Number        PO ID
            {   "31",     "0000331",     "3" },
            {   "32",     "0000332",     "3" },
            {   "33",     "0000333",     "3" },
        };

        private string[,] vouchersArray = {
            //  0             1              
            //  Number        PO ID
            {   "V0009991",   "1" },
            {   "V0009992",   "2" },
            {   "V0009993",   "2" },
        };

        private string[,] lineItemsArray = {
            //   0      1        2                3       4          5               6               7               8                   9             10         11          12                            13
            //   ID     PO ID    Description        Qty     Price      Unit of Issue   Vendor Part     Extended Price  Expctd Dlvry Date   Tax Form      Tax Code   Tax Loc   Comments                   LineItemStatus  
            { "1111",   "1",     "Training",        "6",   "180.00",   "Days",         "VP",         "1,080.00",       "1/20/2015",        "1099-MISC",  "NEC",     "VA",     "Comments line item 1111",    "A" },
            { "1112",   "1",     "Consulting",      "5",   "210.00",   "Hours",        "",           "1,050.00",       "null",             "",           "",          "",     "Comments line item 1112",    "I" },   

            { "2222",   "2",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",             "",           "",          "",     "Comments line item 2222",    "" },

            { "2222",   "3",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",             "",           "",          "",     "Comments line item 2222",    "" },

            { "2222",   "4",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",             "",           "",          "",     "Comments line item 2222",    "" },

            { "2222",   "6",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",             "",           "",          "",     "Comments line item 2222",    "" },

            { "2222",   "7",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",             "",           "",          "",     "Comments line item 2222",    "" },

            { "2222",   "9",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",             "",           "",          "",     "Comments line item 2222",    "" },

            { "2222",   "10",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",             "",           "",          "",     "Comments line item 2222",   "" },

            { "2222",   "11",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",             "",           "",          "",     "Comments line item 2222",   "" },

            { "2222",   "12",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",             "",           "",          "",     "Comments line item 2222",   "" },

            { "3131",  "31",     "First line 31",  "14",   "101.55",   "box",          "VP",           "543.21",       "1/3/2015",         "1099-MISC",  "NEC",     "VA",     "Comments line item 3131",    "" },
            { "3132",  "31",     "Second line 31", "14",   "208.09",   "box",          "VP",           "543.21",       "1/3/2015",         "1099-MISC",  "NEC",     "VA",     "Comments line item 3132",    "" },

            { "3231",  "32",     "First line 32",  "14",   "101.55",   "box",          "VP",           "543.21",       "1/3/2015",         "1099-MISC",  "NEC",     "VA",     "Comments line item 3231",    ""},
            { "3232",  "32",     "Second line 32", "14",   "208.09",   "box",          "VP",           "543.21",       "1/3/2015",         "1099-MISC",  "NEC",     "VA",     "Comments line item 3232",    ""},

            { "3331",  "33",     "First line 33",  "14",   "101.55",   "box",          "VP",           "543.21",       "1/3/2015",         "1099-MISC",  "NEC",     "VA",     "Comments line item 3331",    ""},
            { "3332",  "33",     "Second line 33", "14",   "208.09",   "box",          "VP",           "543.21",       "1/3/2015",         "1099-MISC",  "NEC",     "VA",     "Comments line item 3332",    ""},
        };

        private string[,] glDistributionArray = {
            //  0                           1                 2               3                  4                     5                    6             7
            //  GL Account                  Line Item ID      Project ID      Project Number     Prj Line Item ID      Project Item Code    Quantity      Amount
            {   "11_10_00_01_20601_51000",        "1111",          "100",          "AJK-100",                "50",                "AJK1",       "3",    "540.00" },
            {   "11_10_00_01_20601_51001",        "1111",          "200",          "AJK-200",                "60",                "AJK2",       "2",    "360.00" },
            {   "11_10_00_01_20601_52001",        "1111",          "200",          "AJK-200",                "60",                "AJK2",       "1",    "180.00" },
            
            {   "11_10_00_01_20601_51000",        "1112",          "100",          "AJK-100",                "50",                "AJK1",       "3",    "630.00" },
            {   "11_10_00_01_20601_51001",        "1112",          "200",          "AJK-200",                "60",                "AJK2",       "2",    "420.00" },

            {   "11_10_00_01_20601_51001",        "2222",          "200",          "AJK-200",                "60",                "AJK2",        "1",   " 22.22" },
            {   "11_10_00_01_20601_51002",        "2222",          "300",          "AJK-300",                "70",                "AJK3",        "1",   "110.00" },
            {   "11_10_00_01_20601_51003",        "2222",          "400",          "AJK-400",                "80",                "AJK4",        "1",    "60.00" },

            {   "11_10_00_01_20601_51000",        "3131",          "100",          "AJK-100",                "50",                "AJK1",       "43",    "75.00"  },
            {   "11_10_00_01_20601_51001",        "3131",          "200",          "AJK-200",                "60",                "AJK2",       "10",   "100.00" },
            {   "11_10_00_01_20601_52001",        "3131",          "200",          "AJK-200",                "60",                "AJK2",       "10",   "125.00" },
            
            {   "11_10_00_01_20601_51000",        "3132",          "100",          "AJK-100",                "50",                "AJK1",       "43",   "175.00" },
            {   "11_10_00_01_20601_51001",        "3132",          "200",          "AJK-200",                "60",                "AJK2",       "10",   "125.00" },

            {   "11_10_00_01_20601_51000",        "3231",          "100",           "AJK-100",               "50",                "AJK1",       "43",    "75.00"  },
            {   "11_10_00_01_20601_51001",        "3231",          "200",           "AJK-200",               "60",                "AJK2",       "10",   "100.00" },
            {   "11_10_00_01_20601_52001",        "3231",          "200",           "AJK-200",               "60",                "AJK2",       "10",   "125.00" },
            
            {   "11_10_00_01_20601_51000",        "3232",          "100",           "AJK-100",               "50",                "AJK1",       "43",   "175.00" },
            {   "11_10_00_01_20601_51001",        "3232",          "200",           "AJK-200",               "60",                "AJK2",       "10",   "125.00" },

            {   "11_10_00_01_20601_51000",        "3331",          "100",           "AJK-100",               "50",                "AJK1",       "43",    "75.00"  },
            {   "11_10_00_01_20601_51001",        "3331",          "200",           "AJK-200",               "60",                "AJK2",       "10",   "100.00" },
            {   "11_10_00_01_20601_52001",        "3331",          "200",           "AJK-200",               "60",                "AJK2",       "10",   "125.00" },
            
            {   "11_10_00_01_20601_51000",        "3332",          "100",           "AJK-100",               "50",                "AJK1",       "43",   "175.00" },
            {   "11_10_00_01_20601_51001",        "3332",          "200",           "AJK-200",               "60",                "AJK2",       "10",   "125.00" },
        };

        private string[,] taxArray = {
            //  0       1           2
            //  Code    Amount      Line Item ID
            {   "VA",   "60.75",    "1111" }, // Purchase Order 1
            {   "MD",   "39.25",    "1111" }, // Purchase Order 1

            {   "VA",   "20.00",    "1112" }, // Purchase Order 2
            {   "MD",   "80.00",    "1112" }, // Purchase Order 2

            {   "BC",   "15.00",    "2222" }, // Purchase Order 3
            {   "NV",    "5.00",    "2222" }, // Purchase Order 3
            {   "AL",   "10.00",    "2222" }, // Purchase Order 3

            
            {   "VA",   "60.00",    "3131" }, // Purchase Order 31
            {   "MD",   "40.00",    "3131" }, // Purchase Order 31
            {   "VA",   "25.00",    "3132" }, // Purchase Order 31
            {   "MD",   "75.00",    "3132" }, // Purchase Order 31

            {   "VA",   "60.00",    "3231" }, // Purchase Order 32
            {   "MD",   "40.00",    "3231" }, // Purchase Order 32
            {   "VA",   "25.00",    "3232" }, // Purchase Order 32
            {   "MD",   "75.00",    "3232" }, // Purchase Order 32

            {   "VA",   "60.00",    "3331" }, // Purchase Order 33
            {   "MD",   "40.00",    "3331" }, // Purchase Order 33
            {   "VA",   "25.00",    "3332" }, // Purchase Order 33
            {   "MD",   "75.00",    "3332" }, // Purchase Order 33
        };

        #endregion

        public TestPurchaseOrderRepository()
        {
            Populate();
            PopulatePurchaseOrderSummary();

            // Add a domain entity to the PO list that has one GL account masked.
            var purchaseOrder = new PurchaseOrder("999", "P000999", "Susty Corporation", PurchaseOrderStatus.Accepted, DateTime.Now, DateTime.Now);
            
            var lineItem = new LineItem("999", "Line item #1", 10m, 100m, 125m);
            lineItem.AddGlDistribution(new LineItemGlDistribution("11_10_00_01_20601_51001", 10m, 100m)
                { GlAccountDescription = "GL description #1", Masked = false, ProjectId = "29", ProjectLineItemCode = "AJK1", ProjectLineItemId = "12", ProjectNumber = "AJK-123" });
            lineItem.AddGlDistribution(new LineItemGlDistribution("11_10_00_01_20601_51002", 150m, 1050m)
                { GlAccountDescription = "GL description #2", Masked = true, ProjectId = "291", ProjectLineItemCode = "AJK21", ProjectLineItemId = "112", ProjectNumber = "AJK-1263" });
            purchaseOrder.AddLineItem(lineItem);

            purchaseOrders.Add(purchaseOrder);
        }

        private void PopulatePurchaseOrderSummary()
        {
            #region Populate Purchase Order Summary

            // Loop through the purchase orders array and create purchase orders domain entities.
            string purchaseOrderId,
                    vendorId,
                    vendorName,
                    apType,
                    number,
                    initiatorName,
                    requestorName,
                    shipToCode,
                    comments,
                    internalComments,
                    currencyCode;

            PurchaseOrderStatus status;

            decimal purchaseOrderAmount;

            DateTime date,
                     statusDate,
                     maintenanceDate;

            DateTime? deliveryDate;

            for (var i = 0; i < purchaseOrdersArray.GetLength(0); i++)
            {
                purchaseOrderId = purchaseOrdersArray[i, 0];
                vendorId = purchaseOrdersArray[i, 1];
                vendorName = purchaseOrdersArray[i, 2];

                switch (purchaseOrdersArray[i, 3])
                {
                    case "A":
                        status = PurchaseOrderStatus.Accepted;
                        break;
                    case "B":
                        status = PurchaseOrderStatus.Backordered;
                        break;
                    case "C":
                        status = PurchaseOrderStatus.Closed;
                        break;
                    case "U":
                        status = PurchaseOrderStatus.InProgress;
                        break;
                    case "I":
                        status = PurchaseOrderStatus.Invoiced;
                        break;
                    case "N":
                        status = PurchaseOrderStatus.NotApproved;
                        break;
                    case "O":
                        status = PurchaseOrderStatus.Outstanding;
                        break;
                    case "P":
                        status = PurchaseOrderStatus.Paid;
                        break;
                    case "R":
                        status = PurchaseOrderStatus.Reconciled;
                        break;
                    case "V":
                        status = PurchaseOrderStatus.Voided;
                        break;
                    default:
                        throw new Exception("Invalid status specified in TestPurchaseOrderRepository.");
                }

                apType = purchaseOrdersArray[i, 4];
                purchaseOrderAmount = Convert.ToDecimal(purchaseOrdersArray[i, 5]);
                date = Convert.ToDateTime(purchaseOrdersArray[i, 6]);
                deliveryDate = Convert.ToDateTime(purchaseOrdersArray[i, 7]);
                maintenanceDate = Convert.ToDateTime(purchaseOrdersArray[i, 8]);
                number = purchaseOrdersArray[i, 9];
                statusDate = Convert.ToDateTime(purchaseOrdersArray[i, 10]);
                initiatorName = purchaseOrdersArray[i, 11];
                requestorName = purchaseOrdersArray[i, 12];
                comments = purchaseOrdersArray[i, 13];
                shipToCode = purchaseOrdersArray[i, 14];
                internalComments = purchaseOrdersArray[i, 15];
                currencyCode = purchaseOrdersArray[i, 16];

                var purchaseOrderSummary = new PurchaseOrderSummary(purchaseOrderId, number, vendorName, date);
                purchaseOrderSummary.Status = PurchaseOrderStatus.Invoiced;
                purchaseOrderSummary.VendorId = vendorId;
                purchaseOrderSummary.ApType = apType;
                purchaseOrderSummary.Amount = purchaseOrderAmount;
                purchaseOrderSummary.MaintenanceDate = maintenanceDate;
                purchaseOrderSummary.InitiatorName = initiatorName;
                purchaseOrderSummary.RequestorName = requestorName;              
                purchaseOrderSummary.Comments = comments;             
                purchaseOrderSummary.CurrencyCode = currencyCode;
                purchaseOrdersSummaryList.Add(purchaseOrderSummary);
            }
            #endregion

            #region Populate Requisition

            string requisitionId,
                requisitionNumber,
                requisitionPurchaseOrderId;

            for (var i = 0; i < requisitionsArray.GetLength(0); i++)
            {
                requisitionId = requisitionsArray[i, 0];
                requisitionNumber = requisitionsArray[i, 1];
                requisitionPurchaseOrderId = requisitionsArray[i, 2];
                
                foreach (var purchaseOrdersSummary in purchaseOrdersSummaryList)
                {
                    if (purchaseOrdersSummary.Id == requisitionPurchaseOrderId)
                    {
                        var po = new RequisitionSummary(requisitionId, requisitionNumber, purchaseOrdersSummary.VendorName, purchaseOrdersSummary.Date);
                        purchaseOrdersSummary.AddRequisition(po);
                    }
                }
            }
            #endregion

        }
        public TestPurchaseOrderRepository(string guid)
        {
            PopulateGuid();

            // Add a domain entity to the PO list that has one GL account masked.
            var purchaseOrder = new PurchaseOrder("999", guid, "P000999", "Susty Corporation", PurchaseOrderStatus.Accepted, DateTime.Now, DateTime.Now);

            var lineItem = new LineItem("999", "Line item #1", 10m, 100m, 125m);
            lineItem.AddGlDistribution(new LineItemGlDistribution("11_10_00_01_20601_51001", 10m, 100m)
            { GlAccountDescription = "GL description #1", Masked = false, ProjectId = "29", ProjectLineItemCode = "AJK1", ProjectLineItemId = "12", ProjectNumber = "AJK-123" });
            lineItem.AddGlDistribution(new LineItemGlDistribution("11_10_00_01_20601_51002", 150m, 1050m)
            { GlAccountDescription = "GL description #2", Masked = true, ProjectId = "291", ProjectLineItemCode = "AJK21", ProjectLineItemId = "112", ProjectNumber = "AJK-1263" });
            purchaseOrder.AddLineItem(lineItem);

            purchaseOrders.Add(purchaseOrder);
        }


        public async Task<PurchaseOrder> GetPurchaseOrderAsync(string purchaseOrderId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts)
        {
            var purchaseOrder = await Task.Run(() => purchaseOrders.FirstOrDefault(x => x.Id == purchaseOrderId));
            return purchaseOrder;
        }

        public async Task<PurchaseOrder> GetPurchaseOrderAsync(string guid, string purchaseOrderId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts)
        {
            var purchaseOrder = await Task.Run(() => purchaseOrders.FirstOrDefault(x => x.Id == purchaseOrderId));
            return purchaseOrder;
        }

        private void Populate()
        {
            #region Populate Purchase Orders

            // Loop through the purchase orders array and create purchase orders domain entities.
            string purchaseOrderId,
                    vendorId,
                    vendorName,
                    apType,
                    number,
                    initiatorName,
                    requestorName,
                    shipToCode,
                    comments,
                    internalComments,
                    currencyCode;

            PurchaseOrderStatus status;

            decimal purchaseOrderAmount;

            DateTime date,
                     statusDate,
                     maintenanceDate;

            DateTime? deliveryDate;

            for (var i = 0; i < purchaseOrdersArray.GetLength(0); i++)
            {
                purchaseOrderId = purchaseOrdersArray[i, 0];
                vendorId = purchaseOrdersArray[i, 1];
                vendorName = purchaseOrdersArray[i, 2];

                switch (purchaseOrdersArray[i, 3])
                {
                    case "A":
                        status = PurchaseOrderStatus.Accepted;
                        break;
                    case "B":
                        status = PurchaseOrderStatus.Backordered;
                        break;
                    case "C":
                        status = PurchaseOrderStatus.Closed;
                        break;
                    case "U":
                        status = PurchaseOrderStatus.InProgress;
                        break;
                    case "I":
                        status = PurchaseOrderStatus.Invoiced;
                        break;
                    case "N":
                        status = PurchaseOrderStatus.NotApproved;
                        break;
                    case "O":
                        status = PurchaseOrderStatus.Outstanding;
                        break;
                    case "P":
                        status = PurchaseOrderStatus.Paid;
                        break;
                    case "R":
                        status = PurchaseOrderStatus.Reconciled;
                        break;
                    case "V":
                        status = PurchaseOrderStatus.Voided;
                        break;
                    default:
                        throw new Exception("Invalid status specified in TestPurchaseOrderRepository.");
                }

                apType = purchaseOrdersArray[i, 4];
                purchaseOrderAmount = Convert.ToDecimal(purchaseOrdersArray[i, 5]);
                date = Convert.ToDateTime(purchaseOrdersArray[i, 6]);
                deliveryDate = Convert.ToDateTime(purchaseOrdersArray[i, 7]);
                maintenanceDate = Convert.ToDateTime(purchaseOrdersArray[i, 8]);
                number = purchaseOrdersArray[i, 9];
                statusDate = Convert.ToDateTime(purchaseOrdersArray[i, 10]);
                initiatorName = purchaseOrdersArray[i, 11];
                requestorName = purchaseOrdersArray[i, 12];
                comments = purchaseOrdersArray[i, 13];
                shipToCode = purchaseOrdersArray[i, 14];
                internalComments = purchaseOrdersArray[i, 15];
                currencyCode = purchaseOrdersArray[i, 16];

                var purchaseOrder = new PurchaseOrder(purchaseOrderId, number, vendorName, status, statusDate, date);

                purchaseOrder.VendorId = vendorId;
                purchaseOrder.ApType = apType;
                purchaseOrder.Amount = purchaseOrderAmount;
                purchaseOrder.DeliveryDate = deliveryDate;
                purchaseOrder.MaintenanceDate = maintenanceDate;
                purchaseOrder.InitiatorName = initiatorName;
                purchaseOrder.RequestorName = requestorName;
                purchaseOrder.ShipToCodeName = shipToCode;
                purchaseOrder.Comments = comments;
                purchaseOrder.InternalComments = internalComments;
                purchaseOrder.CurrencyCode = currencyCode;

                purchaseOrders.Add(purchaseOrder);
            }

            #endregion

            #region Populate approvers

            string approverId,
                approvalName,
                approvalPurchaseOrderId;
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
                approvalPurchaseOrderId = approversArray[i, 3];
                var approverDomainEntity = new Approver(approverId);
                approverDomainEntity.SetApprovalName(approvalName);
                approverDomainEntity.ApprovalDate = approvalDate;

                foreach (var purchaseOrder in purchaseOrders)
                {
                    if (purchaseOrder.Id == approvalPurchaseOrderId)
                    {
                        purchaseOrder.AddApprover(approverDomainEntity);
                    }
                }
            }
            #endregion

            #region Populate Requisitions

            string requisitionId,
                requisitionNumber,
                requisitionPurchaseOrderId;

            for (var i = 0; i < requisitionsArray.GetLength(0); i++)
            {
                requisitionId = requisitionsArray[i, 0];
                requisitionNumber = requisitionsArray[i, 1];
                requisitionPurchaseOrderId = requisitionsArray[i, 2];

                foreach (var purchaseOrder in purchaseOrders)
                {
                    if (purchaseOrder.Id == requisitionPurchaseOrderId)
                    {
                        purchaseOrder.AddRequisition(requisitionId);
                    }
                }
            }
            #endregion

            #region Populate Vouchers

            string voucherNumber,
                   voucherPurchaseOrderId;

            for (var i = 0; i < vouchersArray.GetLength(0); i++)
            {
                voucherNumber = vouchersArray[i, 0];
                voucherPurchaseOrderId = vouchersArray[i, 1];

                foreach (var purchaseOrder in purchaseOrders)
                {
                    if (purchaseOrder.Id == voucherPurchaseOrderId)
                    {
                        purchaseOrder.AddVoucher(voucherNumber);
                    }
                }
            }
            #endregion

            #region Populate line items

            string lineItemId,
                lineItemPurchaseOrderId,
                description,
                unitOfIssue,
                vendorPart,
                taxForm,
                taxFormCode,
                taxFormLocation,
                lineItemComments,
                lineItemStatus;

            DateTime? expectedDeliveryDate;

            decimal quantity,
                price,
                extendedPrice;

            for (var i = 0; i < lineItemsArray.GetLength(0); i++)
            {
                lineItemId = lineItemsArray[i, 0];
                lineItemPurchaseOrderId = lineItemsArray[i, 1];
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

                taxForm = lineItemsArray[i, 9];
                taxFormCode = lineItemsArray[i, 10];
                taxFormLocation = lineItemsArray[i, 11];
                lineItemComments = lineItemsArray[i, 12];
                lineItemStatus = lineItemsArray[i, 13];

                var lineItem = new LineItem(lineItemId, description, quantity, price, extendedPrice);

                lineItem.UnitOfIssue = unitOfIssue;
                lineItem.VendorPart = vendorPart;
                lineItem.ExpectedDeliveryDate = expectedDeliveryDate;
                lineItem.TaxForm = taxForm;
                lineItem.TaxFormCode = taxFormCode;
                lineItem.TaxFormLocation = taxFormLocation;
                lineItem.Comments = lineItemComments;
                switch (lineItemStatus.ToUpper())
                {
                    case "A":
                        lineItem.LineItemStatus = LineItemStatus.Accepted;
                        break;
                    case "B":
                        lineItem.LineItemStatus = LineItemStatus.Backordered;
                        break;
                    case "C":
                        lineItem.LineItemStatus = LineItemStatus.Closed;
                        break;
                    case "I":
                        lineItem.LineItemStatus = LineItemStatus.Invoiced;
                        break;
                    case "O":
                        lineItem.LineItemStatus = LineItemStatus.Outstanding;
                        break;
                    case "P":
                        lineItem.LineItemStatus = LineItemStatus.Paid;
                        break;
                    case "R":
                        lineItem.LineItemStatus = LineItemStatus.Reconciled;
                        break;
                    case "V":
                        lineItem.LineItemStatus = LineItemStatus.Voided;
                        break;
                    case "H":
                        lineItem.LineItemStatus = LineItemStatus.Hold;
                        break;
                    default:
                        break;
                }

                foreach (var purchaseOrder in purchaseOrders)
                {
                    if (purchaseOrder.Id == lineItemPurchaseOrderId)
                    {
                        purchaseOrder.AddLineItem(lineItem);
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
                projectItemCode;

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

                var lineItemGlDistribution = new LineItemGlDistribution(glAccount, glDistrQuantity, glDistrAmount);
                lineItemGlDistribution.ProjectId = projectId;
                lineItemGlDistribution.ProjectNumber = projectNumber;
                lineItemGlDistribution.ProjectLineItemId = projectLineItemId;
                lineItemGlDistribution.ProjectLineItemCode = projectItemCode;

                foreach (var purchaseOrder in purchaseOrders)
                {
                    foreach (var lineItem in purchaseOrder.LineItems)
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

                foreach (var purchaseOrder in purchaseOrders)
                {
                    foreach (var lineItem in purchaseOrder.LineItems)
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

        private void PopulateGuid()
        {
            #region Populate Purchase Orders

            // Loop through the purchase orders array and create purchase orders domain entities.
            string purchaseOrderId,
                    vendorId,
                    vendorName,
                    apType,
                    number,
                    initiatorName,
                    requestorName,
                    shipToCode,
                    comments,
                    internalComments,
                    currencyCode;

            PurchaseOrderStatus status;

            decimal purchaseOrderAmount;

            DateTime date,
                     statusDate,
                     maintenanceDate;

            DateTime? deliveryDate;

            for (var i = 0; i < purchaseOrdersArray.GetLength(0); i++)
            {
                purchaseOrderId = purchaseOrdersArray[i, 0];
                vendorId = purchaseOrdersArray[i, 1];
                vendorName = purchaseOrdersArray[i, 2];

                switch (purchaseOrdersArray[i, 3])
                {
                    case "A":
                        status = PurchaseOrderStatus.Accepted;
                        break;
                    case "B":
                        status = PurchaseOrderStatus.Backordered;
                        break;
                    case "C":
                        status = PurchaseOrderStatus.Closed;
                        break;
                    case "U":
                        status = PurchaseOrderStatus.InProgress;
                        break;
                    case "I":
                        status = PurchaseOrderStatus.Invoiced;
                        break;
                    case "N":
                        status = PurchaseOrderStatus.NotApproved;
                        break;
                    case "O":
                        status = PurchaseOrderStatus.Outstanding;
                        break;
                    case "P":
                        status = PurchaseOrderStatus.Paid;
                        break;
                    case "R":
                        status = PurchaseOrderStatus.Reconciled;
                        break;
                    case "V":
                        status = PurchaseOrderStatus.Voided;
                        break;
                    default:
                        throw new Exception("Invalid status specified in TestPurchaseOrderRepository.");
                }

                apType = purchaseOrdersArray[i, 4];
                purchaseOrderAmount = Convert.ToDecimal(purchaseOrdersArray[i, 5]);
                date = Convert.ToDateTime(purchaseOrdersArray[i, 6]);
                deliveryDate = Convert.ToDateTime(purchaseOrdersArray[i, 7]);
                maintenanceDate = Convert.ToDateTime(purchaseOrdersArray[i, 8]);
                number = purchaseOrdersArray[i, 9];
                statusDate = Convert.ToDateTime(purchaseOrdersArray[i, 10]);
                initiatorName = purchaseOrdersArray[i, 11];
                requestorName = purchaseOrdersArray[i, 12];
                comments = purchaseOrdersArray[i, 13];
                shipToCode = purchaseOrdersArray[i, 14];
                internalComments = purchaseOrdersArray[i, 15];
                currencyCode = purchaseOrdersArray[i, 16];
                var guid = purchaseOrdersArray[i, 17];

                var purchaseOrder = new PurchaseOrder(purchaseOrderId, guid, number, vendorName, status, statusDate, date);

                purchaseOrder.VendorId = vendorId;
                purchaseOrder.ApType = apType;
                purchaseOrder.Amount = purchaseOrderAmount;
                purchaseOrder.DeliveryDate = deliveryDate;
                purchaseOrder.MaintenanceDate = maintenanceDate;
                purchaseOrder.InitiatorName = initiatorName;
                purchaseOrder.RequestorName = requestorName;
                purchaseOrder.ShipToCodeName = shipToCode;
                purchaseOrder.Comments = comments;
                purchaseOrder.InternalComments = internalComments;
                purchaseOrder.CurrencyCode = currencyCode;

                purchaseOrders.Add(purchaseOrder);
            }

            #endregion

            #region Populate approvers

            string approverId,
                approvalName,
                approvalPurchaseOrderId;
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
                approvalPurchaseOrderId = approversArray[i, 3];
                var approverDomainEntity = new Approver(approverId);
                approverDomainEntity.SetApprovalName(approvalName);
                approverDomainEntity.ApprovalDate = approvalDate;

                foreach (var purchaseOrder in purchaseOrders)
                {
                    if (purchaseOrder.Id == approvalPurchaseOrderId)
                    {
                        purchaseOrder.AddApprover(approverDomainEntity);
                    }
                }
            }
            #endregion

            #region Populate Requisitions

            string requisitionId,
                requisitionNumber,
                requisitionPurchaseOrderId;

            for (var i = 0; i < requisitionsArray.GetLength(0); i++)
            {
                requisitionId = requisitionsArray[i, 0];
                requisitionNumber = requisitionsArray[i, 1];
                requisitionPurchaseOrderId = requisitionsArray[i, 2];

                foreach (var purchaseOrder in purchaseOrders)
                {
                    if (purchaseOrder.Id == requisitionPurchaseOrderId)
                    {
                        purchaseOrder.AddRequisition(requisitionId);
                    }
                }
            }
            #endregion

            #region Populate Vouchers

            string voucherNumber,
                   voucherPurchaseOrderId;

            for (var i = 0; i < vouchersArray.GetLength(0); i++)
            {
                voucherNumber = vouchersArray[i, 0];
                voucherPurchaseOrderId = vouchersArray[i, 1];

                foreach (var purchaseOrder in purchaseOrders)
                {
                    if (purchaseOrder.Id == voucherPurchaseOrderId)
                    {
                        purchaseOrder.AddVoucher(voucherNumber);
                    }
                }
            }
            #endregion

            #region Populate line items

            string lineItemId,
                lineItemPurchaseOrderId,
                description,
                unitOfIssue,
                vendorPart,
                taxForm,
                taxFormCode,
                taxFormLocation,
                lineItemComments,
                lineItemStatus;

            DateTime? expectedDeliveryDate;

            decimal quantity,
                price,
                extendedPrice;

            for (var i = 0; i < lineItemsArray.GetLength(0); i++)
            {
                lineItemId = lineItemsArray[i, 0];
                lineItemPurchaseOrderId = lineItemsArray[i, 1];
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

                taxForm = lineItemsArray[i, 9];
                taxFormCode = lineItemsArray[i, 10];
                taxFormLocation = lineItemsArray[i, 11];
                lineItemComments = lineItemsArray[i, 12];
                lineItemStatus = lineItemsArray[i, 13];

                var lineItem = new LineItem(lineItemId, description, quantity, price, extendedPrice);

                lineItem.UnitOfIssue = unitOfIssue;
                lineItem.VendorPart = vendorPart;
                lineItem.ExpectedDeliveryDate = expectedDeliveryDate;
                lineItem.TaxForm = taxForm;
                lineItem.TaxFormCode = taxFormCode;
                lineItem.TaxFormLocation = taxFormLocation;
                lineItem.Comments = lineItemComments;
                switch (lineItemStatus.ToUpper())
                {
                    case "A":
                        lineItem.LineItemStatus = LineItemStatus.Accepted;
                        break;
                    case "B":
                        lineItem.LineItemStatus = LineItemStatus.Backordered;
                        break;
                    case "C":
                        lineItem.LineItemStatus = LineItemStatus.Closed;
                        break;
                    case "I":
                        lineItem.LineItemStatus = LineItemStatus.Invoiced;
                        break;
                    case "O":
                        lineItem.LineItemStatus = LineItemStatus.Outstanding;
                        break;
                    case "P":
                        lineItem.LineItemStatus = LineItemStatus.Paid;
                        break;
                    case "R":
                        lineItem.LineItemStatus = LineItemStatus.Reconciled;
                        break;
                    case "V":
                        lineItem.LineItemStatus = LineItemStatus.Voided;
                        break;
                    case "H":
                        lineItem.LineItemStatus = LineItemStatus.Hold;
                        break;
                    default:
                        break;
                }

                foreach (var purchaseOrder in purchaseOrders)
                {
                    if (purchaseOrder.Id == lineItemPurchaseOrderId)
                    {
                        purchaseOrder.AddLineItem(lineItem);
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
                projectItemCode;

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

                var lineItemGlDistribution = new LineItemGlDistribution(glAccount, glDistrQuantity, glDistrAmount);
                lineItemGlDistribution.ProjectId = projectId;
                lineItemGlDistribution.ProjectNumber = projectNumber;
                lineItemGlDistribution.ProjectLineItemId = projectLineItemId;
                lineItemGlDistribution.ProjectLineItemCode = projectItemCode;

                foreach (var purchaseOrder in purchaseOrders)
                {
                    foreach (var lineItem in purchaseOrder.LineItems)
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

                foreach (var purchaseOrder in purchaseOrders)
                {
                    foreach (var lineItem in purchaseOrder.LineItems)
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
        public Task<Tuple<IEnumerable<PurchaseOrder>, int>> GetPurchaseOrdersAsync(int offset, int limit, string orderNumber)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrder> GetPurchaseOrdersByGuidAsync(string guid, bool allowVoid)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrder> UpdatePurchaseOrdersAsync(PurchaseOrder purchaseOrdersEntity)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrder> CreatePurchaseOrdersAsync(PurchaseOrder purchaseOrdersEntity)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPurchaseOrdersIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrder> GetPurchaseOrdersByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetGuidFromIdAsync(string id, string entity)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, string>> GetProjectReferenceIds(string[] projectIds)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, string>> GetProjectIdsFromReferenceNo(string[] projectRefNo)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> EthosExtendedDataDictionary { get; set; }

        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<PurchaseOrderSummary>> GetPurchaseOrderSummaryByPersonIdAsync(string personId)
        {
            return await Task.Run(() => purchaseOrdersSummaryList);
        }

        public Task<PurchaseOrderCreateUpdateResponse> CreatePurchaseOrderAsync(PurchaseOrderCreateUpdateRequest createUpdateRequest)
        {
            return  Task.Run(() => purchaseOrderCreateUpdateResponse);
        }

        public Task<PurchaseOrderCreateUpdateResponse> UpdatePurchaseOrderAsync(PurchaseOrderCreateUpdateRequest createUpdateRequest, PurchaseOrder originalRequisition)
        {
            return Task.Run(() => purchaseOrderCreateUpdateResponse);
        }

        public Task<PurchaseOrderVoidResponse> VoidPurchaseOrderAsync(PurchaseOrderVoidRequest voidRequest)
        {
            throw new NotImplementedException();
        }
    }
}
