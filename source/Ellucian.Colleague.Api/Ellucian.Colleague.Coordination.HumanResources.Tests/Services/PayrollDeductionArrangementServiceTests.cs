/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PayrollDeductionArrangementServiceTests
    {
        [TestClass]
        public class PayrollDeductionArrangementServiceTests_GET : HumanResourcesServiceTestsSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role payrollDeductionArrangementCreateRole = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.PAYROLL.DEDUCTION.ARRANGEMENTS");

            private Mock<IPayrollDeductionArrangementRepository> payrollDeductionArrangementRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IHumanResourcesReferenceDataRepository> referenceDataRepositoryMock;
            public ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            PayrollDeductionArrangementService payrollDeductionArrangementService;
            List<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements> payrollDeductionArrangementEntities;
            Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int> payrollDeductionArrangementTuple;
            List<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason> payrollDeductionArrangementChangeReasonEntityList = new List<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason>();
            List<Domain.HumanResources.Entities.DeductionType> deductionTypeEntities;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                payrollDeductionArrangementRepositoryMock = new Mock<IPayrollDeductionArrangementRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                referenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.PersonDecuctionArrangementsUserFactory();

                BuildData();

                payrollDeductionArrangementService = new PayrollDeductionArrangementService(payrollDeductionArrangementRepositoryMock.Object, referenceDataRepositoryMock.Object, personRepositoryMock.Object,
                    baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                payrollDeductionArrangementRepositoryMock = null;
                personRepositoryMock = null;
                referenceDataRepositoryMock = null;
                payrollDeductionArrangementService = null;
                payrollDeductionArrangementChangeReasonEntityList = null;
            }

            [TestMethod]
            public async Task PayrollDeductionArrangement_GetAll()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                var actuals = await payrollDeductionArrangementService.GetPayrollDeductionArrangementsAsync(0, 100, true, "", "", "", "");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task PayrollDeductionArrangement_GetAll_WithFilterValues()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                var actuals = await payrollDeductionArrangementService.GetPayrollDeductionArrangementsAsync(0, 100, true, "0218817f-b6e0-4184-83ae-b6815e5d1604", "contributionGuid", "16f809ba-f266-458f-b7c1-d332eb5bd343", "active");
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task PayrollDeductionArrangement_GetAll_NoResults()
            {
                payrollDeductionArrangementTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int>(new List<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>(), 0);
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payrollDeductionArrangementTuple);

                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                var actuals = await payrollDeductionArrangementService.GetPayrollDeductionArrangementsAsync(0, 100, true, "", "", "", "");
                Assert.AreEqual(0, actuals.Item2);
            }

            [TestMethod]
            public async Task PayrollDeductionArrangement_GetById()
            {
                var expected = payrollDeductionArrangementEntities.FirstOrDefault();
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(expected);

                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                var actual = await payrollDeductionArrangementService.GetPayrollDeductionArrangementsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>());
                Assert.IsNotNull(actual);
            }        

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PayrollDeductionArrangement_GetAll_DeductionEntity_RepositoryException()
            {
                payrollDeductionArrangementEntities = new List<Domain.HumanResources.Entities.PayrollDeductionArrangements>() 
            {
                new Domain.HumanResources.Entities.PayrollDeductionArrangements("643edf74-10d3-4d97-800e-9d72c2e3cae7", "0003582")
                    {
                        AmountPerPayment = 20,
                        ChangeReason = "MAR",
                        DeductionTypeCode = "CAPLL",
                        Id = "1028",
                        Interval = 2,
                        StartDate = new DateTime(2017, 1, 1),
                        Status = "Active",
                        TotalAmount = 100
                    }
            };
                payrollDeductionArrangementTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int>(payrollDeductionArrangementEntities, payrollDeductionArrangementEntities.Count());
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payrollDeductionArrangementTuple);

                referenceDataRepositoryMock.Setup(repo => repo.GetDeductionTypesAsync(It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                referenceDataRepositoryMock.Setup(repo => repo.GetDeductionTypesGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());


                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                var actuals = await payrollDeductionArrangementService.GetPayrollDeductionArrangementsAsync(0, 100, true, "", "", "", "");
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PayrollDeductionArrangement_GetAll_PersonId_ArgumentException()
            {
                payrollDeductionArrangementEntities = new List<Domain.HumanResources.Entities.PayrollDeductionArrangements>() 
                {
                    new Domain.HumanResources.Entities.PayrollDeductionArrangements("643edf74-10d3-4d97-800e-9d72c2e3cae7", "00035820")
                        {
                            AmountPerPayment = 20,
                            ChangeReason = "MAR",
                            DeductionTypeCode = "CAPL",
                            Id = "1028",
                            Interval = 2,
                            StartDate = new DateTime(2017, 1, 1),
                            Status = "Active",
                            TotalAmount = 100
                        }
                };
                payrollDeductionArrangementTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int>(payrollDeductionArrangementEntities, payrollDeductionArrangementEntities.Count());
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payrollDeductionArrangementTuple);
                personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("");

                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                var actuals = await payrollDeductionArrangementService.GetPayrollDeductionArrangementsAsync(0, 100, true, "", "", "", "");
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task PayrollDeductionArrangement_GetAll_DeductionArrangementTypeCode_ArgumentException()
            {
                payrollDeductionArrangementEntities = new List<Domain.HumanResources.Entities.PayrollDeductionArrangements>() 
                {
                    new Domain.HumanResources.Entities.PayrollDeductionArrangements("643edf74-10d3-4d97-800e-9d72c2e3cae7", "00035820")
                        {
                            AmountPerPayment = 20,
                            ChangeReason = "MARCH",
                            DeductionTypeCode = "CAPL",
                            Id = "1028",
                            Interval = 2,
                            StartDate = new DateTime(2017, 1, 1),
                            Status = "Active",
                            TotalAmount = 100
                        }
                };
                payrollDeductionArrangementTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int>(payrollDeductionArrangementEntities, payrollDeductionArrangementEntities.Count());
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payrollDeductionArrangementTuple);

                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                var actuals = await payrollDeductionArrangementService.GetPayrollDeductionArrangementsAsync(0, 100, true, "", "", "", "");
            }

            private void BuildData()
            {
                //Country
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("CANADA");

                //Deduction Types
                deductionTypeEntities = new List<Domain.HumanResources.Entities.DeductionType>() 
            {
                new Domain.HumanResources.Entities.DeductionType("16f809ba-f266-458f-b7c1-d332eb5bd343", "CAPL", "Coll Adv Pldg Pyment"),
                new Domain.HumanResources.Entities.DeductionType("4bbcea48-a042-47c0-811f-f128e66e2977", "CAMB", "Colleague Adv Membership Dues")
            };
                referenceDataRepositoryMock.Setup(repo => repo.GetDeductionTypesAsync(It.IsAny<bool>())).ReturnsAsync(deductionTypeEntities);

                //PayrollDeductionArrangementChangeReason
                payrollDeductionArrangementChangeReasonEntityList = new List<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason>() 
            {
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("625c69ff-280b-4ed3-9474-662a43616a8a", "MAR", "Marriage"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("bfea651b-8e27-4fcd-abe3-04573443c04c", "BOC", "Birth of Child"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("9ae3a175-1dfd-4937-b97b-3c9ad596e023", "SJC", "Spouse Job Change"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("e9e6837f-2c51-431b-9069-4ac4c0da3041", "DIV", "Divorce"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("80779c4f-b2ac-4ad4-a970-ca5699d9891f", "ADP", "Adoption"),
                new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("ae21110e-991e-405e-9d8b-47eeff210a2d", "DEA", "Death"),
            };
                referenceDataRepositoryMock.Setup(repo => repo.GetPayrollDeductionArrangementChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(payrollDeductionArrangementChangeReasonEntityList);

                //Deduction Types
                deductionTypeEntities = new List<Domain.HumanResources.Entities.DeductionType>() 
                {
                    new Domain.HumanResources.Entities.DeductionType("16f809ba-f266-458f-b7c1-d332eb5bd343", "CAPL", "Coll Adv Pldg Pyment"),
                    new Domain.HumanResources.Entities.DeductionType("4bbcea48-a042-47c0-811f-f128e66e2977", "CAMB", "Colleague Adv Membership Dues")
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetDeductionTypesAsync(It.IsAny<bool>())).ReturnsAsync(deductionTypeEntities);

                //person id's
                personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0218817f-b6e0-4184-83ae-b6815e5d1604");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");
                Dictionary<string, string> personGuidCollection = new Dictionary<string, string>();
                personGuidCollection.Add("0003582", "73251435-1880-4bbf-aab9-4abe6634b2c5");
                personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);

                //PayrollDeductionArrangements
                payrollDeductionArrangementEntities = new List<Domain.HumanResources.Entities.PayrollDeductionArrangements>() 
            {
                new Domain.HumanResources.Entities.PayrollDeductionArrangements("643edf74-10d3-4d97-800e-9d72c2e3cae7", "0003582")
                {
                    AmountPerPayment = 20,
                    ChangeReason = "MAR",
                    Id = "1028",
                    Interval = 2,
                    StartDate = new DateTime(2017, 1, 1),
                    Status = "Active",
                    TotalAmount = 100,
                    CommitmentContributionId = "123",
                    CommitmentType = "Pledge",
                    MonthlyPayPeriods = new List<int?>(){ 2016, 2017 }
                },
                new Domain.HumanResources.Entities.PayrollDeductionArrangements("d272e405-048a-46fa-817b-b1ceb3f00801", "0003582")
                {
                    AmountPerPayment = 10,
                    ChangeReason = "BOC",
                    DeductionTypeCode = "CAMB",
                    Id = "1029",
                    Interval = 2,
                    StartDate = new DateTime(2017, 1, 1),
                    Status = "Active",
                    TotalAmount = 120
                }
            };
                payrollDeductionArrangementTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int>(payrollDeductionArrangementEntities, payrollDeductionArrangementEntities.Count());
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payrollDeductionArrangementTuple);
            }
        }

        [TestClass]
        public class PayrollDeductionArrangementServiceTests_PUT_POST : HumanResourcesServiceTestsSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role payrollDeductionArrangementCreateRole = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.PAYROLL.DEDUCTION.ARRANGEMENTS");

            private Mock<IPayrollDeductionArrangementRepository> payrollDeductionArrangementRepositoryMock;
            private Mock<IPersonRepository> personRepositoryMock;
            private Mock<IHumanResourcesReferenceDataRepository> referenceDataRepositoryMock;
            public ICurrentUserFactory currentUserFactory;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            PayrollDeductionArrangementService payrollDeductionArrangementService;
            List<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements> payrollDeductionArrangementEntities;
            Dtos.PayrollDeductionArrangements payrollDeductionArrangementDto;
            Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int> payrollDeductionArrangementTuple;
            List<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason> payrollDeductionArrangementChangeReasonEntityList = new List<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason>();
            List<Domain.HumanResources.Entities.DeductionType> deductionTypeEntities;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                payrollDeductionArrangementRepositoryMock = new Mock<IPayrollDeductionArrangementRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                referenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.PersonDecuctionArrangementsUserFactory();

                BuildData();

                payrollDeductionArrangementService = new PayrollDeductionArrangementService(payrollDeductionArrangementRepositoryMock.Object, referenceDataRepositoryMock.Object, personRepositoryMock.Object,
                    baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                payrollDeductionArrangementRepositoryMock = null;
                personRepositoryMock = null;
                referenceDataRepositoryMock = null;
                payrollDeductionArrangementService = null;
                payrollDeductionArrangementChangeReasonEntityList = null;
            }

            [TestMethod]
            public async Task PayrollDeductionArrangement_PUT()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(payrollDeductionArrangementEntities.FirstOrDefault());


                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task PayrollDeductionArrangement_POST()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "00000000-0000-0000-0000-000000000000",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(payrollDeductionArrangementEntities.FirstOrDefault());


                var actuals = await payrollDeductionArrangementService.CreatePayrollDeductionArrangementsAsync(payrollDeductionArrangementDto);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            public async Task PayrollDeductionArrangement_PUT_Status_Not_Set()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(payrollDeductionArrangementEntities.FirstOrDefault());

                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayrollDeductionArrangement_PUT_CommitmentType_NotSet()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") },
                        Commitment = new Dtos.DtoProperties.PaymentTargetCommitment() { Type = CommitmentTypes.NotSet }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(payrollDeductionArrangementEntities.FirstOrDefault());

                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
            }

            [TestMethod]
            public async Task PayrollDeductionArrangement_PUT_CommitmentType_MembershipDues_Contribution()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") },
                        Commitment = new Dtos.DtoProperties.PaymentTargetCommitment() { Type = CommitmentTypes.MembershipDues, Contribution = "Contribution" }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(payrollDeductionArrangementEntities.FirstOrDefault());

                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
                Assert.IsNotNull(actuals);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayrollDeductionArrangement_PUT_Entity_KeyNotFound()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(() => null);

                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayrollDeductionArrangement_PUT_Id_Null_ArgumentNullException()
            {
                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("", payrollDeductionArrangementDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayrollDeductionArrangement_PUT_Dto_Null_ArgumentNullException()
            {
                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("1234", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayrollDeductionArrangement_PUT_Person_Null_ArgumentNullException()
            {
                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        Interval = 123,
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = null,
                    StartDate = new DateTime(2017, 1, 1),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayrollDeductionArrangement_PUT_PersonId_Null_ArgumentNullException()
            {
                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        Interval = 123,
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2(),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayrollDeductionArrangement_PUT_PaymentTarget_Null_ArgumentNullException()
            {
                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = null,
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        Interval = 123,
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PayrollDeductionArrangement_PUT_DeductionTypeId_Invalid_ArgumentNullException()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd344") }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        Interval = 123,
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(payrollDeductionArrangementEntities.FirstOrDefault());

                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PayrollDeductionArrangement_PUT_ChangeReasonId_Invalid_ArgumentNullException()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04d"),
                    Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        Interval = 123,
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(payrollDeductionArrangementEntities.FirstOrDefault());

                var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task PayrollDeductionArrangement_PUT_PermissionException()
            //{
            //    payrollDeductionArrangementDto = new PayrollDeductionArrangements()
            //    {
            //        amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
            //        ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
            //        Id = "643edf74-10d3-4d97-800e-9d72c2e3cae7",
            //        PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
            //        {
            //            Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") }
            //        },
            //        PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
            //        {
            //            Interval = 123,
            //            MonthlyPayPeriods = new[] { 2 }
            //        },
            //        Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
            //        StartDate = new DateTime(2017, 1, 1),
            //        Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
            //        TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
            //        {
            //            Currency = CurrencyCodes.USD,
            //            Value = 200
            //        }
            //    };

            //    payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
            //    personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

            //    payrollDeductionArrangementRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(payrollDeductionArrangementEntities.FirstOrDefault());

            //    var actuals = await payrollDeductionArrangementService.UpdatePayrollDeductionArrangementsAsync("643edf74-10d3-4d97-800e-9d72c2e3cae7", payrollDeductionArrangementDto);
            //    Assert.IsNotNull(actuals);
            //}

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PayrollDeductionArrangement_POST_Dto_Null()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = null;

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(payrollDeductionArrangementEntities.FirstOrDefault());

                var actuals = await payrollDeductionArrangementService.CreatePayrollDeductionArrangementsAsync(payrollDeductionArrangementDto);
            }

            //[TestMethod]
            //[ExpectedException(typeof(PermissionsException))]
            //public async Task PayrollDeductionArrangement_PermissionsException()
            //{
            //    payrollDeductionArrangementDto = new PayrollDeductionArrangements();

            //    var actuals = await payrollDeductionArrangementService.CreatePayrollDeductionArrangementsAsync(payrollDeductionArrangementDto);
            //}

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task PayrollDeductionArrangement_POST_Null_Entity_KeyNotFoundException()
            {
                payrollDeductionArrangementCreateRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.CreatePayrollDeductionArrangements));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { payrollDeductionArrangementCreateRole });

                payrollDeductionArrangementDto = new PayrollDeductionArrangements()
                {
                    amountPerPayment = new Dtos.DtoProperties.AmountDtoProperty() { Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value = 200 },
                    ChangeReason = new GuidObject2("bfea651b-8e27-4fcd-abe3-04573443c04c"),
                    Id = "00000000-0000-0000-0000-000000000000",
                    PaymentTarget = new Dtos.DtoProperties.PaymentTargetDtoProperty()
                    {
                        Deduction = new Dtos.DtoProperties.PaymentTargetDeduction() { DeductionType = new GuidObject2("16f809ba-f266-458f-b7c1-d332eb5bd343") }
                    },
                    PayPeriodOccurence = new Dtos.DtoProperties.PayPeriodOccurance()
                    {
                        MonthlyPayPeriods = new[] { 2 }
                    },
                    Person = new GuidObject2("73251435-1880-4bbf-aab9-4abe6634b2c5"),
                    StartDate = new DateTime(2017, 1, 1),
                    Status = Dtos.EnumProperties.PayrollDeductionArrangementStatuses.Active,
                    TotalAmount = new Dtos.DtoProperties.AmountDtoProperty()
                    {
                        Currency = CurrencyCodes.USD,
                        Value = 200
                    }
                };

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1028");
                personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0003582");

                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Domain.HumanResources.Entities.PayrollDeductionArrangements>())).ReturnsAsync(() => null);

                var actuals = await payrollDeductionArrangementService.CreatePayrollDeductionArrangementsAsync(payrollDeductionArrangementDto);
            }

            private void BuildData()
            {
                //Country
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetHostCountryAsync()).ReturnsAsync("CANADA");

                //Deduction Types
                deductionTypeEntities = new List<Domain.HumanResources.Entities.DeductionType>() 
                {
                    new Domain.HumanResources.Entities.DeductionType("16f809ba-f266-458f-b7c1-d332eb5bd343", "CAPL", "Coll Adv Pldg Pyment"),
                    new Domain.HumanResources.Entities.DeductionType("4bbcea48-a042-47c0-811f-f128e66e2977", "CAMB", "Colleague Adv Membership Dues")
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetDeductionTypesAsync(It.IsAny<bool>())).ReturnsAsync(deductionTypeEntities);

                //PayrollDeductionArrangementChangeReason
                payrollDeductionArrangementChangeReasonEntityList = new List<Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason>() 
                {
                    new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("625c69ff-280b-4ed3-9474-662a43616a8a", "MAR", "Marriage"),
                    new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("bfea651b-8e27-4fcd-abe3-04573443c04c", "BOC", "Birth of Child"),
                    new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("9ae3a175-1dfd-4937-b97b-3c9ad596e023", "SJC", "Spouse Job Change"),
                    new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("e9e6837f-2c51-431b-9069-4ac4c0da3041", "DIV", "Divorce"),
                    new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("80779c4f-b2ac-4ad4-a970-ca5699d9891f", "ADP", "Adoption"),
                    new Domain.HumanResources.Entities.PayrollDeductionArrangementChangeReason("ae21110e-991e-405e-9d8b-47eeff210a2d", "DEA", "Death"),
                };
                referenceDataRepositoryMock.Setup(repo => repo.GetPayrollDeductionArrangementChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(payrollDeductionArrangementChangeReasonEntityList);

                //person id's
                personRepositoryMock.Setup(repo => repo.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("0003582");
                Dictionary<string, string> personGuidCollection = new Dictionary<string, string>() ;
                personGuidCollection.Add("0003582", "73251435-1880-4bbf-aab9-4abe6634b2c5");
                personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);

                //PayrollDeductionArrangements
                payrollDeductionArrangementEntities = new List<Domain.HumanResources.Entities.PayrollDeductionArrangements>() 
                {
                    new Domain.HumanResources.Entities.PayrollDeductionArrangements("643edf74-10d3-4d97-800e-9d72c2e3cae7", "0003582")
                    {
                        AmountPerPayment = 20,
                        ChangeReason = "MAR",
                        Id = "1028",
                        Interval = 2,
                        StartDate = new DateTime(2017, 1, 1),
                        Status = "Active",
                        TotalAmount = 100,
                        CommitmentContributionId = "123",
                        CommitmentType = "Pledge",
                        MonthlyPayPeriods = new List<int?>(){ 2016, 2017 }
                    },
                    new Domain.HumanResources.Entities.PayrollDeductionArrangements("d272e405-048a-46fa-817b-b1ceb3f00801", "0003582")
                    {
                        AmountPerPayment = 10,
                        ChangeReason = "BOC",
                        DeductionTypeCode = "CAMB",
                        Id = "1029",
                        Interval = 2,
                        StartDate = new DateTime(2017, 1, 1),
                        Status = "Active",
                        TotalAmount = 120
                    }
                };
                payrollDeductionArrangementTuple = new Tuple<IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.PayrollDeductionArrangements>, int>(payrollDeductionArrangementEntities, payrollDeductionArrangementEntities.Count());
                payrollDeductionArrangementRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(payrollDeductionArrangementTuple);
            }
        }
    }
}
