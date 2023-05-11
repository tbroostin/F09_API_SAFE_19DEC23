// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Configuration;
using Ellucian.Data.Colleague.Exceptions;
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
            try
            {
                var configurationEntity = _configurationRepository.GetFinanceConfiguration();

                var adapter = _adapterRegistry.GetAdapter<Domain.Finance.Entities.Configuration.FinanceConfiguration, FinanceConfiguration>();

                var configurationDto = adapter.MapToType(configurationEntity);

                return configurationDto;
            }
            catch (ColleagueSessionExpiredException tex)
            {
                string message = "Timeout exception has occurred while retrieving student finance configuration.";                
                throw;
            }
            catch (Exception ex)
            {
                string message = "Unable to get student finance configuration.";
                throw;
            }
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
