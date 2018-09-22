// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
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

        private TestVoucherRepository testVoucherRepository;
        private TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        private TestGeneralLedgerUserRepository testGeneralLedgerUserRepository;
        private Mock<IVoucherRepository> mockVoucherRepository;

        private Mock<IRoleRepository> roleRepositoryMock;
        private IRoleRepository roleRepository;

        private Domain.Entities.Permission permissionViewVoucher;
        protected Domain.Entities.Role glUserRoleViewPermissions = new Domain.Entities.Role(229, "VOUCHER.VIEWER");

        private GeneralLedgerCurrentUser.UserFactory currentUserFactory = new GeneralLedgerCurrentUser.UserFactory();
        private GeneralLedgerCurrentUser.UserFactoryNone noPermissionsUser = new GeneralLedgerCurrentUser.UserFactoryNone();

        private Mock<IGeneralLedgerConfigurationRepository> mockGlConfigurationRepository;
        private Mock<IGeneralLedgerUserRepository> mockGeneralLedgerUserRepository;
        private int versionNumber;

        [TestInitialize]
        public void Initialize()
        {
            roleRepositoryMock = new Mock<IRoleRepository>();

            // Initialize the mock voucher repository
            mockVoucherRepository = new Mock<IVoucherRepository>();
            mockGlConfigurationRepository = new Mock<IGeneralLedgerConfigurationRepository>();
            mockGeneralLedgerUserRepository = new Mock<IGeneralLedgerUserRepository>();

            // Create permission domain entities for viewing the voucher.
            permissionViewVoucher = new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewVoucher);
            // Assign view permission to the role that has view permissions.
            glUserRoleViewPermissions.AddPermission(permissionViewVoucher);

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

            testVoucherRepository = null;
            testGeneralLedgerConfigurationRepository = null;
            testGeneralLedgerUserRepository = null;
            mockVoucherRepository = null;
            mockGlConfigurationRepository = null;
            mockGeneralLedgerUserRepository = null;

            roleRepositoryMock = null;
            roleRepository = null;
            glUserRoleViewPermissions = null;
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

                var voucherDto = await service2.GetVoucher2Async("1");
            }
            catch (ArgumentNullException aex)
            {
                actualParamName = aex.ParamName;
            }

            Assert.AreEqual(expectedParamName, actualParamName);
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

        #region Build service method
        /// <summary>
        /// Builds multiple voucher service objects.
        /// </summary>
        /// <returns>Nothing.</returns>
        private void BuildValidVoucherService()
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { glUserRoleViewPermissions });
            roleRepository = roleRepositoryMock.Object;

            var loggerObject = new Mock<ILogger>().Object;

            testVoucherRepository = new TestVoucherRepository();
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            testGeneralLedgerUserRepository = new TestGeneralLedgerUserRepository();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var voucherDtoAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.Voucher, Dtos.ColleagueFinance.Voucher>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.ColleagueFinance.Entities.Voucher, Dtos.ColleagueFinance.Voucher>()).Returns(voucherDtoAdapter);

            // Set up the current user with a subset of projects and set up the service.
            service = new VoucherService(testVoucherRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);
            service2 = new VoucherService(mockVoucherRepository.Object, mockGlConfigurationRepository.Object, mockGeneralLedgerUserRepository.Object, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);

            // Build a service for a user that has no permissions.
            serviceForNoPermission = new VoucherService(testVoucherRepository, testGeneralLedgerConfigurationRepository, testGeneralLedgerUserRepository, adapterRegistry.Object, noPermissionsUser, roleRepository, loggerObject);
        }
        #endregion
    }
}
