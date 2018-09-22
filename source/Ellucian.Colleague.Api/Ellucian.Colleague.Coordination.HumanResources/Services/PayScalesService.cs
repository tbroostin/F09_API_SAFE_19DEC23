//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class PayScalesService : BaseCoordinationService, IPayScalesService
    {
        private readonly IHumanResourcesReferenceDataRepository _ReferenceDataRepository;
        private readonly IPayScalesRepository _PayScalesRepository;

        public PayScalesService(

            IPayScalesRepository payScalesRepository,
			IHumanResourcesReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _PayScalesRepository = payScalesRepository;
            _ReferenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all pay-scales
        /// </summary>
        /// <returns>Collection of PayScales DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PayScales>> GetPayScalesAsync(bool bypassCache = false)
        {
            CheckViewPayScalesPermission();

            var payScalesCollection = new List<Ellucian.Colleague.Dtos.PayScales>();

            var payScalesEntities = await _PayScalesRepository.GetPayScalesAsync(bypassCache);
            if (payScalesEntities != null && payScalesEntities.Any())
            {
                foreach (var payScales in payScalesEntities)
                {
                    payScalesCollection.Add(await ConvertPayScalesEntityToDto(payScales));
                }
            }
            return payScalesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PayScales from its GUID
        /// </summary>
        /// <returns>PayScales DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PayScales> GetPayScalesByGuidAsync(string guid, bool bypassCache = true)
        {
            CheckViewPayScalesPermission();

            try
            {
                return await ConvertPayScalesEntityToDto((await _PayScalesRepository.GetPayScalesByIdAsync(guid)));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("No pay scale was found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("No pay scale was found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Swver domain entity to its corresponding PayScales DTO
        /// </summary>
        /// <param name="source">Swver domain entity</param>
        /// <returns>PayScales DTO</returns>
        private async Task<Ellucian.Colleague.Dtos.PayScales> ConvertPayScalesEntityToDto(PayScale source)
        {
            var payScales = new Ellucian.Colleague.Dtos.PayScales();

            payScales.Id = source.Guid;
            payScales.Title = source.Description;
            if (source.StartDate.HasValue)
                payScales.StartOn = source.StartDate.Value;
            if (source.EndDate.HasValue)
                payScales.EndOn =  source.EndDate.Value;
            if (!string.IsNullOrEmpty(source.WageTableGuid))
            {
                payScales.Classification = new GuidObject2(source.WageTableGuid);
            }

            var hostCountry = await _PayScalesRepository.GetHostCountryAsync();
            var currencyCode = CurrencyIsoCode.USD;
            if (hostCountry == "CANADA") currencyCode = CurrencyIsoCode.CAD;

            payScales.Scales = new List<Dtos.DtoProperties.PayScalesScalesDtoProperty>();
			foreach (var scales in source.Scales)
            {
                var payScale = new Dtos.DtoProperties.PayScalesScalesDtoProperty()
                {
                    Step = scales.Step,
                    Grade = scales.Grade,
                    Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                    {
						Currency = currencyCode,
						Value = scales.Amount
                    }
                };
                payScales.Scales.Add(payScale);
            }
                                                                                                          
            return payScales;
        }

        /// <summary>
        /// Permissions code that allows an external system to do a READ operation. This API will integrate
        /// information related to pay scales that could be deemed personal.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewPayScalesPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.ViewPayScales);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view pay scales.");
            }
        }

    }
}