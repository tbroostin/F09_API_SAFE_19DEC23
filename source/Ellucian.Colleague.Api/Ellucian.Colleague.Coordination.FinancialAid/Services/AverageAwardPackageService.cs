//Copyright 2014-2017 Ellucian Company L.P. and its affiliates
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    [RegisterType]
    public class AverageAwardPackageService : AwardYearCoordinationService, IAverageAwardPackageService
    {
        private readonly IAverageAwardPackageRepository averageAwardPackageRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor used by injection-framework. 
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry object</param>
        /// <param name="averageAwardPackageRepository">AverageAwardPackageRepository object</param>
        /// <param name="studentAwardYearRepository">StudentAwardYearRepository</param>
        /// <param name="financialAidOfficeRepository">FinancialAidOfficeRepository</param>
        /// <param name="currentUserFactory">CurrentUserFactory object</param>
        /// <param name="roleRepository">RoleRepository object</param>
        /// <param name="logger">Logger object</param>
        public AverageAwardPackageService(IAdapterRegistry adapterRegistry,
                                         IAverageAwardPackageRepository averageAwardPackageRepository,
                                         IStudentAwardYearRepository studentAwardYearRepository,
                                         IFinancialAidOfficeRepository financialAidOfficeRepository,
                                         IConfigurationRepository configurationRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, financialAidOfficeRepository, studentAwardYearRepository, configurationRepository, currentUserFactory, roleRepository, logger)
        {
            this.averageAwardPackageRepository = averageAwardPackageRepository;
            this.configurationRepository = configurationRepository;
        }
        
        /// <summary>
        /// Method to get award category averages information across all
        /// active student award years for display
        /// </summary>
        /// <param name="studentId">Student's system id</param>
        /// <returns>Set of average award package records</returns>
        public async Task<IEnumerable<AverageAwardPackage>> GetAverageAwardPackagesAsync(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }

            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to access average award package information for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var activeStudentAwardYears = await GetActiveStudentAwardYearEntitiesAsync(studentId);
            var averageAwardPackageDtos = new List<AverageAwardPackage>();

            // Get the Entity to DTO Adapter
            var averageAwardPackageDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.FinancialAid.Entities.AverageAwardPackage, AverageAwardPackage>();

            var averageAwardPackages = await averageAwardPackageRepository.GetAverageAwardPackagesAsync(studentId, activeStudentAwardYears);
            
            if (averageAwardPackages == null || averageAwardPackages.Count() == 0)
            {
                var message = "No AverageAwardPackages returned by repository";
                logger.Info(message);                
            }

            else
            {
                foreach (var averageAwardPackage in averageAwardPackages)
                {
                    if (averageAwardPackage == null)
                    {
                        var message = "Null AverageAwardPackage returned by repository";
                        logger.Info(message);
                    }
                    // Map to the Dto if present
                    else
                    {
                        averageAwardPackageDtos.Add(averageAwardPackageDtoAdapter.MapToType(averageAwardPackage));
                    }
                }
            }

            return averageAwardPackageDtos;
        }
    }
}
