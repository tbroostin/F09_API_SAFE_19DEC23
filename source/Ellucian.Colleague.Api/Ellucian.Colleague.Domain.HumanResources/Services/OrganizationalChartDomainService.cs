/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Services
{
    /// <summary>
    /// Gets org chart data
    /// </summary>
    [RegisterType]
    public class OrganizationalChartDomainService : IOrganizationalChartDomainService
    {
        private readonly IOrganizationalChartRepository _organizationalChartRepository;
        private readonly IPersonBaseRepository _personBaseRepository;
        private readonly IPositionRepository _positionRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILogger _logger;
        public OrganizationalChartDomainService(IOrganizationalChartRepository organizationalChartRepository,
            IPersonBaseRepository personBaseRepository,
            IPositionRepository positionRepository,
            IReferenceDataRepository referenceDataRepository,
            ILogger logger)
        {
            _logger = logger;
            _organizationalChartRepository = organizationalChartRepository;
            _personBaseRepository = personBaseRepository;
            _positionRepository = positionRepository;
            _referenceDataRepository = referenceDataRepository;
        }
        public async Task<IEnumerable<OrgChartEmployee>> GetOrganizationalChartEmployeesAsync(string rootEmployeeId)
        {
            var orgChartNodes = await _organizationalChartRepository.GetActiveOrgChartEmployeesAsync(rootEmployeeId);
            var orgChartEmployees = new List<OrgChartEmployee>();
            foreach (var orgChartNode in orgChartNodes)
            {
                try
                {
                    var employeePersonData = await _personBaseRepository.GetPersonBaseAsync(orgChartNode.EmployeeId);
                    var positionData = await _positionRepository.GetPositionByIdAsync(orgChartNode.PositionCode);
                    string locationDescription = null;
                    if (!string.IsNullOrEmpty(positionData.PositionLocation))
                    {
                        if (_referenceDataRepository.Locations.Any(loc => loc.Code == positionData.PositionLocation))
                        {
                            locationDescription = _referenceDataRepository.Locations.Where(l => l.Code == positionData.PositionLocation).FirstOrDefault().Description;
                        }
                    }
                    if (employeePersonData != null && positionData != null)
                    {
                        var fullName = new OrgChartEmployeeName()
                        {
                            FirstName = employeePersonData.FirstName,
                            MiddleName = employeePersonData.MiddleName,
                            LastName = employeePersonData.LastName,
                            FullName = String.Format("{0} {1}", employeePersonData.FirstName, employeePersonData.LastName)
                        };

                        orgChartEmployees.Add(new OrgChartEmployee(
                            orgChartNode.PersonPositionId, 
                            orgChartNode.ParentPerposId, 
                            orgChartNode.ParentPersonId, 
                            orgChartNode.PositionCode, 
                            positionData.Title, 
                            employeePersonData.Id, 
                            positionData.PositionLocation, 
                            locationDescription,
                            fullName, 
                            orgChartNode.DirectReportCount));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, "Unable to build person data for this org chart employee.");
                }
            }
            return orgChartEmployees;
        }

        public async Task<OrgChartEmployee> GetOrganizationalChartEmployeeAsync(string rootEmployeeId)
        {
            var orgChartNode = await _organizationalChartRepository.GetActiveOrgChartEmployeeAsync(rootEmployeeId);
            try
            {
                var employeePersonData = await _personBaseRepository.GetPersonBaseAsync(orgChartNode.EmployeeId);
                var positionData = await _positionRepository.GetPositionByIdAsync(orgChartNode.PositionCode);
                string locationDescription = null;
                if (!string.IsNullOrEmpty(positionData.PositionLocation))
                {
                    if (_referenceDataRepository.Locations.Any(loc => loc.Code == positionData.PositionLocation)) {
                        locationDescription = _referenceDataRepository.Locations.Where(l => l.Code == positionData.PositionLocation).FirstOrDefault().Description;
                    }
                }
                if (employeePersonData != null && positionData != null)
                {
                    var fullName = new OrgChartEmployeeName()
                    {
                        FirstName = employeePersonData.FirstName,
                        MiddleName = employeePersonData.MiddleName,
                        LastName = employeePersonData.LastName,
                        FullName = String.Format("{0} {1}", employeePersonData.FirstName, employeePersonData.LastName)
                    };
                    return new OrgChartEmployee(
                        orgChartNode.PersonPositionId, 
                        orgChartNode.ParentPerposId, 
                        orgChartNode.ParentPersonId, 
                        orgChartNode.PositionCode, 
                        positionData.Title, 
                        employeePersonData.Id, 
                        positionData.PositionLocation, 
                        locationDescription,
                        fullName, 
                        orgChartNode.DirectReportCount);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, "Unable to build person data for this org chart employee.");
            }
            return new OrgChartEmployee();
        }
    }

}
