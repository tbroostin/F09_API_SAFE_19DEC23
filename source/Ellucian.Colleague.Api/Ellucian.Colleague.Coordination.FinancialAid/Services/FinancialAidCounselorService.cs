//Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// FinancialAidCounselorService class
    /// </summary>
    [RegisterType]
    public class FinancialAidCounselorService : FinancialAidCoordinationService, IFinancialAidCounselorService
    {
        private readonly IStaffRepository staffRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Constructor for the FinancialAidCounselorService
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="staffRepository">StaffRepository</param>
        /// <param name="currentUserFactory">CurrentUserFactory</param>
        /// <param name="roleRepository">RoleRepository</param>
        /// <param name="logger">Logger</param>
        public FinancialAidCounselorService(
            IAdapterRegistry adapterRegistry,
            IStaffRepository staffRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.staffRepository = staffRepository;
            this.configurationRepository = configurationRepository;
        }


        /// <summary>
        /// Get a FinancialAidCounselor DTO for the given counselorId
        /// </summary>
        /// <param name="counselorId">The Colleague PERSON id of the counselor to get</param>
        /// <returns>FinancialAidCounselor DTO</returns>
        /// <exception cref="ArgumentNullException">Thrown if the counselorId is null or empty</exception>
        /// <exception cref="ApplicationException">Thrown if the StaffRepository returns null, or the requested staff is not active</exception>
        public FinancialAidCounselor GetCounselor(string counselorId)
        {
            if (string.IsNullOrEmpty(counselorId))
            {
                throw new ArgumentNullException("counselorId");
            }

            Colleague.Domain.Base.Entities.Staff staffEntity = null;
            try
            {
                staffEntity = staffRepository.Get(counselorId);
                if (staffEntity == null || !staffEntity.IsActive)
                {
                    throw new ApplicationException(string.Format("Counselor {0} is not a valid staff member", counselorId));
                }
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw new ApplicationException(e.Message);
            }

            var counselorAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Staff, FinancialAidCounselor>();
            return counselorAdapter.MapToType(staffEntity);
        }

        /// <summary>
        /// Get a list of Financial Aid counselors by ids. Returns Name, email address, and Id.
        /// Use for name identification only. 
        /// </summary>
        /// <param name="counselorIds">ids of fa counselors to retrieve</param>
        /// <returns>A List of FinancialAidCounselorDtos</returns>
        public async Task<IEnumerable<FinancialAidCounselor>> GetCounselorsByIdAsync(IEnumerable<string> counselorIds)
        {
            List<FinancialAidCounselor> counselors = new List<FinancialAidCounselor>();
            IEnumerable<Colleague.Domain.Base.Entities.Staff> counselorEntities;
            var counselorAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Staff, FinancialAidCounselor>();
            try
            {
                counselorEntities = await Task.Run(() => staffRepository.Get(counselorIds));
                if (counselorEntities == null)
                {
                    throw new ApplicationException("No Financial Aid counselors were found with provided ids.");

                }
                foreach (var counselor in counselorEntities)
                {
                    if (!counselor.IsActive)
                    {
                        logger.Debug(string.Format("Counselor {0} is not a valid staff member", counselor.Id));
                    }
                    else
                    {
                        counselors.Add(counselorAdapter.MapToType(counselor));
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw new ApplicationException(e.Message);
            }
            
            return counselors;
        }
    }
}
