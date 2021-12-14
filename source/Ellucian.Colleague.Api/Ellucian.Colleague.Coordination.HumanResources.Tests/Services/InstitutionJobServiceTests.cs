//Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

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
    public class InstitutionJobServiceTests_V8 : CurrentUserSetup
    {
        Mock<IPositionRepository> positionRepositoryMock;
        Mock<IInstitutionJobsRepository> institutionJobRepositoryMock;
        Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
        Mock<IPersonRepository> personRepositoryMock;
        Mock<IEmployeeRepository> employeeRepositoryMock;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        ICurrentUserFactory currentUserFactory;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;

        InstitutionJobsService institutionJobService;
        IEnumerable<Domain.HumanResources.Entities.InstitutionJobs> institutionJobEntities;
        Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int> institutionJobEntityTuple;

        //IEnumerable<Domain.Base.Entities.Person> personEntities;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> employmentClassificationEntities;
        IEnumerable<Domain.HumanResources.Entities.JobChangeReason> jobChangeReasonEntities;
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
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            BuildData();
            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            // Mock permissions
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewInstitutionJob);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            institutionJobService = new InstitutionJobsService(positionRepositoryMock.Object, hrReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object, personRepositoryMock.Object, institutionJobRepositoryMock.Object,
                                           baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            institutionJobEntityTuple = null;
            institutionJobEntities = null;
            employmentClassificationEntities = null;
            jobChangeReasonEntities = null;
            institutionJobRepositoryMock = null;
            hrReferenceDataRepositoryMock = null;
            adapterRegistryMock = null;
            currentUserFactory = null;
            roleRepositoryMock = null;
            loggerMock = null;
            referenceDataRepositoryMock = null;
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync()
        {
            var actualsTuple =
                await
                    institutionJobService.GetInstitutionJobsAsync(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

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

        [TestMethod]
        public async Task InstitutionJobs_GETAllFilterAsync()
        {
            string personId = "0000011";
            personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
            string positionId = "x";
            positionRepositoryMock.Setup(i => i.GetPositionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(positionId);

            var actualsTuple =
                await
                    institutionJobService.GetInstitutionJobsAsync(offset, limit, "cd385d31-75ed-4d93-9a1b-4776a951396d",
                    It.IsAny<string>(), "fadbb5f0-e39d-4b1e-82c9-77617ee2164c", "Math", "2000-01-01 00:00:00.000",
                    "2020-12-31 00:00:00.000", "active", It.IsAny<string>(), "primary", It.IsAny<bool>());

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

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobsAsync(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task InstitutionJobs_GET_ById()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            var expected = institutionJobEntities.ToList()[0];
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).ReturnsAsync(expected);
            var actual = await institutionJobService.GetInstitutionJobsByGuidAsync(id);

            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Guid, actual.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstitutionJobs_GET_ById_NullId_ArgumentNullException()
        {
            var actual = await institutionJobService.GetInstitutionJobsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstitutionJobs_GET_ById_ReturnsNullEntity_KeyNotFoundException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<KeyNotFoundException>();
            var actual = await institutionJobService.GetInstitutionJobsByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task InstitutionJobs_GET_ById_ReturnsNullEntity_InvalidOperationException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<InvalidOperationException>();
            var actual = await institutionJobService.GetInstitutionJobsByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task InstitutionJobs_GET_ById_ReturnsNullEntity_RepositoryException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<RepositoryException>();
            var actual = await institutionJobService.GetInstitutionJobsByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobs_GET_ById_ReturnsNullEntity_Exception()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<Exception>();
            var actual = await institutionJobService.GetInstitutionJobsByGuidAsync(id);
        }

        private void BuildData()
        {
            jobChangeReasonEntities = new List<Domain.HumanResources.Entities.JobChangeReason>()
                {
                    new Domain.HumanResources.Entities.JobChangeReason("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.HumanResources.Entities.JobChangeReason("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.HumanResources.Entities.JobChangeReason("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.HumanResources.Entities.JobChangeReason("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.HumanResources.Entities.JobChangeReason("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
            hrReferenceDataRepositoryMock.Setup(i => i.GetJobChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(jobChangeReasonEntities);

            employmentClassificationEntities = new List<Domain.HumanResources.Entities.EmploymentClassification>()
                {
                    new Domain.HumanResources.Entities.EmploymentClassification("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test", Domain.HumanResources.Entities.EmploymentClassificationType.Position),
                    new Domain.HumanResources.Entities.EmploymentClassification("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support", Domain.HumanResources.Entities.EmploymentClassificationType.Position),
                };
            hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(employmentClassificationEntities);


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
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(It.IsAny<string>())).ReturnsAsync(institutionJobEntities.ToList()[0]);
            personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
            positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");


        }
    }

    [TestClass]
    public class InstitutionJobServiceTests_V11 : CurrentUserSetup
    {
        Mock<IPositionRepository> positionRepositoryMock;
        Mock<IInstitutionJobsRepository> institutionJobRepositoryMock;
        Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
        Mock<IPersonRepository> personRepositoryMock;
        Mock<IEmployeeRepository> employeeRepositoryMock;
        Mock<IAdapterRegistry> adapterRegistryMock;
        Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        ICurrentUserFactory currentUserFactory;
        Mock<IRoleRepository> roleRepositoryMock;
        Mock<ILogger> loggerMock;

        InstitutionJobsService institutionJobService;
        IEnumerable<Domain.HumanResources.Entities.InstitutionJobs> institutionJobEntities;
        Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int> institutionJobEntityTuple;

        //IEnumerable<Domain.Base.Entities.Person> personEntities;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> employmentClassificationEntities;
        IEnumerable<Domain.HumanResources.Entities.JobChangeReason> jobChangeReasonEntities;
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
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();

            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            BuildData();
            // Set up current user
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            // Mock permissions
            permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewInstitutionJob);
            personRole.AddPermission(permissionViewAnyPerson);
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

            institutionJobService = new InstitutionJobsService(positionRepositoryMock.Object, hrReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object, personRepositoryMock.Object, institutionJobRepositoryMock.Object,
                                           baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            institutionJobEntityTuple = null;
            institutionJobEntities = null;
            employmentClassificationEntities = null;
            jobChangeReasonEntities = null;
            institutionJobRepositoryMock = null;
            hrReferenceDataRepositoryMock = null;
            adapterRegistryMock = null;
            currentUserFactory = null;
            roleRepositoryMock = null;
            loggerMock = null;
            referenceDataRepositoryMock = null;
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync()
        {
            var actualsTuple =
                await
                    institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

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

        [TestMethod]
        public async Task InstitutionJobs_GETAllFilterAsync()
        {
            string personId = "0000011";
            personRepositoryMock.Setup(i => i.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(personId);
            string positionId = "x";
            positionRepositoryMock.Setup(i => i.GetPositionIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(positionId);

            var actualsTuple =
                await
                    institutionJobService.GetInstitutionJobs2Async(offset, limit, "cd385d31-75ed-4d93-9a1b-4776a951396d",
                    It.IsAny<string>(), "fadbb5f0-e39d-4b1e-82c9-77617ee2164c", "Math", "2000-01-01 00:00:00.000",
                    "2020-12-31 00:00:00.000", "active", It.IsAny<string>(), "primary", It.IsAny<bool>());

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

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple_InvalidPersonFilter()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple_InvalidEmployerFilter()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple_InvalidPositionFilter()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple_InvalidDepartmentFilter()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple_InvalidStartDateFilter()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple_InvalidEndDateFilter()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }


        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple_InvalidStatusFilter()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple_InvalidClassificaitonFilter()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<string>(), It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }

        [TestMethod]
        public async Task InstitutionJobs_GETAllAsync_EmptyTuple_InvalidPreferenceFilter()
        {
            institutionJobEntities = new List<Domain.HumanResources.Entities.InstitutionJobs>()
            {

            };
            institutionJobEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.InstitutionJobs>, int>(institutionJobEntities, 0);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            var actualsTuple = await institutionJobService.GetInstitutionJobs2Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "INVALID", It.IsAny<bool>());

            Assert.AreEqual(0, actualsTuple.Item1.Count());
        }
        [TestMethod]
        public async Task InstitutionJobs_GET_ById()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            var expected = institutionJobEntities.ToList()[0];
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).ReturnsAsync(expected);
            var actual = await institutionJobService.GetInstitutionJobsByGuid2Async(id);

            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Guid, actual.Id);

            var expectgedPerposwgItem = expected.PerposwgItems
                .Where(x => x.AccountingStringAllocation[0].GlNumber == "11-00-02-67-60000-53011").ToList();
            Assert.IsNotNull(expectgedPerposwgItem);
            var accountingStringAllocation = actual.AccountingStringAllocations
                .Where(asa => asa.AccountingString == "11-00-02-67-60000-53011*12345").ToList();
            Assert.IsNotNull(accountingStringAllocation);
            Assert.AreEqual(1, accountingStringAllocation.Count());
            Assert.AreEqual(expectgedPerposwgItem[0].AccountingStringAllocation[0].GlPercentDistribution, accountingStringAllocation[0].AllocatedPercentage);
            Assert.AreEqual(expectgedPerposwgItem[0].StartDate, accountingStringAllocation[0].StartOn);
        }

        [TestMethod]
        public async Task InstitutionJobs_GET_ById_StatusIsActive_endOnIsToday()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            var expected = institutionJobEntities.ToList()[0];
            expected.EndDate = DateTime.Now;
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).ReturnsAsync(expected);
            var actual = await institutionJobService.GetInstitutionJobsByGuid2Async(id);

            Assert.IsNotNull(actual);

            Assert.AreEqual(expected.Guid, actual.Id);
            Assert.AreEqual(Dtos.EnumProperties.InstitutionJobsStatus.Active,actual.Status);
            Assert.AreEqual(expected.EndDate, actual.EndOn);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstitutionJobs_GET_ById_NullId_ArgumentNullException()
        {
            await institutionJobService.GetInstitutionJobsByGuid2Async(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstitutionJobs_GET_ById_ReturnsNullEntity_KeyNotFoundException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<KeyNotFoundException>();
            await institutionJobService.GetInstitutionJobsByGuid2Async(id);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task InstitutionJobs_GET_ById_ReturnsNullEntity_InvalidOperationException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<InvalidOperationException>();
            await institutionJobService.GetInstitutionJobsByGuid2Async(id);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task InstitutionJobs_GET_ById_ReturnsNullEntity_RepositoryException()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<RepositoryException>();
            await institutionJobService.GetInstitutionJobsByGuid2Async(id);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobs_GET_ById_ReturnsNullEntity_Exception()
        {
            var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(id)).Throws<Exception>();
            await institutionJobService.GetInstitutionJobsByGuid2Async(id);
        }

        private void BuildData()
        {
            jobChangeReasonEntities = new List<Domain.HumanResources.Entities.JobChangeReason>()
                {
                    new Domain.HumanResources.Entities.JobChangeReason("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions"),
                    new Domain.HumanResources.Entities.JobChangeReason("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business"),
                    new Domain.HumanResources.Entities.JobChangeReason("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics"),
                    new Domain.HumanResources.Entities.JobChangeReason("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science"),
                    new Domain.HumanResources.Entities.JobChangeReason("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology"),
                };
            hrReferenceDataRepositoryMock.Setup(i => i.GetJobChangeReasonsAsync(It.IsAny<bool>())).ReturnsAsync(jobChangeReasonEntities);

            employmentClassificationEntities = new List<Domain.HumanResources.Entities.EmploymentClassification>()
                {
                    new Domain.HumanResources.Entities.EmploymentClassification("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test", Domain.HumanResources.Entities.EmploymentClassificationType.Position),
                    new Domain.HumanResources.Entities.EmploymentClassification("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support", Domain.HumanResources.Entities.EmploymentClassificationType.Position),
                };
            hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(employmentClassificationEntities);

            var perposwgItems = new List<Domain.HumanResources.Entities.PersonPositionWageItem>()
            {
                new Domain.HumanResources.Entities.PersonPositionWageItem()
                {
                    PayRate = "5000",
                    AccountingStringAllocation = new List<Domain.HumanResources.Entities.PpwgAccountingStringAllocation>
                    {
                        new Domain.HumanResources.Entities.PpwgAccountingStringAllocation()
                        {
                            GlNumber = "11-00-02-67-60000-53011",
                            PpwgProjectsId = "12345",
                            GlPercentDistribution = 100
                        }
                    },
                    StartDate = new DateTime(2017,7,17)
                }
            };

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
                        HostCountry = "USA",
                        PerposwgItems = perposwgItems

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
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync(institutionJobEntityTuple);
            institutionJobRepositoryMock.Setup(i => i.GetInstitutionJobsByGuidAsync(It.IsAny<string>())).ReturnsAsync(institutionJobEntities.ToList()[0]);
            personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");
            positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("db8f690b-071f-4d98-8da8-d4312511a4c2");


        }
    }

    [TestClass]
    public class InstitutionJobServiceTests_POST_PUT_V12 : CurrentUserSetup
    {
        #region DECLARATION

        protected Domain.Entities.Role createInstitutionJob = new Domain.Entities.Role( 1, "CREATE.UPDATE.INSTITUTION.JOB" );
        protected Domain.Entities.Role viewInstitutionJob = new Domain.Entities.Role( 2, "VIEW.INSTITUTION.JOB" );

        private Mock<IPositionRepository> positionRepositoryMock;
        private Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Mock<IPersonRepository> personRepositoryMock;
        private Mock<IInstitutionJobsRepository> institutionJobsRepositoryMock;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private Mock<IRoleRepository> roleRepositoryMock;
        private Mock<ILogger> loggerMock;
        private Mock<IConfigurationRepository> configurationRepositoryMock;

        private ICurrentUserFactory currentUserFactory;

        private InstitutionJobsService institutionJobService;

        private Dtos.InstitutionJobs3 institutionJobs;
        private Domain.HumanResources.Entities.InstitutionJobs domainInstitutionJobs;
        private Domain.HumanResources.Entities.Position position;
        private List<Domain.HumanResources.Entities.JobChangeReason> jobChangeReasons;
        private List<Domain.HumanResources.Entities.EmploymentClassification> classifications;
        private List<Domain.HumanResources.Entities.EmploymentDepartment> departments;
        private Dictionary<string, string> personGuidCollection;
        private List<Domain.HumanResources.Entities.PayClass> payClasses;
        private List<Domain.HumanResources.Entities.PayCycle2> payCycles;
        private List<Domain.HumanResources.Entities.TimeUnits> timeUnits;

        private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            positionRepositoryMock = new Mock<IPositionRepository>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            personRepositoryMock = new Mock<IPersonRepository>();
            configurationRepositoryMock = new Mock<IConfigurationRepository>();
            institutionJobsRepositoryMock = new Mock<IInstitutionJobsRepository>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            loggerMock = new Mock<ILogger>();

            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            InitializeTestData();

            InitializeMock();

            institutionJobService = new InstitutionJobsService(positionRepositoryMock.Object, hrReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object,
                personRepositoryMock.Object, institutionJobsRepositoryMock.Object, configurationRepositoryMock.Object, adapterRegistryMock.Object,
                currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            positionRepositoryMock = null;
            adapterRegistryMock = null;
            hrReferenceDataRepositoryMock = null;
            referenceDataRepositoryMock = null;
            personRepositoryMock = null;
            roleRepositoryMock = null;
            loggerMock = null;
            currentUserFactory = null;
            configurationRepositoryMock = null;
            institutionJobsRepositoryMock = null;
        }

        private void InitializeTestData()
        {
            personGuidCollection = new Dictionary<string, string>();
            personGuidCollection.Add("1", guid);
            domainInstitutionJobs = new Domain.HumanResources.Entities.InstitutionJobs(guid, "1", "1")
            {
                Employer = "1",
                EndReason = "1",
                AccountingStrings = new List<string>() { "1-1" },
                EndDate = DateTime.Today.AddDays(100),
                PayStatus = Domain.HumanResources.Entities.PayStatus.PartialPay,
                BenefitsStatus = Domain.HumanResources.Entities.BenefitsStatus.WithBenefits,
                CycleWorkTimeUnits = "HRS",
                CycleWorkTimeAmount = 100,
                YearWorkTimeUnits = "HRS",
                YearWorkTimeAmount = 1000,
                FullTimeEquivalent = 10,
                SupervisorId = "1",
                AlternateSupervisorId = "1",
                PayRate = "1",
                Grade = "1",
                Step = "1",
                HostCountry = "USA",
                IsSalary = true,
                Classification = "1",
                Primary = true,
                Department = "1",  // making sure change sticks
                PerposwgItems = new List<Domain.HumanResources.Entities.PersonPositionWageItem>()
                {
                    new Domain.HumanResources.Entities.PersonPositionWageItem()
                    {
                         StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(100), PayRate = "10",
                        Grade = "1", Step = "1",
                        AccountingStringAllocation = new List<Domain.HumanResources.Entities.PpwgAccountingStringAllocation>()
                        {
                            new Domain.HumanResources.Entities.PpwgAccountingStringAllocation { PpwgPrjItemId = "1", GlNumber = "1", GlPercentDistribution = 2 }
                        }
                    }
                },
                
            };

            classifications = new List<Domain.HumanResources.Entities.EmploymentClassification>()
            {
                new Domain.HumanResources.Entities.EmploymentClassification("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "desc", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                new Domain.HumanResources.Entities.EmploymentClassification("1a59eed8-5fe7-4120-b1cf-f23266b9e875", "2", "desc", Domain.HumanResources.Entities.EmploymentClassificationType.Position)
            };

            departments = new List<Domain.HumanResources.Entities.EmploymentDepartment>()
            {
                new Domain.HumanResources.Entities.EmploymentDepartment("1","1","abc123Dept")
            };

            jobChangeReasons = new List<Domain.HumanResources.Entities.JobChangeReason>()
            {
                new Domain.HumanResources.Entities.JobChangeReason("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1", "desc"),
                new Domain.HumanResources.Entities.JobChangeReason("1a59eed8-5fe7-4120-b1cf-f23266b9e875", "2", "desc")
            };

            timeUnits = new List<Domain.HumanResources.Entities.TimeUnits>()
            {
                new Domain.HumanResources.Entities.TimeUnits("1a59eed8-5fe7-4120-b1cf-f23266b9e876", "HRS", "Hours", ""),
                new Domain.HumanResources.Entities.TimeUnits("1a59eed8-5fe7-4120-b1cf-f23266b9e877", "DAY", "Days", "1"),
                new Domain.HumanResources.Entities.TimeUnits("1a59eed8-5fe7-4120-b1cf-f23266b9e877", "WKS", "Weeks", "2"),
                new Domain.HumanResources.Entities.TimeUnits("1a59eed8-5fe7-4120-b1cf-f23266b9e877", "MOS", "Months", "3"),
                new Domain.HumanResources.Entities.TimeUnits("1a59eed8-5fe7-4120-b1cf-f23266b9e877", "YRS", "Years", "4")
            };

            position = new Domain.HumanResources.Entities.Position("1", "title", "sTitle", "1", DateTime.Today, true)
            {
                PositionDept = "1",
            };

            payClasses = new List<Domain.HumanResources.Entities.PayClass>()
            {
                new Domain.HumanResources.Entities.PayClass(guid,"1","PayClassDesc")
            };

            payCycles = new List<Domain.HumanResources.Entities.PayCycle2>()
            {
                new Domain.HumanResources.Entities.PayCycle2(guid,"1","PayCycleDesc")
            };

            institutionJobs = new Dtos.InstitutionJobs3()
            {
                Id = guid,
                Person = new Dtos.GuidObject2(guid),
                Position = new Dtos.GuidObject2(guid),
                StartOn = DateTime.Today,
                Employer = new Dtos.GuidObject2(guid),
                Department = new Dtos.GuidObject2("1"),
                PayClass = new Dtos.GuidObject2(guid),
                PayCycle = new Dtos.GuidObject2(guid),
                EndOn = DateTime.Today.AddDays(100),
                Status = Dtos.EnumProperties.InstitutionJobsStatus.Active,
                JobChangeReason = new Dtos.GuidObject2(guid),
                FullTimeEquivalent = 10,
                Preference = Dtos.EnumProperties.JobPreference2.Primary,
                Classification = new Dtos.GuidObject2(guid),
                Supervisors = new List<Dtos.DtoProperties.SupervisorsDtoProperty>()
                {
                    new Dtos.DtoProperties.SupervisorsDtoProperty() { Supervisor = new Dtos.GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e874"), Type = Dtos.EnumProperties.PositionReportsToType.Primary },
                    new Dtos.DtoProperties.SupervisorsDtoProperty() { Supervisor = new Dtos.GuidObject2("1a59eed8-5fe7-4120-b1cf-f23266b9e875"), Type = Dtos.EnumProperties.PositionReportsToType.Alternative }
                },
                HoursPerPeriod = new List<Dtos.DtoProperties.HoursPerPeriodDtoProperty>()
                {
                    new Dtos.DtoProperties.HoursPerPeriodDtoProperty(){ Hours = 10, Period = Dtos.EnumProperties.PayPeriods.PayPeriod }
                },
                AccountingStringAllocations = new List<Dtos.DtoProperties.AccountingStringAllocationsDtoProperty>()
                {
                    new Dtos.DtoProperties.AccountingStringAllocationsDtoProperty()
                    {
                        AccountingString = "1", AllocatedPercentage = 2, StartOn = DateTime.Today, EndOn = DateTime.Today.AddDays(49)
                    },
                    new Dtos.DtoProperties.AccountingStringAllocationsDtoProperty()
                    {
                        AccountingString = "2", AllocatedPercentage = 4, StartOn = DateTime.Today.AddDays(50), EndOn = DateTime.Today.AddDays(50)
                    }
                },
                Salaries = new List<Dtos.DtoProperties.SalaryDtoProperty>()
                {
                    new Dtos.DtoProperties.SalaryDtoProperty()
                    {
                        StartOn = DateTime.Today, EndOn = DateTime.Today.AddDays(49), Grade = "1", Step = "1",
                        SalaryAmount = new Dtos.DtoProperties.SalaryAmountDtoProperty()
                        {
                            Period = Dtos.EnumProperties.SalaryPeriod.Year,
                            Rate = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 1000 }
                        }
                    },
                    new Dtos.DtoProperties.SalaryDtoProperty()
                    {
                        StartOn = DateTime.Today.AddDays(50), EndOn = DateTime.Today.AddDays(50), Grade = "2", Step = "2",
                        SalaryAmount = new Dtos.DtoProperties.SalaryAmountDtoProperty()
                        {
                            Period = Dtos.EnumProperties.SalaryPeriod.Hour,
                            Rate = new Dtos.DtoProperties.Amount2DtoProperty() { Currency = Dtos.EnumProperties.CurrencyIsoCode.USD, Value = 1000 }
                        }
                    }
                }
            };
        }

        private void InitializeMock(bool bypassCache = false)
        {
            createInstitutionJob.AddPermission(new Domain.Entities.Permission(HumanResourcesPermissionCodes.CreateInstitutionJob));
            roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { createInstitutionJob, viewInstitutionJob } );

            personRepositoryMock.Setup(p => p.GetHostCountryAsync()).ReturnsAsync("USA");
            personRepositoryMock.Setup(p => p.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(personGuidCollection);

            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            personRepositoryMock.Setup(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            positionRepositoryMock.Setup(p => p.GetPositionByGuidAsync(It.IsAny<string>())).ReturnsAsync(position);
            positionRepositoryMock.Setup(p => p.GetPositionGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(guid);
            hrReferenceDataRepositoryMock.Setup(h => h.GetJobChangeReasonsAsync(bypassCache)).ReturnsAsync(jobChangeReasons);
            hrReferenceDataRepositoryMock.Setup(h => h.GetEmploymentClassificationsAsync(bypassCache)).ReturnsAsync(classifications);
            institutionJobsRepositoryMock.Setup(i => i.CreateInstitutionJobsAsync(It.IsAny<Domain.HumanResources.Entities.InstitutionJobs>())).ReturnsAsync(domainInstitutionJobs);
            institutionJobsRepositoryMock.Setup(i => i.UpdateInstitutionJobsAsync(It.IsAny<Domain.HumanResources.Entities.InstitutionJobs>())).ReturnsAsync(domainInstitutionJobs);
            institutionJobsRepositoryMock.Setup(i => i.GetInstitutionJobsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            hrReferenceDataRepositoryMock.Setup(h => h.GetEmploymentDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(departments);
            hrReferenceDataRepositoryMock.Setup(h => h.GetPayClassesAsync(bypassCache)).ReturnsAsync(payClasses);
            hrReferenceDataRepositoryMock.Setup(h => h.GetPayCyclesAsync(bypassCache)).ReturnsAsync(payCycles);
            hrReferenceDataRepositoryMock.Setup(h => h.GetTimeUnitsAsync(bypassCache)).ReturnsAsync(timeUnits);
        }

        #endregion

        #region POST

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_Dto_Null()
        {
            await institutionJobService.PostInstitutionJobsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_Dto_Id_Null()
        {
            await institutionJobService.PostInstitutionJobsAsync(new Dtos.InstitutionJobs3() { Id = null });
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_Invalid_HostCountry()
        {
            personRepositoryMock.Setup(p => p.GetHostCountryAsync()).ReturnsAsync(() => null);
            await institutionJobService.PostInstitutionJobsAsync(new Dtos.InstitutionJobs3() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_DtoToEntity_Person_Null()
        {
            institutionJobs.Person = null;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_DtoToEntity_PersonId_Null()
        {
            institutionJobs.Person.Id = null;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_DtoToEntity_Invalid_StartOn()
        {
            institutionJobs.StartOn = default(DateTime);
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Empty_PersonId_From_Repository()
        {
            personRepositoryMock.Setup(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_DtoToEntity_Position_Null()
        {
            institutionJobs.Position = null;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_DtoToEntity_PositionId_Null()
        {
            institutionJobs.Position.Id = null;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Position_Null_From_Repository()
        {
            positionRepositoryMock.Setup(p => p.GetPositionByGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_DtoToEntity_Employer_Null()
        {
            institutionJobs.Employer = null;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_DtoToEntity_EmployerId_Null()
        {
            institutionJobs.Employer.Id = null;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Employer_Null_From_Repository()
        {
            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>(null));
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_DtoToEntity_Department_Null()
        {
            institutionJobs.Department = null;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Department_NotSameAs_Position_Department()
        {
            institutionJobs.Department = new Dtos.GuidObject2("2");
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Invalid_PayPeriod()
        {
            institutionJobs.HoursPerPeriod.FirstOrDefault().Period = Dtos.EnumProperties.PayPeriods.Day;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Invalid_Classification()
        {
            institutionJobs.EndOn = null;
            institutionJobs.Status = Dtos.EnumProperties.InstitutionJobsStatus.Ended;
            institutionJobs.Classification = new Dtos.GuidObject2(Guid.NewGuid().ToString());
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Invalid_Status()
        {
            institutionJobs.EndOn = null;
            institutionJobs.Status = Dtos.EnumProperties.InstitutionJobsStatus.Leave;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_SuperVisor_Null_From_Repository()
        {
            institutionJobs.HoursPerPeriod.FirstOrDefault().Period = Dtos.EnumProperties.PayPeriods.Year;

            personRepositoryMock.SetupSequence(p => p.GetPersonIdFromGuidAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1"))
                .Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>(null));

            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_More_Than_One_Primary_Supervisor()
        {
            institutionJobs.Supervisors.LastOrDefault().Type = Dtos.EnumProperties.PositionReportsToType.Primary;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_More_Than_One_Alternative_Supervisor()
        {
            institutionJobs.Supervisors.FirstOrDefault().Type = Dtos.EnumProperties.PositionReportsToType.Alternative;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Invalid_Supervisor_Type()
        {
            institutionJobs.Supervisors.FirstOrDefault().Type = Dtos.EnumProperties.PositionReportsToType.NotSet;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Duplicate_Salaries()
        {
            institutionJobs.Salaries.LastOrDefault().StartOn = DateTime.Today;

            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_SalaryStartDate_Not_Associated_With_AccountAllocationDate()
        {
            institutionJobs.Salaries.LastOrDefault().StartOn = DateTime.Today.AddDays(-10);
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Negative_SalaryAmount_Rate()
        {
            institutionJobs.Salaries.FirstOrDefault().SalaryAmount.Rate.Value = -1;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_Invalid_SalaryAmount_Period()
        {
            institutionJobs.Salaries.FirstOrDefault().SalaryAmount.Period = Dtos.EnumProperties.SalaryPeriod.Contract;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_SalaryAmount_Currency_Null()
        {
            institutionJobs.Salaries.FirstOrDefault().SalaryAmount.Rate.Currency = null;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_DtoToEntity_SalaryAmount_Invalid_Currency()
        {
            institutionJobs.Salaries.LastOrDefault().SalaryAmount.Rate.Currency = Dtos.EnumProperties.CurrencyIsoCode.GBP;
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_RepositoryException()
        {
            institutionJobsRepositoryMock.Setup(i => i.CreateInstitutionJobsAsync(It.IsAny<Domain.HumanResources.Entities.InstitutionJobs>())).ThrowsAsync(new RepositoryException());
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync_InvalidOperationException()
        {
            institutionJobsRepositoryMock.Setup(i => i.CreateInstitutionJobsAsync(It.IsAny<Domain.HumanResources.Entities.InstitutionJobs>())).ThrowsAsync(new InvalidOperationException());
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task PostInstitutionJobsAsync_EntityToDto_Repository_Returns_InstitutionJob_As_Null()
        {
            institutionJobsRepositoryMock.Setup(i => i.CreateInstitutionJobsAsync(It.IsAny<Domain.HumanResources.Entities.InstitutionJobs>())).ReturnsAsync(() => null);
            await institutionJobService.PostInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        public async Task InstitutionJobsService_PostInstitutionJobsAsync()
        {
            
            institutionJobsRepositoryMock.Setup(i => i.CreateInstitutionJobsAsync(It.IsAny<Domain.HumanResources.Entities.InstitutionJobs>())).ReturnsAsync(domainInstitutionJobs);

            var result = await institutionJobService.PostInstitutionJobsAsync(institutionJobs);

            Assert.IsNotNull(result);
        }

        #endregion

        #region PUT

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstitutionJobsService_PutInstitutionJobsAsync_Dto_Null()
        {
            await institutionJobService.PutInstitutionJobsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task InstitutionJobsService_PutInstitutionJobsAsync_Dto_Id_Null()
        {
            await institutionJobService.PutInstitutionJobsAsync(new Dtos.InstitutionJobs3() { Id = null });
        }

        [TestMethod]
        public async Task InstitutionJobsService_PutInstitutionJobsAsync_Create_With_PutRequest()
        {
            domainInstitutionJobs.IsSalary = false;
            domainInstitutionJobs.HostCountry = "CAN";

            institutionJobsRepositoryMock.Setup(i => i.GetInstitutionJobsIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
            var result = await institutionJobService.PutInstitutionJobsAsync(institutionJobs);

            Assert.IsNotNull(result);
        }

        

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task InstitutionJobsService_PutInstitutionJobsAsync_RepositoryException()
        {
            InitializeMock(true);

            institutionJobsRepositoryMock.Setup(i => i.UpdateInstitutionJobsAsync(It.IsAny<Domain.HumanResources.Entities.InstitutionJobs>())).ThrowsAsync(new RepositoryException());
            await institutionJobService.PutInstitutionJobsAsync(institutionJobs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PutInstitutionJobsAsync_Invalid_HostCountry()
        {
            domainInstitutionJobs.HostCountry = "UK";

            InitializeMock(true);

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            personRepositoryMock.SetupSequence(p => p.GetPersonGuidFromIdAsync(It.IsAny<string>())).Returns(Task.FromResult<string>("1"))
                    .Returns(Task.FromResult<string>("1")).Returns(Task.FromResult<string>("1")).Throws(new ArgumentOutOfRangeException());

            var result = await institutionJobService.PutInstitutionJobsAsync(institutionJobs);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task InstitutionJobsService_PutInstitutionJobsAsync_Empty_HostCountry()
        {
            domainInstitutionJobs.HostCountry = null;

            InitializeMock(true);

            var result = await institutionJobService.PutInstitutionJobsAsync(institutionJobs);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task InstitutionJobsService_PutInstitutionJobsAsync()
        {
            institutionJobs.AccountingStringAllocations.RemoveAt(1);

            institutionJobs.AccountingStringAllocations.FirstOrDefault().StartOn = null;

            institutionJobs.Salaries.RemoveAt(1);

            institutionJobs.Salaries.FirstOrDefault().StartOn = null;

            InitializeMock(true);

            var result = await institutionJobService.PutInstitutionJobsAsync(institutionJobs);

            Assert.IsNotNull(result);
        }

        #endregion
    }
}