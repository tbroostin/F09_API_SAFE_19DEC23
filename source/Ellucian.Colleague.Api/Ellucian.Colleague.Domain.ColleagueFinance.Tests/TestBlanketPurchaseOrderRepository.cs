// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests
{
    /// <summary>
    /// This class represents a set of blanket purchase orders
    /// </summary>
    public class TestBlanketPurchaseOrderRepository : IBlanketPurchaseOrderRepository
    {
        private List<BlanketPurchaseOrder> blanketPurchaseOrders = new List<BlanketPurchaseOrder>();

        #region Define all data for a blanket purchase order

        private string[,] blanketPurchaseOrdersArray = {
            //  0       1               2           3                              4        5               6                7           8          9                   10                   11                  12                         13                  14                       15             16
            //  ID      BPO Number      Vendor ID   Vendor Name                    Status   Status Date     AP Type          Date        Amount     Maintenance Date    Expiration Date      Initiator           Description                Comments            Internal Comments        Currency Code  GUID
            {   "1",    "B0001111",     "0009876",  "Ellucian Consulting, Inc.",    "O",    "03/31/2015",    "AP",    "03/31/2015",   "1,111.11",   "04/01/2015",       "04/30/2015",        "One Initiator",    "Description for BPO1",    "Just comments 1",  "Internal Comments 1",   "",            "b18288c0-cca7-45b1-a310-39e376db0c3d"},
            {   "2",    "B0002222",          null,  "Ellucian Consulting, Inc.",    "O",    "04/10/2015",    "AP",    "04/05/2015",   "2,222.22",   "04/13/2015",       null,                "Two Initiator",    "Description for BPO2",    "Just comments 2",  "Internal Comments 2",   "CAN",         "5001beaf-a1b0-448e-88cd-a895526e3879"},
            {   "3",    "B0003333",     "0009876",  "Ellucian Consulting, Inc.",    "U",    "05/12/2015",    "AP",    "05/12/2015",   "3,333.33",   "05/15/2015",       null,                "Three Initiator",  "Description for BPO3",    "Just comments 3",  "Internal Comments 3",   "",            "c03bc9df-fbbd-4716-ab16-f978c3816dcf"},
            {   "4",    "B0004441",     "0009876",  "Ellucian Consulting, Inc.",    "O",    "05/22/2015",    "AP",    "05/21/2015",   "4,444.44",   "05/22/2015",       null,                "Four Initiator",   "Description for BPO4",    "Just comments 4",  "Internal Comments 4",   "",            "82d06e63-2fee-486a-8136-4faa01988752"},
            {   "5",    "B0005551",     "0009876",  "Ellucian Consulting, Inc.",    "C",    "06/22/2015",    "AP",    "06/21/2015",         null,   "06/22/2015",       null,                            null,                     null,                 null,                   null,   "",            "f49d6308-363c-4ca3-97e3-bb2753e2b726"},
            {   "6",    "B0006661",     "0009876",  "Ellucian Consulting, Inc.",    "N",    "06/22/2015",    "AP",    "06/21/2015",         null,   "06/22/2015",       null,                            null,                     null,                 null,                   null,   "",            "599bf3f5-6565-4063-823a-6fe5200c1555"},
            {   "7",    "B0007771",     "0009876",  "Ellucian Consulting, Inc.",    "V",    "06/22/2015",    "AP",    "06/21/2015",         null,   "06/22/2015",       null,                            null,                     null,                 null,                   null,   "",            "bf6dc54f-df34-4e25-a329-59a235a1197d"},

            {  "31",    "B0007771",     "0009876",  "Ellucian Consulting, Inc.",    "O",    "06/22/2015",    "AP",    "06/21/2015",         null,   "06/22/2015",       null,                            null,                     null,                 null,                   null,   "",            "5509e3e9-cb00-4d9a-b830-b9cae06894f1"},
            {  "32",    "B0007771",     "0009876",  "Ellucian Consulting, Inc.",    "O",    "06/22/2015",    "AP",    "06/21/2015",         null,   "06/22/2015",       null,                            null,                     null,                 null,                   null,   "",            "f2964740-82a3-408b-8147-3948793de22b"}
                                                       };

        private string[,] approversArray = {
            //  0           1                       2               3
            //  ID          Name                    Date            Document ID
            {   "0000001",  "Andy Kleehammer",      "1/1/2015",     "1" },
            {   "0000002",  "Gary Thorne",          "1/3/2015",     "1" },
            {   "0000003",  "Teresa Longerbeam",    "null",         "1" }
        };

        private string[,] requisitionsArray = {
            //  0          1             2                3
            //  ID         Number        BPO ID           GUID
            {   "11",     "0000111",     "1" ,            "f49d6308-363c-4ca3-97e3-bb2753e2b726"},
            {   "12",     "0000112",     "1" ,            "599bf3f5-6565-4063-823a-6fe5200c1555"},
            {   "13",     "0000113",     "1" ,            "bf6dc54f-df34-4e25-a329-59a235a1197d"},
        };

        private string[,] vouchersArray = {
            //  0             1              
            //  Number        BPO ID
            {   "V0009991",   "1" },
            {   "V0009992",   "1" },
            {   "V0009993",   "1" },

            {   "V0009994",   "4" },
            {   "V0009995",   "4" }
        };

        private string[,] glDistributionArray = {
            //  0                              1                2               3                  4                     5                                  6                 7     8
            //  GL Account                     Description      Project ID      Project Number     Prj Line Item ID      Project Item Code     Encumbered Amt      Expensed Amt     BPO ID
            {   "11_10_00_01_20601_51000",  "Object 51001",          "100",          "AJK-100",                "50",                "AJK1",       "18,333.33",           "0.00",    "1" },
            {   "11_10_00_01_20601_51001",  "Object 51001",           null,               null,                null,                  null,       "18,333.33",           "0.00",    "1" },
            {   "11_10_00_01_20601_52001",  "Object 52001",          "200",          "AJK-200",                "60",                "AJK2",       "18,888.89",           "0.00",    "1" },

            {   "11_10_00_01_20601_52001",  "Object 52001",          "200",          "AJK-200",                "60",                "AJK2",       "18,888.89",           "0.00",    "3" },

            {   "11_10_00_01_20601_52001",  "Object 52001",          "200",          "AJK-200",                "60",                "AJK2",       "18,888.89",           "0.00",    "4" },

            {   "11_10_00_01_20601_52001",  "Object 52001",          "200",          "AJK-200",                "60",                "AJK2",       "18,888.89",           "0.00",    "5" },

            {   "11_10_00_01_20601_52001",  "Object 52001",          "200",          "AJK-200",                "60",                "AJK2",       "18,888.89",           "0.00",    "6" },

            {   "11_10_00_01_20601_52001",  "Object 52001",          "200",          "AJK-200",                "60",                "AJK2",       "18,888.89",           "0.00",    "7" },

            {   "11_10_00_01_20601_51001",  "Object 51001",          "200",          "AJK-200",                "60",                "AJK2",       "18,888.89",       "2,222.22",   "31",},
            {   "11_10_00_01_33333_51001",  "Object 51001",          "200",          "AJK-200",                "60",                "AJK2",       "17,777.89",       "3,333.33",   "31",},
            {   "11_10_00_01_20601_52001",  "Object 52001",          "200",          "AJK-200",                "60",                "AJK2",       "18,888.89",       "2,222.22",   "32",}
        };

        Dictionary<string, string> _thosExtendedDataDictionary = new Dictionary<string, string>();
        public Dictionary<string, string> EthosExtendedDataDictionary
        {
            get
            {
                return _thosExtendedDataDictionary;
            }

            set
            {
                _thosExtendedDataDictionary = value;
            }
        }

        #endregion

        public TestBlanketPurchaseOrderRepository()
        {
            Populate();
        }

        public async Task<BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string blanketPurchaseOrderId, string personId, GlAccessLevel glAccessLevel, IEnumerable<string> expenseAccounts)
        {
            var blanketPurchaseOrder = await Task.Run(() => blanketPurchaseOrders.FirstOrDefault(x => x.Id == blanketPurchaseOrderId));
            return blanketPurchaseOrder;
        }

        private void Populate()
        {
            #region Populate blanket purchase orders

            // Loop through the blanket purchase orders array and create blanket purchase orders domain entities.
            string  bpoId,
                    guid,
                    vendorId,
                    vendorName,
                    apType,
                    description,
                    number,
                    initiatorName,
                    comments,
                    internalComments,
                    currencyCode;

            BlanketPurchaseOrderStatus? status;

            decimal bpoAmount;

            DateTime date,
                     statusDate,
                     maintenanceDate;

            DateTime? expirationDate;

            for (var i = 0; i < blanketPurchaseOrdersArray.GetLength(0); i++)
            {
                bpoId = blanketPurchaseOrdersArray[i, 0];
                guid = blanketPurchaseOrdersArray[i, 16];
                number = blanketPurchaseOrdersArray[i, 1];
                vendorId = blanketPurchaseOrdersArray[i, 2];
                vendorName = blanketPurchaseOrdersArray[i, 3];
                
                switch (blanketPurchaseOrdersArray[i, 4])
                {
                    case "C":
                        status = BlanketPurchaseOrderStatus.Closed;
                        break;
                    case "U":
                        status = BlanketPurchaseOrderStatus.InProgress;
                        break;
                    case "N":
                        status = BlanketPurchaseOrderStatus.NotApproved;
                        break;
                    case "O":
                        status = BlanketPurchaseOrderStatus.Outstanding;
                        break;
                    case "V":
                        status = BlanketPurchaseOrderStatus.Voided;
                        break;
                    default:
                        throw new ColleagueWebApiException("Invalid status specified in TestPurchaseOrderRepository.");
                }

                statusDate = Convert.ToDateTime(blanketPurchaseOrdersArray[i, 5]);
                apType = blanketPurchaseOrdersArray[i, 6];
                date = Convert.ToDateTime(blanketPurchaseOrdersArray[i, 7]);
                bpoAmount = Convert.ToDecimal(blanketPurchaseOrdersArray[i, 8]);
                maintenanceDate = Convert.ToDateTime(blanketPurchaseOrdersArray[i, 9]);
                expirationDate = Convert.ToDateTime(blanketPurchaseOrdersArray[i, 10]);
                initiatorName = blanketPurchaseOrdersArray[i, 11];
                description = blanketPurchaseOrdersArray[i, 12];
                comments = blanketPurchaseOrdersArray[i, 13];
                internalComments = blanketPurchaseOrdersArray[i, 14];
                currencyCode = blanketPurchaseOrdersArray[i, 15];

                var blanketPurchaseOrder = new BlanketPurchaseOrder(bpoId, guid, number,  vendorName, status, statusDate, date);
                blanketPurchaseOrder.ApType = apType;
                blanketPurchaseOrder.Amount = bpoAmount;
                blanketPurchaseOrder.MaintenanceDate = maintenanceDate;
                blanketPurchaseOrder.ExpirationDate = expirationDate;
                blanketPurchaseOrder.VendorId = vendorId;
                blanketPurchaseOrder.DefaultInitiator = bpoId;
                blanketPurchaseOrder.InitiatorName = initiatorName;
                blanketPurchaseOrder.Description = description;
                blanketPurchaseOrder.Comments = comments;
                blanketPurchaseOrder.InternalComments = internalComments;
                blanketPurchaseOrder.CurrencyCode = currencyCode;

                // Vendor won't process without at least an address.
                blanketPurchaseOrder.MiscAddress = new List<string>() { "234 Main St." };
                blanketPurchaseOrder.MiscCity = "Anytown";
                blanketPurchaseOrder.MiscCountry = "USA";
                

                blanketPurchaseOrders.Add(blanketPurchaseOrder);
            }
            #endregion

            #region Populate approvers

            string approverId,
                approvalName,
                approvalBpoId;
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
                approvalBpoId = approversArray[i, 3];
                var approverDomainEntity = new Approver(approverId);
                approverDomainEntity.SetApprovalName(approvalName);
                approverDomainEntity.ApprovalDate = approvalDate;

                foreach (var bpo in blanketPurchaseOrders)
                {
                    if (bpo.Id == approvalBpoId)
                    {
                        bpo.AddApprover(approverDomainEntity);
                    }
                }
            }
            #endregion

            #region Populate Requisitions

            string requisitionId,
                requisitionNumber,
                requisitionBpoId;

            for (var i = 0; i < requisitionsArray.GetLength(0); i++)
            {
                requisitionId = requisitionsArray[i, 0];
                requisitionNumber = requisitionsArray[i, 1];
                requisitionBpoId = requisitionsArray[i, 2];

                foreach (var bpo in blanketPurchaseOrders)
                {
                    if (bpo.Id == requisitionBpoId)
                    {
                        bpo.AddRequisition(requisitionId);
                    }
                }
            }
            #endregion

            #region Populate Vouchers

            string voucherNumber,
                   voucherBpoId;

            for (var i = 0; i < vouchersArray.GetLength(0); i++)
            {
                voucherNumber = vouchersArray[i, 0];
                voucherBpoId = vouchersArray[i, 1];

                foreach (var bpo in blanketPurchaseOrders)
                {
                    if (bpo.Id == voucherBpoId)
                    {
                        bpo.AddVoucher(voucherNumber);
                    }
                }
            }
            #endregion

            #region Populate GL Distributions

            string glAccount,
                glDescription,
                projectId,
                projectNumber,
                projectLineItemId,
                projectItemCode,
                glDistBpoId;

            decimal encumberedAmount,
                    expensedAmount;

            for (var i = 0; i < glDistributionArray.GetLength(0); i++)
            {
                glAccount = glDistributionArray[i, 0];
                glDescription = glDistributionArray[i, 1];
                projectId = glDistributionArray[i, 2];
                projectNumber = glDistributionArray[i, 3];
                projectLineItemId = glDistributionArray[i, 4];
                projectItemCode = glDistributionArray[i, 5];
                encumberedAmount = Convert.ToDecimal(glDistributionArray[i, 6]);
                expensedAmount = Convert.ToDecimal(glDistributionArray[i, 7]);
                glDistBpoId = glDistributionArray[i, 8];

                var bpoGlDistribution = new BlanketPurchaseOrderGlDistribution(glAccount, encumberedAmount);
                bpoGlDistribution.GlAccountDescription = glDescription;
                bpoGlDistribution.ExpensedAmount = expensedAmount;
                bpoGlDistribution.ProjectId = projectId;
                bpoGlDistribution.ProjectLineItemCode = projectItemCode;
                bpoGlDistribution.ProjectLineItemId = projectLineItemId;
                bpoGlDistribution.ProjectNumber = projectNumber;


                foreach (var bpo in blanketPurchaseOrders)
                {
                    if (bpo.Id == glDistBpoId)
                    {
                        bpo.AddGlDistribution(bpoGlDistribution);
                    }
                }

            }
            #endregion
        }
        
        public async Task<Tuple<IEnumerable<BlanketPurchaseOrder>, int>> GetBlanketPurchaseOrdersAsync(int offset, int limit, string number)
        {
            var blanketPurchaseOrdersCollection = new List<BlanketPurchaseOrder>();
            int i = 0;
            foreach (var bpo in blanketPurchaseOrders)
            {
                if (i < limit && !string.IsNullOrEmpty(bpo.VendorId) && bpo.Status != BlanketPurchaseOrderStatus.InProgress)
                {
                    if (string.IsNullOrEmpty(number) || bpo.Number == number)
                    {
                        blanketPurchaseOrdersCollection.Add(bpo);
                    }
                }
                i++;
            }
            return await Task.Run(() => new Tuple<IEnumerable<BlanketPurchaseOrder>, int>(blanketPurchaseOrdersCollection, blanketPurchaseOrdersCollection.Count()));
        }
        
        public async Task<BlanketPurchaseOrder> GetBlanketPurchaseOrdersByGuidAsync(string guid)
        {
            var blanketPurchaseOrder = await Task.Run(() => blanketPurchaseOrders.FirstOrDefault(x => x.Guid == guid));
            return blanketPurchaseOrder;
        }

        public async Task<IDictionary<string, string>> GetProjectReferenceIds(string[] projectIds)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();

            foreach (var bpo in blanketPurchaseOrders)
            {
                foreach (var gld in bpo.GlDistributions)
                {
                    if (projectIds.Contains(gld.ProjectId))
                    {
                        if (!dict.ContainsKey(gld.ProjectId))
                        {
                            dict.Add(gld.ProjectId, gld.ProjectNumber);
                        }
                    }
                }
            }
            return await Task.Run(() => dict);
        }

        public Task<string> GetRequisitionsIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetGuidFromIdAsync(string id, string entity)
        {
            for (var i = 0; i < requisitionsArray.GetLength(0); i++)
            {
                if (requisitionsArray[i,0] == id)
                {
                    var returnval = requisitionsArray[i, 3];
                    return await Task.Run(()=>returnval);
                }
            }
            return null;
        }

        public Task<string> GetBlanketPurchaseOrdersIdFromGuidAsync(string guid)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, string>> GetProjectIdsFromReferenceNo(string[] projectRefNo)
        {
            throw new NotImplementedException();
        }

        public Tuple<List<string>, List<string>> GetEthosExtendedDataLists()
        {
            throw new NotImplementedException();
        }

        public Task<BlanketPurchaseOrder> UpdateBlanketPurchaseOrdersAsync(BlanketPurchaseOrder purchaseOrdersEntity)
        {
            throw new NotImplementedException();
        }

        public Task<BlanketPurchaseOrder> CreateBlanketPurchaseOrdersAsync(BlanketPurchaseOrder purchaseOrdersEntity)
        {
            throw new NotImplementedException();
        }
    }
}