// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    /// <summary>
    /// This class represents a set of requisitions
    /// </summary>
    public class TestRequisitionRepository : IRequisitionRepository
    {
        private List<Requisition> requisitions = new List<Requisition>();
        private List<RequisitionSummary> requisitionsSummaryList = new List<RequisitionSummary>();

        #region Define all data for a requisition

        private string[,] requisitionsArray = {
            //  0       1         2                                    3       4            5         6              7                8                   9               10               11                    12                    13                    14          15                       16                17                
            //  ID    Vendor ID   Vendor Name                          Status  AP Type      Amount    Date           Desired Date     Maintenance Date    REQ Number      Status Date      Initiator             Requestor             Comments              Ship To     Internal Comments        Currency Code     BPO ID   
            {  "1",   "0009876",  "Ellucian Consulting, Inc.",         "O",    "AP",    "2,330.00",   "05/11/2015",  "05/30/2015",    "05/21/2015",       "0001111",      "05/21/2015",    "One Initiator",      "Mary Requestor1",    "Just comments 1",    "MC",       "Internal Comments 1",   "",               ""      },      
            {  "2",   "0009876",  "Ellucian Consulting, Inc.",         "N",    "AP",      "222.22",   "2/3/2015",    "2/23/2015",     "2/13/2015",        "0002222",      "2/13/2015",     "Two Initiator",      "null",               "Just comments 2",    "null",     "Internal Comments 2",   "CAN",            ""      }, 
            {  "3",   "0009876",  "Ellucian Consulting, Inc.",         "O",    "AP",      "333.33",   "2/4/2015",    "2/24/2015",     "2/14/2015",        "0003333",      "2/14/2015",     "Three Initiator",    "Mary Requestor",     "Just comments 3",    "null",     "Internal Comments 3",   "",               ""      }, 
            {  "4",   "0009876",  "Ellucian Consulting, Inc.",         "U",    "AP",      "222.22",   "2/5/2015",    "2/25/2015",     "2/15/2015",        "0004444",      "2/25/2015",     "Four Initiator",     "null",               "Just comments 4",    "null",     "Internal Comments 4",   "",               "41"    },
            {  "5",   "",         "Ellucian Consulting",               "U",    "AP",      "555.55",   "2/6/2015",    "2/26/2015",     "2/16/2015",        "null",         "2/26/2015",     "",                   "",                   "Just comments 5",    "MC",       "Internal Comments 5",   "",               ""      }, 
            {  "6",   "",         "Ellucian's Consulting Associates",  "P",    "AP",      "666.66",   "2/7/2015",    "2/27/2015",     "3/17/2015",        "0006666",      "2/27/2015",     "",                   "",                   "Just comments 6",    "null",     "Internal Comments 6",   "",               ""      }, 
            {  "7",   "0001111",  "null",                              "P",    "AP",    "7,777.77",   "3/1/2015",    "3/31/2015",     "3/11/2015",        "0007777",      "3/31/2015",     "Seven Initiator",    "Mary Requestor",     "Just comments 7",    "null",     "Internal Comments 7",   "",               ""      }, 
            {  "8",   "0002222",  "null",                              "P",    "AP",    "8,888.88",   "3/2/2015",    "3/22/2015",     "3/12/2015",        "0008888",      "3/22/2015",     "",                   "",                   "Just comments 8",    "MC",       "Internal Comments 8",   "",               ""      },
            {  "9",   "0005432",  "Offices Supplies Unlimited",        "P",    "AP",    "9,999.99",   "3/3/2015",    "3/23/2015",     "3/13/2015",        "0009999",      "3/23/2015",     "Nine Initiator",     "Mary Requestor",     "Just comments 9",    "null",     "Internal Comments 9",   "",               ""      }, 
            { "10",   "",         "null",                              "P",    "AP",    "1,234.56",   "3/4/2015",    "3/24/2015",     "3/14/2015",        "0010001",      "3/24/2015",     "Ten Initiator",      "Mary Requestor",     "Just comments 10",   "null",     "Internal Comments 10",  "",               ""      },
            { "11",   "0005432",  "Offices Supplies Unlimited",        "P",    "AP",      "876.54",   "3/5/2015",    "3/25/2015",     "3/15/2015",        "0010101",      "3/25/2015",     "Eleven Initiator",   "null",               "Just comments 11",   "null",     "Internal Comments 11",  "",               ""      },  
            { "31",   "0009876",  "Office Supplies",                   "O",    "AP",      "800.00",   "3/31/2015",   "4/15/2015",     "3/31/2015",        "0003131",      "3/31/2015",     "",                   "",                   "Just Comments 31",   "null",     "Internal Comments 31",  "",               ""      }, 
            { "32",   "0009876",  "Office Supplies",                   "O",    "AP",      "800.00",   "3/31/2015",   "4/15/2015",     "3/31/2015",        "0003132",      "3/31/2015",     "",                   "",                   "Just Comments 32",   "null",     "Internal Comments 32",  "",               ""      },  
            { "33",   "0009876",  "Office Supplies",                   "O",    "AP",      "800.00",   "3/31/2015",   "4/15/2015",     "3/31/2015",        "0003133",      "3/31/2015",     "",                   "",                   "Just Comments 33",   "null",     "Internal Comments 33",  "",               ""      },
            { "34",   "",         "",                                  "U",      "",            "",   "3/31/2015",            "",     "3/31/2015",        "0003134",      "3/31/2015",     "",                   "",                                   "",   "MC",                           "",  "",               ""      }
                                              };

        private string[,] approversArray = {
            //  0           1                       2               3
            //  ID          Name                    Date            REQ ID
            {   "0000001",  "Andy Kleehammer",      "1/1/2015",     "1" },
            {   "0000002",  "Gary Thorne",          "1/3/2015",     "1" },
            {   "0000003",  "Teresa Longerbeam",    "null",         "1" },

            {   "0000001",  "Andy Kleehammer",      "1/1/2015",     "9" },
            {   "0000002",  "Gary Thorne",          "1/3/2015",     "9" },
            {   "0000003",  "Teresa Longerbeam",    "null",         "9" }
        };

        private string[,] purchaseOrdersArray = {
            //  0          1              2              
            //  ID         Number         REQ ID
            {   "31",     "P0000331",     "3" },
            {   "32",     "P0000332",     "3" },
            {   "33",     "P0000333",     "3" },
            {   "99",     "P0000999",     "9" }
        };

        private string[,] lineItemsArray = {
            //   0      1         2                 3         4        5               6              7               8                      9          10         11        12
            //   ID     REQ ID    Description       Qty       Price    Unit of Issue   Vendor Part    Ext Price       Desired Date          Tax Form    Tax Code   Tax Loc   Comments
            { "1111",   "1",     "Training",        "6",   "180.00",   "Days",         "VP",         "1,080.00",       "1/20/2015",      "1099-MISC",   "NEC",     "VA",     "Comments line item 1111" },
            { "1112",   "1",     "Consulting",      "5",   "210.00",   "Hours",        "",           "1,050.00",       "null",           "",            "",          "",     "Comments line item 1112" },   

            { "2222",   "2",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",           "",            "",          "",     "Comments line item 2222" },
            { "2222",   "9",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",           "",            "",          "",     "Comments line item 2222" },
            { "2222",   "3",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",           "",            "",          "",     "Comments line item 2222" },
            { "2222",   "6",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",           "",            "",          "",     "Comments line item 2222" },
            { "2222",   "4",     "Mentoring",       "2",   "111.11",   "",             "",             "222.22",       "null",           "",            "",          "",     "Comments line item 2222" },

            { "3131",  "31",     "First line 31",  "14",   "101.55",   "box",          "VP",           "543.21",       "1/3/2015",       "1099-MISC",   "NEC",     "VA",     "Comments line item 3131"},
            { "3132",  "31",     "Second line 31", "14",   "208.09",   "box",          "VP",           "543.21",       "1/3/2015",       "1099-MISC",   "NEC",     "VA",     "Comments line item 3132"},

            { "3231",  "32",     "First line 32",  "14",   "101.55",   "box",          "VP",           "543.21",       "1/3/2015",       "1099-MISC",   "NEC",     "VA",     "Comments line item 3231"},
            { "3232",  "32",     "Second line 32", "14",   "208.09",   "box",          "VP",           "543.21",       "1/3/2015",       "1099-MISC",   "NEC",     "VA",     "Comments line item 3232"},

            { "3331",  "33",     "First line 33",  "14",   "101.55",   "box",          "VP",           "543.21",       "1/3/2015",       "1099-MISC",   "NEC",     "VA",     "Comments line item 3331"},
            { "3332",  "33",     "Second line 33", "14",   "208.09",   "box",          "VP",           "543.21",       "1/3/2015",       "1099-MISC",   "NEC",     "VA",     "Comments line item 3332"},

            { "3341",  "34",     "Firt line 34",    "2",         "",      "",            "",                 "",           "null",                "",      "",       "",     ""},
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
            {   "VA",   "60.75",    "1111" }, // Requisition 1
            {   "MD",   "39.25",    "1111" }, // Requisition 1

            {   "VA",   "20.00",    "1112" }, // Requisition 2
            {   "MD",   "80.00",    "1112" }, // Requisition 2

            {   "BC",   "15.00",    "2222" }, // Requisition 3
            {   "NV",    "5.00",    "2222" }, // Requisition 3
            {   "AL",   "10.00",    "2222" }, // Requisition 3

            
            {   "VA",   "60.00",    "3131" }, // Requisition 31
            {   "MD",   "40.00",    "3131" }, // Requisition 31
            {   "VA",   "25.00",    "3132" }, // Requisition 31
            {   "MD",   "75.00",    "3132" }, // Requisition 31

            {   "VA",   "60.00",    "3231" }, // Requisition 32
            {   "MD",   "40.00",    "3231" }, // Requisition 32
            {   "VA",   "25.00",    "3232" }, // Requisition 32
            {   "MD",   "75.00",    "3232" }, // Requisition 32

            {   "VA",   "60.00",    "3331" }, // Requisition 33
            {   "MD",   "40.00",    "3331" }, // Requisition 33
            {   "VA",   "25.00",    "3332" }, // Requisition 33
            {   "MD",   "75.00",    "3332" }, // Requisition 33
        };

        #endregion

        public TestRequisitionRepository()
        {
            Populate();
            PopulateRequisitionsSummary();

            // Add a domain entity to the requisitions list that has one GL account masked.
            var requisition = new Requisition("999", "000999", "Susty Corporation", RequisitionStatus.InProgress, DateTime.Now, DateTime.Now);

            var lineItem = new LineItem("999", "Line item #1", 10m, 100m, 125m);
            lineItem.AddGlDistribution(new LineItemGlDistribution("11_10_00_01_20601_51001", 10m, 100m)
                { GlAccountDescription = "GL description #1", Masked = false, ProjectId = "29", ProjectLineItemCode = "AJK1", ProjectLineItemId = "12", ProjectNumber = "AJK-123" });
            lineItem.AddGlDistribution(new LineItemGlDistribution("11_10_00_01_20601_51002", 150m, 1050m)
                { GlAccountDescription = "GL description #2", Masked = true, ProjectId = "291", ProjectLineItemCode = "AJK21", ProjectLineItemId = "112", ProjectNumber = "AJK-1263" });
            requisition.AddLineItem(lineItem);

            requisitions.Add(requisition);
        }

        public async Task<Requisition> GetRequisitionAsync(string requisitionId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts)
        {
            var requisition = await Task.Run(() => requisitions.FirstOrDefault(x => x.Id == requisitionId));
            return requisition;
        }

        private void Populate()
        {
            # region Populate Requisitions

            // Loop through the requisition array and create requisition domain entities
            string requisitionId,
                    vendorId,
                    vendorName,
                    apType,
                    number,
                    initiatorName,
                    requestorName,
                    shipToCode,
                    comments,
                    internalComments,
                    currencyCode,
                    blanketPurchaseOrder;

            RequisitionStatus status;

            decimal requisitionAmount;

            DateTime date,
                     statusDate,
                     maintenanceDate;

            DateTime? desiredDate;

            for (var i = 0; i < requisitionsArray.GetLength(0); i++)
            {
                requisitionId = requisitionsArray[i, 0];
                vendorId = requisitionsArray[i, 1];
                vendorName = requisitionsArray[i, 2];

                switch (requisitionsArray[i, 3])
                {
                    case "U":
                        status = RequisitionStatus.InProgress;
                        break;
                    case "N":
                        status = RequisitionStatus.NotApproved;
                        break;
                    case "O":
                        status = RequisitionStatus.Outstanding;
                        break;
                    case "P":
                        status = RequisitionStatus.PoCreated;
                        break;
                    default:
                        throw new Exception("Invalid status specified in TestRequisitionRepository.");
                }

                apType = requisitionsArray[i, 4];
                requisitionAmount = !string.IsNullOrEmpty(requisitionsArray[i, 5]) ? Convert.ToDecimal(requisitionsArray[i, 5]) : 0;
                date = Convert.ToDateTime(requisitionsArray[i, 6]);
                desiredDate = !string.IsNullOrEmpty(requisitionsArray[i, 7]) ? Convert.ToDateTime(requisitionsArray[i, 7]): (DateTime?)null;
                maintenanceDate = Convert.ToDateTime(requisitionsArray[i, 8]);
                number = requisitionsArray[i, 9];
                statusDate = Convert.ToDateTime(requisitionsArray[i, 10]);
                initiatorName = requisitionsArray[i, 11];
                requestorName = requisitionsArray[i, 12];
                comments = requisitionsArray[i, 13];
                shipToCode = requisitionsArray[i, 14];
                internalComments = requisitionsArray[i, 15];
                currencyCode = requisitionsArray[i, 16];
                blanketPurchaseOrder = requisitionsArray[i, 17];

                var requisition = new Requisition(requisitionId, number, vendorName, status, statusDate, date);

                requisition.VendorId = vendorId;
                requisition.ApType = apType;
                requisition.Amount = requisitionAmount;
                requisition.BlanketPurchaseOrder = blanketPurchaseOrder;
                requisition.DesiredDate = desiredDate;
                requisition.MaintenanceDate = maintenanceDate;
                requisition.InitiatorName = initiatorName;
                requisition.RequestorName = requestorName;
                requisition.ShipToCode = shipToCode;
                requisition.Comments = comments;
                requisition.InternalComments = internalComments;
                requisition.CurrencyCode = currencyCode;

                requisitions.Add(requisition);
            }
            #endregion

            #region Populate approvers

            string approverId,
                approvalName,
                approvalRequisitionId;
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
                approvalRequisitionId = approversArray[i, 3];
                var approverDomainEntity = new Approver(approverId);
                approverDomainEntity.SetApprovalName(approvalName);
                approverDomainEntity.ApprovalDate = approvalDate;

                foreach (var requisition in requisitions)
                {
                    if (requisition.Id == approvalRequisitionId)
                    {
                        requisition.AddApprover(approverDomainEntity);
                    }
                }
            }
            #endregion

            #region Populate Purchase Orders

            string purchaseOrderId,
                purchaseOrderNumber,
                purchaseOrderRequisitionId;

            for (var i = 0; i < purchaseOrdersArray.GetLength(0); i++)
            {
                purchaseOrderId = purchaseOrdersArray[i, 0];
                purchaseOrderNumber = purchaseOrdersArray[i, 1];
                purchaseOrderRequisitionId = purchaseOrdersArray[i, 2];

                foreach (var requisition in requisitions)
                {
                    if (requisition.Id == purchaseOrderRequisitionId)
                    {
                        requisition.AddPurchaseOrder(purchaseOrderId);
                    }
                }
            }
            #endregion

            #region Populate line items

            string lineItemId,
                lineItemRequisitionId,
                description,
                unitOfIssue,
                vendorPart,
                taxForm,
                taxFormCode,
                taxFormLocation,
                lineItemComments;

            DateTime? itemDesiredDate;

            decimal quantity,
                price,
                extendedPrice;

            for (var i = 0; i < lineItemsArray.GetLength(0); i++)
            {
                lineItemId = lineItemsArray[i, 0];
                lineItemRequisitionId = lineItemsArray[i, 1];
                description = lineItemsArray[i, 2];
                quantity = Convert.ToDecimal(lineItemsArray[i, 3]);
                price = !string.IsNullOrEmpty(lineItemsArray[i, 4]) ? Convert.ToDecimal(lineItemsArray[i, 4]):0;

                unitOfIssue = lineItemsArray[i, 5];
                vendorPart = lineItemsArray[i, 6];
                extendedPrice = !string.IsNullOrEmpty(lineItemsArray[i, 7]) ? Convert.ToDecimal(lineItemsArray[i, 7]) : 0;

                if (lineItemsArray[i, 8] == "null")
                {
                    itemDesiredDate = null;
                }
                else
                {
                    itemDesiredDate = Convert.ToDateTime(lineItemsArray[i, 8]);
                }

                taxForm = lineItemsArray[i, 9];
                taxFormCode = lineItemsArray[i, 10];
                taxFormLocation = lineItemsArray[i, 11];
                lineItemComments = lineItemsArray[i, 12];

                var lineItem = new LineItem(lineItemId, description, quantity, price, extendedPrice);

                lineItem.UnitOfIssue = unitOfIssue;
                lineItem.VendorPart = vendorPart;
                lineItem.DesiredDate = itemDesiredDate;
                lineItem.TaxForm = taxForm;
                lineItem.TaxFormCode = taxFormCode;
                lineItem.TaxFormLocation = taxFormLocation;
                lineItem.Comments = lineItemComments;

                foreach (var requisition in requisitions)
                {
                    if (requisition.Id == lineItemRequisitionId)
                    {
                        requisition.AddLineItem(lineItem);
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

                foreach (var requisition in requisitions)
                {
                    foreach (var lineItem in requisition.LineItems)
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

                foreach (var requisition in requisitions)
                {
                    foreach (var lineItem in requisition.LineItems)
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

        private void PopulateRequisitionsSummary()
        {
            # region Populate RequisitionsSummary

            // Loop through the requisition array and create requisition domain entities
            string requisitionId,
                    vendorId,
                    vendorName,
                    number,
                    initiatorName,
                    requestorName;

            RequisitionStatus status;

            decimal requisitionAmount;

            DateTime date,
                     statusDate,
                     maintenanceDate;


            for (var i = 0; i < requisitionsArray.GetLength(0); i++)
            {
                requisitionId = requisitionsArray[i, 0];
                vendorId = requisitionsArray[i, 1];
                vendorName = requisitionsArray[i, 2];

                switch (requisitionsArray[i, 3])
                {
                    case "U":
                        status = RequisitionStatus.InProgress;
                        break;
                    case "N":
                        status = RequisitionStatus.NotApproved;
                        break;
                    case "O":
                        status = RequisitionStatus.Outstanding;
                        break;
                    case "P":
                        status = RequisitionStatus.PoCreated;
                        break;
                    default:
                        throw new Exception("Invalid status specified in TestRequisitionRepository.");
                }

                requisitionAmount = !string.IsNullOrEmpty(requisitionsArray[i, 5]) ? Convert.ToDecimal(Convert.ToDecimal(requisitionsArray[i, 5])) : 0;
                date = Convert.ToDateTime(requisitionsArray[i, 6]);
                maintenanceDate = Convert.ToDateTime(requisitionsArray[i, 8]);
                number = requisitionsArray[i, 9];                
                initiatorName = requisitionsArray[i, 11];
                requestorName = requisitionsArray[i, 12];

                var requisitionSummary = new RequisitionSummary(requisitionId, number, vendorName, date);

                requisitionSummary.Status = status;
                requisitionSummary.VendorId = vendorId;
                requisitionSummary.Amount = requisitionAmount;
                requisitionSummary.MaintenanceDate = maintenanceDate;
                requisitionSummary.InitiatorName = initiatorName;
                requisitionSummary.RequestorName = requestorName;

                requisitionsSummaryList.Add(requisitionSummary);
            }
            #endregion
            
            #region Populate Purchase Orders

            string purchaseOrderId,
                purchaseOrderNumber,
                purchaseOrderRequisitionId;

            for (var i = 0; i < purchaseOrdersArray.GetLength(0); i++)
            {
                purchaseOrderId = purchaseOrdersArray[i, 0];
                purchaseOrderNumber = purchaseOrdersArray[i, 1];
                purchaseOrderRequisitionId = purchaseOrdersArray[i, 2];

                foreach (var requisitionSummary in requisitionsSummaryList)
                {
                    if (requisitionSummary.Id == purchaseOrderRequisitionId)
                    {
                        var po = new PurchaseOrderSummary(purchaseOrderId, purchaseOrderNumber, requisitionSummary.VendorName, requisitionSummary.Date);
                        requisitionSummary.AddPurchaseOrder(po);
                    }
                }
            }
            #endregion

        }

        public Task<Tuple<IEnumerable<Requisition>, int>> GetRequisitionsAsync(int offset, int limit)
        {
            throw new NotImplementedException();
        }

        public Task<Requisition> GetRequisitionsByGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<Requisition> DeleteRequisitionAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<Requisition> UpdateRequisitionAsync(Requisition requisition)
        {
            throw new NotImplementedException();
        }

        public Task<Requisition> CreateRequisitionAsync(Requisition requisition)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRequisitionsIdFromGuidAsync(string id)
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

        public async Task<IEnumerable<RequisitionSummary>> GetRequisitionsSummaryByPersonIdAsync(string personId)
        {
            return await Task.Run(() => requisitionsSummaryList);
        }
        

        public Task<RequisitionCreateUpdateResponse> CreateRequisitionsAsync(RequisitionCreateUpdateRequest createUpdateRequest)
        {
            throw new NotImplementedException();
        }

        public Task<RequisitionCreateUpdateResponse> UpdateRequisitionsAsync(RequisitionCreateUpdateRequest createUpdateRequest, Requisition originalRequisition)
        {
            throw new NotImplementedException();
        }
    }
}
