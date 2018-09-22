// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass, System.Runtime.InteropServices.GuidAttribute("B182D2FE-3FF7-4FD0-8421-AD19ECE05E07")]
    public class PayableDepositDirectiveServiceTests
    {
        // Sets up a Current user that is a student
        public abstract class CurrentUserSetup
        {


            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Matt",
                            PersonId = "0003914",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        public string personId;
        public Domain.Entities.Role studentRole;

        public Mock<ILogger> loggerMock;
        public Mock<IPayableDepositDirectiveRepository> payableDepositDirectiveRepositoryMock;
        public Mock<IBankingAuthenticationClaimRepository> bankingAuthenticationClaimRepositoryMock;
        public Mock<IBankingInformationConfigurationRepository> bankingInformationConfigurationRepositoryMock;
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;

        public PayableDepositDirectiveService serviceUnderTest;

        public TestPayableDepositDirectiveRepository testData;

        public AutoMapperAdapter<Domain.Base.Entities.PayableDepositDirective, PayableDepositDirective> payableDepositDirectiveAdapter;

        public DateTimeOffset securityTokenExpiration;
        public Guid securityToken;

        public FunctionEqualityComparer<PayableDepositDirective> dtoComparer;

        public void PayableDepositDirectiveServiceTestsInitialize()
        {
            payableDepositDirectiveRepositoryMock = new Mock<IPayableDepositDirectiveRepository>();
            bankingAuthenticationClaimRepositoryMock = new Mock<IBankingAuthenticationClaimRepository>();
            bankingInformationConfigurationRepositoryMock = new Mock<IBankingInformationConfigurationRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();

            serviceUnderTest = new PayableDepositDirectiveService(
                payableDepositDirectiveRepositoryMock.Object,
                bankingAuthenticationClaimRepositoryMock.Object,
                bankingInformationConfigurationRepositoryMock.Object,
                adapterRegistryMock.Object,
                new CurrentUserSetup.StudentUserFactory(),
                roleRepositoryMock.Object,
                loggerMock.Object);

            personId = "0003914";
            testData = new TestPayableDepositDirectiveRepository();


            payableDepositDirectiveAdapter = new AutoMapperAdapter<Domain.Base.Entities.PayableDepositDirective, PayableDepositDirective>(adapterRegistryMock.Object, loggerMock.Object);
            payableDepositDirectiveAdapter.AddMappingDependency<Domain.Base.Entities.Timestamp, Timestamp>();

            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.Base.Entities.PayableDepositDirective, PayableDepositDirective>())
                .Returns(() => payableDepositDirectiveAdapter);
            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.Base.Entities.Timestamp, Timestamp>())
                .Returns(() => new AutoMapperAdapter<Domain.Base.Entities.Timestamp, Timestamp>(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<PayableDepositDirective, Domain.Base.Entities.PayableDepositDirective>())
                    .Returns(() => new PayableDepositDirectiveDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<Timestamp, Domain.Base.Entities.Timestamp>())
                .Returns(() => new AutoMapperAdapter<Timestamp, Domain.Base.Entities.Timestamp>(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<BankAccountType, Domain.Base.Entities.BankAccountType>())
                .Returns(() => new AutoMapperAdapter<BankAccountType, Domain.Base.Entities.BankAccountType>(adapterRegistryMock.Object, loggerMock.Object));
            adapterRegistryMock.Setup(a => a.GetAdapter<Domain.Base.Entities.BankingAuthenticationToken, BankingAuthenticationToken>())
                .Returns(() => new AutoMapperAdapter<Domain.Base.Entities.BankingAuthenticationToken, BankingAuthenticationToken>(adapterRegistryMock.Object, loggerMock.Object));


            studentRole = new Domain.Entities.Role(55, "Student");
            studentRole.AddPermission(new Domain.Entities.Permission(BasePermissionCodes.EditEChecksBankingInformation));
            roleRepositoryMock.Setup(r => r.Roles)
                .Returns(() => new List<Domain.Entities.Role>() { studentRole });

            securityToken = Guid.NewGuid();
            securityTokenExpiration = DateTimeOffset.Now.AddMinutes(10);
            bankingAuthenticationClaimRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
                .Returns<Guid>((guid) => Task.FromResult(new Domain.Base.Entities.BankingAuthenticationToken(securityTokenExpiration, securityToken)));
            //bankingAuthenticationClaimRepositoryMock.Setup(r => r.Delete(It.IsAny<Guid>()))
            //    .Returns<Guid>((guid) => Task.FromResult(1));

            bankingInformationConfigurationRepositoryMock.Setup(r => r.GetBankingInformationConfigurationAsync())
                .ReturnsAsync(new Domain.Base.Entities.BankingInformationConfiguration() { IsDirectDepositEnabled = false });

            payableDepositDirectiveRepositoryMock.Setup(r => r.GetPayableDepositDirectivesAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((payeeId, id) => testData.GetPayableDepositDirectivesAsync(payeeId, id));
            payableDepositDirectiveRepositoryMock.Setup(r => r.UpdatePayableDepositDirectiveAsync(It.IsAny<Domain.Base.Entities.PayableDepositDirective>()))
                .Returns<Domain.Base.Entities.PayableDepositDirective>((d) => testData.UpdatePayableDepositDirectiveAsync(d));
            payableDepositDirectiveRepositoryMock.Setup(r => r.CreatePayableDepositDirectiveAsync(It.IsAny<Domain.Base.Entities.PayableDepositDirective>()))
                .Returns<Domain.Base.Entities.PayableDepositDirective>((d) => testData.CreatePayableDepositDirectiveAsync(d));
            payableDepositDirectiveRepositoryMock.Setup(r => r.DeletePayableDepositDirectiveAsync(It.IsAny<string>()))
                .Returns<string>(id => testData.DeletePayableDepositDirectiveAsync(id));
            payableDepositDirectiveRepositoryMock.Setup(r => r.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string, string, string>((payeeId, directiveId, accountId, addressId) => Task.FromResult(new Domain.Base.Entities.BankingAuthenticationToken(securityTokenExpiration, securityToken)));

            dtoComparer = new FunctionEqualityComparer<PayableDepositDirective>((a, b) => a.Id == b.Id, (d) => d.Id.GetHashCode());
        }

        [TestClass]
        public class GetAllPayableDepositDirectivesTests : PayableDepositDirectiveServiceTests
        {
            public async Task<List<PayableDepositDirective>> Actual()
            {
                return (await serviceUnderTest.GetPayableDepositDirectivesAsync()).ToList();
            }
            public async Task<List<PayableDepositDirective>> Expected()
            {
                return (await testData.GetPayableDepositDirectivesAsync(personId, "")).Select(d =>
                    payableDepositDirectiveAdapter.MapToType(d)).ToList();
            }

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveServiceTestsInitialize();
            }



            [TestMethod]
            public async Task GetAllTest()
            {
                CollectionAssert.AreEqual(await Expected(), await Actual(), dtoComparer);
            }

            [TestMethod]
            public async Task NullDomainObjectReturnsEmptyListTest()
            {
                payableDepositDirectiveRepositoryMock.Setup(r => r.GetPayableDepositDirectivesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(null);

                Assert.IsNotNull(await Actual());
                Assert.IsTrue(!(await Actual()).Any());
            }
        }

        [TestClass]
        public class GetSinglePayableDepositDirectiveTests : PayableDepositDirectiveServiceTests
        {
            public string inputDirectiveId;

            public async Task<PayableDepositDirective> Actual()
            {
                return await serviceUnderTest.GetPayableDepositDirectiveAsync(inputDirectiveId);
            }
            public async Task<PayableDepositDirective> Expected()
            {
                return payableDepositDirectiveAdapter.MapToType((await testData.GetPayableDepositDirectivesAsync(personId, inputDirectiveId))[0]);
            }

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveServiceTestsInitialize();
                inputDirectiveId = testData.payableDepositDirectiveRecords[0].id;
            }

            [TestMethod]
            public async Task GetTest()
            {
                Assert.AreEqual((await Expected()).Id, (await Actual()).Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InputDirectiveIdRequiredTest()
            {
                inputDirectiveId = null;
                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RepositoryReturnsNullListTest()
            {
                payableDepositDirectiveRepositoryMock.Setup(r => r.GetPayableDepositDirectivesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(null);

                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RepositoryReturnsEmptyListTest()
            {
                payableDepositDirectiveRepositoryMock.Setup(r => r.GetPayableDepositDirectivesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.PayableDepositDirectiveCollection(personId));

                await Actual();
            }

        }

        [TestClass]
        public class CreatePayableDepositDirectiveTests : PayableDepositDirectiveServiceTests
        {
            public async Task<Domain.Base.Entities.PayableDepositDirective> Input()
            {
                return (await testData.GetPayableDepositDirectivesAsync(personId))[0];
            }

            public async Task<PayableDepositDirective> Actual()
            {
                return await serviceUnderTest.CreatePayableDepositDirectiveAsync(securityToken.ToString(),
                    payableDepositDirectiveAdapter.MapToType(await Input()));
            }
            //public async Task<PayableDepositDirective> Expected()
            //{
            //    return payableDepositDirectiveAdapter.MapToType(await testData.CreatePayableDepositDirectiveAsync(await Input()));
            //}

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveServiceTestsInitialize();
            }

            [TestMethod]
            public async Task CreateTest()
            {
                var actual = await Actual();
            }

            [TestMethod]
            public async Task VendorPermissionCreateTest()
            {
                studentRole.RemovePermission(studentRole.Permissions.First(p => p.Code == BasePermissionCodes.EditEChecksBankingInformation));
                studentRole.AddPermission(new Domain.Entities.Permission(BasePermissionCodes.EditVendorBankingInformation));

                var actual = await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InputDirectiveRequiredTest()
            {
                await serviceUnderTest.CreatePayableDepositDirectiveAsync(securityToken.ToString(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task InputDirectiveOwnedByDifferentUserTest()
            {
                personId = "foobar";
                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserDoesNotHaveRequiredPermissionCodesTest()
            {
                studentRole.RemovePermission(studentRole.Permissions.First(p => p.Code == BasePermissionCodes.EditEChecksBankingInformation));
                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task TokenIsRequiredTest()
            {
                await serviceUnderTest.CreatePayableDepositDirectiveAsync(null,
                    payableDepositDirectiveAdapter.MapToType(await Input()));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task TokenIsInvalidGuidTest()
            {
                await serviceUnderTest.CreatePayableDepositDirectiveAsync("foobar",
                    payableDepositDirectiveAdapter.MapToType(await Input()));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ClaimsRepositoryThrowsExceptionTest()
            {
                bankingAuthenticationClaimRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
                    .ThrowsAsync(new ApplicationException());

                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task TokenIsExpiredTest()
            {
                securityTokenExpiration = DateTimeOffset.Now.AddMinutes(-1);

                await Actual();
            }

            //[TestMethod]
            //public async Task TokenIsDeletedTest()
            //{
            //    await Actual();

            //    bankingAuthenticationClaimRepositoryMock.Verify(r => r.Delete(It.Is<Guid>(g => g.Equals(securityToken))));
            //}

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task CreateRepositoryMethodReturnsNullTest()
            {
                payableDepositDirectiveRepositoryMock.Setup(r => r.CreatePayableDepositDirectiveAsync(It.IsAny<Domain.Base.Entities.PayableDepositDirective>()))
                    .ReturnsAsync(null);

                await Actual();

            }
        }

        [TestClass]
        public class UpdatePayableDepositDirectiveTests : PayableDepositDirectiveServiceTests
        {
            public async Task<Domain.Base.Entities.PayableDepositDirective> Input()
            {
                return (await testData.GetPayableDepositDirectivesAsync(personId))[0];
            }

            public async Task<PayableDepositDirective> Actual()
            {
                return await serviceUnderTest.UpdatePayableDepositDirectiveAsync(securityToken.ToString(),
                    payableDepositDirectiveAdapter.MapToType(await Input()));
            }
            //public async Task<PayableDepositDirective> Expected()
            //{
            //    return payableDepositDirectiveAdapter.MapToType(await testData.UpdatePayableDepositDirectiveAsync(await Input()));
            //}

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveServiceTestsInitialize();
            }

            [TestMethod]
            public async Task UpdateTest()
            {
                var actual = await Actual();
            }

            [TestMethod]
            public async Task VendorPermissionUpdateTest()
            {
                studentRole.RemovePermission(studentRole.Permissions.First(p => p.Code == BasePermissionCodes.EditEChecksBankingInformation));
                studentRole.AddPermission(new Domain.Entities.Permission(BasePermissionCodes.EditVendorBankingInformation));

                var actual = await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InputDirectiveRequiredTest()
            {
                await serviceUnderTest.UpdatePayableDepositDirectiveAsync(securityToken.ToString(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task IdOfInputDirectiveRequiredTest()
            {
                await serviceUnderTest.UpdatePayableDepositDirectiveAsync(securityToken.ToString(), new PayableDepositDirective()
                {
                    Id = null,
                    PayeeId = "003914"
                });
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PayeeIdOfInputDirectiveRequiredTest()
            {
                await serviceUnderTest.UpdatePayableDepositDirectiveAsync(securityToken.ToString(), new PayableDepositDirective()
                {
                    Id = "foo",
                    PayeeId = null
                });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserMustOwnDepositDirectiveTest()
            {
                personId = "foobar";
                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserDoesNotHaveRequiredPermissionCodesTest()
            {
                studentRole.RemovePermission(studentRole.Permissions.First(p => p.Code == BasePermissionCodes.EditEChecksBankingInformation));
                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RepositoryReturnsNullListTest()
            {
                payableDepositDirectiveRepositoryMock.Setup(r => r.GetPayableDepositDirectivesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(null);

                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RepositoryReturnsEmptyListTest()
            {
                payableDepositDirectiveRepositoryMock.Setup(r => r.GetPayableDepositDirectivesAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new Domain.Base.Entities.PayableDepositDirectiveCollection(personId));

                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task TokenIsRequiredTest()
            {
                await serviceUnderTest.UpdatePayableDepositDirectiveAsync(null,
                    payableDepositDirectiveAdapter.MapToType(await Input()));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task TokenIsInvalidGuidTest()
            {
                await serviceUnderTest.UpdatePayableDepositDirectiveAsync("foobar",
                    payableDepositDirectiveAdapter.MapToType(await Input()));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ClaimsRepositoryThrowsExceptionTest()
            {
                bankingAuthenticationClaimRepositoryMock.Setup(r => r.Get(It.IsAny<Guid>()))
                    .ThrowsAsync(new ApplicationException());

                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task TokenIsExpiredTest()
            {
                securityTokenExpiration = DateTimeOffset.Now.AddMinutes(-1);

                await Actual();
            }

            //[TestMethod]
            //public async Task TokenIsDeletedTest()
            //{
            //    await Actual();

            //    bankingAuthenticationClaimRepositoryMock.Verify(r => r.Delete(It.Is<Guid>(g => g.Equals(securityToken))));
            //}

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RepositoryUpdateMethodReturnsNullTest()
            {
                payableDepositDirectiveRepositoryMock.Setup(r => r.UpdatePayableDepositDirectiveAsync(It.IsAny<Domain.Base.Entities.PayableDepositDirective>()))
                    .ReturnsAsync(null);

                await Actual();

            }
        }

        [TestClass]
        public class DeletePayableDepositDirectiveTests : PayableDepositDirectiveServiceTests
        {
            public string inputId;

            public async Task Actual()
            {
                await serviceUnderTest.DeletePayableDepositDirectiveAsync(securityToken.ToString(), inputId);
                return;
            }

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveServiceTestsInitialize();
                inputId = testData.payableDepositDirectiveRecords[0].id;
            }

            [TestMethod]
            public async Task DeleteTest()
            {
                await Actual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task IdOfInputDirectiveRequiredTest()
            {
                await serviceUnderTest.DeletePayableDepositDirectiveAsync(securityToken.ToString(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task IdOfInputDirectiveNotFoundTest()
            {
                try
                {
                    await serviceUnderTest.DeletePayableDepositDirectiveAsync(securityToken.ToString(), "99999");
                }
                catch (KeyNotFoundException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

            //[TestMethod, ExpectedException(typeof(PermissionsException))]
            //public async Task UserMustOwnDepositDirectiveTest()
            //{
            //}

            [TestMethod, ExpectedException(typeof(PermissionsException))]
            public async Task UserDoesNotHaveRequiredPermissionCodesTest()
            {
                try
                {
                    studentRole.RemovePermission(studentRole.Permissions.First(p => p.Code == BasePermissionCodes.EditEChecksBankingInformation));
                    await Actual();
                }
                catch (PermissionsException ex)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw ex;
                }
            }

        }

        [TestClass]
        public class AuthenticatePayableDepositDirectiveTests : PayableDepositDirectiveServiceTests
        {
            public string directiveId;
            public string accountId;
            public string addressId;

            public async Task<BankingAuthenticationToken> Actual()
            {
                return await serviceUnderTest.AuthenticatePayableDepositDirectiveAsync(directiveId, accountId, addressId);
            }

            [TestInitialize]
            public void Initialize()
            {
                PayableDepositDirectiveServiceTestsInitialize();
                directiveId = testData.payableDepositDirectiveRecords[0].id;
                accountId = "foobar";
                addressId = null;
            }

            [TestMethod]
            public async Task AuthenticateTest()
            {
                var actual = await Actual();
                Assert.AreEqual(securityToken, actual.Token);
                Assert.AreEqual(securityTokenExpiration, actual.ExpirationDateTimeOffset);

            }

            [TestMethod, ExpectedException(typeof(PermissionsException))]
            public async Task AuthenticateDirectiveIdNotOwnedByUserThrowsExceptionTest()
            {
                var depositDirectives = await serviceUnderTest.AuthenticatePayableDepositDirectiveAsync("99999", accountId, addressId);
            }

        }
    }
}
