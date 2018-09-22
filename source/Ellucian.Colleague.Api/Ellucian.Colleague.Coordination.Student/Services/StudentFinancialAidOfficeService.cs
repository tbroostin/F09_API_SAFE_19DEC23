//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class StudentFinancialAidOfficeService : BaseCoordinationService, IStudentFinancialAidOfficeService
    {
        private IStudentFinancialAidOfficeRepository _studentfinancialAidOfficeRepository;
        private readonly IConfigurationRepository configurationRepository;

        public StudentFinancialAidOfficeService(IAdapterRegistry adapterRegistry,
            IStudentFinancialAidOfficeRepository studentfinancialAidOfficeRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this._studentfinancialAidOfficeRepository = studentfinancialAidOfficeRepository;
            this.configurationRepository = configurationRepository;
        }       

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all financial aid offices
        /// </summary>
        /// <returns>Collection of FinancialAidOffice DTO objects</returns>
        public async Task<IEnumerable<Dtos.FinancialAidOffice>> GetFinancialAidOfficesAsync(bool bypassCache = false)
        {
            var financialAidOfficeCollection = new List<Ellucian.Colleague.Dtos.FinancialAidOffice>();

            var financialAidOfficeEntities = await _studentfinancialAidOfficeRepository.GetFinancialAidOfficesAsync(bypassCache);
            if (financialAidOfficeEntities != null && financialAidOfficeEntities.Any())
            {
                foreach (var financialAidOffice in financialAidOfficeEntities)
                {
                    financialAidOfficeCollection.Add(ConvertFinancialAidOfficeEntityToDto(financialAidOffice));
                }
            }
            return financialAidOfficeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get an financial aid office from its GUID
        /// </summary>
        /// <returns>FinancialAidOffice DTO object</returns>
        public async Task<Dtos.FinancialAidOffice> GetFinancialAidOfficeByGuidAsync(string guid, bool bypassCache = false)
        {
            try
            {
                var faOfficeEntities = await _studentfinancialAidOfficeRepository.GetFinancialAidOfficesAsync(bypassCache);
                var faOfficeEntity = faOfficeEntities.First(fa => fa.Guid.ToString().Equals(guid));
                return ConvertFinancialAidOfficeEntityToDto(faOfficeEntity);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is InvalidOperationException)
                {
                    throw new KeyNotFoundException("Financial aid office not found for GUID " + guid, ex);
                }
                throw;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts an Financial Aid Office domain entity to its corresponding FinancialAidOffice DTO
        /// </summary>
        /// <param name="source">FinancialAidOffice domain entity</param>
        /// <returns>FinancialAidOffice DTO</returns>
        private Dtos.FinancialAidOffice ConvertFinancialAidOfficeEntityToDto(Domain.Student.Entities.FinancialAidOfficeItem source)
        {
            var financialAidOffice = new Dtos.FinancialAidOffice();

            financialAidOffice.Id = source.Guid;
            financialAidOffice.Code = source.Code;
            financialAidOffice.Description = null;
            financialAidOffice.AidAdministrator = source.AidAdministrator;
            financialAidOffice.AddressLines = source.AddressLines;
            financialAidOffice.City = source.City;
            financialAidOffice.State = source.State;
            financialAidOffice.PostalCode = source.PostalCode;
            financialAidOffice.PhoneNumber = new Dtos.DtoProperties.NumberDtoProperty() { Number = source.PhoneNumber };
            financialAidOffice.FaxNumber = new Dtos.DtoProperties.NumberDtoProperty() { Number = source.FaxNumber };
            financialAidOffice.EmailAddress = source.EmailAddress;
            financialAidOffice.Name = source.Name;

            return financialAidOffice;
        }
    }
}
