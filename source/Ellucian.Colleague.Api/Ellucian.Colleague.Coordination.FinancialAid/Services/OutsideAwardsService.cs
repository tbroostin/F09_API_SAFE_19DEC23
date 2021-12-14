/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Dependency;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// OutsideAwards coordination service
    /// </summary>
    [RegisterType]
    public class OutsideAwardsService : FinancialAidCoordinationService, IOutsideAwardsService
    {
        private readonly IOutsideAwardsRepository outsideAwardsRepository;
        private readonly IConfigurationRepository configurationRepository;

        /// <summary>
        /// Instantiate a new OutsideAwardService
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="outsideAwardsRepository"></param>
        /// <param name="currentUserFactory"></param>
        /// <param name="roleRepository"></param>
        /// <param name="logger"></param>
        public OutsideAwardsService(IAdapterRegistry adapterRegistry, IOutsideAwardsRepository outsideAwardsRepository, IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(configurationRepository, adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.outsideAwardsRepository = outsideAwardsRepository;
            this.configurationRepository = configurationRepository;
        }

        /// <summary>
        /// Creates and returns an outside award dto
        /// </summary>
        /// <param name="outsideAward">input outside award dto</param>
        /// <returns>created outside award dto</returns>
        public async Task<OutsideAward> CreateOutsideAwardAsync(OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                throw new ArgumentNullException("outsideAward");
            }            
            if (string.IsNullOrEmpty(outsideAward.StudentId))
            {
                throw new ArgumentException("Outside award student id is required");
            }
            if (string.IsNullOrEmpty(outsideAward.AwardYearCode))
            {
                throw new ArgumentException("Outside award year code is required");
            }
            if (string.IsNullOrEmpty(outsideAward.AwardName))
            {
                throw new ArgumentException("Outside award name is required");
            }
            if (string.IsNullOrEmpty(outsideAward.AwardType))
            {
                throw new ArgumentException("Outside award type is required");
            }
            if (outsideAward.AwardAmount <= 0)
            {
                throw new ArgumentException("Outside award amount cannot be less than or equal to 0");
            } 
            if (string.IsNullOrEmpty(outsideAward.AwardFundingSource))
            {
                throw new ArgumentException("Outside award funding source is required");
            }
            if (!UserIsSelf(outsideAward.StudentId))
            {
                var message = string.Format("{0} does not have permission to create outside award resource for {1}", CurrentUser.PersonId, outsideAward.StudentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            var outsideAwardDtoToEntityAdapter = _adapterRegistry.GetAdapter<OutsideAward, Colleague.Domain.FinancialAid.Entities.OutsideAward>();
            
            var createdOutsideAward = await outsideAwardsRepository.CreateOutsideAwardAsync(outsideAwardDtoToEntityAdapter.MapToType(outsideAward));
            if (createdOutsideAward == null)
            {
                string message = string.Format("Could not create an outside award record for student {0} award year {1}", outsideAward.StudentId, outsideAward.AwardYearCode);
                logger.Error(message);
                throw new Exception(message);
            }
            var outsideAwardEntityToDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward, OutsideAward>();

            return outsideAwardEntityToDtoAdapter.MapToType(createdOutsideAward);
        }

        /// <summary>
        /// Gets outside awards for the specified student id and award year code
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <returns>List of OutsideAward DTOs</returns>
        public async Task<IEnumerable<OutsideAward>> GetOutsideAwardsAsync(string studentId, string awardYearCode)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(awardYearCode))
            {
                throw new ArgumentNullException("awardYearCode");
            }
            if (!UserHasAccessPermission(studentId))
            {
                var message = string.Format("{0} does not have permission to get outside awards for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
            List<OutsideAward> outsideAwardDtos = new List<OutsideAward>();
            AutoMapperAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward, OutsideAward> outsideAwardEntityToDtoAdapter =
                new AutoMapperAdapter<Domain.FinancialAid.Entities.OutsideAward, OutsideAward>(_adapterRegistry, logger);

            IEnumerable<Colleague.Domain.FinancialAid.Entities.OutsideAward> outsideAwardEntities = await outsideAwardsRepository.GetOutsideAwardsAsync(studentId, awardYearCode);

            foreach (var entity in outsideAwardEntities)
            {
                outsideAwardDtos.Add(outsideAwardEntityToDtoAdapter.MapToType(entity));
            }

            return outsideAwardDtos;
        }

        /// <summary>
        /// Deletes an outside award record with the specified id
        /// </summary>
        /// <param name="studentId">student id record belongs to</param>
        /// <param name="outsideAwardId">outside award record id</param>
        /// <returns></returns>
        public async Task DeleteOutsideAwardAsync(string studentId, string outsideAwardId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId");
            }
            if (string.IsNullOrEmpty(outsideAwardId))
            {
                throw new ArgumentNullException("outsideAwardId");
            }
            if (!UserIsSelf(studentId))
            {
                var message = string.Format("{0} does not have permission to delete outside award resource for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);            
            }

            var outsideAward = await outsideAwardsRepository.GetOutsideAwardsByAwardIdAsync(outsideAwardId);

            if (outsideAward.StudentId != studentId)
            {
                var message = string.Format("{0} does not have permission to delete outside award resource for {1}", CurrentUser.PersonId, studentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            await outsideAwardsRepository.DeleteOutsideAwardAsync(outsideAwardId);
        }

        /// <summary>
        /// Updates an outside award record with the specified information.
        /// </summary>
        /// <param name="outsideAward">record data to use to update.</param>
        /// <returns></returns>
        public async Task<OutsideAward> UpdateOutsideAwardAsync(OutsideAward outsideAward)
        {
            if (outsideAward == null)
            {
                throw new ArgumentNullException("outsideAward");
            }
            if (string.IsNullOrEmpty(outsideAward.Id))
            {
                throw new ArgumentException("Outside award record Id to update is null");
            }
            if (string.IsNullOrEmpty(outsideAward.StudentId))
            {
                throw new ArgumentException("Outside award student id is required");
            }
            if (string.IsNullOrEmpty(outsideAward.AwardYearCode))
            {
                throw new ArgumentException("Outside award year code is required");
            }
            if (string.IsNullOrEmpty(outsideAward.AwardName))
            {
                throw new ArgumentException("Outside award name is required");
            }
            if (string.IsNullOrEmpty(outsideAward.AwardType))
            {
                throw new ArgumentException("Outside award type is required");
            }
            if (outsideAward.AwardAmount <= 0)
            {
                throw new ArgumentException("Outside award amount cannot be less than or equal to 0");
            }
            if (string.IsNullOrEmpty(outsideAward.AwardFundingSource))
            {
                throw new ArgumentException("Outside award funding source is required");
            }

            if (!UserIsSelf(outsideAward.StudentId))
            {
                var message = string.Format("{0} does not have permission to create outside award resource for {1}", CurrentUser.PersonId, outsideAward.StudentId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var awardsBelongingToStudent = new List<Colleague.Domain.FinancialAid.Entities.OutsideAward>();
            var awardsForStudent = await outsideAwardsRepository.GetOutsideAwardsAsync(outsideAward.StudentId, outsideAward.AwardYearCode);
            foreach (var award in awardsForStudent)
            {
                if (award.Id == outsideAward.Id)
                {
                    awardsBelongingToStudent.Add(award);
                }
            }

            if (!awardsBelongingToStudent.Any())
            {
                var message = string.Format("{0} does not have permission to access to update the outside award for this resource.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }

            var outsideAwardDtoToEntityAdapter = _adapterRegistry.GetAdapter<OutsideAward, Colleague.Domain.FinancialAid.Entities.OutsideAward>();

            var updatedOutsideAward = await outsideAwardsRepository.UpdateOutsideAwardAsync(outsideAwardDtoToEntityAdapter.MapToType(outsideAward));

            if (updatedOutsideAward == null)
            {
                string message = string.Format("Could not update an outside award record for student {0} award year {1}", outsideAward.StudentId, outsideAward.AwardYearCode);
                logger.Error(message);
                throw new Exception(message);
            }
            var outsideAwardEntityToDtoAdapter = _adapterRegistry.GetAdapter<Colleague.Domain.FinancialAid.Entities.OutsideAward, OutsideAward>();

            return outsideAwardEntityToDtoAdapter.MapToType(updatedOutsideAward);
        }
    }
}
