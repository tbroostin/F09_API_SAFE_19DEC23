/* Copyright 2016 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class InstitutionPositionService : BaseCoordinationService, IInstitutionPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;


        public InstitutionPositionService(
            IPositionRepository positionRepository,
            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this._positionRepository = positionRepository;
            this._hrReferenceDataRepository = hrReferenceDataRepository;
            this._referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;
        }

        private IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> _employmentClassification = null;

        private async Task<IEnumerable<Domain.HumanResources.Entities.EmploymentClassification>> GetAllEmploymentClassificationAsync(bool bypassCache)
        {
            if (_employmentClassification == null)
            {
                _employmentClassification = await _hrReferenceDataRepository.GetEmploymentClassificationsAsync(bypassCache);
            }
            return _employmentClassification;
        }

        private IEnumerable<Domain.Base.Entities.Department> _departments = null;

        private async Task<IEnumerable<Domain.Base.Entities.Department>> GetAllDepartmentsAsync(bool bypassCache)
        {
            if (_departments == null)
            {
                _departments = await _referenceDataRepository.GetDepartmentsAsync(bypassCache);
            }
            return _departments;
        }

        private IEnumerable<Domain.HumanResources.Entities.EmploymentDepartment> _employmentDepartments = null;

        private async Task<IEnumerable<Domain.HumanResources.Entities.EmploymentDepartment>> GetAllEmploymentDepartmentsAsync(bool bypassCache)
        {
            if (_employmentDepartments == null)
            {
                _employmentDepartments = await _hrReferenceDataRepository.GetEmploymentDepartmentsAsync(bypassCache);
            }
            return _employmentDepartments;
        }

        private IEnumerable<Domain.Base.Entities.Location> _locations = null;

        private async Task<IEnumerable<Domain.Base.Entities.Location>> GetAllLocationsAsync(bool bypassCache)
        {
            if (_locations == null)
            {
                _locations = await _referenceDataRepository.GetLocationsAsync(bypassCache);
            }
            return _locations;
        }

        private IEnumerable<Domain.HumanResources.Entities.BargainingUnit> _bargainingUnits = null;

        private async Task<IEnumerable<Domain.HumanResources.Entities.BargainingUnit>> GetAllGetBargainingUnitsAsync(bool bypassCache)
        {
            if (_bargainingUnits == null)
            {
                _bargainingUnits = await _hrReferenceDataRepository.GetBargainingUnitsAsync(bypassCache);
            }
            return _bargainingUnits;
        }

        /// <summary>
        /// Get an Institution Position from its GUID
        /// </summary>
        /// <returns>A Institution Position DTO <see cref="Ellucian.Colleague.Dtos.InstitutionPosition">object</see></returns>
        public async Task<Ellucian.Colleague.Dtos.InstitutionPosition> GetInstitutionPositionByGuidAsync(string guid, bool ignoreCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Institution Position.");
            }
            CheckGetInstitutionPositionsPermission();
            var positionEntity = (await _positionRepository.GetPositionByGuidAsync(guid));
            if (positionEntity == null)
            {
                throw new KeyNotFoundException("Institution Position not found for GUID " + guid);

            }

            return (await ConvertPositionEntityToInstitutionPositionDto(positionEntity, ignoreCache));
        }

        /// <summary>
        /// Get an Institution Position from its GUID (v11)
        /// </summary>
        /// <returns>A Institution Position DTO <see cref="Ellucian.Colleague.Dtos.InstitutionPosition">object</see></returns>
        public async Task<Ellucian.Colleague.Dtos.InstitutionPosition> GetInstitutionPositionByGuid2Async(string guid, bool ignoreCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Institution Position.");
            }
            CheckGetInstitutionPositionsPermission();
            var positionEntity = (await _positionRepository.GetPositionByGuidAsync(guid));
            if (positionEntity == null)
            {
                throw new KeyNotFoundException("Institution Position not found for GUID " + guid);

            }

            return (await ConvertPositionEntityToInstitutionPositionDto2(positionEntity, ignoreCache));
        }

        /// <summary>
        /// Get an Institution Position from its GUID (v12)
        /// </summary>
        /// <returns>A Institution Position DTO <see cref="Ellucian.Colleague.Dtos.InstitutionPosition">object</see></returns>
        public async Task<Ellucian.Colleague.Dtos.InstitutionPosition2> GetInstitutionPositionByGuid3Async(string guid, bool ignoreCache = false)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("guid", "GUID is required to get an Institution Position.");
            }
            CheckGetInstitutionPositionsPermission();
            var positionEntity = (await _positionRepository.GetPositionByGuidAsync(guid));
            if (positionEntity == null)
            {
                throw new KeyNotFoundException("Institution Position not found for GUID " + guid);

            }

            return (await ConvertPositionEntityToInstitutionPositionDto3(positionEntity, ignoreCache));
        }

        /// <summary>
        /// Return a list of InstitutionPositions objects based on selection criteria.
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="campus">The physical location of the institution position</param>
        /// <param name="status">The status of the position (e.g. active, frozen, cancelled, inactive)</param>
        /// <param name="bargainingUnit">The group or union associated with the position</param>
        /// <param name="reportsToPosition">The position to which this position reports</param>
        /// <param name="exemptionType">An indicator if the position is exempt or non-exempt</param>
        /// <param name="compensationType">The type of compensation awarded (e.g. salary, wages, etc.)</param>
        /// <param name="startOn">The date when the position is first available</param>
        /// <param name="endOn">The date when the position is last available</param>
        /// <returns>List of InstitutionPositions <see cref="Dtos.InstitutionPosition"/> objects representing matching Institution Position</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition>, int>> GetInstitutionPositionsAsync(int offset, int limit, string campus = "", string status = "", string bargainingUnit = "",
            string reportsToPosition = "", string exemptionType = "", string compensationType = "", string startOn = "", string endOn = "", bool bypassCache = false)
        {
            try
            {
                //check permissions
                CheckGetInstitutionPositionsPermission();

                //if campus filter present find code for location
                var campusCode = string.Empty;
                if (!string.IsNullOrEmpty(campus))
                {
                    var allLocations = (await GetAllLocationsAsync(bypassCache)).ToList();
                    if (allLocations.Any())
                    {
                        var location = allLocations.FirstOrDefault(sc => sc.Guid == campus);
                        if (location != null)
                        {
                            campusCode = location.Code;
                        }
                        else
                        {
                            //throw new ArgumentException(string.Concat("Invalid value for campus filter sent in. No campus was found for id '", campus, "'"));
                            return new Tuple<IEnumerable<Dtos.InstitutionPosition>, int> (new List<Dtos.InstitutionPosition>(), 0);
                        }
                    }
                }

                //if bargainingUnit filter is present get the code
                var bargainingUnitCode = string.Empty;
                if (!string.IsNullOrEmpty(bargainingUnit))
                {
                    var allBargainingUnits = (await GetAllGetBargainingUnitsAsync(bypassCache)).ToList();
                    if (allBargainingUnits.Any())
                    {
                        var bargainingUnitEntity = allBargainingUnits.FirstOrDefault(bu => bu.Guid == bargainingUnit);
                        if (bargainingUnitEntity != null)
                        {
                            bargainingUnitCode = bargainingUnitEntity.Code;
                        }
                        else
                        {
                            //throw new ArgumentException(string.Concat("Invalid value for bargainingUnit filter sent in. No bargainingUnit was found for id '", bargainingUnit, "'"));
                            return new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(new List<Dtos.InstitutionPosition>(), 0);
                        }
                    }
                }

                //if reportsToPosition filter is present, get the positionid
               List<string> positionIdFilter = null;
                if (!string.IsNullOrEmpty(reportsToPosition))
                {
                    try
                    {
                        positionIdFilter = new List<string>();
                        positionIdFilter.Add(await _positionRepository.GetPositionIdFromGuidAsync(reportsToPosition));
                    }
                    catch (Exception)
                    {
                        //throw new ArgumentException(string.Concat("Invalid value for reportsToPosition filter sent in. No position was found for id '", reportsToPosition, "'"));
                        return new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(new List<Dtos.InstitutionPosition>(), 0);
                    }
                }

                //validate exemptionType
                if (!string.IsNullOrEmpty(exemptionType))
                {
                    if (exemptionType != "exempt" && exemptionType != "nonExempt")
                    {
                        throw new ArgumentException(string.Format("{0} is an invalid enumeration value", exemptionType));                    
                    }
                }

                //validate compensationType
                if (!string.IsNullOrEmpty(compensationType))
                {
                    if (compensationType != "wages" && compensationType != "salary")
                    {
                        throw new ArgumentException(string.Format("{0} is an invalid enumeration value", compensationType));
                    }
                }

                //convert the start on if supplied
                var startOnFilter = string.Empty;
                if(!string.IsNullOrEmpty(startOn))
                {
                    startOnFilter = await ConvertDateArgument(startOn);
                }

                //convert the end of if supplied
                var endOnFilter = string.Empty;
                if(!string.IsNullOrEmpty(endOn))
                {
                    endOnFilter = await ConvertDateArgument(endOn);
                }

                var positionEntitiesTuple = await _positionRepository.GetPositionsAsync(offset, limit, campusCode, status,
                            bargainingUnitCode, positionIdFilter, exemptionType, compensationType, startOnFilter, endOnFilter, bypassCache);
                if (positionEntitiesTuple != null)
                {
                    var positionEntities = positionEntitiesTuple.Item1.ToList();
                    var totalCount = positionEntitiesTuple.Item2;

                    if (positionEntities.Any())
                    {
                        var institutionPositions = new List<Colleague.Dtos.InstitutionPosition>();

                        foreach (var positionEntity in positionEntities)
                        {
                            institutionPositions.Add(await ConvertPositionEntityToInstitutionPositionDto(positionEntity, bypassCache));
                        }
                        return new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositions, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(new List<Dtos.InstitutionPosition>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(new List<Dtos.InstitutionPosition>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// Return a list of InstitutionPositions objects based on selection criteria. (v11)
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="campus">The physical location of the institution position</param>
        /// <param name="status">The status of the position (e.g. active, frozen, cancelled, inactive)</param>
        /// <param name="bargainingUnit">The group or union associated with the position</param>
        /// <param name="reportsToPositions">The position to which this position reports</param>
        /// <param name="exemptionType">An indicator if the position is exempt or non-exempt</param>
        /// <param name="compensationType">The type of compensation awarded (e.g. salary, wages, etc.)</param>
        /// <param name="startOn">The date when the position is first available</param>
        /// <param name="endOn">The date when the position is last available</param>
        /// <returns>List of InstitutionPositions <see cref="Dtos.InstitutionPosition"/> objects representing matching Institution Position</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition>, int>> GetInstitutionPositions2Async(int offset, int limit, string campus = "", string status = "", string bargainingUnit = "",
            List<string> reportsToPositions = null, string exemptionType = "", string compensationType = "", string startOn = "", string endOn = "", bool bypassCache = false)
        {
            try
            {
                //if campus filter present find code for location
                var campusCode = string.Empty;
                if (!string.IsNullOrEmpty(campus))
                {
                    var allLocations = (await GetAllLocationsAsync(bypassCache)).ToList();
                    if (allLocations.Any())
                    {
                        var location = allLocations.FirstOrDefault(sc => sc.Guid == campus);
                        if (location != null)
                        {
                            campusCode = location.Code;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<InstitutionPosition>, int>(new List<InstitutionPosition>(), 0);
                        }
                    }
                }

                //if bargainingUnit filter is present get the code
                var bargainingUnitCode = string.Empty;
                if (!string.IsNullOrEmpty(bargainingUnit))
                {
                    var allBargainingUnits = (await GetAllGetBargainingUnitsAsync(bypassCache)).ToList();
                    if (allBargainingUnits.Any())
                    {
                        var bargainingUnitEntity = allBargainingUnits.FirstOrDefault(bu => bu.Guid == bargainingUnit);
                        if (bargainingUnitEntity != null)
                        {
                            bargainingUnitCode = bargainingUnitEntity.Code;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<InstitutionPosition>, int>(new List<InstitutionPosition>(), 0);
                        }
                    }
                }

                //if reportsToPosition filter is present, get the positionid
                List<string> positionIdFilter = null;
                if (reportsToPositions != null && reportsToPositions.Any())
                {
                    positionIdFilter = new List<string>();
                    foreach (var reportsToPosition in reportsToPositions)
                    {
                        try
                        {
                            //  if position not found, throws key not found exception
                            positionIdFilter.Add(await _positionRepository.GetPositionIdFromGuidAsync(reportsToPosition));
                        }
                        catch (Exception)
                        {
                            return new Tuple<IEnumerable<InstitutionPosition>, int>(new List<InstitutionPosition>(), 0);
                        }
                    }
                }

                //validate exemptionType
                if (!string.IsNullOrEmpty(exemptionType))
                {
                    if (exemptionType.ToLower() != "exempt" && exemptionType.ToLower() != "nonexempt")
                    {
                        //return new Tuple<IEnumerable<InstitutionPosition>, int>(new List<InstitutionPosition>(), 0);
                        throw new ArgumentException(string.Concat("'", exemptionType, "' is an invalid enumeration value"));

                    }
                }

                //validate compensationType
                if (!string.IsNullOrEmpty(compensationType))
                {
                    if (compensationType != "wages" && compensationType != "salary")
                    {
                        //return new Tuple<IEnumerable<InstitutionPosition>, int>(new List<InstitutionPosition>(), 0);
                        throw new ArgumentException(string.Concat("'", compensationType, "' is an invalid enumeration value"));
                    }
                }

                //convert the start on if supplied
                var startOnFilter = string.Empty;
                if (!string.IsNullOrEmpty(startOn))
                {
                    startOnFilter = await ConvertDateArgument(startOn);
                }

                //convert the end of if supplied
                var endOnFilter = string.Empty;
                if (!string.IsNullOrEmpty(endOn))
                {
                    endOnFilter = await ConvertDateArgument(endOn);
                }


                //check permissions
                CheckGetInstitutionPositionsPermission();

                var positionEntitiesTuple = await _positionRepository.GetPositionsAsync(offset, limit, campusCode, status,
                            bargainingUnitCode, positionIdFilter, exemptionType, compensationType, startOnFilter, endOnFilter, bypassCache);
                if (positionEntitiesTuple != null)
                {
                    var positionEntities = positionEntitiesTuple.Item1.ToList();
                    var totalCount = positionEntitiesTuple.Item2;

                    if (positionEntities.Any())
                    {
                        var institutionPositions = new List<Colleague.Dtos.InstitutionPosition>();

                        foreach (var positionEntity in positionEntities)
                        {
                            institutionPositions.Add(await ConvertPositionEntityToInstitutionPositionDto2(positionEntity, bypassCache));
                        }
                        return new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(institutionPositions, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(new List<Dtos.InstitutionPosition>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.InstitutionPosition>, int>(new List<Dtos.InstitutionPosition>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// Return a list of InstitutionPositions objects based on selection criteria. (v12)
        /// </summary>
        /// <param name="bypassCache"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="campus">The physical location of the institution position</param>
        /// <param name="status">The status of the position (e.g. active, frozen, cancelled, inactive)</param>
        /// <param name="bargainingUnit">The group or union associated with the position</param>
        /// <param name="reportsToPositions">The position to which this position reports</param>
        /// <param name="exemptionType">An indicator if the position is exempt or non-exempt</param>
        /// <param name="compensationType">The type of compensation awarded (e.g. salary, wages, etc.)</param>
        /// <param name="startOn">The date when the position is first available</param>
        /// <param name="endOn">The date when the position is last available</param>
        /// <returns>List of InstitutionPositions <see cref="Dtos.InstitutionPosition"/> objects representing matching Institution Position</returns>
        public async Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.InstitutionPosition2>, int>> GetInstitutionPositions3Async(int offset, int limit, string campus = "", string status = "", string bargainingUnit = "",
            List<string> reportsToPositions = null, string exemptionType = "", string compensationType = "", string startOn = "", string endOn = "", bool bypassCache = false)
        {
            try
            {
                //if campus filter present find code for location
                var campusCode = string.Empty;
                if (!string.IsNullOrEmpty(campus))
                {
                    var allLocations = (await GetAllLocationsAsync(bypassCache)).ToList();
                    if (allLocations.Any())
                    {
                        var location = allLocations.FirstOrDefault(sc => sc.Guid == campus);
                        if (location != null)
                        {
                            campusCode = location.Code;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<InstitutionPosition2>, int>(new List<InstitutionPosition2>(), 0);
                        }
                    }
                }

                //if bargainingUnit filter is present get the code
                var bargainingUnitCode = string.Empty;
                if (!string.IsNullOrEmpty(bargainingUnit))
                {
                    var allBargainingUnits = (await GetAllGetBargainingUnitsAsync(bypassCache)).ToList();
                    if (allBargainingUnits.Any())
                    {
                        var bargainingUnitEntity = allBargainingUnits.FirstOrDefault(bu => bu.Guid == bargainingUnit);
                        if (bargainingUnitEntity != null)
                        {
                            bargainingUnitCode = bargainingUnitEntity.Code;
                        }
                        else
                        {
                            return new Tuple<IEnumerable<InstitutionPosition2>, int>(new List<InstitutionPosition2>(), 0);
                        }
                    }
                }

                //if reportsToPosition filter is present, get the positionid
                List<string> positionIdFilter = null;
                if (reportsToPositions != null && reportsToPositions.Any())
                {
                    positionIdFilter = new List<string>();
                    foreach (var reportsToPosition in reportsToPositions)
                    {
                        try
                        {
                            //  if position not found, throws key not found exception
                            positionIdFilter.Add(await _positionRepository.GetPositionIdFromGuidAsync(reportsToPosition));
                        }
                        catch (Exception)
                        {
                            return new Tuple<IEnumerable<InstitutionPosition2>, int>(new List<InstitutionPosition2>(), 0);
                        }
                    }
                }

                //validate exemptionType
                if (!string.IsNullOrEmpty(exemptionType))
                {
                    if (exemptionType.ToLower() != "exempt" && exemptionType.ToLower() != "nonexempt")
                    {
                        //return new Tuple<IEnumerable<InstitutionPosition2>, int>(new List<InstitutionPosition2>(), 0);
                        throw new ArgumentException(string.Concat("'", exemptionType, "' is an invalid enumeration value"));
                    }
                }

                //validate compensationType
                if (!string.IsNullOrEmpty(compensationType))
                {
                    if (compensationType != "wages" && compensationType != "salary")
                    {
                        //return new Tuple<IEnumerable<InstitutionPosition2>, int>(new List<InstitutionPosition2>(), 0);
                        throw new ArgumentException(string.Concat("'", compensationType, "' is an invalid enumeration value"));
                    }
                }

                //convert the start on if supplied
                var startOnFilter = string.Empty;
                if (!string.IsNullOrEmpty(startOn))
                {
                    startOnFilter = await ConvertDateArgument(startOn);
                }

                //convert the end of if supplied
                var endOnFilter = string.Empty;
                if (!string.IsNullOrEmpty(endOn))
                {
                    endOnFilter = await ConvertDateArgument(endOn);
                }


                //check permissions
                CheckGetInstitutionPositionsPermission();

                var positionEntitiesTuple = await _positionRepository.GetPositionsAsync(offset, limit, campusCode, status,
                            bargainingUnitCode, positionIdFilter, exemptionType, compensationType, startOnFilter, endOnFilter, bypassCache);
                if (positionEntitiesTuple != null)
                {
                    var positionEntities = positionEntitiesTuple.Item1.ToList();
                    var totalCount = positionEntitiesTuple.Item2;

                    if (positionEntities.Any())
                    {
                        var institutionPositions = new List<Colleague.Dtos.InstitutionPosition2>();

                        foreach (var positionEntity in positionEntities)
                        {
                            institutionPositions.Add(await ConvertPositionEntityToInstitutionPositionDto3(positionEntity, bypassCache));
                        }
                        return new Tuple<IEnumerable<Dtos.InstitutionPosition2>, int>(institutionPositions, totalCount);
                    }
                    // no results
                    return new Tuple<IEnumerable<Dtos.InstitutionPosition2>, int>(new List<Dtos.InstitutionPosition2>(), totalCount);
                }
                //no results
                return new Tuple<IEnumerable<Dtos.InstitutionPosition2>, int>(new List<Dtos.InstitutionPosition2>(), 0);
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// Converts a Position domain entity to a Institution Position DTO
        /// </summary>
        /// <param name="positionEntity">A list of <see cref="InstitutionPosition">InstitutionPosition</see> domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>A <see cref="InstitutionPosition">InstitutionPositions</see> DTO</returns>
        private async Task<InstitutionPosition> ConvertPositionEntityToInstitutionPositionDto(Ellucian.Colleague.Domain.HumanResources.Entities.Position positionEntity, bool bypassCache)
        {

            if (positionEntity == null)
            {
                throw new ArgumentNullException("Position Entity is required.");
            }

            if (string.IsNullOrEmpty(positionEntity.Guid))
            {
                throw new ArgumentNullException("Position GUID is required.");
            }

            Domain.HumanResources.Entities.PositionPay currentPositionPay = null;
            var posPayIDs = positionEntity.PositionPayScheduleIds;
            if (posPayIDs != null && posPayIDs.Any())
            {
                var positionPayCollection = (await _positionRepository.GetPositionPayByIdsAsync(posPayIDs)).ToList();
                var currentDate = DateTime.Now.Date;
                if (positionPayCollection.Any())
                {
                    currentPositionPay =
                       positionPayCollection.FirstOrDefault(posPay => !posPay.EndDate.HasValue
                                          && posPay.StartDate <= currentDate)
                        ?? positionPayCollection.FirstOrDefault(posPay => posPay.EndDate.HasValue
                                          && posPay.StartDate <= currentDate && posPay.EndDate >= currentDate);
                }
            }

            var institutionPositionDto = new Colleague.Dtos.InstitutionPosition();
            try
            {
                institutionPositionDto.Id = positionEntity.Guid;

                institutionPositionDto.Title = positionEntity.Title;
                institutionPositionDto.Description = string.IsNullOrWhiteSpace(positionEntity.PositionJobDesc) ? null : positionEntity.PositionJobDesc;

                if (!string.IsNullOrEmpty(positionEntity.PositionLocation))
                {
                    var allLocations = (await GetAllLocationsAsync(bypassCache)).ToList();
                    if (allLocations.Any())
                    {
                        var location = allLocations.FirstOrDefault(sc => sc.Code == positionEntity.PositionLocation);
                        if (location != null)
                        {
                            institutionPositionDto.Campus = new GuidObject2(location.Guid);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(positionEntity.PositionDept))
                {
                    var allDepartments = (await GetAllDepartmentsAsync(bypassCache)).ToList();
                    if (allDepartments.Any())
                    {
                        var department = allDepartments.FirstOrDefault(sc => sc.Code == positionEntity.PositionDept);
                        if (department != null)
                        {
                            var departmentProperty = new NameDetailDtoProperty
                            {
                                Detail = new GuidObject2(department.Guid),
                                Name = department.Description
                            };
                            institutionPositionDto.Departments = new List<NameDetailDtoProperty>() {departmentProperty};
                        }
                    }
                }

                // If the request date is on or after the position start date (POS.START.DATE)
                // AND the position end date (POS.END.DATE) is null or the request date is on or before the end date, 
                // then return "active".
                if ((DateTime.Now.Date.CompareTo(positionEntity.StartDate) >= 0)
                    && ((!positionEntity.EndDate.HasValue) || (DateTime.Now.Date <= positionEntity.EndDate)))
                {
                    institutionPositionDto.Status = PositionStatus.Active;
                }
                //If the position start date (POS.START.DATE) is after the request date, then return "inactive".
                else if (positionEntity.StartDate.CompareTo(DateTime.Now.Date) > 0)
                {
                    institutionPositionDto.Status = PositionStatus.Inactive;
                }
                //If the request date is after the position end date (POS.END.DATE) then return "cancelled".
                else if ((positionEntity.EndDate.HasValue) || (DateTime.Now.Date > positionEntity.EndDate))
                {
                    institutionPositionDto.Status = PositionStatus.Cancelled;
                }
                // Default - shouldnt hit, but this is a required field
                else
                {
                    throw new Exception("Unable to determine institution position status." + positionEntity.Id);
                }

                if (currentPositionPay != null)
                {

                    var accountingStrings = new List<string>();
                    foreach (var source in currentPositionPay.FundingSource)
                    {
                        if (!string.IsNullOrEmpty(source.FundingSourceId))
                        {
                            var fundingSource = source.FundingSourceId.Replace("_", "-");

                            accountingStrings.Add(string.IsNullOrEmpty(source.ProjectRefNumber) ?
                                fundingSource : string.Concat(fundingSource, '*', source.ProjectRefNumber));
                        }
                    }
                    if (accountingStrings.Any())
                        institutionPositionDto.AccountingStrings = accountingStrings;


                    var hoursPerPeriodDtoProperties = new List<HoursPerPeriodDtoProperty>();
                    if ((currentPositionPay.CycleWorkTimeUnits == "HRS") && (currentPositionPay.CycleWorkTimeAmount.HasValue))
                    {
                        hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                        {
                            Hours = currentPositionPay.CycleWorkTimeAmount,
                            Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.PayPeriod
                        });
                    }
                    if ((currentPositionPay.YearWorkTimeUnits == "HRS") && (currentPositionPay.YearWorkTimeAmount.HasValue))
                    {
                        hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                        {
                            Hours = currentPositionPay.YearWorkTimeAmount,
                            Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.Year
                        });
                    }
                    institutionPositionDto.HoursPerPeriod = hoursPerPeriodDtoProperties.Any() ? hoursPerPeriodDtoProperties : null;


                    if (!string.IsNullOrEmpty(currentPositionPay.BargainingUnit))
                    {
                        var allBargainingUnits = (await GetAllGetBargainingUnitsAsync(bypassCache)).ToList();
                        if (allBargainingUnits.Any())
                        {
                            var bargainingUnit = allBargainingUnits.FirstOrDefault(bu => bu.Code == currentPositionPay.BargainingUnit);
                            if (bargainingUnit != null)
                            {
                                institutionPositionDto.BargainingUnit = new GuidObject2(bargainingUnit.Guid);
                            }
                        }
                    }

                    if ((!string.IsNullOrEmpty(currentPositionPay.SalaryMinimum)) && (!string.IsNullOrEmpty(currentPositionPay.SalaryMaximum)))
                    {
                        // Salary information may actually reflect an hourly wage that is stored up to four decimal 
                        // places, or a salary amount that is stored up to two decimals. As a result, to publish the upper/lower Bound 
                        // properly, we need to convert the data to the appropriate format depending on how POS.HRLY.OR.SLRY is set. 
                        var lowerBound = FormatSalary(currentPositionPay.SalaryMinimum, positionEntity.IsSalary);
                        var upperBound = FormatSalary(currentPositionPay.SalaryMaximum, positionEntity.IsSalary);

                        var hostCountry = currentPositionPay.HostCountry;

                        var currencyCode = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                            CurrencyIsoCode.USD;

                        if ((lowerBound.HasValue) && (upperBound.HasValue))
                        {
                            institutionPositionDto.Compensation = new CompensationDtoProperty
                            {
                                Type = positionEntity.IsSalary ? CompensationType.Salary : CompensationType.Wages,
                                Range = new CompensationRangeDtoProperty()
                                {
                                    CurrencyCode = currencyCode,
                                    LowerBound = lowerBound,
                                    UpperBound = upperBound
                                }
                            };
                        }
                    }
                }

                var reportsToDtoProperties = new List<ReportsToDtoProperty>();
                if (!string.IsNullOrEmpty(positionEntity.SupervisorPositionId))
                {
                    try
                    {
                        var supervisorPositionGuid = await _positionRepository.GetPositionGuidFromIdAsync(positionEntity.SupervisorPositionId);
                        if (!string.IsNullOrEmpty(supervisorPositionGuid))
                        {
                            var reportsToDtoProperty = new ReportsToDtoProperty
                            {
                                Postition = new GuidObject2(supervisorPositionGuid),
                                Type = PositionReportsToType.Primary
                            };
                            reportsToDtoProperties.Add(reportsToDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, "Institution Position exception occurred:");
                        }
                    }

                }
                if (!string.IsNullOrEmpty(positionEntity.AlternateSupervisorPositionId))
                {
                    try
                    {
                        var altSupervisorPositionGuid = await _positionRepository.GetPositionGuidFromIdAsync(positionEntity.AlternateSupervisorPositionId);
                        if (!string.IsNullOrEmpty(altSupervisorPositionGuid))
                        {
                            var reportsToDtoProperty = new ReportsToDtoProperty
                            {
                                Postition = new GuidObject2(altSupervisorPositionGuid),
                                Type = PositionReportsToType.Alternative
                            };
                            reportsToDtoProperties.Add(reportsToDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, "Institution Position exception occurred:");
                        }
                    }
                }
                institutionPositionDto.ReportsTo = reportsToDtoProperties.Any() ? reportsToDtoProperties : null;


                institutionPositionDto.ExemptionType = positionEntity.IsExempt ? ExemptionType.Exempt : ExemptionType.NonExempt;

                institutionPositionDto.StartOn = positionEntity.StartDate;
                institutionPositionDto.EndOn = positionEntity.EndDate;
                institutionPositionDto.AuthorizedOn = positionEntity.PositionAuthorizedDate;

                if (!string.IsNullOrEmpty(positionEntity.PositionClass))
                {
                    var allStudentClassification = (await GetAllEmploymentClassificationAsync(bypassCache)).ToList();
                    if (allStudentClassification.Any())
                    {
                        var studentClassification = allStudentClassification.FirstOrDefault(sc => sc.Code == positionEntity.PositionClass);
                        if (studentClassification != null)
                        {
                            institutionPositionDto.Classification = new GuidObject2(studentClassification.Guid);
                        }
                    }
                }

                return institutionPositionDto;
            }
            catch (Exception ex)
            {
                if (logger.IsErrorEnabled)
                {
                    logger.Error(ex, "Institution Position exception occurred:");
                }
                throw new Exception("Institution Position exception occurred." + ex.Message);
            }
        }

        /// <summary>
        /// Converts a Position domain entity to a Institution Position DTO (v11)
        /// </summary>
        /// <param name="positionEntity">A list of <see cref="InstitutionPosition">InstitutionPosition</see> domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>A <see cref="InstitutionPosition">InstitutionPositions</see> DTO</returns>
        private async Task<InstitutionPosition> ConvertPositionEntityToInstitutionPositionDto2(Ellucian.Colleague.Domain.HumanResources.Entities.Position positionEntity, bool bypassCache)
        {

            if (positionEntity == null)
            {
                throw new ArgumentNullException("Position Entity is required.");
            }

            if (string.IsNullOrEmpty(positionEntity.Guid))
            {
                throw new ArgumentNullException("Position GUID is required.");
            }

            Domain.HumanResources.Entities.PositionPay currentPositionPay = null;
            var posPayIDs = positionEntity.PositionPayScheduleIds;
            if (posPayIDs != null && posPayIDs.Any())
            {
                var positionPayCollection = (await _positionRepository.GetPositionPayByIdsAsync(posPayIDs)).ToList();
                var currentDate = DateTime.Now.Date;
                if (positionPayCollection.Any())
                {
                    currentPositionPay =
                       positionPayCollection.FirstOrDefault(posPay => !posPay.EndDate.HasValue
                                          && posPay.StartDate <= currentDate)
                        ?? positionPayCollection.FirstOrDefault(posPay => posPay.EndDate.HasValue
                                          && posPay.StartDate <= currentDate && posPay.EndDate >= currentDate);
                }
            }

            var institutionPositionDto = new Colleague.Dtos.InstitutionPosition();
            try
            {
                institutionPositionDto.Id = positionEntity.Guid;
                institutionPositionDto.Code = positionEntity.Id;
                institutionPositionDto.Title = positionEntity.Title;
                institutionPositionDto.Description = string.IsNullOrWhiteSpace(positionEntity.PositionJobDesc) ? null : positionEntity.PositionJobDesc;

                if (!string.IsNullOrEmpty(positionEntity.PositionLocation))
                {
                    var allLocations = (await GetAllLocationsAsync(bypassCache)).ToList();
                    if (allLocations.Any())
                    {
                        var location = allLocations.FirstOrDefault(sc => sc.Code == positionEntity.PositionLocation);
                        if (location != null)
                        {
                            institutionPositionDto.Campus = new GuidObject2(location.Guid);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(positionEntity.PositionDept))
                {
                    var allDepartments = (await GetAllDepartmentsAsync(bypassCache)).ToList();
                    if (allDepartments.Any())
                    {
                        var department = allDepartments.FirstOrDefault(sc => sc.Code == positionEntity.PositionDept);
                        if (department != null)
                        {
                            var departmentProperty = new NameDetailDtoProperty
                            {
                                Detail = new GuidObject2(department.Guid),
                                Name = department.Description
                            };
                            institutionPositionDto.Departments = new List<NameDetailDtoProperty>() { departmentProperty };
                        }
                    }
                }

                // If the request date is on or after the position start date (POS.START.DATE)
                // AND the position end date (POS.END.DATE) is null or the request date is on or before the end date, 
                // then return "active".
                if ((DateTime.Now.Date.CompareTo(positionEntity.StartDate) >= 0)
                    && ((!positionEntity.EndDate.HasValue) || (DateTime.Now.Date <= positionEntity.EndDate)))
                {
                    institutionPositionDto.Status = PositionStatus.Active;
                }
                //If the position start date (POS.START.DATE) is after the request date, then return "inactive".
                else if (positionEntity.StartDate.CompareTo(DateTime.Now.Date) > 0)
                {
                    institutionPositionDto.Status = PositionStatus.Inactive;
                }
                //If the request date is after the position end date (POS.END.DATE) then return "cancelled".
                else if ((positionEntity.EndDate.HasValue) || (DateTime.Now.Date > positionEntity.EndDate))
                {
                    institutionPositionDto.Status = PositionStatus.Cancelled;
                }
                // Default - shouldnt hit, but this is a required field
                else
                {
                    throw new Exception("Unable to determine institution position status." + positionEntity.Id);
                }

                if (currentPositionPay != null)
                {

                    var accountingStrings = new List<AccountingStringAllocationsDtoProperty>();
                    var idx = 0;
                    foreach (var source in currentPositionPay.FundingSource)
                    {
                        if (!string.IsNullOrEmpty(source.FundingSourceId))
                        {
                            var fundingSource = source.FundingSourceId.Replace("_", "-");

                            var accountAllocation = new AccountingStringAllocationsDtoProperty()
                            {
                                AccountingString = string.IsNullOrEmpty(source.ProjectRefNumber) ?
                                    fundingSource : string.Concat(fundingSource, '*', source.ProjectRefNumber),
                                AllocatedPercentage = currentPositionPay.PospayFndgPct.ElementAt(idx)
                            };
                            idx++;

                            accountingStrings.Add(accountAllocation);
                        }
                    }
                    if (accountingStrings.Any())
                        institutionPositionDto.AccountingStringAllocations = accountingStrings;


                    var hoursPerPeriodDtoProperties = new List<HoursPerPeriodDtoProperty>();
                    if ((currentPositionPay.CycleWorkTimeUnits == "HRS") && (currentPositionPay.CycleWorkTimeAmount.HasValue))
                    {
                        hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                        {
                            Hours = currentPositionPay.CycleWorkTimeAmount,
                            Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.PayPeriod
                        });
                    }
                    if ((currentPositionPay.YearWorkTimeUnits == "HRS") && (currentPositionPay.YearWorkTimeAmount.HasValue))
                    {
                        hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                        {
                            Hours = currentPositionPay.YearWorkTimeAmount,
                            Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.Year
                        });
                    }
                    institutionPositionDto.HoursPerPeriod = hoursPerPeriodDtoProperties.Any() ? hoursPerPeriodDtoProperties : null;


                    if (!string.IsNullOrEmpty(currentPositionPay.BargainingUnit))
                    {
                        var allBargainingUnits = (await GetAllGetBargainingUnitsAsync(bypassCache)).ToList();
                        if (allBargainingUnits.Any())
                        {
                            var bargainingUnit = allBargainingUnits.FirstOrDefault(bu => bu.Code == currentPositionPay.BargainingUnit);
                            if (bargainingUnit != null)
                            {
                                institutionPositionDto.BargainingUnit = new GuidObject2(bargainingUnit.Guid);
                            }
                        }
                    }

                    if ((!string.IsNullOrEmpty(currentPositionPay.SalaryMinimum)) && (!string.IsNullOrEmpty(currentPositionPay.SalaryMaximum)))
                    {
                        // Salary information may actually reflect an hourly wage that is stored up to four decimal 
                        // places, or a salary amount that is stored up to two decimals. As a result, to publish the upper/lower Bound 
                        // properly, we need to convert the data to the appropriate format depending on how POS.HRLY.OR.SLRY is set. 
                        var lowerBound = FormatSalary(currentPositionPay.SalaryMinimum, positionEntity.IsSalary);
                        var upperBound = FormatSalary(currentPositionPay.SalaryMaximum, positionEntity.IsSalary);

                        var hostCountry = currentPositionPay.HostCountry;

                        var currencyCode = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                            CurrencyIsoCode.USD;

                        if ((lowerBound.HasValue) && (upperBound.HasValue))
                        {
                            institutionPositionDto.Compensation = new CompensationDtoProperty
                            {
                                Type = positionEntity.IsSalary ? CompensationType.Salary : CompensationType.Wages,
                                Range = new CompensationRangeDtoProperty()
                                {
                                    CurrencyCode = currencyCode,
                                    LowerBound = lowerBound,
                                    UpperBound = upperBound
                                }
                            };
                        }
                    }
                }

                var reportsToDtoProperties = new List<ReportsToDtoProperty>();
                if (!string.IsNullOrEmpty(positionEntity.SupervisorPositionId))
                {
                    try
                    {
                        var supervisorPositionGuid = await _positionRepository.GetPositionGuidFromIdAsync(positionEntity.SupervisorPositionId);
                        if (!string.IsNullOrEmpty(supervisorPositionGuid))
                        {
                            var reportsToDtoProperty = new ReportsToDtoProperty
                            {
                                Postition = new GuidObject2(supervisorPositionGuid),
                                Type = PositionReportsToType.Primary
                            };
                            reportsToDtoProperties.Add(reportsToDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, "Institution Position exception occurred:");
                        }
                    }

                }
                if (!string.IsNullOrEmpty(positionEntity.AlternateSupervisorPositionId))
                {
                    try
                    {
                        var altSupervisorPositionGuid = await _positionRepository.GetPositionGuidFromIdAsync(positionEntity.AlternateSupervisorPositionId);
                        if (!string.IsNullOrEmpty(altSupervisorPositionGuid))
                        {
                            var reportsToDtoProperty = new ReportsToDtoProperty
                            {
                                Postition = new GuidObject2(altSupervisorPositionGuid),
                                Type = PositionReportsToType.Alternative
                            };
                            reportsToDtoProperties.Add(reportsToDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, "Institution Position exception occurred:");
                        }
                    }
                }
                institutionPositionDto.ReportsTo = reportsToDtoProperties.Any() ? reportsToDtoProperties : null;


                institutionPositionDto.ExemptionType = positionEntity.IsExempt ? ExemptionType.Exempt : ExemptionType.NonExempt;

                institutionPositionDto.StartOn = positionEntity.StartDate;
                institutionPositionDto.EndOn = positionEntity.EndDate;
                institutionPositionDto.AuthorizedOn = positionEntity.PositionAuthorizedDate;

                if (!string.IsNullOrEmpty(positionEntity.PositionClass))
                {
                    var allStudentClassification = (await GetAllEmploymentClassificationAsync(bypassCache)).ToList();
                    if (allStudentClassification.Any())
                    {
                        var studentClassification = allStudentClassification.FirstOrDefault(sc => sc.Code == positionEntity.PositionClass);
                        if (studentClassification != null)
                        {
                            institutionPositionDto.Classification = new GuidObject2(studentClassification.Guid);
                        }
                    }
                }

                return institutionPositionDto;
            }
            catch (Exception ex)
            {
                if (logger.IsErrorEnabled)
                {
                    logger.Error(ex, "Institution Position exception occurred:");
                }
                throw new Exception("Institution Position exception occurred." + ex.Message);
            }
        }

        /// <summary>
        /// Converts a Position domain entity to a Institution Position DTO (v11)
        /// </summary>
        /// <param name="positionEntity">A list of <see cref="InstitutionPosition">InstitutionPosition</see> domain entity</param>
        /// <param name="bypassCache"></param>
        /// <returns>A <see cref="InstitutionPosition">InstitutionPositions</see> DTO</returns>
        private async Task<InstitutionPosition2> ConvertPositionEntityToInstitutionPositionDto3(Ellucian.Colleague.Domain.HumanResources.Entities.Position positionEntity, bool bypassCache)
        {

            if (positionEntity == null)
            {
                throw new ArgumentNullException("Position Entity is required.");
            }

            if (string.IsNullOrEmpty(positionEntity.Guid))
            {
                throw new ArgumentNullException("Position GUID is required.");
            }

            Domain.HumanResources.Entities.PositionPay currentPositionPay = null;
            var posPayIDs = positionEntity.PositionPayScheduleIds;
            if (posPayIDs != null && posPayIDs.Any())
            {
                var positionPayCollection = (await _positionRepository.GetPositionPayByIdsAsync(posPayIDs)).ToList();
                var currentDate = DateTime.Now.Date;
                if (positionPayCollection.Any())
                {
                    currentPositionPay =
                       positionPayCollection.FirstOrDefault(posPay => !posPay.EndDate.HasValue
                                          && posPay.StartDate <= currentDate)
                        ?? positionPayCollection.FirstOrDefault(posPay => posPay.EndDate.HasValue
                                          && posPay.StartDate <= currentDate && posPay.EndDate >= currentDate);
                }
            }

            var institutionPositionDto = new Colleague.Dtos.InstitutionPosition2();
            try
            {
                institutionPositionDto.Id = positionEntity.Guid;
                institutionPositionDto.Code = positionEntity.Id;
                institutionPositionDto.Title = positionEntity.Title;
                institutionPositionDto.Description = string.IsNullOrWhiteSpace(positionEntity.PositionJobDesc) ? null : positionEntity.PositionJobDesc;

                if (!string.IsNullOrEmpty(positionEntity.PositionLocation))
                {
                    var allLocations = (await GetAllLocationsAsync(bypassCache)).ToList();
                    if (allLocations.Any())
                    {
                        var location = allLocations.FirstOrDefault(sc => sc.Code == positionEntity.PositionLocation);
                        if (location != null)
                        {
                            institutionPositionDto.Campus = new GuidObject2(location.Guid);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(positionEntity.PositionDept))
                {
                    // Someone changes this to GetAllEmploymentDepartmentsAsync() - a nice idea, but there is no restriction
                    // preventing a position from having an academic department.  This causes issues.

                    var allDepartments = (await GetAllDepartmentsAsync(bypassCache)).ToList();
                    if (allDepartments.Any())
                    {
                        var department = allDepartments.FirstOrDefault(sc => sc.Code == positionEntity.PositionDept);
                        if (department != null)
                        {
                            institutionPositionDto.Departments = new List<GuidObject2>() { new GuidObject2(department.Guid) };
                        }
                    }
                }

                // If the request date is on or after the position start date (POS.START.DATE)
                // AND the position end date (POS.END.DATE) is null or the request date is on or before the end date, 
                // then return "active".
                if ((DateTime.Now.Date.CompareTo(positionEntity.StartDate) >= 0)
                    && ((!positionEntity.EndDate.HasValue) || (DateTime.Now.Date <= positionEntity.EndDate)))
                {
                    institutionPositionDto.Status = PositionStatus.Active;
                }
                //If the position start date (POS.START.DATE) is after the request date, then return "inactive".
                else if (positionEntity.StartDate.CompareTo(DateTime.Now.Date) > 0)
                {
                    institutionPositionDto.Status = PositionStatus.Inactive;
                }
                //If the request date is after the position end date (POS.END.DATE) then return "cancelled".
                else if ((positionEntity.EndDate.HasValue) || (DateTime.Now.Date > positionEntity.EndDate))
                {
                    institutionPositionDto.Status = PositionStatus.Cancelled;
                }
                // Default - shouldnt hit, but this is a required field
                else
                {
                    throw new Exception("Unable to determine institution position status." + positionEntity.Id);
                }

                if (currentPositionPay != null)
                {

                    var accountingStrings = new List<AccountingStringAllocationsDtoProperty>();
                    var idx = 0;
                    foreach (var source in currentPositionPay.FundingSource)
                    {
                        if (!string.IsNullOrEmpty(source.FundingSourceId))
                        {
                            var fundingSource = source.FundingSourceId.Replace("_", "-");

                            var accountAllocation = new AccountingStringAllocationsDtoProperty()
                            {
                                AccountingString = string.IsNullOrEmpty(source.ProjectRefNumber) ?
                                    fundingSource : string.Concat(fundingSource, '*', source.ProjectRefNumber),
                                AllocatedPercentage = currentPositionPay.PospayFndgPct.ElementAt(idx)
                            };
                            idx++;

                            accountingStrings.Add(accountAllocation);
                        }
                    }
                    if (accountingStrings.Any())
                        institutionPositionDto.AccountingStringAllocations = accountingStrings;


                    var hoursPerPeriodDtoProperties = new List<HoursPerPeriodDtoProperty>();
                    if ((currentPositionPay.CycleWorkTimeUnits == "HRS") && (currentPositionPay.CycleWorkTimeAmount.HasValue))
                    {
                        hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                        {
                            Hours = currentPositionPay.CycleWorkTimeAmount,
                            Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.PayPeriod
                        });
                    }
                    if ((currentPositionPay.YearWorkTimeUnits == "HRS") && (currentPositionPay.YearWorkTimeAmount.HasValue))
                    {
                        hoursPerPeriodDtoProperties.Add(new HoursPerPeriodDtoProperty()
                        {
                            Hours = currentPositionPay.YearWorkTimeAmount,
                            Period = Ellucian.Colleague.Dtos.EnumProperties.PayPeriods.Year
                        });
                    }
                    institutionPositionDto.HoursPerPeriod = hoursPerPeriodDtoProperties.Any() ? hoursPerPeriodDtoProperties : null;


                    if (!string.IsNullOrEmpty(currentPositionPay.BargainingUnit))
                    {
                        var allBargainingUnits = (await GetAllGetBargainingUnitsAsync(bypassCache)).ToList();
                        if (allBargainingUnits.Any())
                        {
                            var bargainingUnit = allBargainingUnits.FirstOrDefault(bu => bu.Code == currentPositionPay.BargainingUnit);
                            if (bargainingUnit != null)
                            {
                                institutionPositionDto.BargainingUnit = new GuidObject2(bargainingUnit.Guid);
                            }
                        }
                    }

                    if ((!string.IsNullOrEmpty(currentPositionPay.SalaryMinimum)) && (!string.IsNullOrEmpty(currentPositionPay.SalaryMaximum)))
                    {
                        // Salary information may actually reflect an hourly wage that is stored up to four decimal 
                        // places, or a salary amount that is stored up to two decimals. As a result, to publish the upper/lower Bound 
                        // properly, we need to convert the data to the appropriate format depending on how POS.HRLY.OR.SLRY is set. 
                        var lowerBound = FormatSalary(currentPositionPay.SalaryMinimum, positionEntity.IsSalary);
                        var upperBound = FormatSalary(currentPositionPay.SalaryMaximum, positionEntity.IsSalary);

                        var hostCountry = currentPositionPay.HostCountry;

                        var currencyCode = ((hostCountry == "CAN") || (hostCountry == "CANADA")) ? CurrencyIsoCode.CAD :
                            CurrencyIsoCode.USD;

                        if ((lowerBound.HasValue) && (upperBound.HasValue))
                        {
                            institutionPositionDto.Compensation = new CompensationDtoProperty
                            {
                                Type = positionEntity.IsSalary ? CompensationType.Salary : CompensationType.Wages,
                                Range = new CompensationRangeDtoProperty()
                                {
                                    CurrencyCode = currencyCode,
                                    LowerBound = lowerBound,
                                    UpperBound = upperBound
                                }
                            };
                        }
                    }
                }

                var reportsToDtoProperties = new List<ReportsToDtoProperty>();
                if (!string.IsNullOrEmpty(positionEntity.SupervisorPositionId))
                {
                    try
                    {
                        var supervisorPositionGuid = await _positionRepository.GetPositionGuidFromIdAsync(positionEntity.SupervisorPositionId);
                        if (!string.IsNullOrEmpty(supervisorPositionGuid))
                        {
                            var reportsToDtoProperty = new ReportsToDtoProperty
                            {
                                Postition = new GuidObject2(supervisorPositionGuid),
                                Type = PositionReportsToType.Primary
                            };
                            reportsToDtoProperties.Add(reportsToDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, "Institution Position exception occurred:");
                        }
                    }

                }
                if (!string.IsNullOrEmpty(positionEntity.AlternateSupervisorPositionId))
                {
                    try
                    {
                        var altSupervisorPositionGuid = await _positionRepository.GetPositionGuidFromIdAsync(positionEntity.AlternateSupervisorPositionId);
                        if (!string.IsNullOrEmpty(altSupervisorPositionGuid))
                        {
                            var reportsToDtoProperty = new ReportsToDtoProperty
                            {
                                Postition = new GuidObject2(altSupervisorPositionGuid),
                                Type = PositionReportsToType.Alternative
                            };
                            reportsToDtoProperties.Add(reportsToDtoProperty);
                        }
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        if (logger.IsErrorEnabled)
                        {
                            logger.Error(ex, "Institution Position exception occurred:");
                        }
                    }
                }
                institutionPositionDto.ReportsTo = reportsToDtoProperties.Any() ? reportsToDtoProperties : null;


                institutionPositionDto.ExemptionType = positionEntity.IsExempt ? ExemptionType.Exempt : ExemptionType.NonExempt;

                institutionPositionDto.StartOn = positionEntity.StartDate;
                institutionPositionDto.EndOn = positionEntity.EndDate;
                institutionPositionDto.AuthorizedOn = positionEntity.PositionAuthorizedDate;

                if (!string.IsNullOrEmpty(positionEntity.PositionClass))
                {
                    var allStudentClassification = (await GetAllEmploymentClassificationAsync(bypassCache)).ToList();
                    if (allStudentClassification.Any())
                    {
                        var studentClassification = allStudentClassification.FirstOrDefault(sc => sc.Code == positionEntity.PositionClass);
                        if (studentClassification != null)
                        {
                            institutionPositionDto.Classification = new GuidObject2(studentClassification.Guid);
                        }
                    }
                }

                return institutionPositionDto;
            }
            catch (Exception ex)
            {
                if (logger.IsErrorEnabled)
                {
                    logger.Error(ex, "Institution Position exception occurred:");
                }
                throw new Exception("Institution Position exception occurred." + ex.Message);
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Institution Position.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckGetInstitutionPositionsPermission()
        {
            var hasPermission = HasPermission(HumanResourcesPermissionCodes.ViewInstitutionPosition);

            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view Institution Position.");
            }
        }

        private Decimal? FormatSalary(string amount, bool isSalary)
        {
            if (string.IsNullOrWhiteSpace(amount) || amount == "0") return null;
            try
            {
                var paddedValue = isSalary ? amount.Insert(amount.Length - 2, ".") : amount.Insert(amount.Length - 4, ".");
                return Convert.ToDecimal(paddedValue);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Converts date to unidata Date
        /// </summary>
        /// <param name="date">UTC datetime</param>
        /// <returns>Unidata Date</returns>
        private async Task<string> ConvertDateArgument(string date)
        {
            try
            {
                return await _referenceDataRepository.GetUnidataFormattedDate(date);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid Date format in arguments");
            }
        }
    }
}