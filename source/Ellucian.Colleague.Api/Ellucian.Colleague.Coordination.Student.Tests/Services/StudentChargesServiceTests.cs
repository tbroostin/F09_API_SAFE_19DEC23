// Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using AccountReceivableType = Ellucian.Colleague.Domain.Student.Entities.AccountReceivableType;
using StudentCharge = Ellucian.Colleague.Domain.Student.Entities.StudentCharge;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentChargeServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Ellucian.Colleague.Domain.Entities.Role ViewStudentChargeRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.ACADEMIC.PERIOD.PROFILE");
            protected Ellucian.Colleague.Domain.Entities.Role CreateStudentChargeRole = new Ellucian.Colleague.Domain.Entities.Role(2, "CREATE.STUDENT.CHARGES");

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "STU1",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() {},
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            // Represents a third party system like ILP
            public class ThirdPartyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "ILP",
                            PersonId = "ILP",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "ILP",
                            Roles = new List<string>() {"VIEW.STUDENT.ACADEMIC.PERIOD.PROFILE", "CREATE.STUDENT.CHARGES"},
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }



        [TestClass]
        public class StudentChargeServiceTestsGet : CurrentUserSetup
        {
            private Mock<IStudentChargeRepository> _studentChargeRepositoryMock;
            private Mock<IStudentRepository> _studentRepositoryMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
            private Mock<ICatalogRepository> _catalogRepositoryMock;
            private Mock<ITermRepository> _termRepositoryMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepositoryMock;
            private Mock<IStudentProgramRepository> _studentProgramRepositoryMock;
            private Mock<IAcademicCreditRepository> _academicCreditRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<ILogger> _loggerMock;
            private ICurrentUserFactory _currentUserFactory;
            private StudentChargeService _studentChargeService;


            private Tuple<IEnumerable<StudentCharge>, int> _stuChargesTuple;

            private IEnumerable<Ellucian.Colleague.Dtos.StudentCharge> _studentChargeDtos;
            private IEnumerable<Ellucian.Colleague.Dtos.StudentCharge1> _studentCharge1Dtos;

            private IEnumerable<StudentCharge> _studentChargeEntities;
            private IEnumerable<Domain.Student.Entities.AcademicPeriod> _academicPeriodEntities;


            private List<Domain.Student.Entities.AccountingCode> _accountingCodeEntities;
            private List<Domain.Student.Entities.AccountReceivableType> _accountReceivableTypeEntities;

            private IEnumerable<Domain.Student.Entities.Term> _termEntities;

            private const string PersonGuid = "ed809943-eb26-42d0-9a95-d8db912a581f";


            [TestInitialize]
            public void Initialize()
            {
                _studentChargeRepositoryMock = new Mock<IStudentChargeRepository>();
                _studentRepositoryMock = new Mock<IStudentRepository>();
                _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _catalogRepositoryMock = new Mock<ICatalogRepository>();
                _termRepositoryMock = new Mock<ITermRepository>();
                _personRepositoryMock = new Mock<IPersonRepository>();
                _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                _configurationRepositoryMock = new Mock<IConfigurationRepository>();
                _studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                _academicCreditRepositoryMock = new Mock<IAcademicCreditRepository>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactory = new CurrentUserSetup.ThirdPartyUserFactory();

                BuildData();
                BuildMocks();
                _studentChargeService = new StudentChargeService(_studentChargeRepositoryMock.Object,
                    _personRepositoryMock.Object, _referenceDataRepositoryMock.Object, _studentReferenceDataRepositoryMock.Object,
                    _termRepositoryMock.Object,_configurationRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactory, _roleRepositoryMock.Object,
                    _loggerMock.Object);
            }

            private void BuildData()
            {
                _studentChargeEntities = new List<StudentCharge>()
                {
                    new Domain.Student.Entities.StudentCharge("00001", "tuition", Convert.ToDateTime("2016-10-17"))
                    {
                        AccountsReceivableTypeCode = "01",
                        AccountsReceivableCode = "TUIIN",
                        ChargeAmount = 400m,
                        ChargeCurrency = "CAD",
                        Comments = new List<string> {"This is a comment"},
                        InvoiceItemID = "168999",
                        Guid = "54c677e7-24ad-4591-be3f-d2175b7b0710",
                        Term = "2016/Spr",

                    }
                };
                _stuChargesTuple = new Tuple<IEnumerable<StudentCharge>, int>(_studentChargeEntities, _studentChargeEntities.Count());

                _studentChargeDtos = new List<Dtos.StudentCharge>
                {
                    new Dtos.StudentCharge()
                    {
                        Id = "54c677e7-24ad-4591-be3f-d2175b7b0710",
                        AcademicPeriod = new GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                        AccountingCode = new GuidObject2("d3a4a98f-e858-495e-b12b-e6464442bbf4"),
                        AccountReceivableType = new GuidObject2("3c29e3f2-0399-4f73-aea7-ed424e6c5801"),
                        ChargeableOn = Convert.ToDateTime("2016-10-17"),
                        ChargedAmount = new ChargedAmountDtoProperty()
                        {
                            Amount = new AmountDtoProperty() {Currency = CurrencyCodes.CAD, Value = 400},

                        },
                        ChargeType = StudentChargeTypes.tuition,
                        Comments = new List<string>() {"This is a comment"},
                        Person = new GuidObject2("b371fba4-797d-4c2c-8adc-bedd6d9db730")

                    },

                };

                _studentCharge1Dtos = new List<Dtos.StudentCharge1>
                {
                    new Dtos.StudentCharge1()
                    {
                        Id = "54c677e7-24ad-4591-be3f-d2175b7b0710",
                        AcademicPeriod = new GuidObject2("b9691210-8516-45ca-9cd1-7e5aa1777234"),
                        FundingDestination = new GuidObject2("d3a4a98f-e858-495e-b12b-e6464442bbf4"),
                        FundingSource = new GuidObject2("3c29e3f2-0399-4f73-aea7-ed424e6c5801"),
                        ChargeableOn = Convert.ToDateTime("2016-10-17"),
                        ChargedAmount = new ChargedAmountDtoProperty()
                        {
                            Amount = new AmountDtoProperty() {Currency = CurrencyCodes.CAD, Value = 400},

                        },
                        ChargeType = StudentChargeTypes.tuition,
                        Comments = new List<string>() {"This is a comment"},
                        Person = new GuidObject2("b371fba4-797d-4c2c-8adc-bedd6d9db730")

                    },

                };


                _termEntities = new List<Term>()
                {

                    new Term("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "", new DateTime(2016, 01, 01), new DateTime(2016, 05, 01), 2016, 1, false, false, "Spring", false),
                    new Term("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2016/Fall", "", new DateTime(2016, 09, 01), new DateTime(2016, 10, 15), 2016, 2, false, false, "Fall", false),
                    new Term("8f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Spr", "", new DateTime(2017, 01, 01), new DateTime(2017, 05, 01), 2017, 3, false, false, "Spring", false),
                    new Term("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", "", new DateTime(2017, 09, 01), new DateTime(2017, 12, 31), 2017, 4, false, false, "Fall", false)
                };
                _academicPeriodEntities = new List<Domain.Student.Entities.AcademicPeriod>()
                {
                    new Domain.Student.Entities.AcademicPeriod("b9691210-8516-45ca-9cd1-7e5aa1777234", "2016/Spr", "2016 Spring", new DateTime(2016, 01, 01), new DateTime(2016, 05, 01), 2016, 1, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("7f3aac22-e0b5-4159-b4e2-da158362c41b", "2016/Fall", "2016 Fall", new DateTime(2016, 09, 01), new DateTime(2016, 10, 15), 2016, 2, "Fall", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("8f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Spr", "2017 Spring", new DateTime(2017, 01, 01), new DateTime(2017, 05, 01), 2017, 3, "Spring", "", "", null),
                    new Domain.Student.Entities.AcademicPeriod("9f3aac22-e0b5-4159-b4e2-da158362c41b", "2017/Fall", "2017 Fall", new DateTime(2017, 09, 01), new DateTime(2017, 12, 31), 2017, 4, "Fall", "", "", null)
                };

                _accountingCodeEntities = new List<Domain.Student.Entities.AccountingCode>()
                {
                    new Domain.Student.Entities.AccountingCode("baa6cc46-48fc-4635-a22c-437df391a3fd", "CEMAC", "CE Materials Charge"),
                    new Domain.Student.Entities.AccountingCode("d3a4a98f-e858-495e-b12b-e6464442bbf4", "TUIIN", "Tuition-In State"),
                    new Domain.Student.Entities.AccountingCode("62cfb7e0-9939-4489-a523-25a3da71842d", "GRDFE", "Graduation Fee"),
                    new Domain.Student.Entities.AccountingCode("c0633708-eabb-4da2-88ba-cea7d930bd25", "PHONE", "Phone Installation"),
                };

                _accountReceivableTypeEntities = new List<AccountReceivableType>()
                {
                    new AccountReceivableType("eb6b8e21-1778-4b42-b793-8b0e1c22f7bf", "03", "Sponsor Receivable"),
                    new AccountReceivableType("06373dc5-862c-4eca-bd52-a6a8fb008f52", "06", "Miscellaneous Receivable"),
                    new AccountReceivableType("7dd8ad2b-68ae-4f12-a1f5-77c064718d24", "09", "Hr Employee Arrears Receivable"),
                    new AccountReceivableType("3c29e3f2-0399-4f73-aea7-ed424e6c5801", "01", "Student Receivable Ar Type"),
                };

            }

            private void BuildMocks()
            {
                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(PersonGuid)).ReturnsAsync("00001");
                _termRepositoryMock.Setup(i => i.GetAsync(It.IsAny<bool>())).ReturnsAsync(_termEntities);
                _termRepositoryMock.Setup(i => i.GetAsync()).ReturnsAsync(_termEntities);
                _termRepositoryMock.Setup(i => i.GetAcademicPeriods(_termEntities)).Returns(_academicPeriodEntities);
                _studentReferenceDataRepositoryMock.Setup(i => i.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(_accountingCodeEntities);
                _studentReferenceDataRepositoryMock.Setup(i => i.GetAccountReceivableTypesAsync(It.IsAny<bool>())).ReturnsAsync(_accountReceivableTypeEntities);

            }

            [TestCleanup]
            public void Cleanup()
            {
                _studentChargeRepositoryMock = null;
                _studentRepositoryMock = null;
                _studentReferenceDataRepositoryMock = null;
                _catalogRepositoryMock = null;
                _termRepositoryMock = null;
                _personRepositoryMock = null;
                _referenceDataRepositoryMock = null;
                _configurationRepositoryMock = null;
                _studentProgramRepositoryMock = null;
                _academicCreditRepositoryMock = null;
                _adapterRegistryMock = null;
                _roleRepositoryMock = null;
                _loggerMock = null;
                _currentUserFactory = null;
                _studentChargeService = null;
            }

            [TestMethod]
            public async Task StudentChargeService_GetAsync()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() {ViewStudentChargeRole});

                _studentChargeRepositoryMock.Setup(i =>
                    i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(_stuChargesTuple);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var actuals = await _studentChargeService.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                    Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                    Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                    Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                    Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);

                }
            }

            [TestMethod]
            public async Task StudentChargeService_GetAsync1()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });

                _studentChargeRepositoryMock.Setup(i =>
                    i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(_stuChargesTuple);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var actuals = await _studentChargeService.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                    Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                    Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                    Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                    Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);

                }
            }


            [TestMethod]
            public async Task StudentChargeService_GetAsyncWithFilters()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });

                _studentChargeRepositoryMock.Setup(i =>
                    i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(_stuChargesTuple);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var actuals = await _studentChargeService.GetAsync(0, 10, false, "b371fba4-797d-4c2c-8adc-bedd6d9db730", "b9691210-8516-45ca-9cd1-7e5aa1777234",
                    "d3a4a98f-e858-495e-b12b-e6464442bbf4", "tuition");
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                    Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                    Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                    Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                    Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);

                }
            }

            [TestMethod]
            public async Task StudentChargeService_GetAsync1WithFilters()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });

                _studentChargeRepositoryMock.Setup(i =>
                    i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(_stuChargesTuple);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var actuals = await _studentChargeService.GetAsync1(0, 10, false, "b371fba4-797d-4c2c-8adc-bedd6d9db730", "b9691210-8516-45ca-9cd1-7e5aa1777234",
                    "d3a4a98f-e858-495e-b12b-e6464442bbf4", "3c29e3f2-0399-4f73-aea7-ed424e6c5801", "tuition");
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                    Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                    Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                    Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                    Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                    Assert.AreEqual(expected.Person.Id, actual.Person.Id);

                }
            }


            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentChargeService_GetAsync_PermissionsException()
            {              
                _studentChargeRepositoryMock.Setup(i =>
                    i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(_stuChargesTuple);
           
                var actuals = await _studentChargeService.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                    Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                    Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                    Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                    Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                   
                }
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentChargeService_GetAsync1_PermissionsException()
            {
                _studentChargeRepositoryMock.Setup(i =>
                    i.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(_stuChargesTuple);

                var actuals = await _studentChargeService.GetAsync1(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>());
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals.Item1)
                {
                    var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);
                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                    Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                    Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                    Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                    Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                    Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                    Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);

                }
            }


            [TestMethod]
            public async Task StudentChargeService_GetByIdAsync()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() {ViewStudentChargeRole});

                _studentChargeRepositoryMock.Setup(i =>
                    i.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentChargeEntities.FirstOrDefault(x => x.Guid == "54c677e7-24ad-4591-be3f-d2175b7b0710"));

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var actual = await _studentChargeService.GetByIdAsync(It.IsAny<string>());
                Assert.IsNotNull(actual);

                var expected = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentChargeService_GetByIdAsync_EmptyID()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });

                _studentChargeRepositoryMock.Setup(i =>
                    i.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                await _studentChargeService.GetByIdAsync("");
               
            }

            [TestMethod]
            public async Task StudentChargeService_GetByIdAsync1()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });

                _studentChargeRepositoryMock.Setup(i =>
                    i.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentChargeEntities.FirstOrDefault(x => x.Guid == "54c677e7-24ad-4591-be3f-d2175b7b0710"));

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var actual = await _studentChargeService.GetByIdAsync1(It.IsAny<string>());
                Assert.IsNotNull(actual);

                var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);

            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentChargeService_GetByIdAsync1_EmptyID()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });

                _studentChargeRepositoryMock.Setup(i =>
                    i.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                await _studentChargeService.GetByIdAsync1("");

            }


            [TestMethod]
            public async Task StudentChargeService_CreateAsync()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() {ViewStudentChargeRole});

                var response = _studentChargeEntities.FirstOrDefault(x => x.Guid == "54c677e7-24ad-4591-be3f-d2175b7b0710");

                _studentChargeRepositoryMock.Setup(i =>
                    i.CreateAsync(It.IsAny<StudentCharge>())).ReturnsAsync(response);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var expected = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                var actual = await _studentChargeService.CreateAsync(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            }


            [TestMethod]
            public async Task StudentChargeService_CreateAsync1()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });

                var response = _studentChargeEntities.FirstOrDefault(x => x.Guid == "54c677e7-24ad-4591-be3f-d2175b7b0710");

                _studentChargeRepositoryMock.Setup(i =>
                    i.CreateAsync(It.IsAny<StudentCharge>())).ReturnsAsync(response);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                var actual = await _studentChargeService.CreateAsync1(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentChargeService_CreateAsync_PermissionsException()
            {
               
                var response = _studentChargeEntities.FirstOrDefault(x => x.Guid == "54c677e7-24ad-4591-be3f-d2175b7b0710");

                _studentChargeRepositoryMock.Setup(i =>
                    i.CreateAsync(It.IsAny<StudentCharge>())).ReturnsAsync(response);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var expected = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                var actual = await _studentChargeService.CreateAsync(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentChargeService_CreateAsync1_PermissionsException()
            {

                var response = _studentChargeEntities.FirstOrDefault(x => x.Guid == "54c677e7-24ad-4591-be3f-d2175b7b0710");

                _studentChargeRepositoryMock.Setup(i =>
                    i.CreateAsync(It.IsAny<StudentCharge>())).ReturnsAsync(response);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var expected = _studentCharge1Dtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                var actual = await _studentChargeService.CreateAsync1(expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            }


            [TestMethod]
            public async Task StudentChargeService_UpdateAsync()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() {ViewStudentChargeRole});

                var response = _studentChargeEntities.FirstOrDefault(x => x.Guid == "54c677e7-24ad-4591-be3f-d2175b7b0710");

                _studentChargeRepositoryMock.Setup(i =>
                    i.UpdateAsync(It.IsAny<string>(), It.IsAny<StudentCharge>()))
                    .ReturnsAsync(response);

                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var expected = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                var actual = await _studentChargeService.UpdateAsync(expected.Id, expected);
                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.ChargeType, actual.ChargeType);
                Assert.AreEqual(expected.ChargeableOn, actual.ChargeableOn);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.ChargedAmount.UnitCost, actual.ChargedAmount.UnitCost);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentChargeService_CreateAsync_EmptyAcademicPeriodException()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });


                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var request = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(request);
                request.AcademicPeriod = null;
                await _studentChargeService.CreateAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentChargeService_CreateAsync_EmptyChargeAmountException()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });


                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var request = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(request);
                request.ChargedAmount = null;
                await _studentChargeService.CreateAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentChargeService_CreateAsync_EmptyUnitCostException()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });


                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var request = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(request);
                request.ChargedAmount.Amount = null;
                request.ChargedAmount.UnitCost = null;
                await _studentChargeService.CreateAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentChargeService_CreateAsync_EmptyChargeTypeException()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });


                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var request = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(request);
                request.ChargeType = StudentChargeTypes.notset;
                await _studentChargeService.CreateAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentChargeService_CreateAsync_EmptyPersonException()
            {
                ViewStudentChargeRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentCharges));
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { ViewStudentChargeRole });


                _personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730")).ReturnsAsync("00001");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("00001")).ReturnsAsync("b371fba4-797d-4c2c-8adc-bedd6d9db730");

                var request = _studentChargeDtos.FirstOrDefault(i => i.Id.Equals("54c677e7-24ad-4591-be3f-d2175b7b0710", StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(request);
                request.Person = null;
                await _studentChargeService.CreateAsync(request);
            }
        }
    }
}