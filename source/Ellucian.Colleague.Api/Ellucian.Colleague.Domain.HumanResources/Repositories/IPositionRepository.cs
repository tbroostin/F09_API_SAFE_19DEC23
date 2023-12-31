﻿/* Copyright 2016-2021 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Domain.HumanResources.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Repositories
{
    public interface IPositionRepository
    {
        Task<IEnumerable<Position>> GetPositionsAsync();

        Task<IEnumerable<Position>> GetPositionsErrorCollectionAsync();


        Task<Position> GetPositionByGuidAsync(string guid);

        Task<Position> GetPositionByIdAsync(string id);

        Task<Tuple<IEnumerable<Position>, int>> GetPositionsAsync(int offset, int limit, string code = "", string campus = "", string status = "", string bargainingUnit = "",
            List<string> reportsToPositions = null, string exemptionType = "", string compensationType = "", string startOn = "", string endOn = "", bool bypassCache = false);
        Task<string> GetPositionGuidFromIdAsync(string positionId);
        Task<string> GetPositionIdFromGuidAsync(string guid);

        Task<PositionPay> GetPositionPayByIdAsync(string id);
        Task<IEnumerable<PositionPay>> GetPositionPayByIdsAsync(IEnumerable<string> ids);
    }
}
