/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.Base.Services;
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
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class EmploymentClassificationService : BaseCoordinationService, IEmploymentClassificationService
    {

        private readonly IHumanResourcesReferenceDataRepository _hrReferenceDataRepository;

        public EmploymentClassificationService(

            IHumanResourcesReferenceDataRepository hrReferenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IConfigurationRepository configurationRepository,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {

            _hrReferenceDataRepository = hrReferenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 7</remarks>
        /// <summary>
        /// Gets all employee classifications
        /// </summary>
        /// <returns>Collection of EmployeeClassifications DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmploymentClassification>> GetEmploymentClassificationsAsync(bool bypassCache = false)
        {
            var employeeClassificationCollection = new List<Ellucian.Colleague.Dtos.EmploymentClassification>();

            var employeeClassificationEntities = await _hrReferenceDataRepository.GetEmploymentClassificationsAsync(bypassCache);
            if (employeeClassificationEntities != null && employeeClassificationEntities.Any())
            {
                foreach (var employeeClassification in employeeClassificationEntities)
                {
                    employeeClassificationCollection.Add(ConvertEmployeeClassificationEntityToDto(employeeClassification));
                }
            }
            return employeeClassificationCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 7</remarks>
        /// <summary>
        /// Get a employee classification from its GUID
        /// </summary>
        /// <returns>EmployeeClassification DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmploymentClassification> GetEmploymentClassificationByGuidAsync(string guid)
        {
            try
            {
                return ConvertEmployeeClassificationEntityToDto((await _hrReferenceDataRepository.GetEmploymentClassificationsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Employee classification not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a EmployeeClassification domain entity to its corresponding EmployeeClassification DTO
        /// </summary>
        /// <param name="source">EmployeeClassification domain entity</param>
        /// <returns>EmployeeClassification DTO</returns>
        private Ellucian.Colleague.Dtos.EmploymentClassification ConvertEmployeeClassificationEntityToDto(EmploymentClassification source)
        {
            var employeeClassification = new Ellucian.Colleague.Dtos.EmploymentClassification();

            employeeClassification.Id = source.Guid;
            employeeClassification.Code = source.Code;
            employeeClassification.Title = source.Description;
            employeeClassification.Description = null;
            employeeClassification.employeeClassificationType = ConvertEmployeeClassificationTypeDomainEnumToEmployeeClassificationTypeDtoEnum(source.EmploymentClassificationType);

            return employeeClassification;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a EmployeeClassificationType domain enumeration value to its corresponding EmployeeClassificationType DTO enumeration value
        /// </summary>
        /// <param name="source">EmployeeClassificationType domain enumeration value</param>
        /// <returns>EmployeeClassificationType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EmploymentClassificationType ConvertEmployeeClassificationTypeDomainEnumToEmployeeClassificationTypeDtoEnum(EmploymentClassificationType source)
        {
            switch (source)
            {
                case EmploymentClassificationType.Position:
                    return Dtos.EmploymentClassificationType.Position;
                case EmploymentClassificationType.Employee:
                    return Dtos.EmploymentClassificationType.Employee;
                default:
                    return Dtos.EmploymentClassificationType.Position;
            }
        }

    }
}