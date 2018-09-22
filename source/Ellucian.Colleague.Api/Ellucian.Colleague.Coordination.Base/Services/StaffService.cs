// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class StaffService : BaseCoordinationService, IStaffService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IPersonRestrictionRepository _personRestrictionRepository;

        public StaffService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         IStaffRepository staffRepository,
                                         IPersonRestrictionRepository personRestrictionRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _referenceDataRepository = referenceDataRepository;
            _staffRepository = staffRepository;
            _personRestrictionRepository = personRestrictionRepository;
        }

        public async Task<Dtos.Base.Staff> GetAsync(string staffId)
        {
            if (!CurrentUser.IsPerson(staffId))
            {
                throw new PermissionsException("User does not have permissions to access this information");
            }

            try
            {
                Domain.Base.Entities.Staff staff = await _staffRepository.GetAsync(staffId);
                
                if (staff == null)
                {
                    throw new ApplicationException("Staff Id is not a valid staff member.");
                }

                // Get the right adapter for the type mapping
                var staffAdapter = new StaffEntityAdapter(_adapterRegistry, logger);

                var staffDto = staffAdapter.MapToType(staff);

                return staffDto;
            }
            catch (Exception)
            {
                throw new ApplicationException("Staff Id is not a valid staff member.");
            }
        }

        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PersonRestriction>> GetStaffRestrictionsAsync(string staffId)
        {
            if (!CurrentUser.IsPerson(staffId))
            {
                throw new PermissionsException("User does not have permissions to access this information");
            }

            List<Dtos.Base.PersonRestriction> results = new List<Dtos.Base.PersonRestriction>();
            try
            {
                Domain.Base.Entities.Staff staff = _staffRepository.Get(staffId);
                // Only return restrictions for active staff members - former staff should not see restrictions
                if (staff == null || !staff.IsActive)
                {
                    throw new ApplicationException("Staff Id is not a valid staff member.");
                }
            }
            catch (Exception)
            {
                throw new ApplicationException("Staff Id is not a valid staff member.");
            }

            DateTime today = DateTime.Now;
            IEnumerable<Domain.Base.Entities.PersonRestriction> personRestrictions = (await _personRestrictionRepository.GetAsync(staffId)).Where(sr => sr.OfficeUseOnly == false && (sr.StartDate.HasValue && sr.StartDate.Value <= today) && (sr.EndDate.HasValue == false || (sr.EndDate.HasValue && sr.EndDate.Value > today)));
            IEnumerable<Restriction> restrictions = await _referenceDataRepository.RestrictionsAsync();

            // Get the right adapter for the type mapping
            var personRestrictionAdapter = new PersonRestrictionEntityAdapter(_adapterRegistry, logger);

            // Create the PersonRestriction DTO which is a blend of PersonRestriction info and Restriction info.
            foreach (var personRestriction in personRestrictions)
            {
                Restriction restriction = restrictions.Where(r => r.Code == personRestriction.RestrictionId).FirstOrDefault();
                if (restriction != null && restriction.OfficeUseOnly == false)
                {
                    var personRestrictionDto = personRestrictionAdapter.MapToType(personRestriction, restriction);
                    results.Add(personRestrictionDto);
                }
            }

            return results.AsEnumerable();
        }
    }
}
