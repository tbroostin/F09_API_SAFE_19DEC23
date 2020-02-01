// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using System.Collections.ObjectModel;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{

    #region Student Payment V6
    [TestClass]
    public class StudentPaymentServiceTest
    {
        private Mock<IStudentPaymentRepository> _studentPaymentRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
        private Mock<ITermRepository> _termRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<ILogger> _loggerMock;
        protected Ellucian.Colleague.Domain.Entities.Role createStudentPayments = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.STUDENT.PAYMENTS");
        protected Ellucian.Colleague.Domain.Entities.Role viewStudentPayments = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.PAYMENTS");
        private StudentPaymentsUserFactory currentUserFactory = new StudentPaymentsUserFactory();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public class StudentPaymentsUserFactory : ICurrentUserFactory
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
                        Roles = new List<string>() {"CREATE.STUDENT.PAYMENTS", "VIEW.STUDENT.PAYMENTS"},
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        private StudentPaymentService _studentPaymentService;
        private Domain.Student.Entities.StudentPayment studentPaymentEntity;
        private Collection<Domain.Student.Entities.StudentPayment> studentPaymentEntityCollection;
        private List<Domain.Student.Entities.Term> termCollection;
        private List<Domain.Student.Entities.AccountingCode> accountingCodeCollection;
        private List<Domain.Student.Entities.AccountReceivableType> accountReceivalbeTypesCollection;
        private Dtos.StudentPayment studentPaymentDto;

        [TestInitialize]
        public void Initialize()
        {
            _studentPaymentRepositoryMock = new Mock<IStudentPaymentRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _termRepositoryMock = new Mock<ITermRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _loggerMock = new Mock<ILogger>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            createStudentPayments.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentPayments));
            viewStudentPayments.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentPayments));
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentPayments, createStudentPayments });

            _studentPaymentService = new StudentPaymentService(_studentPaymentRepositoryMock.Object,
                _personRepositoryMock.Object, _referenceDataRepositoryMock.Object,
                _studentReferenceDataRepositoryMock.Object, _termRepositoryMock.Object, baseConfigurationRepository,
                _adapterRegistryMock.Object, currentUserFactory, _roleRepositoryMock.Object,
                _loggerMock.Object);

            studentPaymentEntity = new Domain.Student.Entities.StudentPayment("0000123", "sponsor",
                new DateTime(2017 - 01 - 30))
            {
                AccountsReceivableCode = "TUI",
                AccountsReceivableTypeCode = "01",
                Comments = new List<string>() {"Elevate Tuition payment using the student-payments API and POST request This is the second line of data."},
                PaymentAmount = 10m,
                PaymentCurrency = "CAD",
                PaymentID = "20282",
                Term = "2016/FA",
                Guid = "9a5a8793-c661-4c57-a47b-41a425c659c5"
            };

            studentPaymentDto = new Dtos.StudentPayment()
            {
                Id = "9a5a8793-c661-4c57-a47b-41a425c659c5",
                Person = new GuidObject2() {Id = "e6857066-13a2-4316-981f-308d1474eabf"},
                AccountReceivableType = new GuidObject2() {Id = "375c836b-cf4c-475e-bad4-c45d98bdc697"},
                AccountingCode = new GuidObject2() {Id = "05cce1d0-c75c-40d7-9be0-88b61f2acfa6"},
                AcademicPeriod = new GuidObject2() {Id = "1869dab7-12dc-4ea6-8c6d-8bedd36ebefe"},
                PaymentType = Dtos.EnumProperties.StudentPaymentTypes.sponsor,
                PaidOn = new DateTime(2017 - 01 - 30),
                Comments = new List<string>() {"Elevate Tuition payment using the student-payments API and POST request This is the second line of data."},
                Amount = new Dtos.DtoProperties.AmountDtoProperty()
                {
                    Value = 10m,
                    Currency = Dtos.EnumProperties.CurrencyCodes.CAD
                }
            };

            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>()
            {
                new Domain.Student.Entities.StudentPayment("0000122", "cash", new DateTime(2017 - 01 - 30))
                {
                    AccountsReceivableCode = "TUI",
                    AccountsReceivableTypeCode = "01",
                    Comments = new List<string>() {"another Comment. This is the second line of data."},
                    PaymentAmount = 20m,
                    PaymentCurrency = "USD",
                    PaymentID = "20283",
                    Term = "2017RSP",
                    Guid = "b3c23e64-b447-40a9-aa05-1d9853005194"
                }

            };
            studentPaymentEntityCollection.Add(studentPaymentEntity);

            termCollection = new List<Term>()
            {
                new Term("1869dab7-12dc-4ea6-8c6d-8bedd36ebefe", "2016/FA", "term1", new DateTime(2016, 09, 01), new DateTime(2016, 12, 01), 2016, 1, true, true, "2016/FA", false),
                new Term("23a234a3-12dc-4ea6-8c6d-83edfdd363sexc", "2017RSP", "term2", new DateTime(2017, 09, 01), new DateTime(2017, 12, 01), 2017, 1, true, true, "2017/FA", false),
            };

            accountingCodeCollection = new List<Domain.Student.Entities.AccountingCode>()
            {
                new Domain.Student.Entities.AccountingCode("05cce1d0-c75c-40d7-9be0-88b61f2acfa6", "TUI", "Description1")
            };

            accountReceivalbeTypesCollection = new List<Domain.Student.Entities.AccountReceivableType>()
            {
                new Domain.Student.Entities.AccountReceivableType("375c836b-cf4c-475e-bad4-c45d98bdc697", "01", "Desc123")
            };

            _termRepositoryMock.Setup(x => x.GetAsync(It.IsAny<bool>())).ReturnsAsync(termCollection);
            _termRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(termCollection);
            _studentReferenceDataRepositoryMock.Setup(x => x.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodeCollection);
            _studentReferenceDataRepositoryMock.Setup(x => x.GetAccountReceivableTypesAsync(It.IsAny<bool>())).ReturnsAsync(accountReceivalbeTypesCollection);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentPaymentRepositoryMock = null;
            _personRepositoryMock = null;
            _referenceDataRepositoryMock = null;
            _studentReferenceDataRepositoryMock = null;
            _termRepositoryMock = null;
            _adapterRegistryMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _loggerMock = null;

            _studentPaymentService = null;
            studentPaymentEntity = null;
            studentPaymentEntityCollection = null;
        }

        [TestMethod]
        public async Task StudentPaymentService_GetByIDAsync_FieldValidate()
        {
            _studentPaymentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(studentPaymentEntity);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");

            var actual = await _studentPaymentService.GetByIdAsync(studentPaymentEntity.Guid);

            Assert.IsNotNull(actual);
            Assert.AreEqual(studentPaymentEntity.Guid, actual.Id);
            Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
            Assert.AreEqual(accountReceivalbeTypesCollection[0].Guid, actual.AccountReceivableType.Id);
            Assert.AreEqual(accountingCodeCollection[0].Guid, actual.AccountingCode.Id);
            Assert.AreEqual(termCollection[0].RecordGuid, actual.AcademicPeriod.Id);
            Assert.AreEqual(Dtos.EnumProperties.StudentPaymentTypes.sponsor, actual.PaymentType);
            Assert.AreEqual(studentPaymentEntity.PaymentDate, actual.PaidOn);
            Assert.AreEqual(studentPaymentEntity.Comments, actual.Comments);
            Assert.AreEqual(studentPaymentEntity.PaymentAmount, actual.Amount.Value);
            Assert.AreEqual(Dtos.EnumProperties.CurrencyCodes.CAD, actual.Amount.Currency);

        }
        
        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task StudentPaymentService_GetByIDAsync_NoPermissions()
        {
            _studentPaymentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(studentPaymentEntity);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>());
            var actual = await _studentPaymentService.GetByIdAsync(studentPaymentEntity.Guid);
        }
        [TestMethod]
        public async Task StudentPaymentService_GetByIDAsync_CreatePermissionGrantsViewPermission()
        {
            _studentPaymentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(studentPaymentEntity);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createStudentPayments });
            var actual = await _studentPaymentService.GetByIdAsync(studentPaymentEntity.Guid);
            Assert.IsNotNull(actual);
        }
        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task StudentPaymentService_GetByIDAsync_NotFound()
        {
            _studentPaymentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null);
            var actual = await _studentPaymentService.GetByIdAsync(studentPaymentEntity.Guid);
        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync_FieldValidate()
        {
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(studentPaymentEntityCollection, 2);

            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "")).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync(0, 100, true, "", "", "", "");

            Assert.AreEqual(2, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = accountingCodeCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.AccountReceivableType.Id);
                Assert.AreEqual(actCode.Guid, actual.AccountingCode.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync_PersonFilter_FieldValidate()
        {
            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>();
            studentPaymentEntityCollection.Add(studentPaymentEntity);
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>
                (studentPaymentEntityCollection, 1);
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0000122");
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), "", "", "")).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync(0, 100, true, "e6857066-13a2-4316-981f-308d1474eabf", "", "", "");

            Assert.AreEqual(1, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = accountingCodeCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.AccountReceivableType.Id);
                Assert.AreEqual(actCode.Guid, actual.AccountingCode.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync_academicPeriod_FieldValidate()
        {
            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>();
            studentPaymentEntityCollection.Add(studentPaymentEntity);
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple =
                new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(studentPaymentEntityCollection, 1);

            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", It.IsAny<string>(), "", "")).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync(0, 100, true, "", "1869dab7-12dc-4ea6-8c6d-8bedd36ebefe", "", "");

            Assert.AreEqual(1, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = accountingCodeCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.AccountReceivableType.Id);
                Assert.AreEqual(actCode.Guid, actual.AccountingCode.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync_accountingCode_FieldValidate()
        {
            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>();
            studentPaymentEntityCollection.Add(studentPaymentEntity);
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(studentPaymentEntityCollection, 1);

            _studentReferenceDataRepositoryMock.Setup(x => x.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodeCollection);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", It.IsAny<string>(), "")).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync(0, 100, true, "", "", "05cce1d0-c75c-40d7-9be0-88b61f2acfa6", "");

            Assert.AreEqual(1, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = accountingCodeCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.AccountReceivableType.Id);
                Assert.AreEqual(actCode.Guid, actual.AccountingCode.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync_paymentType_FieldValidate()
        {
            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>();
            studentPaymentEntityCollection.Add(studentPaymentEntity);
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple =
                new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(studentPaymentEntityCollection, 1);

            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", It.IsAny<string>())).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync(0, 100, true, "", "", "", "sponsor");

            Assert.AreEqual(1, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = accountingCodeCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.AccountReceivableType.Id);
                Assert.AreEqual(actCode.Guid, actual.AccountingCode.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync_paymentType_NoResultPerson()
        {
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);

            var actuals = await _studentPaymentService.GetAsync(0, 100, true, "234", "", "", "");
            Assert.AreEqual(0, actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync_paymentType_NoResultAcademicPeriod()
        {
            _termRepositoryMock = new Mock<ITermRepository>();

            var actuals = await _studentPaymentService.GetAsync(0, 100, true, "", "234", "", "");
            Assert.AreEqual(0, actuals.Item2);
        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync_paymentType_NoResultAccountingCode()
        {
            _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

            var actuals = await _studentPaymentService.GetAsync(0, 100, true, "", "", "234", "");
            Assert.AreEqual(0, actuals.Item2);
        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync_paymentType_NoResult()
        {
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple =
                new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(null, 0);

            _studentPaymentRepositoryMock.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), "", "", "", "")).ReturnsAsync(studentPaymentTuple);
            var actuals = await _studentPaymentService.GetAsync(0, 100, true, "", "", "", "");
            Assert.AreEqual(0, actuals.Item2);
        }

        [TestMethod]
        public async Task StudentPaymentService_CreateAsync_validateFields()
        {
            var guid = new GuidLookupResult()
            {
                Entity = "AR.PAY.ITEMS.INTG",
                PrimaryKey = "1"

            };

            _studentPaymentRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Domain.Student.Entities.StudentPayment>())).ReturnsAsync(studentPaymentEntity);
            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guid);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0000122");
            var actual = await _studentPaymentService.CreateAsync(studentPaymentDto);

            var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
            var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
            var actCode = accountingCodeCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableCode);
            var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
            Assert.AreEqual(actRcvType.Guid, actual.AccountReceivableType.Id);
            Assert.AreEqual(actCode.Guid, actual.AccountingCode.Id);
            Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
            var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
            if (expected.PaymentType == "sponsor")
            {
                payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
            }
            Assert.AreEqual(payType, actual.PaymentType);
            Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
            var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
            if (expected.PaymentCurrency == "USD")
            {
                currency = Dtos.EnumProperties.CurrencyCodes.USD;
            }
            Assert.AreEqual(currency, actual.Amount.Currency);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task StudentPaymentService_CreateAsync_NullPerson()
        {

            studentPaymentDto.Person = null;
            var actual = await _studentPaymentService.CreateAsync(studentPaymentDto);

        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task StudentPaymentService_CreateAsync_PaymentType_notset()
        {
            studentPaymentDto.PaymentType = Dtos.EnumProperties.StudentPaymentTypes.notset;
            var actual = await _studentPaymentService.CreateAsync(studentPaymentDto);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task StudentPaymentService_CreateAsync_NullAccountingCode()
        {
            var actual = await _studentPaymentService.CreateAsync(studentPaymentDto);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public async Task StudentPaymentService_CreateAsync_AccountReceivableType()
        {
            studentPaymentDto.AccountingCode = null;
            var actual = await _studentPaymentService.CreateAsync(studentPaymentDto);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public async Task StudentPaymentService_CreateAsync_TermEntity_Null()
        {
            studentPaymentDto.AccountingCode = null;
            studentPaymentDto.AccountReceivableType = null;
            studentPaymentDto.AcademicPeriod = null;
            var actual = await _studentPaymentService.CreateAsync(studentPaymentDto);
        }

        [TestMethod]
        public async Task StudentPaymentService_UpdateAsync_validateFields()
        {
            var guid = new GuidLookupResult()
            {
                Entity = "AR.PAY.ITEMS.INTG",
                PrimaryKey = "1"

            };

            _studentPaymentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<Domain.Student.Entities.StudentPayment>())).ReturnsAsync(studentPaymentEntity);
            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guid);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0000122");
            var actual = await _studentPaymentService.UpdateAsync("0000122", studentPaymentDto);

            var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
            var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
            var actCode = accountingCodeCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableCode);
            var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
            Assert.AreEqual(actRcvType.Guid, actual.AccountReceivableType.Id);
            Assert.AreEqual(actCode.Guid, actual.AccountingCode.Id);
            Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
            var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
            if (expected.PaymentType == "sponsor")
            {
                payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
            }
            Assert.AreEqual(payType, actual.PaymentType);
            Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
            var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
            if (expected.PaymentCurrency == "USD")
            {
                currency = Dtos.EnumProperties.CurrencyCodes.USD;
            }
            Assert.AreEqual(currency, actual.Amount.Currency);
        }
    }

    #endregion

    #region Student Payment V11
    [TestClass]
    public class StudentPaymentServiceTest_V11
    {
        private Mock<IStudentPaymentRepository> _studentPaymentRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
        private Mock<ITermRepository> _termRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<ILogger> _loggerMock;
        protected Ellucian.Colleague.Domain.Entities.Role createStudentPayments = new Ellucian.Colleague.Domain.Entities.Role(1, "CREATE.STUDENT.PAYMENTS");
        protected Ellucian.Colleague.Domain.Entities.Role viewStudentPayments = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.STUDENT.PAYMENTS");
        private StudentPaymentsUserFactory currentUserFactory = new StudentPaymentsUserFactory();

        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public class StudentPaymentsUserFactory : ICurrentUserFactory
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
                        Roles = new List<string>() { "CREATE.STUDENT.PAYMENTS", "VIEW.STUDENT.PAYMENTS" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        private StudentPaymentService _studentPaymentService;
        private Domain.Student.Entities.StudentPayment studentPaymentEntity;
        private Collection<Domain.Student.Entities.StudentPayment> studentPaymentEntityCollection;
        private List<Domain.Student.Entities.Term> termCollection;
        private List<Domain.Student.Entities.DistributionMethod> distributionCodeCollection;
        private List<Domain.Student.Entities.AccountReceivableType> accountReceivalbeTypesCollection;
        private Dtos.StudentPayment2 studentPaymentDto;

        [TestInitialize]
        public void Initialize()
        {
            _studentPaymentRepositoryMock = new Mock<IStudentPaymentRepository>();
            _personRepositoryMock = new Mock<IPersonRepository>();
            _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _termRepositoryMock = new Mock<ITermRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _loggerMock = new Mock<ILogger>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            createStudentPayments.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreateStudentPayments));
            viewStudentPayments.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.ViewStudentPayments));
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentPayments, createStudentPayments });

            _studentPaymentService = new StudentPaymentService(_studentPaymentRepositoryMock.Object,
                _personRepositoryMock.Object, _referenceDataRepositoryMock.Object,
                _studentReferenceDataRepositoryMock.Object, _termRepositoryMock.Object, baseConfigurationRepository,
                _adapterRegistryMock.Object, currentUserFactory, _roleRepositoryMock.Object,
                _loggerMock.Object);

            studentPaymentEntity = new Domain.Student.Entities.StudentPayment("0000123", "sponsor",
                new DateTime(2017 - 01 - 30))
            {
                AccountsReceivableCode = "TUI",
                AccountsReceivableTypeCode = "01",
                Comments = new List<string>() { "Elevate Tuition payment using the student-payments API and POST request This is the second line of data." },
                PaymentAmount = 10m,
                PaymentCurrency = "CAD",
                PaymentID = "20282",
                Term = "2016/FA",
                Guid = "9a5a8793-c661-4c57-a47b-41a425c659c5",
                DistributionCode = "WEBA",
                ChargeFromElevate = false
            };

            studentPaymentDto = new Dtos.StudentPayment2()
            {
                Id = "9a5a8793-c661-4c57-a47b-41a425c659c5",
                Person = new GuidObject2() { Id = "e6857066-13a2-4316-981f-308d1474eabf" },
                FundingDestination = new GuidObject2() { Id = "375c836b-cf4c-475e-bad4-c45d98bdc697" },
                FundingSource = new GuidObject2() { Id = "05cce1d0-c75c-40d7-9be0-88b61f2acfa6" },
                AcademicPeriod = new GuidObject2() { Id = "1869dab7-12dc-4ea6-8c6d-8bedd36ebefe" },
                PaymentType = Dtos.EnumProperties.StudentPaymentTypes.sponsor,
                PaidOn = new DateTime(2017 - 01 - 30),
                Comments = new List<string>() { "Elevate Tuition payment using the student-payments API and POST request This is the second line of data." },
                Amount = new Dtos.DtoProperties.AmountDtoProperty()
                {
                    Value = 10m,
                    Currency = Dtos.EnumProperties.CurrencyCodes.CAD
                }
            };

            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>()
            {
                new Domain.Student.Entities.StudentPayment("0000122", "cash", new DateTime(2017 - 01 - 30))
                {
                    AccountsReceivableCode = "TUI",
                    AccountsReceivableTypeCode = "01",
                    Comments = new List<string>() {"another Comment. This is the second line of data."},
                    PaymentAmount = 20m,
                    PaymentCurrency = "USD",
                    PaymentID = "20283",
                    Term = "2017RSP",
                    Guid = "b3c23e64-b447-40a9-aa05-1d9853005194",
                    DistributionCode = "WEBA"
                }

            };
            studentPaymentEntityCollection.Add(studentPaymentEntity);

            termCollection = new List<Term>()
            {
                new Term("1869dab7-12dc-4ea6-8c6d-8bedd36ebefe", "2016/FA", "term1", new DateTime(2016, 09, 01), new DateTime(2016, 12, 01), 2016, 1, true, true, "2016/FA", false),
                new Term("23a234a3-12dc-4ea6-8c6d-83edfdd363sexc", "2017RSP", "term2", new DateTime(2017, 09, 01), new DateTime(2017, 12, 01), 2017, 1, true, true, "2017/FA", false),
            };

            distributionCodeCollection = new List<Domain.Student.Entities.DistributionMethod>()
            {
                new Domain.Student.Entities.DistributionMethod("05cce1d0-c75c-40d7-9be0-88b61f2acfa6", "WEBA", "Description1")
            };

            accountReceivalbeTypesCollection = new List<Domain.Student.Entities.AccountReceivableType>()
            {
                new Domain.Student.Entities.AccountReceivableType("375c836b-cf4c-475e-bad4-c45d98bdc697", "01", "Desc123")
            };

            _termRepositoryMock.Setup(x => x.GetAsync(It.IsAny<bool>())).ReturnsAsync(termCollection);
            _termRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(termCollection);

            foreach (var tc in termCollection)
            {
                _termRepositoryMock.Setup(x => x.GetAcademicPeriodsGuidAsync(tc.Code)).ReturnsAsync(tc.RecordGuid);
                _termRepositoryMock.Setup(x => x.GetAcademicPeriodsCodeFromGuidAsync(tc.RecordGuid)).ReturnsAsync(tc.Code);
            }

            _studentReferenceDataRepositoryMock.Setup(x => x.GetDistrMethodCodesAsync(It.IsAny<bool>())).ReturnsAsync(distributionCodeCollection);
            foreach (var dc in distributionCodeCollection)
            {
                _studentReferenceDataRepositoryMock.Setup(x => x.GetDistrMethodGuidAsync(dc.Code)).ReturnsAsync(dc.Guid);
                _studentReferenceDataRepositoryMock.Setup(x => x.GetDistrMethodCodeFromGuidAsync(dc.Guid)).ReturnsAsync(dc.Code);
            }

            _studentReferenceDataRepositoryMock.Setup(x => x.GetAccountReceivableTypesAsync(It.IsAny<bool>())).ReturnsAsync(accountReceivalbeTypesCollection);
            foreach (var ar in accountReceivalbeTypesCollection)
            {
                _studentReferenceDataRepositoryMock.Setup(x => x.GetAccountReceivableTypesGuidAsync(ar.Code)).ReturnsAsync(ar.Guid);
                _studentReferenceDataRepositoryMock.Setup(x => x.GetAccountReceivableTypesCodeFromGuidAsync(ar.Guid)).ReturnsAsync(ar.Code);
            }


            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("0000122", "e6857066-13a2-4316-981f-308d1474eabf");
            dict.Add("0000123", "e6857066-13a2-4316-981f-308d1474eabf");
            _personRepositoryMock.Setup(repo => repo.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(dict);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentPaymentRepositoryMock = null;
            _personRepositoryMock = null;
            _referenceDataRepositoryMock = null;
            _studentReferenceDataRepositoryMock = null;
            _termRepositoryMock = null;
            _adapterRegistryMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _loggerMock = null;

            _studentPaymentService = null;
            studentPaymentEntity = null;
            studentPaymentEntityCollection = null;
        }

        [TestMethod]
        public async Task StudentPaymentService_GetByIDAsync2_FieldValidate()
        {
            _studentPaymentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(studentPaymentEntity);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");

            var actual = await _studentPaymentService.GetByIdAsync2(studentPaymentEntity.Guid);

            Assert.IsNotNull(actual);
            Assert.AreEqual(studentPaymentEntity.Guid, actual.Id);
            Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
            Assert.AreEqual(accountReceivalbeTypesCollection[0].Guid, actual.FundingDestination.Id);
            Assert.AreEqual(distributionCodeCollection[0].Guid, actual.FundingSource.Id);
            Assert.AreEqual(termCollection[0].RecordGuid, actual.AcademicPeriod.Id);
            Assert.AreEqual(Dtos.EnumProperties.StudentPaymentTypes.sponsor, actual.PaymentType);
            Assert.AreEqual(studentPaymentEntity.PaymentDate, actual.PaidOn);
            Assert.AreEqual(studentPaymentEntity.Comments, actual.Comments);
            Assert.AreEqual(studentPaymentEntity.PaymentAmount, actual.Amount.Value);
            Assert.AreEqual(Dtos.EnumProperties.CurrencyCodes.CAD, actual.Amount.Currency);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentPaymentService_GetByIDAsync2_NotFound()
        {
            _studentPaymentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null);
            var actual = await _studentPaymentService.GetByIdAsync2(studentPaymentEntity.Guid);
        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync2_FieldValidate()
        {
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(studentPaymentEntityCollection, 2);

            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "",It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync2(0, 100, true, "", "", "", "");

            Assert.AreEqual(2, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = distributionCodeCollection.FirstOrDefault(x => x.Code == expected.DistributionCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.FundingDestination.Id);
                Assert.AreEqual(actCode.Guid, actual.FundingSource.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync2_PersonFilter_FieldValidate()
        {
            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>();
            studentPaymentEntityCollection.Add(studentPaymentEntity);
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>
                (studentPaymentEntityCollection, 1);
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0000122");
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), "", "", "", "", "")).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync2(0, 100, true, "e6857066-13a2-4316-981f-308d1474eabf", "", "", "");

            Assert.AreEqual(1, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = distributionCodeCollection.FirstOrDefault(x => x.Code == expected.DistributionCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.FundingDestination.Id);
                Assert.AreEqual(actCode.Guid, actual.FundingSource.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync2_academicPeriod_FieldValidate()
        {
            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>();
            studentPaymentEntityCollection.Add(studentPaymentEntity);
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple =
                new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(studentPaymentEntityCollection, 1);

            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", It.IsAny<string>(), "", "", "", "")).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync2(0, 100, true, "", "1869dab7-12dc-4ea6-8c6d-8bedd36ebefe", "", "");

            Assert.AreEqual(1, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = distributionCodeCollection.FirstOrDefault(x => x.Code == expected.DistributionCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.FundingDestination.Id);
                Assert.AreEqual(actCode.Guid, actual.FundingSource.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync2_accountingCode_FieldValidate()
        {
            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>();
            studentPaymentEntityCollection.Add(studentPaymentEntity);
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(studentPaymentEntityCollection, 1);

            _studentReferenceDataRepositoryMock.Setup(x => x.GetDistrMethodCodesAsync(It.IsAny<bool>())).ReturnsAsync(distributionCodeCollection);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", It.IsAny<string>(), "", "", "")).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync2(0, 100, true, "", "", "05cce1d0-c75c-40d7-9be0-88b61f2acfa6", "");

            Assert.AreEqual(1, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = distributionCodeCollection.FirstOrDefault(x => x.Code == expected.DistributionCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.FundingDestination.Id);
                Assert.AreEqual(actCode.Guid, actual.FundingSource.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync2_paymentType_FieldValidate()
        {
            studentPaymentEntityCollection = new Collection<Domain.Student.Entities.StudentPayment>();
            studentPaymentEntityCollection.Add(studentPaymentEntity);
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple =
                new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(studentPaymentEntityCollection, 1);

            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _studentPaymentRepositoryMock.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", It.IsAny<string>(), "", "")).ReturnsAsync(studentPaymentTuple);

            var actuals = await _studentPaymentService.GetAsync2(0, 100, true, "", "", "", "sponsor");

            Assert.AreEqual(1, actuals.Item1.Count());
            foreach (var actual in actuals.Item1)
            {
                var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
                var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
                var actCode = distributionCodeCollection.FirstOrDefault(x => x.Code == expected.DistributionCode);
                var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
                Assert.AreEqual(actRcvType.Guid, actual.FundingDestination.Id);
                Assert.AreEqual(actCode.Guid, actual.FundingSource.Id);
                Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
                var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
                if (expected.PaymentType == "sponsor")
                {
                    payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
                }
                Assert.AreEqual(payType, actual.PaymentType);
                Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
                Assert.AreEqual(expected.Comments, actual.Comments);
                Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
                var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
                if (expected.PaymentCurrency == "USD")
                {
                    currency = Dtos.EnumProperties.CurrencyCodes.USD;
                }
                Assert.AreEqual(currency, actual.Amount.Currency);
            }

        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync2_paymentType_NoResultPerson()
        {
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(null);

            var actuals = await _studentPaymentService.GetAsync2(0, 100, true, "234", "", "", "");
            Assert.AreEqual(0, actuals.Item1.Count());
        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync2_paymentType_NoResultAcademicPeriod()
        {
            _termRepositoryMock = new Mock<ITermRepository>();

            var actuals = await _studentPaymentService.GetAsync2(0, 100, true, "", "234", "", "");
            Assert.AreEqual(0, actuals.Item2);
        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync2_paymentType_NoResultAccountingCode()
        {
            _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

            var actuals = await _studentPaymentService.GetAsync2(0, 100, true, "", "", "234", "");
            Assert.AreEqual(0, actuals.Item2);
        }

        [TestMethod]
        public async Task StudentPaymentService_GetAsync2_paymentType_NoResult()
        {
            Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int> studentPaymentTuple =
                new Tuple<IEnumerable<Domain.Student.Entities.StudentPayment>, int>(null, 0);

            _studentPaymentRepositoryMock.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "", "", "", "", "")).ReturnsAsync(studentPaymentTuple);
            var actuals = await _studentPaymentService.GetAsync2(0, 100, true, "", "", "", "");
            Assert.AreEqual(0, actuals.Item2);
        }

        [TestMethod]
        public async Task StudentPaymentService_CreateAsync2_validateFields()
        {
            var guid = new GuidLookupResult()
            {
                Entity = "AR.PAY.ITEMS.INTG",
                PrimaryKey = "1"

            };

            _studentPaymentRepositoryMock.Setup(x => x.CreateAsync2(It.IsAny<Domain.Student.Entities.StudentPayment>())).ReturnsAsync(studentPaymentEntity);
            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guid);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0000122");
            var actual = await _studentPaymentService.CreateAsync2(studentPaymentDto);

            var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
            var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
            var actCode = distributionCodeCollection.FirstOrDefault(x => x.Code == expected.DistributionCode);
            var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
            Assert.AreEqual(actRcvType.Guid, actual.FundingDestination.Id);
            Assert.AreEqual(actCode.Guid, actual.FundingSource.Id);
            Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
            var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
            if (expected.PaymentType == "sponsor")
            {
                payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
            }
            Assert.AreEqual(payType, actual.PaymentType);
            Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
            var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
            if (expected.PaymentCurrency == "USD")
            {
                currency = Dtos.EnumProperties.CurrencyCodes.USD;
            }
            Assert.AreEqual(currency, actual.Amount.Currency);
        }

        [TestMethod]
        public async Task StudentPaymentService_CreateAsync2_ByElevate()
        {
            var guid = new GuidLookupResult()
            {
                Entity = "AR.PAY.ITEMS.INTG",
                PrimaryKey = "1"

            };

            studentPaymentEntity.ChargeFromElevate = true;
            studentPaymentDto.MetadataObject = new Dtos.DtoProperties.MetaDataDtoProperty() { CreatedBy = "Elevate" };

            _studentPaymentRepositoryMock.Setup(x => x.CreateAsync2(It.IsAny<Domain.Student.Entities.StudentPayment>())).ReturnsAsync(studentPaymentEntity);
            _referenceDataRepositoryMock.Setup(x => x.GetGuidLookupResultFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guid);
            _personRepositoryMock.Setup(x => x.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("e6857066-13a2-4316-981f-308d1474eabf");
            _personRepositoryMock.Setup(x => x.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("0000122");
            var actual = await _studentPaymentService.CreateAsync2(studentPaymentDto);

            var expected = studentPaymentEntityCollection.FirstOrDefault(x => x.Guid == actual.Id);
            var actRcvType = accountReceivalbeTypesCollection.FirstOrDefault(x => x.Code == expected.AccountsReceivableTypeCode);
            var actCode = distributionCodeCollection.FirstOrDefault(x => x.Code == expected.DistributionCode);
            var term = termCollection.FirstOrDefault(x => x.Code == expected.Term);
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual("e6857066-13a2-4316-981f-308d1474eabf", actual.Person.Id);
            Assert.AreEqual(actRcvType.Guid, actual.FundingDestination.Id);
            Assert.AreEqual(actCode.Guid, actual.FundingSource.Id);
            Assert.AreEqual(term.RecordGuid, actual.AcademicPeriod.Id);
            var payType = Dtos.EnumProperties.StudentPaymentTypes.cash;
            if (expected.PaymentType == "sponsor")
            {
                payType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
            }
            Assert.AreEqual(payType, actual.PaymentType);
            Assert.AreEqual(expected.PaymentDate, actual.PaidOn);
            Assert.AreEqual(expected.Comments, actual.Comments);
            Assert.AreEqual(expected.PaymentAmount, actual.Amount.Value);
            var currency = Dtos.EnumProperties.CurrencyCodes.CAD;
            if (expected.PaymentCurrency == "USD")
            {
                currency = Dtos.EnumProperties.CurrencyCodes.USD;
            }
            Assert.AreEqual(currency, actual.Amount.Currency);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentPaymentService_CreateAsync2_NullPerson()
        {

            studentPaymentDto.Person = null;
            var actual = await _studentPaymentService.CreateAsync2(studentPaymentDto);

        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentPaymentService_CreateAsync2_PaymentType_notset()
        {
            studentPaymentDto.PaymentType = Dtos.EnumProperties.StudentPaymentTypes.notset;
            var actual = await _studentPaymentService.CreateAsync2(studentPaymentDto);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentPaymentService_CreateAsync2_NullAccountingCode()
        {
            var actual = await _studentPaymentService.CreateAsync2(studentPaymentDto);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentPaymentService_CreateAsync2_FundingSourceIsNull()
        {
            studentPaymentDto.FundingSource = null;
            var actual = await _studentPaymentService.CreateAsync2(studentPaymentDto);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentPaymentService_CreateAsync2_TermEntity_Null()
        {
            studentPaymentDto.FundingDestination = null;
            studentPaymentDto.AcademicPeriod = null;
            var actual = await _studentPaymentService.CreateAsync2(studentPaymentDto);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentPaymentService_CreateAsync2_OriginatedOn_Null()
        {
            studentPaymentDto.ReportingDetail = new Dtos.DtoProperties.StudentPaymentsReportingDtoProperty();
            studentPaymentDto.ReportingDetail.Usage = Dtos.EnumProperties.StudentPaymentUsageTypes.taxReportingOnly;
            var actual = await _studentPaymentService.CreateAsync2(studentPaymentDto);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task StudentPaymentService_CreateAsync2_Usage_Null()
        {
            studentPaymentDto.ReportingDetail = new Dtos.DtoProperties.StudentPaymentsReportingDtoProperty();
            studentPaymentDto.ReportingDetail.OriginatedOn = new DateTime(2019, 11, 12);

            var actual = await _studentPaymentService.CreateAsync2(studentPaymentDto);
        }

    }

    #endregion
}