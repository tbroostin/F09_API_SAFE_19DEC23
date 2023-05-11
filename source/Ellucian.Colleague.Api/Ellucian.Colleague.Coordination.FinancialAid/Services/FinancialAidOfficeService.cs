//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    [RegisterType]
    public class FinancialAidOfficeService : FinancialAidCoordinationService, IFinancialAidOfficeService
    {
        private IFinancialAidOfficeRepository financialAidOfficeRepository;
        private readonly IConfigurationRepository configurationRepository;

        public FinancialAidOfficeService(IAdapterRegistry adapterRegistry,
            IFinancialAidOfficeRepository financialAidOfficeRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.financialAidOfficeRepository = financialAidOfficeRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Get a list of Financial Aid Offices
        /// </summary>
        /// <returns>A List of FinancialAidOffice3 objects</returns>
        public async Task<IEnumerable<Dtos.FinancialAid.FinancialAidOffice3>> GetFinancialAidOffices3Async()
        {
            var officeDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice3>();
            try
            {
                var officeEntityList = await financialAidOfficeRepository.GetFinancialAidOfficesAsync();


                if (officeEntityList == null)
                {
                    var message = "Null FinancialAidOffice object returned by repository";
                    logger.Error(message);
                    throw new KeyNotFoundException(message);
                }

                var officeDtoList = new List<Dtos.FinancialAid.FinancialAidOffice3>();
                foreach (var officeEntity in officeEntityList)
                {
                    officeDtoList.Add(officeDtoAdapter.MapToType(officeEntity));
                }

                return officeDtoList;
            }
            catch(ColleagueSessionExpiredException csee)
            {
                throw;
            }
        }

        /// <summary>
        /// Get a list of Financial Aid Offices
        /// </summary>
        /// <returns>A List of FinancialAidOffice2 objects</returns>
        [Obsolete("Obsolete as of Api version 1.15, use version 3 of this method")]
        public IEnumerable<Dtos.FinancialAid.FinancialAidOffice2> GetFinancialAidOffices2()
        {
            var officeDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice2>();

            var officeEntityList = Task.Run(async() => await financialAidOfficeRepository.GetFinancialAidOfficesAsync()).GetAwaiter().GetResult();

            if (officeEntityList == null)
            {
                var message = "Null FinancialAidOffice object returned by repository";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var officeDtoList = new List<Dtos.FinancialAid.FinancialAidOffice2>();
            foreach (var officeEntity in officeEntityList)
            {
                officeDtoList.Add(officeDtoAdapter.MapToType(officeEntity));
            }

            return officeDtoList;
        }

        /// <summary>
        /// Get a list of Financial Aid Offices
        /// </summary>
        /// <returns>A List of FinancialAidOffice2 objects</returns>
        [Obsolete("Obsolete as of Api version 1.15, use version 3 of this method")]
        public async Task<IEnumerable<Dtos.FinancialAid.FinancialAidOffice2>> GetFinancialAidOffices2Async()
        {
            var officeDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice2>();

            var officeEntityList = await financialAidOfficeRepository.GetFinancialAidOfficesAsync();

            if (officeEntityList == null)
            {
                var message = "Null FinancialAidOffice object returned by repository";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var officeDtoList = new List<Dtos.FinancialAid.FinancialAidOffice2>();
            foreach (var officeEntity in officeEntityList)
            {
                officeDtoList.Add(officeDtoAdapter.MapToType(officeEntity));
            }

            return officeDtoList;
        }


        /// <summary>
        /// Get a list of Financial Aid Offices
        /// </summary>
        /// <returns>A List of FinancialAidOffice objects</returns>
        [Obsolete("Obsolete as of Api version 1.14, use version 2 of this API")]
        public IEnumerable<Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice> GetFinancialAidOffices()
        {
            var officeDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Dtos.FinancialAid.FinancialAidOffice>();

            var officeEntityList = Task.Run(async() => await financialAidOfficeRepository.GetFinancialAidOfficesAsync()).GetAwaiter().GetResult();

            if (officeEntityList == null)
            {
                var message = "Null FinancialAidOffice object returned by repository";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var officeDtoList = new List<Dtos.FinancialAid.FinancialAidOffice>();
            foreach (var officeEntity in officeEntityList)
            {
                officeDtoList.Add(officeDtoAdapter.MapToType(officeEntity));
            }

            return officeDtoList;
        }

        /// <summary>
        /// Get a list of Financial Aid Offices
        /// </summary>
        /// <returns>A List of FinancialAidOffice objects</returns>
        [Obsolete("Obsolete as of Api version 1.14, use version 2 of this API")]
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice>> GetFinancialAidOfficesAsync()
        {
            var officeDtoAdapter = _adapterRegistry.GetAdapter<Domain.FinancialAid.Entities.FinancialAidOffice, Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice>();

            var officeEntityList = await financialAidOfficeRepository.GetFinancialAidOfficesAsync();

            if (officeEntityList == null)
            {
                var message = "Null FinancialAidOffice object returned by repository";
                logger.Error(message);
                throw new KeyNotFoundException(message);
            }

            var officeDtoList = new List<Ellucian.Colleague.Dtos.FinancialAid.FinancialAidOffice>();
            foreach (var officeEntity in officeEntityList)
            {
                officeDtoList.Add(officeDtoAdapter.MapToType(officeEntity));
            }

            return officeDtoList;
        }
    }
}
