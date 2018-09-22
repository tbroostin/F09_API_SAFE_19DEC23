/* Copyright 2017 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class FacultyContractService : BaseCoordinationService, IFacultyContractService
    {

        private IFacultyContractDomainService _facultyContractDomainService;
        public FacultyContractService(IFacultyContractDomainService facultyContractDomainService, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository, ILogger logger, IStaffRepository staffRepository = null, IConfigurationRepository configurationRepository = null) :
            base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            _facultyContractDomainService = facultyContractDomainService;
        }

        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.FacultyContract>> GetFacultyContractsByFacultyIdAsync(string facultyId)
        {
            if (string.IsNullOrEmpty(facultyId))
            {
                throw new ArgumentNullException("facultyIds", "facultyIds cannot be null or empty");
            }
           
                if (CurrentUser.PersonId != facultyId)
                {
                    throw new PermissionsException("User does not have permission to retrieve faculty ID " + facultyId);
                }

            var facultyContractEntities = await _facultyContractDomainService.GetFacultyContractsByFacultyIdAsync(facultyId);

            var facultyContractAdapter = _adapterRegistry.GetAdapter<Domain.HumanResources.Entities.FacultyContract, Dtos.Base.FacultyContract>();
            var facultyContractDtos = new List<Dtos.Base.FacultyContract>();

            foreach (var facultyContractEntity in facultyContractEntities)
            {
                facultyContractDtos.Add(facultyContractAdapter.MapToType(facultyContractEntity));
            }
            return facultyContractDtos;
        }

       
    }
}
