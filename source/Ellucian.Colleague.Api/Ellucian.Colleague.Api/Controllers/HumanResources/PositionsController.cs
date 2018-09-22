﻿/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.HumanResources
{
    /// <summary>
    /// Expose Human Resources Employment Positions data
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.HumanResources)]
    public class PositionsController : BaseCompressedApiController
    {
        private readonly ILogger logger;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly IPositionRepository positionRepository;

        /// <summary>
        /// PositionsController constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="adapterRegistry"></param>
        /// <param name="positionRepository"></param>
        public PositionsController(ILogger logger, IAdapterRegistry adapterRegistry, IPositionRepository positionRepository)
        {
            this.logger = logger;
            this.adapterRegistry = adapterRegistry;
            this.positionRepository = positionRepository;
        }

        /// <summary>
        /// Gets a list of employee positions for an institution
        /// The list is unfiltered and will return all active and inactive positions
        /// </summary>
        /// <returns>A List of Position objects</returns>
        public async Task<IEnumerable<Position>> GetPositionsAsync()
        {
            try
            {
                var positionEntities = await positionRepository.GetPositionsAsync();
                var entityToDtoAdapter = adapterRegistry.GetAdapter<Domain.HumanResources.Entities.Position, Position>();
                return positionEntities.Select(pos => entityToDtoAdapter.MapToType(pos));
            }
            catch (Exception e)
            {
                logger.Error(e, "Unknown error occurred");
                throw CreateHttpResponseException(e.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}