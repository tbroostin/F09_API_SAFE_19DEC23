// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    /// <summary>
    /// Coordination service for person agreements
    /// </summary>
    public class PersonAgreementsService : BaseCoordinationService, IPersonAgreementsService
    {
        private IAgreementsRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAgreementsService"/> class.
        /// </summary>
        /// <param name="repository">The agreements repository.</param>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        public PersonAgreementsService(IAgreementsRepository repository,
            IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get <see cref="PersonAgreement"/>person agreements</see> by person ID
        /// </summary>
        /// <param name="id">Person identifier</param>
        /// <returns>Collection of person agreements for a given person</returns>
        public async Task<IEnumerable<PersonAgreement>> GetPersonAgreementsByPersonIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id", "A person ID is required to retrieve person agreements by person ID.");
            }
            CheckIfUserIsSelf(id);
            try
            {
                var entities = await _repository.GetPersonAgreementsByPersonIdAsync(id);
                var dtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement, PersonAgreement>();
                var dtos = new List<PersonAgreement>();
                foreach (var entity in entities)
                {
                    dtos.Add(dtoAdapter.MapToType(entity));
                }
                return dtos;
            }
            catch (Exception ex)
            {
                string message = string.Format("An error occurred while retrieving person agreements for person {0}.", id);
                logger.Error(ex, message);
                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Updates a <see cref="PersonAgreement">person agreement</see>
        /// </summary>
        /// <param name="agreement">The <see cref="PersonAgreement">person agreement</see> to update</param>
        /// <returns>An updated <see cref="PersonAgreement">person agreement</see></returns>
        public async Task<PersonAgreement> UpdatePersonAgreementAsync(PersonAgreement agreement)
        {
            if (agreement == null)
            {
                throw new ArgumentNullException("agreement", "A person agreement is required when updating a person agreement.");
            }
            if (string.IsNullOrEmpty(agreement.Id))
            {
                throw new ArgumentNullException("agreement.Id", "A person agreement ID is required when updating a person agreement.");
            }
            if (string.IsNullOrEmpty(agreement.PersonId))
            {
                throw new ArgumentNullException("agreement.PersonId", "A person ID is required when updating a person agreement.");
            }
            if (!agreement.Status.HasValue)
            {
                throw new ArgumentNullException("agreement.Status", "A person agreement status is required when updating a person agreement.");
            }
            if (agreement.ActionTimestamp == null)
            {
                throw new ArgumentNullException("agreement.ActionTimestamp", "A timestamp is required when updating a person agreement.");
            }
            if (string.IsNullOrEmpty(agreement.Title))
            {
                throw new ArgumentNullException("agreement.Title", "A person agreement title is required when updating a person agreement.");
            }

            CheckIfUserIsSelf(agreement.PersonId);
            try
            {
                // Convert DTO to domain object
                var dtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Dtos.Base.PersonAgreement, Ellucian.Colleague.Domain.Base.Entities.PersonAgreement>();
                var entity = dtoAdapter.MapToType(agreement);

                // Call the repository to process the update
                var updatedAgreement = await _repository.UpdatePersonAgreementAsync(entity);

                // Convert updated agreement to DTO
                var entityAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonAgreement, Ellucian.Colleague.Dtos.Base.PersonAgreement>();
                return entityAdapter.MapToType(updatedAgreement);
            }
            catch (Exception ex)
            {
                string message = string.Format("An error occurred while updating person agreement {0} for person {1}.", agreement.Id, agreement.PersonId);
                logger.Error(ex, message);
                throw new ApplicationException(message);
            }
        }
    }
}
