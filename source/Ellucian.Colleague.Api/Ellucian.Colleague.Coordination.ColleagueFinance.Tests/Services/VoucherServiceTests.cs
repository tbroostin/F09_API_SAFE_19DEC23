// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    /// <summary>
    /// This class tests that the service returns a specified voucher.
    /// </summary>
    [TestClass]
    public class VoucherServiceTests : GeneralLedgerCurrentUser
    {
        #region Initialize and Cleanup
        private VoucherService service;
        private VoucherService service2;
        private VoucherService serviceForNoPermission;
        private VoucherService voucherApprovalRolesService;

        private TestVoucherRepository testVoucherRepository;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository;
        private TestGeneralLedgerAccountRepository testGeneralLedgerAccountRepository;
        private Mock<IVoucherRepository> mockVoucherRepository;

        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<IStaffRepository> staffRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewVoucher;
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(229, "VOUCHER.VIEWER");

        private Domain.Entities.Permission permissionCreateUpdateVoucher;
        protected Domain.Entities.Role glUserRoleCreateUpdatePermissions = new Domain.Entities.Role(230, "VOUCHER.CREATER");

        private GeneralLedgerCurrentUser.UserFactory currentUserFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();

        private Mock<IGeneralLedgerConfigurationRepository> mockGlConfigurationRepository;
        private Mock<IApprovalConfigurationRepository> mockApprovalConfigurationRepositoryFalse;
        private Mock<IApprovalConfigurationRepository> mockApprovalConfigurationRepositoryTrue;
        private Mock<IGeneralLedgerUserRepository> mockGeneralLedgerUserRepository;
        private Mock<IGeneralLedgerAccountRepository> mockGeneralLedgerAccountRepository;
        private Mock<IProcurementsUtilityService> mockProcurementsUtilityService;
        private int versionNumber;
        private VendorsVoucherSearchResult vendorEntities;
        private VoucherCreateUpdateResponse voucherCreateUpdateResponse;
        private Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest voucherCreateUpdateRequest;
        private VoucherCreateUpdateRequest voucherCreateUpdateRequestEntity;

        private Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria filterCriteria;
        private ProcurementDocumentFilterCriteria filterCriteriaDomainEntity;

        [TestInitialize]
        public void Initialize()
        {
            roleRepositoryMock = new Mock<IRoleRepository>();

            // Initialize the mock voucher repository
            mockVoucherRepository = new Mock<IVoucherRepository>();
            mockGlConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            mockApprovalConfigurationRepositoryFalse = new Mock<IApprovalConfigurationRepository>();
            ApprovalConfiguration approvalConfigurationFalse = new ApprovalConfiguration()
            {
                VouchersUseApprovalRoles = false
            };
            mockApprovalConfigurationRepositoryFalse.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult(approvalConfigurationFalse));
            mockApprovalConfigurationRepositoryTrue = new Mock<IApprovalConfigurationRepository>();
            ApprovalConfiguration approvalConfigurationTrue = new ApprovalConfiguration()
            {
                VouchersUseApprovalRoles = true
            };
            mockApprovalConfigurationRepositoryTrue.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult(approvalConfigurationTrue));

            mockGeneralLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();
            mockGeneralLedgerAccountRepository = new Mock<IGeneralLedgerAccountRepository>();
            mockProcurementsUtilityService = new Mock<IProcurementsUtilityService>();

            Dictionary<string, string> descDictionary = new Dictionary<string, string>();
            List<string> glAccountIds = new List<string>() { "11-10-00-01-20601-51000", "11-10-00-01-20601-51001", "11-10-00-01-20601-52001" };

            for (int i = 0; i < glAccountIds.Count(); i++)
            {
                if (!descDictionary.ContainsKey(glAccountIds.ElementAt(i)))
                {
                    descDictionary.Add(glAccountIds.ElementAt(i), "Description " + i.ToString());
                }
            }

            mockGeneralLedgerAccountRepository.Setup(x => x.GetGlAccountDescriptionsAsync(It.IsAny<List<string>>(), It.IsAny<GeneralLedgerAccountStructure>())).Returns(() =>
            {
                return Task.FromResult(descDictionary);
            });


            // Create permission domain entities for viewing the voucher.
            permissionViewVoucher = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewVoucher);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionViewVoucher);
            // Create permission domain entities for creating/update the voucher.
            permissionCreateUpdateVoucher = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.CreateUpdateVoucher);
            glUserRoleCreateUpdatePermissions.AddPermission(permissionCreateUpdateVoucher);
            // build all service objects to use in testing
            staffRepositoryMock = new Mock<IStaffRepository>();

            // Build all service objects to use each of the user factories built above
            BuildValidVoucherService();
            versionNumber = 2;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Reset all of the services and repository variables.
            service = null;
            service2 = null;
            serviceForNoPermission = null;
            voucherApprovalRolesService = null;

            testVoucherRepository = null;
            testGeneralLedgerConfigurationRepository = null;
            testGeneralLedgerUserRepository = null;
            testGeneralLedgerAccountRepository = null;
            mockVoucherRepository = null;
            mockGlConfigurationRepository = null;
            mockGeneralLedgerUserRepository = null;
            mockApprovalConfigurationRepositoryFalse = null;
            mockApprovalConfigurationRepositoryTrue = null;

            roleRepositoryMock = null;
            roleRepository = null;
            glUserRoleViewPermissions = null;
            glUserRoleCreateUpdatePermissions = null;
            vendorEntities = null;
        }
        #endregion

        #region GetVoucherAsyncTests with a view permission

        [TestMethod]
        public async Task GetVoucherAsync_FullAccess()
        {
            // Get a specified voucher
            var voucherId = "1";
            var personId = "0000005";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);


            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.VoucherId, voucherDomainEntity.Id);
            Assert.AreEqual(voucherDto.Amount, voucherDomainEntity.Amount);
            Assert.AreEqual(voucherDto.ApType, voucherDomainEntity.ApType);
            Assert.AreEqual(voucherDto.BlanketPurchaseOrderId, voucherDomainEntity.BlanketPurchaseOrderId);
            Assert.AreEqual(voucherDto.CheckDate, voucherDomainEntity.CheckDate);
            Assert.AreEqual(voucherDto.CheckNumber, voucherDomainEntity.CheckNumber);
            Assert.AreEqual(voucherDto.Comments, voucherDomainEntity.Comments);
            Assert.AreEqual(voucherDto.CurrencyCode, voucherDomainEntity.CurrencyCode);
            Assert.AreEqual(voucherDto.Date, voucherDomainEntity.Date);
            Assert.AreEqual(voucherDto.DueDate, voucherDomainEntity.DueDate);
            Assert.AreEqual(voucherDto.InvoiceDate, voucherDomainEntity.InvoiceDate);
            Assert.AreEqual(voucherDto.InvoiceNumber, voucherDomainEntity.InvoiceNumber);
            Assert.AreEqual(voucherDto.MaintenanceDate, voucherDomainEntity.MaintenanceDate);
            Assert.AreEqual(voucherDto.PurchaseOrderId, voucherDomainEntity.PurchaseOrderId);
            Assert.AreEqual(voucherDto.RecurringVoucherId, voucherDomainEntity.RecurringVoucherId);
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
            Assert.AreEqual(voucherDto.VendorId, voucherDomainEntity.VendorId);
            Assert.AreEqual(voucherDto.VendorName, voucherDomainEntity.VendorName);

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < voucherDto.Approvers.Count(); i++)
            {
                var approverDto = voucherDto.Approvers[i];
                var approverDomain = voucherDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the line item DTOs matches the domain entity
            for (int i = 0; i < voucherDto.LineItems.Count(); i++)
            {
                var lineItemDto = voucherDto.LineItems[i];
                var lineItemDomain = voucherDomainEntity.LineItems[i];
                Assert.AreEqual(lineItemDto.Comments, lineItemDomain.Comments);
                Assert.AreEqual(lineItemDto.Description, lineItemDomain.Description);
                Assert.AreEqual(lineItemDto.ExtendedPrice, lineItemDomain.ExtendedPrice);
                Assert.AreEqual(lineItemDto.InvoiceNumber, lineItemDomain.InvoiceNumber);
                Assert.AreEqual(lineItemDto.Price, lineItemDomain.Price);
                Assert.AreEqual(lineItemDto.Quantity, lineItemDomain.Quantity);
                Assert.AreEqual(lineItemDto.TaxForm, lineItemDomain.TaxForm);
                Assert.AreEqual(lineItemDto.TaxFormCode, lineItemDomain.TaxFormCode);
                Assert.AreEqual(lineItemDto.TaxFormLocation, lineItemDomain.TaxFormLocation);
                Assert.AreEqual(lineItemDto.UnitOfIssue, lineItemDomain.UnitOfIssue);

                // Confirm that the data in the line item GL distribution DTOs matches the domain entity
                for (int j = 0; j < lineItemDto.GlDistributions.Count(); j++)
                {
                    var glDistributionDto = lineItemDto.GlDistributions[j];
                    var glDistributionDomain = lineItemDomain.GlDistributions[j];
                    Assert.AreEqual(glDistributionDto.Amount, glDistributionDomain.Amount);
                    Assert.AreEqual(glDistributionDto.GlAccount, glDistributionDomain.GlAccountNumber);
                    Assert.AreEqual(glDistributionDto.ProjectLineItemCode, glDistributionDomain.ProjectLineItemCode);
                    Assert.AreEqual(glDistributionDto.ProjectNumber, glDistributionDomain.ProjectNumber);
                    Assert.AreEqual(glDistributionDto.Quantity, glDistributionDomain.Quantity);
                    Assert.AreEqual(glDistributionDto.GlAccountDescription, glDistributionDomain.GlAccountDescription);
                }

                // Confirm that the data in the line item tax DTOs matches the domain entity
                for (int k = 0; k < lineItemDto.LineItemTaxes.Count(); k++)
                {
                    var lineItemTaxDto = lineItemDto.LineItemTaxes[k];
                    var lineItemTaxDomain = lineItemDomain.LineItemTaxes[k];
                    Assert.AreEqual(lineItemTaxDto.TaxCode, lineItemTaxDomain.TaxCode);
                    Assert.AreEqual(lineItemTaxDto.TaxAmount, lineItemTaxDomain.TaxAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_OneGlAccountIsMasked()
        {
            var voucherId = "999";
            var personId = "1";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);


            // Get the purchase order domain entity from the test repository
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(voucherDto.VoucherId, voucherDomainEntity.Id);

            // There should only be one line item.
            Assert.AreEqual(1, voucherDomainEntity.LineItems.Count);
            Assert.AreEqual(1, voucherDto.LineItems.Count);

            // Confirm that the DTO line item data matches the data in the domain entity.
            var lineItemDto = voucherDto.LineItems.First();
            var lineItemEntity = voucherDomainEntity.LineItems.First();

            Assert.AreEqual(lineItemDto.Comments, lineItemEntity.Comments);
            Assert.AreEqual(lineItemDto.Description, lineItemEntity.Description);
            Assert.AreEqual(lineItemDto.ExpectedDeliveryDate, lineItemEntity.ExpectedDeliveryDate);
            Assert.AreEqual(lineItemDto.ExtendedPrice, lineItemEntity.ExtendedPrice);
            Assert.AreEqual(lineItemDto.Price, lineItemEntity.Price);
            Assert.AreEqual(lineItemDto.Quantity, lineItemEntity.Quantity);
            Assert.AreEqual(lineItemDto.TaxForm, lineItemEntity.TaxForm);
            Assert.AreEqual(lineItemDto.TaxFormCode, lineItemEntity.TaxFormCode);
            Assert.AreEqual(lineItemDto.TaxFormLocation, lineItemEntity.TaxFormLocation);
            Assert.AreEqual(lineItemDto.UnitOfIssue, lineItemEntity.UnitOfIssue);
            Assert.AreEqual(lineItemDto.VendorPart, lineItemEntity.VendorPart);

            var glConfiguration = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();

            // Confirm that the data in the line item GL distribution DTOs matches the domain entity
            foreach (var glDistributionEntity in lineItemEntity.GlDistributions)
            {
                var glDistributionDto = lineItemDto.GlDistributions.FirstOrDefault(x => x.GlAccount == glDistributionEntity.GlAccountNumber);

                // Check the values in each of the non-masked GL accounts.
                if (glDistributionDto != null)
                {
                    Assert.AreEqual(glDistributionDto.GlAccount, glDistributionEntity.GlAccountNumber);
                    Assert.AreEqual(glDistributionDto.FormattedGlAccount, glDistributionEntity.GetFormattedMaskedGlAccount(glConfiguration.MajorComponentStartPositions));
                    Assert.AreEqual(glDistributionDto.ProjectNumber, glDistributionEntity.ProjectNumber);
                    Assert.AreEqual(glDistributionDto.ProjectLineItemCode, glDistributionEntity.ProjectLineItemCode);
                    Assert.AreEqual(glDistributionDto.Quantity, glDistributionEntity.Quantity);
                    Assert.AreEqual(glDistributionDto.Amount, glDistributionEntity.Amount);
                }
            }

            // Get all of the masked GL account DTOs and confirm that their values are either 0 or null.
            var maskedGlDistributionEntity = lineItemEntity.GlDistributions.FirstOrDefault(x => x.Masked);
            var maskedGlAccountNumber = maskedGlDistributionEntity.GetFormattedMaskedGlAccount(glConfiguration.MajorComponentStartPositions);
            var maskedGlDistributionDtos = lineItemDto.GlDistributions.Where(x => x.FormattedGlAccount == maskedGlAccountNumber).ToList();

            foreach (var glDistributionDto in maskedGlDistributionDtos)
            {
                Assert.AreEqual(null, glDistributionDto.GlAccount);
                Assert.AreEqual(maskedGlAccountNumber, glDistributionDto.FormattedGlAccount);
                Assert.AreEqual(null, glDistributionDto.ProjectNumber);
                Assert.AreEqual(null, glDistributionDto.ProjectLineItemCode);
                Assert.AreEqual(0m, glDistributionDto.Quantity);
                Assert.AreEqual(0m, glDistributionDto.Amount);
            }
        }

        [TestMethod]
        public async Task GetVoucherAsync_PossibleAccess()
        {
            // Get a specified voucher
            var voucherId = "28";
            var personId = "0000006";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);


            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.VoucherId, voucherDomainEntity.Id);
            Assert.AreEqual(voucherDto.Amount, voucherDomainEntity.Amount);
            Assert.AreEqual(voucherDto.ApType, voucherDomainEntity.ApType);
            Assert.AreEqual(voucherDto.BlanketPurchaseOrderId, voucherDomainEntity.BlanketPurchaseOrderId);
            Assert.AreEqual(voucherDto.CheckDate, voucherDomainEntity.CheckDate);
            Assert.AreEqual(voucherDto.CheckNumber, voucherDomainEntity.CheckNumber);
            Assert.AreEqual(voucherDto.Comments, voucherDomainEntity.Comments);
            Assert.AreEqual(voucherDto.CurrencyCode, voucherDomainEntity.CurrencyCode);
            Assert.AreEqual(voucherDto.Date, voucherDomainEntity.Date);
            Assert.AreEqual(voucherDto.DueDate, voucherDomainEntity.DueDate);
            Assert.AreEqual(voucherDto.InvoiceDate, voucherDomainEntity.InvoiceDate);
            Assert.AreEqual(voucherDto.InvoiceNumber, voucherDomainEntity.InvoiceNumber);
            Assert.AreEqual(voucherDto.MaintenanceDate, voucherDomainEntity.MaintenanceDate);
            Assert.AreEqual(voucherDto.PurchaseOrderId, voucherDomainEntity.PurchaseOrderId);
            Assert.AreEqual(voucherDto.RecurringVoucherId, voucherDomainEntity.RecurringVoucherId);
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
            Assert.AreEqual(voucherDto.VendorId, voucherDomainEntity.VendorId);
            Assert.AreEqual(voucherDto.VendorName, voucherDomainEntity.VendorName);

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < voucherDto.Approvers.Count(); i++)
            {
                var approverDto = voucherDto.Approvers[i];
                var approverDomain = voucherDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the line item DTOs matches the domain entity
            for (int i = 0; i < voucherDto.LineItems.Count(); i++)
            {
                var lineItemDto = voucherDto.LineItems[i];
                var lineItemDomain = voucherDomainEntity.LineItems[i];
                Assert.AreEqual(lineItemDto.Comments, lineItemDomain.Comments);
                Assert.AreEqual(lineItemDto.Description, lineItemDomain.Description);
                Assert.AreEqual(lineItemDto.ExtendedPrice, lineItemDomain.ExtendedPrice);
                Assert.AreEqual(lineItemDto.InvoiceNumber, lineItemDomain.InvoiceNumber);
                Assert.AreEqual(lineItemDto.Price, lineItemDomain.Price);
                Assert.AreEqual(lineItemDto.Quantity, lineItemDomain.Quantity);
                Assert.AreEqual(lineItemDto.TaxForm, lineItemDomain.TaxForm);
                Assert.AreEqual(lineItemDto.TaxFormCode, lineItemDomain.TaxFormCode);
                Assert.AreEqual(lineItemDto.TaxFormLocation, lineItemDomain.TaxFormLocation);
                Assert.AreEqual(lineItemDto.UnitOfIssue, lineItemDomain.UnitOfIssue);

                // Confirm that the data in the line item GL distribution DTOs matches the domain entity
                for (int j = 0; j < lineItemDto.GlDistributions.Count(); j++)
                {
                    var glDistributionDto = lineItemDto.GlDistributions[j];
                    var glDistributionDomain = lineItemDomain.GlDistributions[j];

                    if (glDistributionDomain.Masked)
                    {
                        Assert.AreEqual(0.00m, glDistributionDto.Amount, "Amount should be 0");
                        Assert.IsNull(glDistributionDto.GlAccount, "GL number should be null");
                        Assert.IsNull(glDistributionDto.ProjectLineItemCode, "Project line item code should be null");
                        Assert.IsNull(glDistributionDto.ProjectNumber, "Project number should be null");
                        Assert.AreEqual(0.00m, glDistributionDto.Quantity, "Quantity should be 0");

                        Regex rx = new Regex("[^_]");
                        var expectedFormattedGlNumber = rx.Replace(glDistributionDomain.GlAccountNumber, "#").Replace("_", "-");
                        Assert.AreEqual(expectedFormattedGlNumber, glDistributionDto.FormattedGlAccount, "Formatted GL number should be masked");
                    }
                    else
                    {
                        Assert.AreEqual(glDistributionDto.Amount, glDistributionDomain.Amount);
                        Assert.AreEqual(glDistributionDto.GlAccount, glDistributionDomain.GlAccountNumber);
                        Assert.AreEqual(glDistributionDto.ProjectLineItemCode, glDistributionDomain.ProjectLineItemCode);
                        Assert.AreEqual(glDistributionDto.ProjectNumber, glDistributionDomain.ProjectNumber);
                        Assert.AreEqual(glDistributionDto.Quantity, glDistributionDomain.Quantity);
                        Assert.AreEqual(glDistributionDto.FormattedGlAccount, glDistributionDomain.GlAccountNumber.Replace("_", "-"));
                    }

                }

                // Confirm that the data in the line item tax DTOs matches the domain entity
                for (int k = 0; k < lineItemDto.LineItemTaxes.Count(); k++)
                {
                    var lineItemTaxDto = lineItemDto.LineItemTaxes[k];
                    var lineItemTaxDomain = lineItemDomain.LineItemTaxes[k];
                    Assert.AreEqual(lineItemTaxDto.TaxCode, lineItemTaxDomain.TaxCode);
                    Assert.AreEqual(lineItemTaxDto.TaxAmount, lineItemTaxDomain.TaxAmount);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetVoucherAsync_NoAccess()
        {
            // Get a specified voucher
            var voucherId = "30";
            var personId = "0000007";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);
        }

        [TestMethod]
        public async Task GetVoucherAsync_StatusNotApproved()
        {
            // Get a specified voucher
            var voucherId = "17";
            var personId = "0000005";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucherAsync_StatusOutstanding()
        {
            // Get a specified voucher
            var voucherId = "3";
            var personId = "0000005";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);


            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucherAsync_StatusPaid()
        {
            // Get a specified voucher
            var voucherId = "4";
            var personId = "0000005";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucherAsync_StatusReconciled()
        {
            // Get a specified voucher
            var voucherId = "18";
            var personId = "0000005";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);


            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucherAsync_StatusVoided()
        {
            // Get a specified voucher
            var voucherId = "19";
            var personId = "0000005";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucherAsync_StatusCancelled()
        {
            // Get a specified voucher
            var voucherId = "20";
            var personId = "0000005";
            versionNumber = 1;
            var voucherDto = await service.GetVoucherAsync(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucherAsync_NullId()
        {
            var expectedParamName = "voucherDomainEntity";
            var actualParamName = "";
            try
            {
                var voucherDto = await service.GetVoucherAsync(null);
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetVoucherAsync_EmptyId()
        {
            var expectedParamName = "voucherDomainEntity";
            var actualParamName = "";
            try
            {
                var voucherDto = await service.GetVoucherAsync("");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucherAsync_NullAccountStructure()
        {
            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            await service2.GetVoucherAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucherAsync_NullGlClassConfiguration()
        {
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            await service2.GetVoucherAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucherAsync_NullGeneralLedgerUser()
        {
            // Mock the general ledger user repository method to return a null object within the service method
            GeneralLedgerUser glUser = null;
            mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(glUser));
            await service2.GetVoucherAsync("1");
        }

        [TestMethod]
        public async Task GetVoucherAsync_RepositoryReturnsNullObject()
        {
            var expectedParamName = "voucherDomainEntity";
            var actualParamName = "";
            try
            {
                Voucher nullVoucher = null;
                this.mockVoucherRepository.Setup(repo => repo.GetVoucherAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<List<string>>(), versionNumber)).Returns(Task.FromResult(nullVoucher));

                GeneralLedgerClassConfiguration glClassConfiguration = await testGeneralLedgerConfigurationRepository.GetClassConfigurationAsync();
                this.mockGlConfigurationRepository.Setup(repo => repo.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));

                GeneralLedgerAccountStructure accountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
                this.mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));

                GeneralLedgerUser glUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync("0000028", null, null, null);
                this.mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(glUser));

                versionNumber = 1;
                var voucherDto = await service2.GetVoucherAsync("1");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }
        #endregion

        #region GetVoucherAsyncTests without a view permission
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetVoucherAsync_PermissionException()
        {
            await serviceForNoPermission.GetVoucherAsync("1");
        }
        #endregion

        #region GetVoucher2AsyncTests with a view permission

        [TestMethod]
        public async Task GetVoucher2Async_FullAccess()
        {
            // Get a specified voucher
            var voucherId = "1";
            var personId = "0000005";
            var voucherDto = await service.GetVoucher2Async(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.VoucherId, voucherDomainEntity.Id);
            Assert.AreEqual(voucherDto.Amount, voucherDomainEntity.Amount);
            Assert.AreEqual(voucherDto.ApType, voucherDomainEntity.ApType);
            Assert.AreEqual(voucherDto.BlanketPurchaseOrderId, voucherDomainEntity.BlanketPurchaseOrderId);
            Assert.AreEqual(voucherDto.CheckDate, voucherDomainEntity.CheckDate);
            Assert.AreEqual(voucherDto.CheckNumber, voucherDomainEntity.CheckNumber);
            Assert.AreEqual(voucherDto.Comments, voucherDomainEntity.Comments);
            Assert.AreEqual(voucherDto.CurrencyCode, voucherDomainEntity.CurrencyCode);
            Assert.AreEqual(voucherDto.Date, voucherDomainEntity.Date);
            Assert.AreEqual(voucherDto.DueDate, voucherDomainEntity.DueDate);
            Assert.AreEqual(voucherDto.InvoiceDate, voucherDomainEntity.InvoiceDate);
            Assert.AreEqual(voucherDto.InvoiceNumber, voucherDomainEntity.InvoiceNumber);
            Assert.AreEqual(voucherDto.MaintenanceDate, voucherDomainEntity.MaintenanceDate);
            Assert.AreEqual(voucherDto.PurchaseOrderId, voucherDomainEntity.PurchaseOrderId);
            Assert.AreEqual(voucherDto.RecurringVoucherId, voucherDomainEntity.RecurringVoucherId);
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
            Assert.AreEqual(voucherDto.VendorId, voucherDomainEntity.VendorId);
            Assert.AreEqual(voucherDto.VendorName, voucherDomainEntity.VendorName);

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < voucherDto.Approvers.Count(); i++)
            {
                var approverDto = voucherDto.Approvers[i];
                var approverDomain = voucherDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the line item DTOs matches the domain entity
            for (int i = 0; i < voucherDto.LineItems.Count(); i++)
            {
                var lineItemDto = voucherDto.LineItems[i];
                var lineItemDomain = voucherDomainEntity.LineItems[i];
                Assert.AreEqual(lineItemDto.Comments, lineItemDomain.Comments);
                Assert.AreEqual(lineItemDto.Description, lineItemDomain.Description);
                Assert.AreEqual(lineItemDto.ExtendedPrice, lineItemDomain.ExtendedPrice);
                Assert.AreEqual(lineItemDto.InvoiceNumber, lineItemDomain.InvoiceNumber);
                Assert.AreEqual(lineItemDto.Price, lineItemDomain.Price);
                Assert.AreEqual(lineItemDto.Quantity, lineItemDomain.Quantity);
                Assert.AreEqual(lineItemDto.TaxForm, lineItemDomain.TaxForm);
                Assert.AreEqual(lineItemDto.TaxFormCode, lineItemDomain.TaxFormCode);
                Assert.AreEqual(lineItemDto.TaxFormLocation, lineItemDomain.TaxFormLocation);
                Assert.AreEqual(lineItemDto.UnitOfIssue, lineItemDomain.UnitOfIssue);

                // Confirm that the data in the line item GL distribution DTOs matches the domain entity
                for (int j = 0; j < lineItemDto.GlDistributions.Count(); j++)
                {
                    var glDistributionDto = lineItemDto.GlDistributions[j];
                    var glDistributionDomain = lineItemDomain.GlDistributions[j];
                    Assert.AreEqual(glDistributionDto.Amount, glDistributionDomain.Amount);
                    Assert.AreEqual(glDistributionDto.GlAccount, glDistributionDomain.GlAccountNumber);
                    Assert.AreEqual(glDistributionDto.ProjectLineItemCode, glDistributionDomain.ProjectLineItemCode);
                    Assert.AreEqual(glDistributionDto.ProjectNumber, glDistributionDomain.ProjectNumber);
                    Assert.AreEqual(glDistributionDto.Quantity, glDistributionDomain.Quantity);
                }

                // Confirm that the data in the line item tax DTOs matches the domain entity
                for (int k = 0; k < lineItemDto.LineItemTaxes.Count(); k++)
                {
                    var lineItemTaxDto = lineItemDto.LineItemTaxes[k];
                    var lineItemTaxDomain = lineItemDomain.LineItemTaxes[k];
                    Assert.AreEqual(lineItemTaxDto.TaxCode, lineItemTaxDomain.TaxCode);
                    Assert.AreEqual(lineItemTaxDto.TaxAmount, lineItemTaxDomain.TaxAmount);
                }
            }
        }

        [TestMethod]
        public async Task GetVoucher2Async_OneGlAccountIsMasked()
        {
            var voucherId = "999";
            var personId = "1";
            var voucherDto = await service.GetVoucher2Async(voucherId);

            // Get the purchase order domain entity from the test repository
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(voucherDto.VoucherId, voucherDomainEntity.Id);

            // There should only be one line item.
            Assert.AreEqual(1, voucherDomainEntity.LineItems.Count);
            Assert.AreEqual(1, voucherDto.LineItems.Count);

            // Confirm that the DTO line item data matches the data in the domain entity.
            var lineItemDto = voucherDto.LineItems.First();
            var lineItemEntity = voucherDomainEntity.LineItems.First();

            Assert.AreEqual(lineItemDto.Comments, lineItemEntity.Comments);
            Assert.AreEqual(lineItemDto.Description, lineItemEntity.Description);
            Assert.AreEqual(lineItemDto.ExpectedDeliveryDate, lineItemEntity.ExpectedDeliveryDate);
            Assert.AreEqual(lineItemDto.ExtendedPrice, lineItemEntity.ExtendedPrice);
            Assert.AreEqual(lineItemDto.Price, lineItemEntity.Price);
            Assert.AreEqual(lineItemDto.Quantity, lineItemEntity.Quantity);
            Assert.AreEqual(lineItemDto.TaxForm, lineItemEntity.TaxForm);
            Assert.AreEqual(lineItemDto.TaxFormCode, lineItemEntity.TaxFormCode);
            Assert.AreEqual(lineItemDto.TaxFormLocation, lineItemEntity.TaxFormLocation);
            Assert.AreEqual(lineItemDto.UnitOfIssue, lineItemEntity.UnitOfIssue);
            Assert.AreEqual(lineItemDto.VendorPart, lineItemEntity.VendorPart);

            var glConfiguration = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();

            // Confirm that the data in the line item GL distribution DTOs matches the domain entity
            foreach (var glDistributionEntity in lineItemEntity.GlDistributions)
            {
                var glDistributionDto = lineItemDto.GlDistributions.FirstOrDefault(x => x.GlAccount == glDistributionEntity.GlAccountNumber);

                // Check the values in each of the non-masked GL accounts.
                if (glDistributionDto != null)
                {
                    Assert.AreEqual(glDistributionDto.GlAccount, glDistributionEntity.GlAccountNumber);
                    Assert.AreEqual(glDistributionDto.FormattedGlAccount, glDistributionEntity.GetFormattedMaskedGlAccount(glConfiguration.MajorComponentStartPositions));
                    Assert.AreEqual(glDistributionDto.ProjectNumber, glDistributionEntity.ProjectNumber);
                    Assert.AreEqual(glDistributionDto.ProjectLineItemCode, glDistributionEntity.ProjectLineItemCode);
                    Assert.AreEqual(glDistributionDto.Quantity, glDistributionEntity.Quantity);
                    Assert.AreEqual(glDistributionDto.Amount, glDistributionEntity.Amount);
                    Assert.AreEqual(glDistributionDto.GlAccountDescription, glDistributionEntity.GlAccountDescription);
                }
            }

            // Get all of the masked GL account DTOs and confirm that their values are either 0 or null.
            var maskedGlDistributionEntity = lineItemEntity.GlDistributions.FirstOrDefault(x => x.Masked);
            var maskedGlAccountNumber = maskedGlDistributionEntity.GetFormattedMaskedGlAccount(glConfiguration.MajorComponentStartPositions);
            var maskedGlDistributionDtos = lineItemDto.GlDistributions.Where(x => x.FormattedGlAccount == maskedGlAccountNumber).ToList();

            foreach (var glDistributionDto in maskedGlDistributionDtos)
            {
                Assert.AreEqual(null, glDistributionDto.GlAccount);
                Assert.AreEqual(maskedGlAccountNumber, glDistributionDto.FormattedGlAccount);
                Assert.AreEqual(null, glDistributionDto.ProjectNumber);
                Assert.AreEqual(null, glDistributionDto.ProjectLineItemCode);
                Assert.AreEqual(0m, glDistributionDto.Quantity);
                Assert.AreEqual(0m, glDistributionDto.Amount);
                Assert.AreEqual(null, glDistributionDto.GlAccountDescription);
            }
        }

        [TestMethod]
        public async Task GetVoucher2Async_PossibleAccess()
        {
            // Get a specified voucher
            var voucherId = "28";
            var personId = "0000006";
            var voucherDto = await service.GetVoucher2Async(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Possible_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.VoucherId, voucherDomainEntity.Id);
            Assert.AreEqual(voucherDto.Amount, voucherDomainEntity.Amount);
            Assert.AreEqual(voucherDto.ApType, voucherDomainEntity.ApType);
            Assert.AreEqual(voucherDto.BlanketPurchaseOrderId, voucherDomainEntity.BlanketPurchaseOrderId);
            Assert.AreEqual(voucherDto.CheckDate, voucherDomainEntity.CheckDate);
            Assert.AreEqual(voucherDto.CheckNumber, voucherDomainEntity.CheckNumber);
            Assert.AreEqual(voucherDto.Comments, voucherDomainEntity.Comments);
            Assert.AreEqual(voucherDto.CurrencyCode, voucherDomainEntity.CurrencyCode);
            Assert.AreEqual(voucherDto.Date, voucherDomainEntity.Date);
            Assert.AreEqual(voucherDto.DueDate, voucherDomainEntity.DueDate);
            Assert.AreEqual(voucherDto.InvoiceDate, voucherDomainEntity.InvoiceDate);
            Assert.AreEqual(voucherDto.InvoiceNumber, voucherDomainEntity.InvoiceNumber);
            Assert.AreEqual(voucherDto.MaintenanceDate, voucherDomainEntity.MaintenanceDate);
            Assert.AreEqual(voucherDto.PurchaseOrderId, voucherDomainEntity.PurchaseOrderId);
            Assert.AreEqual(voucherDto.RecurringVoucherId, voucherDomainEntity.RecurringVoucherId);
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
            Assert.AreEqual(voucherDto.VendorId, voucherDomainEntity.VendorId);
            Assert.AreEqual(voucherDto.VendorName, voucherDomainEntity.VendorName);

            // Confirm that the data in the approvers DTOs matches the domain entity
            for (int i = 0; i < voucherDto.Approvers.Count(); i++)
            {
                var approverDto = voucherDto.Approvers[i];
                var approverDomain = voucherDomainEntity.Approvers[i];
                Assert.AreEqual(approverDto.ApprovalName, approverDomain.ApprovalName);
                Assert.AreEqual(approverDto.ApprovalDate, approverDomain.ApprovalDate);
            }

            // Confirm that the data in the line item DTOs matches the domain entity
            for (int i = 0; i < voucherDto.LineItems.Count(); i++)
            {
                var lineItemDto = voucherDto.LineItems[i];
                var lineItemDomain = voucherDomainEntity.LineItems[i];
                Assert.AreEqual(lineItemDto.Comments, lineItemDomain.Comments);
                Assert.AreEqual(lineItemDto.Description, lineItemDomain.Description);
                Assert.AreEqual(lineItemDto.ExtendedPrice, lineItemDomain.ExtendedPrice);
                Assert.AreEqual(lineItemDto.InvoiceNumber, lineItemDomain.InvoiceNumber);
                Assert.AreEqual(lineItemDto.Price, lineItemDomain.Price);
                Assert.AreEqual(lineItemDto.Quantity, lineItemDomain.Quantity);
                Assert.AreEqual(lineItemDto.TaxForm, lineItemDomain.TaxForm);
                Assert.AreEqual(lineItemDto.TaxFormCode, lineItemDomain.TaxFormCode);
                Assert.AreEqual(lineItemDto.TaxFormLocation, lineItemDomain.TaxFormLocation);
                Assert.AreEqual(lineItemDto.UnitOfIssue, lineItemDomain.UnitOfIssue);

                // Confirm that the data in the line item GL distribution DTOs matches the domain entity
                for (int j = 0; j < lineItemDto.GlDistributions.Count(); j++)
                {
                    var glDistributionDto = lineItemDto.GlDistributions[j];
                    var glDistributionDomain = lineItemDomain.GlDistributions[j];

                    if (glDistributionDomain.Masked)
                    {
                        Assert.AreEqual(0.00m, glDistributionDto.Amount, "Amount should be 0");
                        Assert.IsNull(glDistributionDto.GlAccount, "GL number should be null");
                        Assert.IsNull(glDistributionDto.ProjectLineItemCode, "Project line item code should be null");
                        Assert.IsNull(glDistributionDto.ProjectNumber, "Project number should be null");
                        Assert.AreEqual(0.00m, glDistributionDto.Quantity, "Quantity should be 0");
                        Assert.AreEqual(null, glDistributionDto.GlAccountDescription, "GlAccountDescription should be null");

                        Regex rx = new Regex("[^_]");
                        var expectedFormattedGlNumber = rx.Replace(glDistributionDomain.GlAccountNumber, "#").Replace("_", "-");
                        Assert.AreEqual(expectedFormattedGlNumber, glDistributionDto.FormattedGlAccount, "Formatted GL number should be masked");
                    }
                    else
                    {
                        Assert.AreEqual(glDistributionDto.Amount, glDistributionDomain.Amount);
                        Assert.AreEqual(glDistributionDto.GlAccount, glDistributionDomain.GlAccountNumber);
                        Assert.AreEqual(glDistributionDto.ProjectLineItemCode, glDistributionDomain.ProjectLineItemCode);
                        Assert.AreEqual(glDistributionDto.ProjectNumber, glDistributionDomain.ProjectNumber);
                        Assert.AreEqual(glDistributionDto.Quantity, glDistributionDomain.Quantity);
                        Assert.AreEqual(glDistributionDto.FormattedGlAccount, glDistributionDomain.GlAccountNumber.Replace("_", "-"));
                        Assert.AreEqual(glDistributionDto.GlAccountDescription, glDistributionDomain.GlAccountDescription);
                    }

                }

                // Confirm that the data in the line item tax DTOs matches the domain entity
                for (int k = 0; k < lineItemDto.LineItemTaxes.Count(); k++)
                {
                    var lineItemTaxDto = lineItemDto.LineItemTaxes[k];
                    var lineItemTaxDomain = lineItemDomain.LineItemTaxes[k];
                    Assert.AreEqual(lineItemTaxDto.TaxCode, lineItemTaxDomain.TaxCode);
                    Assert.AreEqual(lineItemTaxDto.TaxAmount, lineItemTaxDomain.TaxAmount);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetVoucher2Async_NoAccess()
        {
            // Get a specified voucher
            var voucherId = "30";
            var personId = "0000007";
            var voucherDto = await service.GetVoucher2Async(voucherId);
        }

        [TestMethod]
        public async Task GetVoucher2Async_StatusNotApproved()
        {
            // Get a specified voucher
            var voucherId = "17";
            var personId = "0000005";
            var voucherDto = await service.GetVoucher2Async(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucher2Async_StatusOutstanding()
        {
            // Get a specified voucher
            var voucherId = "3";
            var personId = "0000005";
            var voucherDto = await service.GetVoucher2Async(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucher2Async_StatusPaid()
        {
            // Get a specified voucher
            var voucherId = "4";
            var personId = "0000005";
            var voucherDto = await service.GetVoucher2Async(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucher2Async_StatusReconciled()
        {
            // Get a specified voucher
            var voucherId = "18";
            var personId = "0000005";
            var voucherDto = await service.GetVoucher2Async(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucher2Async_StatusVoided()
        {
            // Get a specified voucher
            var voucherId = "19";
            var personId = "0000005";
            var voucherDto = await service.GetVoucher2Async(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucher2Async_StatusCancelled()
        {
            // Get a specified voucher
            var voucherId = "20";
            var personId = "0000005";
            var voucherDto = await service.GetVoucher2Async(voucherId);

            // Build the projects accounting user object and get the list of project domain entities from the test repository.
            var voucherDomainEntity = await testVoucherRepository.GetVoucherAsync(voucherId, personId, GlAccessLevel.Full_Access, null, versionNumber);

            // Confirm that the data in the vouchers DTO matches the domain entity
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
        }

        [TestMethod]
        public async Task GetVoucher2Async_NullId()
        {
            var expectedParamName = "voucherDomainEntity";
            var actualParamName = "";
            try
            {
                var voucherDto = await service.GetVoucher2Async(null);
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        public async Task GetVoucher2Async_EmptyId()
        {
            var expectedParamName = "voucherDomainEntity";
            var actualParamName = "";
            try
            {
                var voucherDto = await service.GetVoucher2Async("");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucher2Async_NullAccountStructure()
        {
            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            await service2.GetVoucher2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucher2Async_NullGlClassConfiguration()
        {
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            await service2.GetVoucher2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucher2Async_NullGeneralLedgerUser()
        {
            // Mock the general ledger user repository method to return a null object within the service method
            GeneralLedgerUser glUser = null;
            mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns(Task.FromResult(glUser));
            await service2.GetVoucher2Async("1");
        }

        [TestMethod]
        public async Task GetVoucher2Async_RepositoryReturnsNullObject()
        {
            var expectedParamName = "voucherDomainEntity";
            var actualParamName = "";
            try
            {
                Voucher nullVoucher = null;
                this.mockVoucherRepository.Setup(repo => repo.GetVoucherAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<List<string>>(), versionNumber)).Returns(Task.FromResult(nullVoucher));

                GeneralLedgerClassConfiguration glClassConfiguration = await testGeneralLedgerConfigurationRepository.GetClassConfigurationAsync();
                this.mockGlConfigurationRepository.Setup(repo => repo.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));

                GeneralLedgerAccountStructure accountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
                this.mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));

                GeneralLedgerUser glUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2("0000028", null, glClassConfiguration);
                this.mockGeneralLedgerUserRepository.Setup(repo => repo.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(Task.FromResult(glUser));

                this.mockApprovalConfigurationRepositoryFalse.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult(new ApprovalConfiguration()));

                var voucherDto = await service2.GetVoucher2Async("1");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
        }
        #endregion


        #region Tests for GetVoucher2Async with GL approval roles functionality

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucher2Async_ApprovalConfigurationNull()
        {
            GeneralLedgerUser glUser = new GeneralLedgerUser("0000001", "Test");
            glUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            mockGeneralLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
                {
                    return Task.FromResult(glUser);
                });

            mockApprovalConfigurationRepositoryTrue.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult<ApprovalConfiguration>(null));
            var loggerObject = new Mock<ILogger>().Object;
            var adapterRegistry = new Mock<IAdapterRegistry>();
            voucherApprovalRolesService = new VoucherService(mockVoucherRepository.Object, testGeneralLedgerConfigurationRepository, mockGeneralLedgerUserRepository.Object, testGeneralLedgerAccountRepository,
              adapterRegistry.Object, currentUserFactory, roleRepository, staffRepositoryMock.Object, mockApprovalConfigurationRepositoryTrue.Object, mockProcurementsUtilityService.Object, loggerObject);

            await voucherApprovalRolesService.GetVoucher2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucher2Async_ApprovalConfigurationException()
        {
            GeneralLedgerUser glUser = new GeneralLedgerUser("0000001", "Test");
            glUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            mockGeneralLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });
            mockApprovalConfigurationRepositoryTrue.Setup(repo => repo.GetApprovalConfigurationAsync()).Throws(new Exception());
            var loggerObject = new Mock<ILogger>().Object;
            var adapterRegistry = new Mock<IAdapterRegistry>();
            voucherApprovalRolesService = new VoucherService(mockVoucherRepository.Object, testGeneralLedgerConfigurationRepository, mockGeneralLedgerUserRepository.Object, testGeneralLedgerAccountRepository,
              adapterRegistry.Object, currentUserFactory, roleRepository, staffRepositoryMock.Object, mockApprovalConfigurationRepositoryTrue.Object, mockProcurementsUtilityService.Object, loggerObject);

            await voucherApprovalRolesService.GetVoucher2Async("1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucher2Async_ApprovalConfigurationVouchersUseApprovalRoles()
        {
            GeneralLedgerUser glUser = new GeneralLedgerUser("0000001", "Test");
            glUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
            mockGeneralLedgerUserRepository.Setup(x => x.GetGeneralLedgerUserAsync2(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GeneralLedgerClassConfiguration>())).Returns(() =>
            {
                return Task.FromResult(glUser);
            });
            mockApprovalConfigurationRepositoryTrue.Setup(repo => repo.GetApprovalConfigurationAsync()).Returns(Task.FromResult<ApprovalConfiguration>(new ApprovalConfiguration() { VouchersUseApprovalRoles = true }));
            mockGeneralLedgerUserRepository.Setup(x => x.GetGlUserApprovalAndGlAccessAccountsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>())).Returns(() =>
            {
                IEnumerable<string> approvalAccess = new List<string>() { "11_11_11_11_00000_11111", "11_11_11_11_00000_11112" };
                return Task.FromResult(approvalAccess);
            });
            var loggerObject = new Mock<ILogger>().Object;
            var adapterRegistry = new Mock<IAdapterRegistry>();
            voucherApprovalRolesService = new VoucherService(mockVoucherRepository.Object, testGeneralLedgerConfigurationRepository, mockGeneralLedgerUserRepository.Object, testGeneralLedgerAccountRepository,
              adapterRegistry.Object, currentUserFactory, roleRepository, staffRepositoryMock.Object, mockApprovalConfigurationRepositoryTrue.Object, mockProcurementsUtilityService.Object, loggerObject);

            await voucherApprovalRolesService.GetVoucher2Async("1");
        }

        #endregion

        #region GetVoucher2AsyncTests without a view permission
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetVoucher2Async_PermissionException()
        {
            await serviceForNoPermission.GetVoucher2Async("1");
        }
        #endregion

        #region Tests for GetVoucherSummariesAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVoucherSummariesAsync_EmptyPersonId_ArgumentNullException()
        {
            await service2.GetVoucherSummariesAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetVoucherSummariesAsync_CurrentUserDifferentFromRequest()
        {
            await service2.GetVoucherSummariesAsync("0016357");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetVoucherSummariesAsync_MissingPermissionException()
        {
            await serviceForNoPermission.GetVoucherSummariesAsync(currentUserFactory.CurrentUser.PersonId);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetVoucherSummariesAsync_StaffRecordMissingException()
        {
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));
            await service2.GetVoucherSummariesAsync(currentUserFactory.CurrentUser.PersonId);
        }

        [TestMethod]
        public async Task GetVoucherSummariesAsync()
        {
            var voucherId = "31";
            var personId = currentUserFactory.CurrentUser.PersonId;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));
            var voucherSummaryListDto = await service.GetVoucherSummariesAsync(personId);


            // Get the requisition domain entity from the test repository
            var voucherSummaryListDomainEntity = await testVoucherRepository.GetVoucherSummariesByPersonIdAsync(personId);

            Assert.IsNotNull(voucherSummaryListDto);
            Assert.AreEqual(voucherSummaryListDto.ToList().Count, voucherSummaryListDomainEntity.ToList().Count);

            var voucherDto = voucherSummaryListDto.Where(x => x.Id == voucherId).FirstOrDefault();
            var voucherDomainEntity = voucherSummaryListDomainEntity.Where(x => x.Id == voucherId).FirstOrDefault();

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(voucherDto.Id, voucherDomainEntity.Id);
            Assert.AreEqual(voucherDto.InvoiceNumber, voucherDomainEntity.InvoiceNumber);
            Assert.AreEqual(voucherDto.Amount, voucherDomainEntity.Amount);
            Assert.AreEqual(voucherDto.Date, voucherDomainEntity.Date);
            Assert.AreEqual(voucherDto.RequestorName, voucherDomainEntity.RequestorName);
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
            Assert.AreEqual(voucherDto.VendorId, voucherDomainEntity.VendorId);
            Assert.AreEqual(voucherDto.VendorName, voucherDomainEntity.VendorName);

            // Confirm that the data in the list of purchase order DTOs matches the domain entity
            for (int i = 0; i < voucherDto.PurchaseOrders.Count(); i++)
            {
                Assert.AreEqual(voucherDto.PurchaseOrders[i].Id, voucherDomainEntity.PurchaseOrders[i].Id);
                Assert.AreEqual(voucherDto.PurchaseOrders[i].Number, voucherDomainEntity.PurchaseOrders[i].Number);
            }
        }
        #endregion

        #region CreateUpdateVoucherAsync

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdateVoucherAsync_Null_VoucherCreateUpdateRequest_ArgumentNullException()
        {
            await service2.CreateUpdateVoucherAsync(null);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdateVoucherAsync_EmptyPersonId_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest();
            abc.PersonId = "";
            await service2.CreateUpdateVoucherAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task CreateUpdateVoucherAsync_CheckUserIsSelf_NotSelfUser_PermissionsException()
        {
            // Having personid different from Current User Person Id
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest()
            {
                PersonId = "134231",
                Voucher = new Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2()
            };
            await service2.CreateUpdateVoucherAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdateVoucherAsync_VoucherNull_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest();
            abc.PersonId = "0000001";
            abc.Voucher = null;
            await service2.CreateUpdateVoucherAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdateVoucherAsync_glConfiguration_ArgumentNullException()
        {
            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            Domain.Base.Entities.Staff validStaff = new Domain.Base.Entities.Staff("1", "last");
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(validStaff));
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest()
            {
                PersonId = currentUserFactory.CurrentUser.PersonId,
                Voucher = new Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2()
            };
            await service2.CreateUpdateVoucherAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdateVoucherAsync_glClassConfiguration_ArgumentNullException()
        {
            Domain.Base.Entities.Staff validStaff = new Domain.Base.Entities.Staff("1", "lastName");
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(validStaff));
            // Mock the general ledger configuration repository method to return a null object within the service method
            GeneralLedgerAccountStructure accountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest()
            {
                PersonId = currentUserFactory.CurrentUser.PersonId,
                Voucher = new Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2()
            };
            await service2.CreateUpdateVoucherAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdateVoucherAsync_generalLedgerUser_ArgumentNullException()
        {
            Domain.Base.Entities.Staff validStaff = new Domain.Base.Entities.Staff("1", "lastName");
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(validStaff));
            // Mock the general ledger configuration repository method to return a valid object within the service method
            GeneralLedgerAccountStructure accountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            // Mock the general ledger class configuration repository method to return a valid object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = await testGeneralLedgerConfigurationRepository.GetClassConfigurationAsync();
            glClassConfiguration = null;
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerUser glUser = null;
            mockGeneralLedgerUserRepository.Setup(glConfig => glConfig.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICollection<string>>())).Returns(Task.FromResult(glUser));
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest()
            {
                PersonId = currentUserFactory.CurrentUser.PersonId,
                Voucher = new Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2()
            };
            await service2.CreateUpdateVoucherAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateUpdateVoucherAsync_EmptyVoucherDto_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest();
            abc.Voucher = null;
            await service2.CreateUpdateVoucherAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task CreateUpdateVoucherAsync_StaffRecordMissingException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest();
            request.PersonId = "1";
            Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2 voucher = new Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2();
            voucher.VoucherId = "";
            request.Voucher = voucher;
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));
            await service2.CreateUpdateVoucherAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task CreateUpdateVoucherAsync_StaffRecordKeynotFoundException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest();
            request.PersonId = currentUserFactory.CurrentUser.PersonId;
            Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2 voucher = new Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2();
            voucher.VoucherId = "";
            request.Voucher = voucher;
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await service2.CreateUpdateVoucherAsync(request);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task CreateUpdateVoucherAsync_CreateVoucher_WithoutPermission()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest request = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest();
            request.PersonId = "0000001";
            request.ConfEmailAddresses = new List<string>() { "abc@ellucian.com" };
            request.Voucher = new Dtos.ColleagueFinance.Voucher2
            {
                VoucherId = "",
                Status = 0,
                Amount = 90.00M,
                CurrencyCode = "",
                Date = new DateTime(2020, 01, 01),
                InvoiceDate = new DateTime(2020, 01, 01),
                InvoiceNumber = "123341",
                MaintenanceDate = new DateTime(2020, 01, 01),
                VendorId = "0000189",
                VendorName = "Beatrice Clarke & Company",
                ApType = "AP",
                Comments = "This is Purchase Order creation",
            };

            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            await serviceForNoPermission.CreateUpdateVoucherAsync(request);
        }

        [TestMethod]
        public async Task CreateUpdateVoucherAsync_CreateVoucher_Valid()
        {
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            // Mock the general ledger configuration repository method to return a valid object within the service method
            GeneralLedgerAccountStructure accountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            // Mock the general ledger class configuration repository method to return a valid object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = await testGeneralLedgerConfigurationRepository.GetClassConfigurationAsync();
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerUser glUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2("0000028", null, glClassConfiguration);
            mockGeneralLedgerUserRepository.Setup(glConfig => glConfig.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICollection<string>>())).Returns(Task.FromResult(glUser));
            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var logger = new Mock<ILogger>();
            var response = await service2.CreateUpdateVoucherAsync(voucherCreateUpdateRequest);
            Assert.IsNotNull(response);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task CreateUpdateVoucherAsync_CreateVoucher_Update()
        {
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));
            var testGlAccountStructure = await new TestGeneralLedgerConfigurationRepository().GetAccountStructureAsync();
            mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).ReturnsAsync(testGlAccountStructure);
            // Mock the general ledger configuration repository method to return a valid object within the service method
            GeneralLedgerAccountStructure accountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));
            // Mock the general ledger class configuration repository method to return a valid object within the service method
            GeneralLedgerClassConfiguration glClassConfiguration = await testGeneralLedgerConfigurationRepository.GetClassConfigurationAsync();
            mockGlConfigurationRepository.Setup(acctStructure => acctStructure.GetClassConfigurationAsync()).Returns(Task.FromResult(glClassConfiguration));
            // Mock the general ledger class configuration repository method to return a null object within the service method
            GeneralLedgerUser glUser = await testGeneralLedgerUserRepository.GetGeneralLedgerUserAsync2("0000028", null, glClassConfiguration);
            mockGeneralLedgerUserRepository.Setup(glConfig => glConfig.GetGeneralLedgerUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICollection<string>>())).Returns(Task.FromResult(glUser));
            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var logger = new Mock<ILogger>();
            voucherCreateUpdateRequest.Voucher.VoucherId = "1";
            Voucher originalVoucher = new Voucher("1", new DateTime(), VoucherStatus.Outstanding, "VenName");
            //mockVoucherRepository.Setup(s => s.GetVoucherAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GlAccessLevel>(), It.IsAny<string[]>(), It.IsAny<int>())).ReturnsAsync(originalVoucher);
            await service2.CreateUpdateVoucherAsync(voucherCreateUpdateRequest);
        }

        #endregion

        #region ReImburse Myself

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VoucherService_GetReimbursePersonAddressForVoucherAsync_PermissionException()
        {
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
            await service.GetReimbursePersonAddressForVoucherAsync();
        }

        [TestMethod]
        public async Task VoucherService_GetReimbursePersonAddressForVoucherAsync_ReturnsNull()
        {
            mockVoucherRepository.Setup(i => i.GetReimbursePersonAddressForVoucherAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            var resultDto = await service2.GetReimbursePersonAddressForVoucherAsync();
            Assert.IsNull(resultDto.VendorId);
            Assert.IsNull(resultDto.AddressId);
            Assert.IsNull(resultDto.VendorNameLines);
        }

        [TestMethod]
        public async Task VoucherService_GetReimbursePersonAddressForVoucherAsync_ReturnsVendorDetails()
        {
            mockVoucherRepository.Setup(i => i.GetReimbursePersonAddressForVoucherAsync(It.IsAny<string>())).ReturnsAsync(vendorEntities);
            var vendorSearchResultDto = await service2.GetReimbursePersonAddressForVoucherAsync();

            var vendorDomainEntity = vendorEntities;

            Assert.AreEqual(vendorSearchResultDto.VendorId, vendorDomainEntity.VendorId);
            Assert.AreEqual(vendorSearchResultDto.Zip, vendorDomainEntity.Zip);
            Assert.AreEqual(vendorSearchResultDto.Country, vendorDomainEntity.Country);
            Assert.AreEqual(vendorSearchResultDto.FormattedAddress, vendorDomainEntity.FormattedAddress);
            Assert.AreEqual(vendorSearchResultDto.AddressId, vendorDomainEntity.AddressId);
        }

        #endregion

        #region VendorsVoucherSearchResult Mappings Tests
        [TestMethod]
        public async Task VoucherService_VendorsVoucherSearchResult_MappingsTest()
        {
            Dtos.ColleagueFinance.VendorsVoucherSearchResult vendorSearchResultDto = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorsVoucherSearchResult()
            {
                VendorId = "0000192",
                VendorNameLines = new List<string> { "Blue Cross Office supply" },
                VendorMiscName = "Misc Vendor",
                AddressLines = new List<string> { "PO Box 69845" },
                City = "Minneapolis",
                State = "MN",
                Zip = "55430",
                Country = "US",
                FormattedAddress = "PO Box 69845 Minneapolis MN 55430",
                AddressId = "143",
                IsInternationalAddress = true,
                MiscVendor = true,
                ReImburseMyself = true
            };

            var adapterRegistry = new Mock<IAdapterRegistry>();
            VendorsVoucherSearchResultDtoToEntityAdapter adapter = new VendorsVoucherSearchResultDtoToEntityAdapter(new Mock<IAdapterRegistry>().Object, new Mock<ILogger>().Object);
            var vendorsVoucherSearchResultEntity = adapter.MapToType(vendorSearchResultDto);
            Assert.AreEqual(vendorSearchResultDto.VendorId, vendorsVoucherSearchResultEntity.VendorId);
            Assert.AreEqual(vendorSearchResultDto.VendorNameLines.Count, vendorsVoucherSearchResultEntity.VendorNameLines.Count);
            Assert.AreEqual(vendorSearchResultDto.VendorNameLines.First().Trim(), vendorsVoucherSearchResultEntity.VendorNameLines.First().Trim());
            Assert.AreEqual(vendorSearchResultDto.VendorMiscName, vendorsVoucherSearchResultEntity.VendorMiscName);
            Assert.AreEqual(vendorSearchResultDto.AddressLines.Count, vendorsVoucherSearchResultEntity.AddressLines.Count);
            Assert.AreEqual(vendorSearchResultDto.AddressLines.First().Trim(), vendorsVoucherSearchResultEntity.AddressLines.First().Trim());
            Assert.AreEqual(vendorSearchResultDto.Zip, vendorsVoucherSearchResultEntity.Zip);
            Assert.AreEqual(vendorSearchResultDto.Country, vendorsVoucherSearchResultEntity.Country);
            Assert.AreEqual(vendorSearchResultDto.FormattedAddress, vendorsVoucherSearchResultEntity.FormattedAddress);
            Assert.AreEqual(vendorSearchResultDto.City, vendorsVoucherSearchResultEntity.City);
            Assert.AreEqual(vendorSearchResultDto.Country, vendorsVoucherSearchResultEntity.Country);
            Assert.AreEqual(vendorSearchResultDto.AddressId, vendorsVoucherSearchResultEntity.AddressId);
            Assert.AreEqual(vendorSearchResultDto.IsInternationalAddress, vendorsVoucherSearchResultEntity.IsInternationalAddress);
            Assert.AreEqual(vendorSearchResultDto.MiscVendor, vendorsVoucherSearchResultEntity.MiscVendor);
            Assert.AreEqual(vendorSearchResultDto.ReImburseMyself, vendorsVoucherSearchResultEntity.ReImburseMyself);
        }

        #endregion

        #region Build service method
        /// <summary>
        /// Builds multiple voucher service objects.
        /// </summary>
        /// <returns>Nothing.</returns>
        private void BuildValidVoucherService()
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleViewPermissions, glUserRoleCreateUpdatePermissions });
            roleRepository = roleRepositoryMock.Object;

            var loggerObject = new Mock<ILogger>().Object;

            testVoucherRepository = new TestVoucherRepository();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();
            testGeneralLedgerAccountRepository = new TestGeneralLedgerAccountRepository();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var voucherDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Voucher, Dtos.ColleagueFinance.Voucher>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.Voucher, Dtos.ColleagueFinance.Voucher>()).Returns(voucherDtoAdapter);

            var reimburseMyselfDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult, Dtos.ColleagueFinance.VendorsVoucherSearchResult>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult, Dtos.ColleagueFinance.VendorsVoucherSearchResult>()).Returns(reimburseMyselfDtoAdapter);

            var procurementCriteria_Adapter = new AutoMapperAdapter<Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ProcurementDocumentFilterCriteria>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria, Domain.ColleagueFinance.Entities.ProcurementDocumentFilterCriteria>()).Returns(procurementCriteria_Adapter);

            // Set up the current user with a subset of projects and set up the service.
            service = new VoucherService(testVoucherRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, testGeneralLedgerAccountRepository,
                adapterRegistry.Object, currentUserFactory, roleRepository, staffRepositoryMock.Object, mockApprovalConfigurationRepositoryFalse.Object, mockProcurementsUtilityService.Object, loggerObject);
            service2 = new VoucherService(mockVoucherRepository.Object, mockGlConfigurationRepository.Object, mockGeneralLedgerUserRepository.Object, mockGeneralLedgerAccountRepository.Object,
                adapterRegistry.Object, currentUserFactory, roleRepository, staffRepositoryMock.Object, mockApprovalConfigurationRepositoryFalse.Object, mockProcurementsUtilityService.Object, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new VoucherService(testVoucherRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, testGeneralLedgerAccountRepository,
                adapterRegistry.Object, noPermissionsUser, roleRepository, staffRepositoryMock.Object, mockApprovalConfigurationRepositoryFalse.Object, mockProcurementsUtilityService.Object, loggerObject);

            // Build the voucherApprovalRolesService object.
            voucherApprovalRolesService = new VoucherService(mockVoucherRepository.Object, testGeneralLedgerConfigurationRepository, mockGeneralLedgerUserRepository.Object, testGeneralLedgerAccountRepository,
                adapterRegistry.Object, currentUserFactory, roleRepository, staffRepositoryMock.Object, mockApprovalConfigurationRepositoryTrue.Object, mockProcurementsUtilityService.Object, loggerObject);


            vendorEntities =
                    new Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult()
                    {
                        VendorId = "0000192",
                        VendorNameLines = new List<string> { "Blue Cross Office supply" },
                        VendorMiscName = null,
                        AddressLines = new List<string> { "PO Box 69845" },
                        City = "Minneapolis",
                        State = "MN",
                        Zip = "55430",
                        Country = "",
                        FormattedAddress = "PO Box 69845 Minneapolis MN 55430",
                        AddressId = "143"
                    };
            voucherCreateUpdateRequest = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherCreateUpdateRequest
            {
                ConfEmailAddresses = new List<string> { "abc@gmail.com" },
                PersonId = currentUserFactory.CurrentUser.PersonId,
                VendorsVoucherInfo = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorsVoucherSearchResult()
                {
                    VendorId = "0000192",
                    VendorNameLines = new List<string> { "Blue Cross Office supply" },
                    VendorMiscName = null,
                    AddressLines = new List<string> { "PO Box 69845" },
                    City = "Minneapolis",
                    State = "MN",
                    Zip = "55430",
                    Country = "",
                    FormattedAddress = "PO Box 69845 Minneapolis MN 55430",
                    AddressId = "143"
                },
                Voucher = new Dtos.ColleagueFinance.Voucher2()
                {
                    Date = new DateTime(2020, 4, 6),
                    DueDate = new DateTime(2020, 4, 10),
                    InvoiceNumber = "001230349",
                    LineItems = new List<Dtos.ColleagueFinance.LineItem>()
                    {
                        new Dtos.ColleagueFinance.LineItem()
                        {
                            Description = "Paper",
                            Quantity = 1,
                            Price =2,
                            ExtendedPrice =2,
                            GlDistributions =new List<Dtos.ColleagueFinance.LineItemGlDistribution>()
                            {
                                new Dtos.ColleagueFinance.LineItemGlDistribution()
                                {
                                    Amount =1,
                                    GlAccount = "11_00_01_02_10000_54004",
                                    FormattedGlAccount = "11_00_01_02_10000_54004",
                                    ProjectNumber = "FVS-001"
                                },

                                new Dtos.ColleagueFinance.LineItemGlDistribution()
                                {
                                    Amount =1,
                                    GlAccount = "11_00_01_02_10000_54004",
                                    FormattedGlAccount = "11_00_01_02_10000_54004",
                                }
                            },
                            Comments = "new line item",
                            ReqLineItemTaxCodes = new List<Dtos.ColleagueFinance.LineItemReqTax>()
                            {
                                new Dtos.ColleagueFinance.LineItemReqTax()
                                {
                                    TaxReqTaxCode = "FL1",
                                    TaxReqTaxCodeDescription = "FL1"
                                },
                                 new Dtos.ColleagueFinance.LineItemReqTax()
                                 {
                                    TaxReqTaxCode = "FL2",
                                    TaxReqTaxCodeDescription = "FL2"
                                 }
                            }
                        }
                    }
                }
            };
            // response object for mocking
            voucherCreateUpdateResponse = new VoucherCreateUpdateResponse
            {
                ErrorMessages = new List<string>(),
                ErrorOccured = false,
                VoucherDate = new DateTime(2020, 4, 6),
                VoucherId = "V000100",
                WarningMessages = new List<string>(),
                WarningOccured = false
            };
            mockVoucherRepository.Setup(r => r.CreateVoucherAsync(voucherCreateUpdateRequestEntity)).ReturnsAsync(voucherCreateUpdateResponse);

            filterCriteria = new Ellucian.Colleague.Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria();
            filterCriteria.PersonId = currentUserFactory.CurrentUser.PersonId;
            filterCriteria.VendorIds = new List<string>() { "0001234" };

            filterCriteriaDomainEntity = new ProcurementDocumentFilterCriteria();
            filterCriteriaDomainEntity.PersonId = currentUserFactory.CurrentUser.PersonId;
            filterCriteriaDomainEntity.VendorIds = new List<string>() { "0001234" };
        }
        #endregion

        #region "VoidVoucherAsync"

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoidVoucherAsync_NullvoucherVoidRequest_ArgumentNullException()
        {
            await service2.VoidVoucherAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoidVoucherAsync_EmptyPersonId_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest();
            abc.PersonId = "";
            await service2.VoidVoucherAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoidVoucherAsync_EmptyVoucherId_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest();
            abc.PersonId = "0000004";
            abc.VoucherId = "";
            await service2.VoidVoucherAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task VoidVoucherAsync_EmptyEmail_ArgumentNullException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest();
            abc.PersonId = "000123";
            abc.VoucherId = "000123";
            abc.ConfirmationEmailAddresses = "";
            await service2.VoidVoucherAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VoidVoucherAsync_NotLoggedInuser()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest();
            abc.PersonId = "000123";
            abc.VoucherId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";
            await service2.VoidVoucherAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VoidVoucherAsync_StaffRecordMissingException()
        {
            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest();
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));
            abc.PersonId = "0000004";
            abc.VoucherId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";
            await service2.VoidVoucherAsync(abc);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task VoidVoucherAsync_PermissionsException()
        {
            List<Domain.Entities.Role> roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"UPDATE.PO")
                    };

            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(roles);

            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest();

            abc.PersonId = "0000004";
            abc.VoucherId = "V000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";
            await service2.VoidVoucherAsync(abc);
        }

        [TestMethod]
        public async Task VoidVoucherAsync_Success()
        {
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));


            Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest abc = new Ellucian.Colleague.Dtos.ColleagueFinance.VoucherVoidRequest();

            abc.PersonId = "0000001";
            abc.VoucherId = "000123";
            abc.ConfirmationEmailAddresses = "abc@gmail.com";


            var adapterRegistry = new Mock<IAdapterRegistry>();
            var loggerObject = new Mock<ILogger>().Object;

            var voucherDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.VoucherVoidResponse, Dtos.ColleagueFinance.VoucherVoidResponse>(adapterRegistry.Object, loggerObject);

            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.VoucherVoidResponse, Dtos.ColleagueFinance.VoucherVoidResponse>()).Returns(voucherDtoAdapter);

            var voidVoucherDtos = await service2.VoidVoucherAsync(abc);

            Assert.IsNotNull(voidVoucherDtos);
        }

        #endregion

        #region "GetVouchersByVendorAndInvoiceNoAsync"

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVouchersByVendorAndInvoiceNoAsync_ArgumentNullException()
        {
            await service2.GetVouchersByVendorAndInvoiceNoAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVouchersByVendorAndInvoiceNoAsync_VendorisNull_ArgumentNullException()
        {
            await service2.GetVouchersByVendorAndInvoiceNoAsync(null, "1234");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVouchersByVendorAndInvoiceNoAsync_InvoicNoIsNull_ArgumentNullException()
        {
            await service2.GetVouchersByVendorAndInvoiceNoAsync("0001234", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetVouchersByVendorAndInvoiceNoAsync_EmptyCriteria_ArgumentNullException()
        {
            await service2.GetVouchersByVendorAndInvoiceNoAsync(string.Empty, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetVouchersByVendorAndInvoiceNoAsync_PermissionsException()
        {
            List<Domain.Entities.Role> roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"VIEW.REQ")
                    };

            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(roles);

            var invoiceNumber = "IN12345";
            var vendorId = "0001234";
            await service2.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);
        }

        [TestMethod]
        public async Task GetVouchersByVendorAndInvoiceNoAsync_RepositoryReturnsNull()
        {
            var invoiceNumber = "1234567";
            var vendorId = "0000111";

            GeneralLedgerAccountStructure accountStructure = await testGeneralLedgerConfigurationRepository.GetAccountStructureAsync();
            this.mockGlConfigurationRepository.Setup(repo => repo.GetAccountStructureAsync()).Returns(Task.FromResult(accountStructure));

            this.mockVoucherRepository.Setup(repo => repo.GetVouchersByVendorAndInvoiceNoAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() => null);


            var voucher2Dtos = await service2.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);

            Assert.IsNotNull(voucher2Dtos);
            Assert.IsTrue(voucher2Dtos.ToList().Count == 0);
        }

        [TestMethod]
        public async Task GetVouchersByVendorAndInvoiceNoAsync_Success()
        {
            var invoiceNumber = "IN12345";
            var vendorId = "0001234";

            var voucher2Dtos = await service.GetVouchersByVendorAndInvoiceNoAsync(vendorId, invoiceNumber);

            Assert.IsNotNull(voucher2Dtos);
            Assert.IsTrue(voucher2Dtos.ToList().Count > 0);
        }

        #endregion

        #region QueryVoucherSummariesAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryVoucherSummariesAsync_EmptyCriteria_ArgumentNullException()
        {
            await service2.QueryVoucherSummariesAsync(new Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task QueryVoucherSummariesAsync_NullCriteria_ArgumentNullException()
        {
            await service2.QueryVoucherSummariesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryVoucherSummariesAsync_CurrentUserDifferentFromRequest()
        {
            filterCriteria = new Dtos.ColleagueFinance.ProcurementDocumentFilterCriteria();
            filterCriteria.PersonId = "0016357";

            await service2.QueryVoucherSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryVoucherSummariesAsync_MissingPermissionException()
        {
            await serviceForNoPermission.QueryVoucherSummariesAsync(filterCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task QueryVoucherSummariesAsync_StaffRecordMissingException()
        {
            Domain.Base.Entities.Staff nullStaff = null;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(nullStaff));
            await service2.QueryVoucherSummariesAsync(filterCriteria);
        }

        [TestMethod]
        public async Task QueryVoucherSummariesAsync()
        {
            var voucherId = "31";
            var personId = currentUserFactory.CurrentUser.PersonId;
            staffRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(new Domain.Base.Entities.Staff("0000001", "Test LastName")));
            var voucherSummaryListDto = await service.QueryVoucherSummariesAsync(filterCriteria);

            // Get the requisition domain entity from the test repository
            var voucherSummaryListDomainEntity = await testVoucherRepository.QueryVoucherSummariesAsync(filterCriteriaDomainEntity);

            Assert.IsNotNull(voucherSummaryListDto);
            Assert.AreEqual(voucherSummaryListDto.ToList().Count, voucherSummaryListDomainEntity.ToList().Count);

            var voucherDto = voucherSummaryListDto.Where(x => x.Id == voucherId).FirstOrDefault();
            var voucherDomainEntity = voucherSummaryListDomainEntity.Where(x => x.Id == voucherId).FirstOrDefault();

            // Confirm that the data in the DTO matches the domain entity
            Assert.AreEqual(voucherDto.Id, voucherDomainEntity.Id);
            Assert.AreEqual(voucherDto.InvoiceNumber, voucherDomainEntity.InvoiceNumber);
            Assert.AreEqual(voucherDto.Amount, voucherDomainEntity.Amount);
            Assert.AreEqual(voucherDto.Date, voucherDomainEntity.Date);
            Assert.AreEqual(voucherDto.RequestorName, voucherDomainEntity.RequestorName);
            Assert.AreEqual(voucherDto.Status.ToString(), voucherDomainEntity.Status.ToString());
            Assert.AreEqual(voucherDto.VendorId, voucherDomainEntity.VendorId);
            Assert.AreEqual(voucherDto.VendorName, voucherDomainEntity.VendorName);

            // Confirm that the data in the list of purchase order DTOs matches the domain entity
            for (int i = 0; i < voucherDto.PurchaseOrders.Count(); i++)
            {
                Assert.AreEqual(voucherDto.PurchaseOrders[i].Id, voucherDomainEntity.PurchaseOrders[i].Id);
                Assert.AreEqual(voucherDto.PurchaseOrders[i].Number, voucherDomainEntity.PurchaseOrders[i].Number);
            }
        }
        #endregion
    }
}
