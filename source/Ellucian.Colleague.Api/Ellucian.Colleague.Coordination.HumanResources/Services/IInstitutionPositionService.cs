/* Copyright 2016 Ellucian Company L.P. and its affiliates. */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IInstitutionPositionService : IBaseService
    {
        //Task<IEnumerable<InstitutionPosition>> GetInstitutionPositionsAsync();
        Task<Dtos.InstitutionPosition> GetInstitutionPositionByGuidAsync(string guid, bool ignoreCache);

        Task<Dtos.InstitutionPosition> GetInstitutionPositionByGuid2Async(string guid, bool ignoreCache);

        Task<Dtos.InstitutionPosition2> GetInstitutionPositionByGuid3Async(string guid, bool ignoreCache);

        Task<Tuple<IEnumerable<Dtos.InstitutionPosition>, int>> GetInstitutionPositionsAsync(int offset, int limit, string campus = "", string status = "", string bargainingUnit = "",
            string reportsToPosition = "", string exemptionType = "", string compensationType = "", string startOn = "", string endOn = "", bool bypassCache = false);

        Task<Tuple<IEnumerable<Dtos.InstitutionPosition>, int>> GetInstitutionPositions2Async(int offset, int limit, string campus = "", string status = "", string bargainingUnit = "",
            List<string> reportsToPositions = null, string exemptionType = "", string compensationType = "", string startOn = "", string endOn = "", bool bypassCache = false);

        Task<Tuple<IEnumerable<Dtos.InstitutionPosition2>, int>> GetInstitutionPositions3Async(int offset, int limit, string campus = "", string status = "", string bargainingUnit = "",
            List<string> reportsToPositions = null, string exemptionType = "", string compensationType = "", string startOn = "", string endOn = "", bool bypassCache = false);

    }
}
