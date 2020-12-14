﻿/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Dtos.HumanResources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IPersonPositionService
    {
        /// <summary>
        /// Retrieves person positions
        /// </summary>
        /// <param name="effectivePersonId">person id requesting this info</param>
        /// <param name="lookupStartDate">lookup start date, all records with end date before this date will not be retrieved</param>
        /// <returns></returns>
        Task<IEnumerable<PersonPosition>> GetPersonPositionsAsync(string effectivePersonId = null, DateTime? lookupStartDate = null);
    }
}
