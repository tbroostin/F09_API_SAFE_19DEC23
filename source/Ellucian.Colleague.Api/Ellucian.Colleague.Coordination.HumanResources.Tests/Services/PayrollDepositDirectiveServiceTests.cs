// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
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
    public class PayrollDepositDirectiveServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<IPayrollDepositDirectivesRepository> payrollDepositDirectivesRepositoryMock;
        public Mock<IBankRepository> bankRepositoryMock;
        public Mock<IBankingAuthenticationClaimRepository> bankingAuthenticationClaimRepositoryMock;
        public Mock<IBankingInformationConfigurationRepository> bankingInformationConfigurationRepoMock;

        public PayrollDepositDirectiveService serviceUnderTest;

        public FunctionEqualityComparer<Dtos.Base.PayrollDepositDirective> dtoComparer;

        public void PayrollDepositDirectiveServiceTestsInitialize()
        {
            MockInitialize();

            payrollDepositDirectivesRepositoryMock = new Mock<IPayrollDepositDirectivesRepository>();
            bankRepositoryMock = new Mock<IBankRepository>();
            bankingAuthenticationClaimRepositoryMock = new Mock<IBankingAuthenticationClaimRepository>();
            bankingInformationConfigurationRepoMock = new Mock<IBankingInformationConfigurationRepository>();
            serviceUnderTest = new PayrollDepositDirectiveService(
                payrollDepositDirectivesRepositoryMock.Object,
                bankRepositoryMock.Object,
                bankingAuthenticationClaimRepositoryMock.Object,
                bankingInformationConfigurationRepoMock.Object,
                adapterRegistryMock.Object,
                employeeCurrentUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object);

            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.HumanResources.Entities.PayrollDepositDirective, Dtos.Base.PayrollDepositDirective>())
                .Returns(() => new AutoMapperAdapter<Domain.HumanResources.Entities.PayrollDepositDirective, Dtos.Base.PayrollDepositDirective>(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.Base.Entities.BankingAuthenticationToken, Dtos.Base.BankingAuthenticationToken>())
                .Returns(() => new AutoMapperAdapter<Domain.Base.Entities.BankingAuthenticationToken, Dtos.Base.BankingAuthenticationToken>(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.Base.PayrollDepositDirective, Domain.HumanResources.Entities.PayrollDepositDirective>())
                .Returns(() => new AutoMapperAdapter<Dtos.Base.PayrollDepositDirective, Domain.HumanResources.Entities.PayrollDepositDirective>(adapterRegistryMock.Object, loggerMock.Object));

            dtoComparer = new FunctionEqualityComparer<Dtos.Base.PayrollDepositDirective>((a, b) => a.Id == b.Id, (d) => d.Id.GetHashCode());

        }

        [TestClass]
        public class GetPayrollDepositDirectivesTests : PayrollDepositDirectiveServiceTests
        {


            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveServiceTestsInitialize();
                payrollDepositDirectivesRepositoryMock.Setup(r => r.GetPayrollDepositDirectivesAsync(It.IsAny<string>()))
                    .Returns<string>(employeeId => Task.FromResult<Domain.HumanResources.Entities.PayrollDepositDirectiveCollection>(new Domain.HumanResources.Entities.PayrollDepositDirectiveCollection(employeeId)
                    {
                        new Domain.HumanResources.Entities.PayrollDepositDirective("foo", employeeId, "275079633", "Badger Meter Bank", BankAccountType.Checking, "4444", "myBank", false, 1, 1234, new DateTime(2017, 1, 1), null, null),
                        new Domain.HumanResources.Entities.PayrollDepositDirective("bar", employeeId, "091000019", "Water Meter Bank", BankAccountType.Checking, "4445", "myBankz", false, 2, 1234, new DateTime(2017, 1, 1), null, null)
                    }));

            }

            [TestMethod]
            public async Task Test()
            {
                var result = (await serviceUnderTest.GetPayrollDepositDirectivesAsync()).ToList();
                Assert.AreEqual(2, result.Count());
                Assert.AreEqual("foo", result[0].Id);
                Assert.AreEqual("bar", result[1].Id);
            }
        }

        [TestClass]
        public class GetPayrollDepositDirectiveTests : PayrollDepositDirectiveServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveServiceTestsInitialize();
                payrollDepositDirectivesRepositoryMock.Setup(r => r.GetPayrollDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>((depositId, employeeId) => Task.FromResult<Domain.HumanResources.Entities.PayrollDepositDirective>((new Domain.HumanResources.Entities.PayrollDepositDirectiveCollection(employeeId)
                    {
                        new Domain.HumanResources.Entities.PayrollDepositDirective("foo", employeeId, "275079633", "Badger Meter Bank", BankAccountType.Checking, "4444", "myBank", false, 1, 1234, new DateTime(2017, 1, 1), null, null),
                        new Domain.HumanResources.Entities.PayrollDepositDirective("bar", employeeId, "091000019", "Water Meter Bank", BankAccountType.Checking, "4445", "myBankz", false, 2, 1234, new DateTime(2017, 1, 1), null, null),
                        new Domain.HumanResources.Entities.PayrollDepositDirective("var", employeeId, "091000019", "Gas Meter Bank", BankAccountType.Checking, "4446", "miBanco", false, 3, 1234, new DateTime(2017, 1, 1), null, null)
                    }).FirstOrDefault(dir => dir.Id == depositId)));

            }

            [TestMethod]
            public async Task Test()
            {
                var result = await serviceUnderTest.GetPayrollDepositDirectiveAsync("var");
                Assert.AreEqual("var", result.Id);
            }
        }

        [TestClass]
        public class UpdatePayrollDepositDirectiveTests : PayrollDepositDirectiveServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveServiceTestsInitialize();
                payrollDepositDirectivesRepositoryMock.Setup(r => r.UpdatePayrollDepositDirectivesAsync(It.IsAny<Domain.HumanResources.Entities.PayrollDepositDirectiveCollection>()))
                    .Returns<Domain.HumanResources.Entities.PayrollDepositDirectiveCollection>((collection) =>
                        Task.FromResult(new Domain.HumanResources.Entities.PayrollDepositDirectiveCollection(collection.EmployeeId, collection)));

                bankingAuthenticationClaimRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
                    .Returns<Guid>((guid) => Task.FromResult(new BankingAuthenticationToken(DateTimeOffset.Now.AddMinutes(1D), new Guid())));
                bankingInformationConfigurationRepoMock.Setup(r => r.GetBankingInformationConfigurationAsync())
                    .ReturnsAsync(new BankingInformationConfiguration() { IsDirectDepositEnabled = false });

            }

            //[TestMethod]
            //public async Task Test()
            //{
            //    var directive = new Dtos.HumanResources.PayrollDepositDirective(){
            //        Id="1", PersonId="0003914",RoutingId="091000019",BankName="hey-oh",BankAccountType=Dtos.Base.BankAccountType.Checking,AccountIdLastFour="$$$$",Nickname="harperlewis",IsVerified=false,Priority=1,DepositAmount=1.00M,StartDate=DateTime.Now,EndDate=null,Timestamp=null
            //    };
            //    var result = await serviceUnderTest.UpdatePayrollDepositDirectiveAsync((new Guid()).ToString(),directive);
            //    Assert.AreEqual(directive.Id, result.Id);
            //}
        }

        [TestClass]
        public class UpdatePayrollDepositDirectivesTests : PayrollDepositDirectiveServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveServiceTestsInitialize();
                payrollDepositDirectivesRepositoryMock.Setup(r => r.UpdatePayrollDepositDirectivesAsync(It.IsAny<Domain.HumanResources.Entities.PayrollDepositDirectiveCollection>()))
                    .Returns<Domain.HumanResources.Entities.PayrollDepositDirectiveCollection>((collection) =>
                        Task.FromResult(new Domain.HumanResources.Entities.PayrollDepositDirectiveCollection(collection.EmployeeId, collection)));

                bankingAuthenticationClaimRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
                    .Returns<Guid>((guid) => Task.FromResult(new BankingAuthenticationToken(DateTimeOffset.Now.AddMinutes(1D), new Guid())));

                bankingInformationConfigurationRepoMock.Setup(r => r.GetBankingInformationConfigurationAsync())
                    .ReturnsAsync(new BankingInformationConfiguration() { IsDirectDepositEnabled = false });
            }

            //[TestMethod]
            //public async Task Test()
            //{
            //    var directives = new List<Dtos.HumanResources.PayrollDepositDirective>()
            //    {
            //        new Dtos.HumanResources.PayrollDepositDirective()
            //        {
            //            Id = "1",
            //            PersonId = "0003914",
            //            RoutingId = "091000019",
            //            BankName = "hey-oh",
            //            BankAccountType = Dtos.Base.BankAccountType.Checking,
            //            AccountIdLastFour = "$$$$",
            //            Nickname = "harperlewis",
            //            IsVerified = false,
            //            Priority = 1,
            //            DepositAmount = 1.00M,
            //            StartDate = DateTime.Now,
            //            EndDate = null,
            //            Timestamp = null
            //        }
            //    };
            //    var result = await serviceUnderTest.UpdatePayrollDepositDirectivesAsync((new Guid()).ToString(), directives);
            //    CollectionAssert.AreEqual(directives, result.ToList(), dtoComparer);
            //}
        }

        [TestClass]
        public class CreatePayrollDepositDirectiveTests : PayrollDepositDirectiveServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveServiceTestsInitialize();
                payrollDepositDirectivesRepositoryMock.Setup(r => r.CreatePayrollDepositDirectiveAsync(It.IsAny<Domain.HumanResources.Entities.PayrollDepositDirective>()))
                    .Returns<Domain.HumanResources.Entities.PayrollDepositDirective>((directive) =>
                        Task.FromResult(new Domain.HumanResources.Entities.PayrollDepositDirective(
                            (new Random().Next()).ToString(),
                            directive.PersonId, directive.RoutingId, directive.BankName, directive.BankAccountType, directive.AccountIdLastFour, directive.Nickname,
                            directive.IsVerified, directive.Priority, directive.DepositAmount, directive.StartDate, directive.EndDate, directive.Timestamp
                    )));

                bankingAuthenticationClaimRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
                    .Returns<Guid>((guid) => Task.FromResult(new BankingAuthenticationToken(DateTimeOffset.Now.AddMinutes(1D), new Guid())));

                bankingInformationConfigurationRepoMock.Setup(r => r.GetBankingInformationConfigurationAsync())
                    .ReturnsAsync(new BankingInformationConfiguration() { IsDirectDepositEnabled = false });
            }

            [TestMethod]
            public async Task Test()
            {
                var directive = new Dtos.Base.PayrollDepositDirective()
                {
                    Id = "",
                    NewAccountId = "janedoe",
                    PersonId = "0003914",
                    RoutingId = "091000019",
                    BankName = "hey-oh",
                    BankAccountType = Dtos.Base.BankAccountType.Checking,
                    AccountIdLastFour = "$$$$",
                    Nickname = "harperlewis",
                    IsVerified = false,
                    Priority = 1,
                    DepositAmount = 1.00M,
                    StartDate = DateTime.Now,
                    EndDate = null,
                    Timestamp = null
                };
                var result = await serviceUnderTest.CreatePayrollDepositDirectiveAsync((new Guid()).ToString(), directive);
                Assert.IsFalse(string.IsNullOrEmpty(result.Id));
                Assert.IsTrue(string.IsNullOrEmpty(result.NewAccountId));
            }
        }


        [TestClass]
        public class DeletePayrollDepositDirectiveTests : PayrollDepositDirectiveServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveServiceTestsInitialize();
                payrollDepositDirectivesRepositoryMock.Setup(r => r.DeletePayrollDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>((id, empId) => Task.FromResult(true));

                bankingAuthenticationClaimRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
                    .Returns<Guid>((guid) => Task.FromResult(new BankingAuthenticationToken(DateTimeOffset.Now.AddMinutes(1D), new Guid())));

                bankingInformationConfigurationRepoMock.Setup(r => r.GetBankingInformationConfigurationAsync())
                    .ReturnsAsync(new BankingInformationConfiguration() { IsDirectDepositEnabled = false });
            }

            [TestMethod]
            public async Task DeletePayrollDepositDirectiveAsync_ReturnsBool()
            {
                var result = await serviceUnderTest.DeletePayrollDepositDirectiveAsync((new Guid()).ToString(), "theid");
                Assert.IsInstanceOfType(result, typeof(bool));
            }
        }

        [TestClass]
        public class AuthorizeCurrentUserTest : PayrollDepositDirectiveServiceTests
        {
            public DateTimeOffset expectedExpiration;
            public Guid expectedToken;

            public string inputDirectiveId;

            [TestInitialize]
            public void Initialize()
            {
                PayrollDepositDirectiveServiceTestsInitialize();

                expectedExpiration = new DateTimeOffset(new DateTime(2017, 3, 21, 13, 17, 0), TimeSpan.FromHours(-4));
                expectedToken = Guid.NewGuid();

                inputDirectiveId = "foobar";

                payrollDepositDirectivesRepositoryMock.Setup(r => r.AuthenticatePayrollDepositDirective(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string, string>((employeeId, recordId, accountId) => Task.FromResult(new BankingAuthenticationToken(expectedExpiration, expectedToken)));

                payrollDepositDirectivesRepositoryMock.Setup(r => r.GetPayrollDepositDirectivesAsync(It.IsAny<string>()))
                    .Returns<string>(employeeId => Task.FromResult<Domain.HumanResources.Entities.PayrollDepositDirectiveCollection>(new Domain.HumanResources.Entities.PayrollDepositDirectiveCollection(employeeId)
                    {
                        new Domain.HumanResources.Entities.PayrollDepositDirective(inputDirectiveId, employeeId, "275079633", "Badger Meter Bank", BankAccountType.Checking, "4444", "myBank", false, 1, 1234, new DateTime(2017, 1, 1), null, null)
                    }));


            }

            [TestMethod]
            public async Task AuthenticateCurrentUserAsync_ReturnsToken()
            {
                bankingInformationConfigurationRepoMock.Setup(r => r.GetBankingInformationConfigurationAsync())
                    .ReturnsAsync(new BankingInformationConfiguration() { IsDirectDepositEnabled = false });
                var result = await serviceUnderTest.AuthenticateCurrentUserAsync(inputDirectiveId, "raboof");
                Assert.AreEqual(expectedExpiration, result.ExpirationDateTimeOffset);
                Assert.AreEqual(expectedToken, result.Token);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AuthenticateCurrentUserAsync_ThrowsExceptionWithoutBankInfoConfig()
            {
                bankingInformationConfigurationRepoMock.Setup(r => r.GetBankingInformationConfigurationAsync())
                    .ReturnsAsync(null);
                var result = await serviceUnderTest.AuthenticateCurrentUserAsync(inputDirectiveId, "raboof");
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task AuthenticateCurrentUserAsync_ThrowsExceptionWhenAuthDisabled()
            {
                bankingInformationConfigurationRepoMock.Setup(r => r.GetBankingInformationConfigurationAsync())
                    .ReturnsAsync(new BankingInformationConfiguration() { IsDirectDepositEnabled = false, IsAccountAuthenticationDisabled = true });
                var result = await serviceUnderTest.AuthenticateCurrentUserAsync(inputDirectiveId, "raboof");
            }
        }
    }
}
