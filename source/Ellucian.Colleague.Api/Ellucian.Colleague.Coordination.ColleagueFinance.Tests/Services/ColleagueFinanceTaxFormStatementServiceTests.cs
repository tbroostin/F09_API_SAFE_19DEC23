// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class ColleagueFinanceTaxFormStatementServiceTests
    {
        #region Initialize and Cleanup
        private ColleagueFinanceTaxFormStatementService service;

        [TestInitialize]
        public void Initialize()
        {
            BuildService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            service = null;
        }
        #endregion

        #region GetAsync
        [TestMethod]
        public async Task GetAsync_NullPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await service.GetAsync(null, Dtos.Base.TaxForms.FormT4A);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_EmptyPersonId()
        {
            var expectedParam = "personid";
            var actualParam = "";
            try
            {
                await service.GetAsync("", Dtos.Base.TaxForms.FormT4A);
            }
            catch (ArgumentNullException anex)
            {
                actualParam = anex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_InvalidTaxFormId()
        {
            var expectedParam = "taxform";
            var actualParam = "";
            try
            {
                await service.GetAsync("1", Dtos.Base.TaxForms.FormW2);
            }
            catch (ArgumentException aex)
            {
                actualParam = aex.ParamName.ToLower();
            }

            Assert.AreEqual(expectedParam, actualParam);
        }

        [TestMethod]
        public async Task GetAsync_Success()
        {
            var actualTaxFormStatements = await service.GetAsync("1", Dtos.Base.TaxForms.FormT4A);

            foreach (var expectedStatement in expectedTaxFormStatements)
            {
                var actualStatements = actualTaxFormStatements.Where(x =>
                    x.Notation == Dtos.Base.TaxFormNotations2.None
                    && x.PdfRecordId == expectedStatement.PdfRecordId
                    && x.PersonId == expectedStatement.PersonId
                    && x.TaxForm == Dtos.Base.TaxForms.FormT4A
                    && x.TaxYear == expectedStatement.TaxYear).ToList();
                Assert.AreEqual(1, actualStatements.Count());
            }
        }
        #endregion

        #region Build service method

        /// <summary>
        /// Builds multiple cost center service objects.
        /// </summary>
        private void BuildService()
        {
            var statementRepositoryMock = new Mock<IColleagueFinanceTaxFormStatementRepository>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            var userFactory = new GeneralLedgerCurrentUser.TaxInformationUserFactory();
            var roleRepositoryMock = new Mock<IRoleRepository>();
            var loggerMock = new Mock<ILogger>();

            var roles = new List<Domain.Entities.Role>();

            var role = new Domain.Entities.Role(1, "VIEW.T4A");
            role.AddPermission(new Domain.Entities.Permission("VIEW.T4A"));
            roles.Add(role);

            roleRepositoryMock.Setup(r => r.Roles).Returns(roles);

            // Set up and mock the adapter, and setup the GetAdapter method.
            var taxFormDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.TaxFormStatement2, Dtos.Base.TaxFormStatement2>()).Returns(taxFormDtoAdapter);

            statementRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Domain.Base.Entities.TaxForms>())).Returns(() =>
                {
                    return Task.FromResult(getStatements());
                });

            service = new ColleagueFinanceTaxFormStatementService(statementRepositoryMock.Object, adapterRegistryMock.Object, userFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        private List<Domain.Base.Entities.TaxFormStatement2> expectedTaxFormStatements = new List<Domain.Base.Entities.TaxFormStatement2>()
            {
                new TaxFormStatement2("1", "2016", TaxForms.FormT4A, "1234"),
                new TaxFormStatement2("1", "2015", TaxForms.FormT4A, "3452")
            };
        private IEnumerable<Domain.Base.Entities.TaxFormStatement2> getStatements()
        {
            return expectedTaxFormStatements;
        }

        #endregion
    }
}
