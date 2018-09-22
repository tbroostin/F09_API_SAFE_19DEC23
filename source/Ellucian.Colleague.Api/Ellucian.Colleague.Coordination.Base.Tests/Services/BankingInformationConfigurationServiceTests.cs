// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
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
    [TestClass]
    public class BankingInformationConfigurationServiceTests
    {
        private Mock<IBankingInformationConfigurationRepository> repoMock;
        private AdapterRegistry adapterRegistry;
        private Mock<ILogger> loggerMock;
        private ILogger loggerObject;
        private Mock<ICurrentUserFactory> currentUserFactoryMock;
        private Mock<IRoleRepository> roleRepoMock;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            loggerObject = loggerMock.Object;
            currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            roleRepoMock = new Mock<IRoleRepository>();

            var adapters = new HashSet<ITypeAdapter>();
            var bankInfoAdapter = new AutoMapperAdapter<Domain.Base.Entities.BankingInformationConfiguration, Dtos.Base.BankingInformationConfiguration>(adapterRegistry, loggerObject);
            adapterRegistry = new AdapterRegistry(adapters, loggerObject);
            adapterRegistry.AddAdapter(bankInfoAdapter);
            repoMock = new Mock<IBankingInformationConfigurationRepository>();

        }

        [TestMethod]
        public async Task GetBankingInformationConfiguration_ReturnsDto()
        {
            var bankInfoConfigEntity = new Domain.Base.Entities.BankingInformationConfiguration()
            {
                AddEditAccountTermsAndConditions = "Terms and Conditions",
                IsDirectDepositEnabled = true,
                IsAccountAuthenticationDisabled = true,
                IsPayableDepositEnabled = true,
                IsRemainderAccountRequired = true,
                PayrollEffectiveDateMessage = "Payroll Effective Date Message",
                PayrollMessage = "Payroll Message",
                UseFederalRoutingDirectory = true
            };
            repoMock.Setup(repo => repo.GetBankingInformationConfigurationAsync())
                    .ReturnsAsync(bankInfoConfigEntity);
            var service = new BankingInformationConfigurationService(adapterRegistry, currentUserFactoryMock.Object, roleRepoMock.Object, loggerObject, repoMock.Object);
            var bankInfoConfigDto = await service.GetBankingInformationConfigurationAsync();

            Assert.AreEqual(bankInfoConfigEntity.AddEditAccountTermsAndConditions, bankInfoConfigDto.AddEditAccountTermsAndConditions);
            Assert.AreEqual(bankInfoConfigEntity.IsAccountAuthenticationDisabled, bankInfoConfigDto.IsAccountAuthenticationDisabled);
            Assert.AreEqual(bankInfoConfigEntity.IsDirectDepositEnabled, bankInfoConfigDto.IsDirectDepositEnabled);
            Assert.AreEqual(bankInfoConfigEntity.IsPayableDepositEnabled, bankInfoConfigDto.IsPayableDepositEnabled);
            Assert.AreEqual(bankInfoConfigEntity.IsRemainderAccountRequired, bankInfoConfigDto.IsRemainderAccountRequired);
            Assert.AreEqual(bankInfoConfigEntity.PayrollEffectiveDateMessage, bankInfoConfigDto.PayrollEffectiveDateMessage);
            Assert.AreEqual(bankInfoConfigEntity.PayrollMessage, bankInfoConfigDto.PayrollMessage);
            Assert.AreEqual(bankInfoConfigEntity.UseFederalRoutingDirectory, bankInfoConfigDto.UseFederalRoutingDirectory);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetBankingInformationConfiguration_PassThroughRepoException()
        {
            repoMock.Setup(repo => repo.GetBankingInformationConfigurationAsync())
                    .ThrowsAsync(new KeyNotFoundException());
            var service = new BankingInformationConfigurationService(adapterRegistry, currentUserFactoryMock.Object, roleRepoMock.Object, loggerObject, repoMock.Object);
            var bankInfoConfigDto = await service.GetBankingInformationConfigurationAsync();
        }
    }
}
