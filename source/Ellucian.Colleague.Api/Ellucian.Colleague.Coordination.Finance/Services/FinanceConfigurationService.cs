// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Configuration;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;

namespace Ellucian.Colleague.Coordination.Finance.Services
{
    [RegisterType]
    public class FinanceConfigurationService : IFinanceConfigurationService
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IFinanceConfigurationRepository _configurationRepository;

        public FinanceConfigurationService(IAdapterRegistry adapterRegistry, IFinanceConfigurationRepository configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _adapterRegistry = adapterRegistry;
        }

        /// <summary>
        /// Get the configuration information for Student Finance
        /// </summary>
        /// <returns>Student finance configuration</returns>
        public FinanceConfiguration GetFinanceConfiguration()
        {
            var configurationEntity = _configurationRepository.GetFinanceConfiguration();

            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.Configuration.FinanceConfiguration, FinanceConfiguration>();

            var configurationDto = adapter.MapToType(configurationEntity);

            return configurationDto;
        }

        /// <summary>
        /// Get the control information for Immediate Payment Control
        /// </summary>
        /// <returns>IPC Configuration</returns>
        public ImmediatePaymentControl GetImmediatePaymentControl()
        {
            var ipcEntity = _configurationRepository.GetImmediatePaymentControl();
            var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.ImmediatePaymentControl, ImmediatePaymentControl>();
            var ipcDto = adapter.MapToType(ipcEntity);

            return ipcDto;
        }
    }
}
