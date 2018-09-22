// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class TaxFormStatementServiceTests : GenericUserFactory
    {
        #region Initialize and Cleanup
        private TaxFormStatementService service = null;
        private TaxFormStatementService service2 = null;
        private Mock<ITaxFormStatementRepository> mockTaxFormStatementRepository;
        private Mock<IConfigurationRepository> mockConfigurationRepository;
        private TestTaxFormStatementRepository testStatementRepository;
        private TestConfigurationRepository testConfigurationRepository;
        private List<TaxFormStatementAvailability> statementAvailabilities;
        private ICurrentUserFactory currentUserFactory;
        private string personId = "0000001";

        [TestInitialize]
        public void Initialize()
        {
            // Initialize the mock repositories
            this.mockTaxFormStatementRepository = new Mock<ITaxFormStatementRepository>();
            this.mockConfigurationRepository = new Mock<IConfigurationRepository>();

            // Populate the seed data coming from the statement and configuration repositories
            this.statementAvailabilities = new List<TaxFormStatementAvailability>()
            {
                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2010", Domain.Base.Entities.TaxForms.FormW2, "1"),
                    Availability = new TaxFormAvailability("2010", new DateTime(2011, 01, 24))
                },
                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2011", Domain.Base.Entities.TaxForms.FormW2, "2"),
                    Availability = new TaxFormAvailability("2011", new DateTime(2012, 02, 02))
                },
                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2012", Domain.Base.Entities.TaxForms.FormW2, "3"),
                    Availability = new TaxFormAvailability("2012", new DateTime(2013, 01, 16))
                },
                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2013", Domain.Base.Entities.TaxForms.FormW2, "4"),
                    Availability = new TaxFormAvailability("2013", new DateTime(2014, 01, 05))
                },
                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2014", Domain.Base.Entities.TaxForms.FormW2, "5"),
                    Availability = new TaxFormAvailability("2014", new DateTime(2015, 01, 10))
                },

                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2010", Domain.Base.Entities.TaxForms.Form1095C, "6"),
                    Availability = new TaxFormAvailability("2010", new DateTime(2011, 01, 15))
                },
                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2011", Domain.Base.Entities.TaxForms.Form1095C, "7"),
                    Availability = new TaxFormAvailability("2011", new DateTime(2012, 01, 16))
                },
                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2012", Domain.Base.Entities.TaxForms.Form1095C, "8"),
                    Availability = new TaxFormAvailability("2012", new DateTime(2013, 01, 17))
                },
                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2013", Domain.Base.Entities.TaxForms.Form1095C, "9"),
                    Availability = new TaxFormAvailability("2013", new DateTime(2014, 01, 18))
                },
                new TaxFormStatementAvailability()
                {
                    Statement = new TaxFormStatement("000001", "2014", Domain.Base.Entities.TaxForms.Form1095C, "10"),
                    Availability = new TaxFormAvailability("2014", new DateTime(2015, 01, 19))
                },
            };

            // Build all service objects to use each of the user factories built above
            BuildTaxFormStatementService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
            service2 = null;
            testStatementRepository = null;
            testConfigurationRepository = null;
            mockTaxFormStatementRepository = null;
            mockConfigurationRepository = null;
        }
        #endregion

        #region W-2

        [TestMethod]
        public async Task GetAsync_Success_W2()
        {
            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormW2);
            var statementDtos = await service.GetAsync(personId, Dtos.Base.TaxForms.FormW2);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure each domain entity was mapped into a DTO.
            foreach (var entity in statementEntities)
            {
                var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString()
                    && x.Notation.ToString() == entity.Notation.ToString());
                Assert.AreEqual(1, selectedDtos.Count(), "Each domain entity should have been converted into a DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_2012AvailableToday_W2()
        {
            // Remove the 2012 availability entry and replace it with a 2012 entry with an availability date of "today".
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.FormW2).RemoveAvailability("2012");
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.FormW2).AddAvailability(new TaxFormAvailability("2012", DateTime.Now.Date));

            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormW2);
            var statementDtos = await service.GetAsync(personId, Dtos.Base.TaxForms.FormW2);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure each domain entity was mapped into a DTO.
            foreach (var entity in statementEntities)
            {
                var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString()
                    && x.Notation.ToString() == entity.Notation.ToString());
                Assert.AreEqual(1, selectedDtos.Count(), "Each domain entity should have been converted into a DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_2012AvailableToday_Correction_W2()
        {
            // Remove the 2012 availability entry and replace it with a 2012 entry with an availability date of "today".
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.FormW2).RemoveAvailability("2012");
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.FormW2).AddAvailability(new TaxFormAvailability("2012", DateTime.Now.Date));

            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormW2);
            var statementDtos = await service.GetAsync(personId, Dtos.Base.TaxForms.FormW2);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure each domain entity was mapped into a DTO.
            foreach (var entity in statementEntities)
            {
                if (entity.Notation == TaxFormNotations.Correction)
                {
                    var selectedDtos = statementDtos.Where(x =>
                        x.PersonId == entity.PersonId
                        && x.TaxYear == entity.TaxYear
                        && x.TaxForm.ToString() == entity.TaxForm.ToString()
                        && x.Notation.ToString() == entity.Notation.ToString());
                    Assert.AreEqual(1, selectedDtos.Count(), "Each domain entity should have been converted into a DTO.");
                    Assert.AreEqual(string.Empty, selectedDtos.FirstOrDefault().PdfRecordId);
                }
            }
        }

        [TestMethod]
        public async Task GetAsync_2011AvailableDateIsTomorrow_W2()
        {
            // Remove the 2011 availability entry and replace it with a 2011 entry with an availability date of "tomorrow".
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.FormW2).RemoveAvailability("2011");
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.FormW2).AddAvailability(new TaxFormAvailability("2011", DateTime.Now.AddDays(1)));

            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormW2);
            var statementDtos = await service.GetAsync(personId, Dtos.Base.TaxForms.FormW2);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure each domain entity was mapped into a DTO.
            foreach (var entity in statementEntities)
            {
                var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString());
                Assert.AreEqual(1, selectedDtos.Count(), "Each domain entity should have been converted into a DTO.");

                // For the 2011 year make sure the DTO has a "NotAvailable" notation.
                if (entity.TaxYear == "2011")
                {
                    Assert.AreEqual(Dtos.HumanResources.TaxFormNotations.NotAvailable, selectedDtos.FirstOrDefault().Notation);
                    Assert.AreEqual(string.Empty, selectedDtos.FirstOrDefault().PdfRecordId);
                }
            }
        }

        [TestMethod]
        public async Task GetAsync_2014AvailabilityNotSpecified_W2()
        {
            // Remove the 2014 availability entry.
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.FormW2).RemoveAvailability("2014");

            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormW2);
            var statementDtos = await service.GetAsync(personId, Dtos.Base.TaxForms.FormW2);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure each domain entity was mapped into a DTO.
            foreach (var entity in statementEntities)
            {
                var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString());
                Assert.AreEqual(1, selectedDtos.Count(), "Each domain entity should have been converted into a DTO.");

                // For the 2011 year make sure the DTO has a "NotAvailable" notation.
                if (entity.TaxYear == "2014")
                {
                    Assert.AreEqual(Dtos.HumanResources.TaxFormNotations.NotAvailable, selectedDtos.FirstOrDefault().Notation);
                }
            }
        }

        [TestMethod]
        public async Task GetAsync_StatementRepositorySetWithOneNullObject_W2()
        {
            // Mock the repository method to return a null object within the Service method and set one of the statements to null.
            testStatementRepository.Statements[1] = null;
            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.FormW2);
            // testStatementRepository contains the statements for all forms. We need the W2s only
            var statementDomainEntities = testStatementRepository.Statements.Where(x => x != null && x.TaxForm == Domain.Base.Entities.TaxForms.FormW2);

            this.mockTaxFormStatementRepository.Setup<Task<IEnumerable<Domain.HumanResources.Entities.TaxFormStatement>>>(
                x => x.GetAsync(It.IsAny<string>(), Domain.Base.Entities.TaxForms.FormW2))
                .Returns(Task.FromResult(statementEntities));
            var statementDtos = await service.GetAsync("000001", Dtos.Base.TaxForms.FormW2);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementDomainEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure the data within the remaining DTOs match the domain entites.
            foreach (var entity in statementEntities)
            {
                if (entity != null)
                {
                    var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString()
                    && x.Notation.ToString() == entity.Notation.ToString());
                    Assert.AreEqual(1, selectedDtos.Count(), "Each remaining domain entity should have been converted into a DTO.");
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_NullPersonId()
        {
            await service.GetAsync(null, Dtos.Base.TaxForms.Form1095C);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_EmptyPersonId()
        {
            await service.GetAsync(string.Empty, Dtos.Base.TaxForms.FormW2);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAsync_StatementRepositoryReturnsNullObject_W2()
        {
            // Mock the repository method to return a null object within the Service method
            // Set the domain entity for the statements to null
            IEnumerable<Domain.HumanResources.Entities.TaxFormStatement> statements = null;

            this.mockTaxFormStatementRepository.Setup<Task<IEnumerable<Domain.HumanResources.Entities.TaxFormStatement>>>(
                x => x.GetAsync(It.IsAny<string>(), Domain.Base.Entities.TaxForms.FormW2))
                .Returns(Task.FromResult(statements));
            await service2.GetAsync("000001", Dtos.Base.TaxForms.FormW2);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAsync_ConfigurationRepositoryReturnsNull_W2()
        {
            // Mock the repository method to return a null object within the Service method
            var configuration = await this.testConfigurationRepository.GetTaxFormAvailabilityConfigurationAsync(Colleague.Domain.Base.Entities.TaxForms.FormW2);
            configuration = null;

            this.mockConfigurationRepository.Setup<Task<Domain.Base.Entities.TaxFormConfiguration>>(
                x => x.GetTaxFormAvailabilityConfigurationAsync(Colleague.Domain.Base.Entities.TaxForms.FormW2))
                .Returns(Task.FromResult(configuration));
            await service2.GetAsync("000001", Dtos.Base.TaxForms.FormW2);
        }

        #endregion

        #region 1095-C

        [TestMethod]
        public async Task GetAsync_Success_1095C()
        {
            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.Form1095C);
            var statementDtos = await service.GetAsync(personId, Dtos.Base.TaxForms.Form1095C);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementEntities.Count(), statementDtos.Count(), "We should have built the correct number of 1095-C DTOs.");

            // Make sure each domain entity was mapped into a DTO.
            foreach (var entity in statementEntities)
            {
                var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString()
                    && x.Notation.ToString() == entity.Notation.ToString());
                Assert.AreEqual(1, selectedDtos.Count(), "Each  1095-C domain entity should have been converted into a DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_2012AvailableToday_1095C()
        {
            // Remove the 2012 availability entry and replace it with a 2012 entry with an availability date of "today".
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.Form1095C).RemoveAvailability("2012");
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.Form1095C).AddAvailability(new TaxFormAvailability("2012", DateTime.Now.Date));

            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.Form1095C);
            var statementDtos = await service.GetAsync(personId, Dtos.Base.TaxForms.Form1095C);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure each domain entity was mapped into a DTO.
            foreach (var entity in statementEntities)
            {
                var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString()
                    && x.Notation.ToString() == entity.Notation.ToString());
                Assert.AreEqual(1, selectedDtos.Count(), "Each domain entity should have been converted into a DTO.");
            }
        }

        [TestMethod]
        public async Task GetAsync_2011AvailableDateIsTomorrow_1095C()
        {
            // Remove the 2011 availability entry and replace it with a 2011 entry with an availability date of "tomorrow".
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.Form1095C).RemoveAvailability("2011");
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.Form1095C).AddAvailability(new TaxFormAvailability("2011", DateTime.Now.AddDays(1)));

            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.Form1095C);
            var statementDtos = await service.GetAsync(personId, Dtos.Base.TaxForms.Form1095C);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure each domain entity was mapped into a DTO.
            foreach (var entity in statementEntities)
            {
                var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString());
                Assert.AreEqual(1, selectedDtos.Count(), "Each domain entity should have been converted into a DTO.");

                // For the 2011 year make sure the DTO has a "NotAvailable" notation.
                if (entity.TaxYear == "2011")
                {
                    Assert.AreEqual(Dtos.HumanResources.TaxFormNotations.NotAvailable, selectedDtos.FirstOrDefault().Notation);
                    Assert.AreEqual(string.Empty, selectedDtos.FirstOrDefault().PdfRecordId);
                }
            }
        }

        [TestMethod]
        public async Task GetAsync_2014AvailabilityNotSpecified_1095C()
        {
            // Remove the 2014 availability entry.
            testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.Form1095C).RemoveAvailability("2014");

            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.Form1095C);
            var statementDtos = await service.GetAsync(personId, Dtos.Base.TaxForms.Form1095C);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure each domain entity was mapped into a DTO.
            foreach (var entity in statementEntities)
            {
                var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString());
                Assert.AreEqual(1, selectedDtos.Count(), "Each domain entity should have been converted into a DTO.");

                // For the 2011 year make sure the DTO has a "NotAvailable" notation.
                if (entity.TaxYear == "2014")
                {
                    Assert.AreEqual(Dtos.HumanResources.TaxFormNotations.NotAvailable, selectedDtos.FirstOrDefault().Notation);
                }
            }
        }

        [TestMethod]
        public async Task GetAsync_StatementRepositorySetWithOneNullObject_1095C()
        {
            // Mock the repository method to return a null object within the Service method and set one of the statements to null.
            testStatementRepository.Statements[1] = null;
            var statementEntities = await testStatementRepository.GetAsync(personId, Domain.Base.Entities.TaxForms.Form1095C);
            // testStatementRepository contains the statements for all forms. We need the 1095Cs only
            var statementDomainEntities = testStatementRepository.Statements.Where(x => x != null && x.TaxForm == Domain.Base.Entities.TaxForms.Form1095C);

            this.mockTaxFormStatementRepository.Setup<Task<IEnumerable<Domain.HumanResources.Entities.TaxFormStatement>>>(
                x => x.GetAsync(It.IsAny<string>(), Domain.Base.Entities.TaxForms.Form1095C))
                .Returns(Task.FromResult(statementEntities));
            var statementDtos = await service.GetAsync("000001", Dtos.Base.TaxForms.Form1095C);

            // Make sure we have built the correct number of DTOs.
            Assert.AreEqual(statementDomainEntities.Count(), statementDtos.Count(), "We should have built the correct number of DTOs.");

            // Make sure the data within the remaining DTOs match the domain entites.
            foreach (var entity in statementEntities)
            {
                if (entity != null)
                {
                    var selectedDtos = statementDtos.Where(x =>
                    x.PersonId == entity.PersonId
                    && x.TaxYear == entity.TaxYear
                    && x.TaxForm.ToString() == entity.TaxForm.ToString()
                    && x.Notation.ToString() == entity.Notation.ToString());
                    Assert.AreEqual(1, selectedDtos.Count(), "Each remaining domain entity should have been converted into a DTO.");
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAsync_StatementRepositoryReturnsNullObject_1095C()
        {
            // Mock the repository method to return a null object within the Service method
            // Set the domain entity for the statements to null
            IEnumerable<Domain.HumanResources.Entities.TaxFormStatement> statements = null;

            this.mockTaxFormStatementRepository.Setup<Task<IEnumerable<Domain.HumanResources.Entities.TaxFormStatement>>>(
                x => x.GetAsync(It.IsAny<string>(), Domain.Base.Entities.TaxForms.Form1095C))
                .Returns(Task.FromResult(statements));
            await service2.GetAsync("000001", Dtos.Base.TaxForms.Form1095C);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public async Task GetAsync_ConfigurationRepositoryReturnsNull_1095C()
        {
            // Mock the repository method to return a null object within the Service method
            var configuration = await this.testConfigurationRepository.GetTaxFormAvailabilityConfigurationAsync(Domain.Base.Entities.TaxForms.FormW2);
            configuration = null;

            this.mockConfigurationRepository.Setup<Task<Domain.Base.Entities.TaxFormConfiguration>>(
                x => x.GetTaxFormAvailabilityConfigurationAsync(Domain.Base.Entities.TaxForms.FormW2))
                .Returns(Task.FromResult(configuration));
            await service2.GetAsync("000001", Dtos.Base.TaxForms.Form1095C);
        }

        #endregion

        #region Private methods and helper classes

        private void BuildTaxFormStatementService()
        {
            // Set up current user
            currentUserFactory = new GenericUserFactory.UserFactory();

            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces.
            var roleRepository = new Mock<IRoleRepository>().Object;
            var loggerObject = new Mock<ILogger>().Object;

            testStatementRepository = new TestTaxFormStatementRepository();
            testConfigurationRepository = new TestConfigurationRepository();
            foreach (var statementAvailability in this.statementAvailabilities)
            {
                if (statementAvailability.Statement.TaxYear == "2012" && statementAvailability.Statement.TaxForm == TaxForms.FormW2)
                    statementAvailability.Statement.Notation = TaxFormNotations.Correction;
                testStatementRepository.Statements.Add(statementAvailability.Statement);

                if (statementAvailability.Statement.TaxForm == TaxForms.FormW2)
                {
                    testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.FormW2)
                        .AddAvailability(statementAvailability.Availability);
                }

                if (statementAvailability.Statement.TaxForm == TaxForms.Form1095C)
                {
                    testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.Form1095C)
                        .AddAvailability(statementAvailability.Availability);
                }

                if (statementAvailability.Statement.TaxForm == TaxForms.Form1098)
                {
                    testConfigurationRepository.TaxFormConfigurations.FirstOrDefault(x => x.TaxFormId == TaxForms.Form1098)
                        .AddAvailability(statementAvailability.Availability);
                }
            }

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();
            var taxFormStatementDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.TaxFormStatement, Dtos.HumanResources.TaxFormStatement>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.HumanResources.Entities.TaxFormStatement, Dtos.HumanResources.TaxFormStatement>()).Returns(taxFormStatementDtoAdapter);

            // Set up the current user with a subset of tax form statements and set up the service.
            service = new TaxFormStatementService(testStatementRepository, testConfigurationRepository, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);
            service2 = new TaxFormStatementService(this.mockTaxFormStatementRepository.Object, this.mockConfigurationRepository.Object, adapterRegistry.Object, currentUserFactory, roleRepository, loggerObject);
        }

        public class TaxFormStatementAvailability
        {
            public TaxFormStatement Statement { get; set; }

            public TaxFormAvailability Availability { get; set; }
        }
        #endregion
    }
}
