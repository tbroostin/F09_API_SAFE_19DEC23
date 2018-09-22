//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class InstitutionJobSupervisorServiceTests
    {
        [TestClass]
        public class InstitutionJobSupervisorServiceTests_GET: CurrentUserSetup
        {
            Mock<IPositionRepository> positionRepositoryMock;
            Mock<IInstitutionJobsRepository> institutionJobRepositoryMock;
            Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            Mock<IPersonRepository> personRepositoryMock;
            Mock<IEmployeeRepository> employeeRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IConfigurationRepository> configurationRepositoryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            InstitutionJobSupervisorsService institutionJobSupervisorService;
            IEnumerable<Domain.HumanResources.Entities.InstitutionJobs> institutionJobEntities;
            Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int> institutionJobEntityTuple;

            //IEnumerable<Domain.Base.Entities.Person> personEntities;

            //IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> employmentClassificationEntities;
            //IEnumerable<Domain.HumanResources.Entities.JobChangeReason> jobChangeReasonEntities;
            //IEnumerable<Domain.ColleagueFinance.Entities.AccountsPayableSources> acctPaySourceEntities;
            //IEnumerable<Domain.ColleagueFinance.Entities.CurrencyConversion> currencyConversionEntities;
            //IEnumerable<Domain.Base.Entities.Institution> institutionsEntities;
            //IEnumerable<Domain.HumanResources.Entities.PositionPay> positionPayEntities;

            private Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize() 
            {
                positionRepositoryMock = new Mock<IPositionRepository>();
                institutionJobRepositoryMock = new Mock<IInstitutionJobsRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                personRepositoryMock = new Mock<IPersonRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewInstitutionJobSupervisor);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                institutionJobSupervisorService = new InstitutionJobSupervisorsService(positionRepositoryMock.Object, hrReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object, personRepositoryMock.Object, institutionJobRepositoryMock.Object,
                                                configurationRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup() 
            {
                institutionJobEntityTuple = null;
                institutionJobEntities = null;
                institutionJobRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
                referenceDataRepositoryMock = null;
            }

            [TestMethod]
            public async Task InstitutionJobSupervisors_GETAllAsync()
            {
                var actualsTuple =
                    await
                        institutionJobSupervisorService.GetInstitutionJobSupervisorsAsync(offset, limit, It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = institutionJobEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                }
            }

            //[TestMethod]
            //public async Task InstitutionJobSupervisors_GETAllFilterAsync()
            //{
            //    string personId = "0000011";
            //    personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
            //    string positionId = "x";
            //    positionRepositoryMock.Setup(i => i.GetPositionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(positionId);

            //    var actualsTuple =
            //        await
            //            institutionJobSupervisorService.GetInstitutionJobsAsync(offset, limit, "cd385d31-75ed-4d93-9a1b-4776a951396d",
            //            It.IsAny<string>(), "fadbb5f0-e39d-4b1e-82c9-77617ee2164c", "Math", "2000-01-01 00:00:00.000",
            //            "2020-12-31 00:00:00.000", "active", It.IsAny<string>(), "primary", It.IsAny<bool>());

            //    Assert.IsNotNull(actualsTuple);

            //    int count = actualsTuple.Item1.Count();

            //    for (int i = 0; i < count; i++)
            //    {
            //        var expected = institutionJobEntities.ToList()[i];
            //        var actual = actualsTuple.Item1.ToList()[i];

            //        Assert.IsNotNull(actual);

            //        Assert.AreEqual(expected.Guid, actual.Id);
            //    }
            //}

            [TestMethod]
            public async Task InstitutionJobSupervisors_GETAllAsync_EmptyTuple()
            {
                institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
                {

                };
                institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
                institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionJobEntityTuple);
                var actualsTuple = await institutionJobSupervisorService.GetInstitutionJobSupervisorsAsync(offset, limit, It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task InstitutionJobSupervisors_GET_ById()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = institutionJobEntities.ToList()[0];
                institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).ReturnsAsync(expected);
                var actual = await institutionJobSupervisorService.GetInstitutionJobSupervisorsByGuidAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
            }       

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstitutionJobSupervisors_GET_ById_NullId_ArgumentNullException()
            {
                var actual = await institutionJobSupervisorService.GetInstitutionJobSupervisorsByGuidAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstitutionJobSupervisors_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<KeyNotFoundException>();
                var actual = await institutionJobSupervisorService.GetInstitutionJobSupervisorsByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task InstitutionJobSupervisors_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<InvalidOperationException>();
                var actual = await institutionJobSupervisorService.GetInstitutionJobSupervisorsByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task InstitutionJobSupervisors_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<RepositoryException>();
                var actual = await institutionJobSupervisorService.GetInstitutionJobSupervisorsByGuidAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InstitutionJobSupervisors_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<Exception>();
                var actual = await institutionJobSupervisorService.GetInstitutionJobSupervisorsByGuidAsync(id);
            }

            private void BuildData()
            {
              
                institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>() 
                {
                    new Domain.HumanResources.Entities.InstitutionJobs("ce4d68f6-257d-4052-92c8-17eed0f088fa", "e9e6837f-2c51-431b-9069-4ac4c0da3041", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", "bfea651b-8e27-4fcd-abe3-04573443c04c", DateTime.Now)
                    { 
                        Employer = "ID",
                        Department = "5b05410c-c94c-464a-98ee-684198bde60b",
                        EndDate = DateTime.Now,
                        EndReason = "Admissions",
                        AccountingStrings = new List<string>() 
                        {
                            "accounting_string",
                        },
                        PayStatus = Domain.HumanResources.Entities.PayStatus.WithPay,
                        BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                        CycleWorkTimeUnits = "HRS",
                        CycleWorkTimeAmount = new decimal(40.0),
                        FullTimeEquivalent = new decimal(40.0),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = new decimal(1600.0),
                        SupervisorId = "supId",
                        AlternateSupervisorId = "altSupId",
                        PayRate = "40000",
                        Grade = "grade",
                        Step = "step",
                        Classification = "ADJ",
                        Primary = true,
                        HostCountry = "USA"

                    },
                    new Domain.HumanResources.Entities.InstitutionJobs("5bc2d86c-6a0c-46b1-824d-485ccb27dc67", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", "e9e6837f-2c51-431b-9069-4ac4c0da3041", "g5u4827d-1a54-232b-9239-5ac4f6dt3257", DateTime.Now)
                    {
                        Employer = "ID",
                        Department = "5b05410c-c94c-464a-98ee-684198bde60b"
                    },
                    new Domain.HumanResources.Entities.InstitutionJobs("7ea5142f-12f1-4ac9-b9f3-73e4205dfc11", "e9e6837f-2c51-431b-9069-4ac4c0da3041", "g5u4827d-1a54-232b-9239-5ac4f6dt3257", "bfea651b-8e27-4fcd-abe3-04573443c04c", DateTime.Now)
                    {
                        Employer = "ID",
                        Department = "5b05410c-c94c-464a-98ee-684198bde60b"   
                    },
                    new Domain.HumanResources.Entities.InstitutionJobs("db8f690b-071f-4d98-8da8-d4312511a4c1", "bfea651b-8e27-4fcd-abe3-04573443c04c", "g5u4827d-1a54-232b-9239-5ac4f6dt3257", "9ae3a175-1dfd-4937-b97b-3c9ad596e023", DateTime.Now)
                    {
                        Employer = "ID",
                        Department = "5b05410c-c94c-464a-98ee-684198bde60b"
                    }
                };
                institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, institutionJobEntities.Count());
                institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(institutionJobEntityTuple);
                institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(It.IsAny<string>())).ReturnsAsync(institutionJobEntities.ToList()[0]);
                personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");

               
            }
        }
    }
}
