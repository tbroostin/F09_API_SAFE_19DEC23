// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Service for proxy functionality
    /// </summary>
    [RegisterType]
    public class ProxyService : BaseCoordinationService, IProxyService
    {
        private IProxyRepository _proxyRepository;
        private IProfileRepository _profileRepository;
        private IPersonProxyUserRepository _personProxyUserRepository;


        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyService"/> class.
        /// </summary>
        /// <param name="proxyRepository">The proxy repository.</param>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        public ProxyService(IProxyRepository proxyRepository, IProfileRepository profileRepository, IPersonProxyUserRepository personProxyUserRepository,
            IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            _proxyRepository = proxyRepository;
            _profileRepository = profileRepository;
            _personProxyUserRepository = personProxyUserRepository;
        }

        /// <summary>
        /// Creates or updates the proxy permissions for a proxy subject
        /// </summary>
        /// <param name="assignment">A proxy permission assignment object</param>
        /// <param name="useEmployeeGroups">Optional parameter used to differentiate between employee proxy and person proxy</param>
        /// <returns>A collection of proxy access permissions</returns>
        public async Task<IEnumerable<ProxyAccessPermission>> PostUserProxyPermissionsAsync(ProxyPermissionAssignment assignment, bool useEmployeeGroups = false)
        {
            if (assignment == null)
            {
                string message = "Assignment cannot be null";
                logger.Error(message);
                throw new ArgumentNullException(message);
            }
            else
            {
                if (!CurrentUser.IsPerson(assignment.ProxySubjectId))
                {
                    string message = "User " + assignment.ProxySubjectId + " does not have sufficient privileges to update proxy subjects.";
                    logger.Error(message);
                    throw new PermissionsException(message);
                }
                else
                {
                    var assignmentAdapter = _adapterRegistry.GetAdapter<ProxyPermissionAssignment, Domain.Base.Entities.ProxyPermissionAssignment>();
                    var assignmentEntity = assignmentAdapter.MapToType(assignment);

                    if (useEmployeeGroups)
                    {
                        // Set the IsAssigned from the input ProxyPermissionAssignment dto.
                        for (int i= 0; i < assignment.Permissions.Count; i++)
                        {
                            assignmentEntity.Permissions[i].IsGranted = assignment.Permissions[i].IsGranted;
                        }
                    }

                    // Reject the request if a deceased user is granted a permission.
                    var usersToCheck = assignmentEntity.Permissions.Where(p => p.IsGranted).Select(p => p.ProxyUserId).Distinct().ToList();
                    foreach (var proxyUserId in usersToCheck)
                    {
                        var profileEntity = await _profileRepository.GetProfileAsync(proxyUserId);
                        if (profileEntity.IsDeceased)
                        {
                            string message = "Cannot assign proxy permissions to a deceased person " + proxyUserId + ".";
                            logger.Error(message);
                            throw new InvalidOperationException(message);
                        }
                    }
                    var entities = await _proxyRepository.PostUserProxyPermissionsAsync(assignmentEntity, useEmployeeGroups);
                    var entityAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.ProxyAccessPermission, ProxyAccessPermission>();
                    var dtos = new List<ProxyAccessPermission>();
                    foreach (var entity in entities)
                    {
                        dtos.Add(entityAdapter.MapToType(entity));
                    }

                    return dtos;
                }
            }
        }

        /// <summary>
        /// Gets the proxy configuration
        /// </summary>
        /// <returns>The proxy configuration</returns>
        public async Task<Dtos.Base.ProxyConfiguration> GetProxyConfigurationAsync()
        {
            var proxyConfig = await _proxyRepository.GetProxyConfigurationAsync();

            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.ProxyConfiguration, ProxyConfiguration>();

            return adapter.MapToType(proxyConfig);
        }

        /// <summary>
        /// Gets a collection of proxy access permissions, by user, for the supplied person
        /// </summary>
        /// <param name="id">The identifier of the entity of interest</param>
        /// <param name="useEmployeeGroups">Optional parameter used to differentiate between employee proxy and person proxy</param>
        /// <returns>A collection of proxy access permissions, by user, for the supplied person</returns>
        public async Task<IEnumerable<Dtos.Base.ProxyUser>> GetUserProxyPermissionsAsync(string id, bool useEmployeeGroups = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }
            if (!CurrentUser.IsPerson(id))
            {
                throw new PermissionsException("User does not have sufficient privileges to retrieve proxy access permissions for person " + id);
            }

            var dtos = new List<Dtos.Base.ProxyUser>();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.ProxyUser, ProxyUser>();

            var users = await _proxyRepository.GetUserProxyPermissionsAsync(id, useEmployeeGroups);
            foreach (var user in users)
            {
                dtos.Add(adapter.MapToType(user));
            }
            return dtos;
        }

        /// <summary>
        /// Gets a collection of proxy subjects who have granted access to the proxy user.
        /// </summary>
        /// <param name="proxyPersonId">the proxy user person ID</param>
        /// <returns></returns>
        public async Task<IEnumerable<Dtos.Base.ProxySubject>> GetUserProxySubjectsAsync(string proxyPersonId)
        {
            if (string.IsNullOrEmpty(proxyPersonId))
            {
                throw new ArgumentNullException("proxyPersonId");
            }
            if (!CurrentUser.IsPerson(proxyPersonId))
            {
                throw new PermissionsException(
                    string.Format("User {0} does not have sufficient privileges to retrieve proxy subjects.", proxyPersonId));
            }
            var dtos = new List<Dtos.Base.ProxySubject>();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.ProxySubject,
                Ellucian.Colleague.Dtos.Base.ProxySubject>();

            var proxySubjects = await _proxyRepository.GetUserProxySubjectsAsync(proxyPersonId);

            foreach (var proxySubject in proxySubjects)
            {
                var proxySubjectProfile = await _profileRepository.GetProfileAsync(proxySubject.Id);
                proxySubject.FullName = proxySubjectProfile.PreferredName;

                dtos.Add(adapter.MapToType(proxySubject));
            }
            return dtos;
        }

        /// <summary>
        /// Create a proxy candidate record in the database
        /// </summary>
        /// <param name="candidate">the <see cref="ProxyCandidate"/> record to create</param>
        /// <returns>The created <see cref="ProxyCandidate"/> record</returns>
        public async Task<ProxyCandidate> PostProxyCandidateAsync(ProxyCandidate candidate)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException("candidate");
            }
            if (!CurrentUser.IsPerson(candidate.ProxySubject))
            {
                throw new PermissionsException(
                    string.Format("User {0} does not have sufficient privileges to create proxy candidate.", candidate.ProxySubject));
            }
            var entAdapter = _adapterRegistry.GetAdapter<Dtos.Base.ProxyCandidate, Domain.Base.Entities.ProxyCandidate>();
            var dtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.ProxyCandidate, Dtos.Base.ProxyCandidate>();

            var candidateEntity = entAdapter.MapToType(candidate);
            var resultEntity = await _proxyRepository.PostProxyCandidateAsync(candidateEntity);
            var result = dtoAdapter.MapToType(resultEntity);
            return result;
        }

        /// <summary>
        /// Gets a collection of proxy candidates that the proxy user has submitted for evaluation.
        /// </summary>
        /// <param name="grantorId">ID of the user granting access</param>
        /// <returns>A collection of proxy candidates</returns>
        public async Task<IEnumerable<Dtos.Base.ProxyCandidate>> GetUserProxyCandidatesAsync(string grantorId)
        {
            if (string.IsNullOrEmpty(grantorId))
            {
                throw new ArgumentNullException("proxyPersonId");
            }
            if (!CurrentUser.IsPerson(grantorId))
            {
                throw new PermissionsException(
                    string.Format("User {0} does not have sufficient privileges to retrieve proxy candidates.", grantorId));
            }
            var dtos = new List<Dtos.Base.ProxyCandidate>();
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.ProxyCandidate,
                Ellucian.Colleague.Dtos.Base.ProxyCandidate>();

            var proxyCandidates = await _proxyRepository.GetUserProxyCandidatesAsync(grantorId);

            foreach (var proxyCandidate in proxyCandidates)
            {
                dtos.Add(adapter.MapToType(proxyCandidate));
            }
            return dtos;
        }

        /// <summary>
        /// Creates a person for the purposes of becoming a proxy user
        /// </summary>
        /// <param name="user">The <see cref="PersonProxyUser"/> to create</param>
        /// <returns>The created <see cref="personProxyUser"/></returns>
        public async Task<PersonProxyUser> PostPersonProxyUserAsync(PersonProxyUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            var proxyConfig = await _proxyRepository.GetProxyConfigurationAsync();
            if (proxyConfig == null)
            {
                throw new Exception("Unable to create proxy user due to missing proxy configuration.");
            }
            if (!proxyConfig.CanAddOtherUsers)
            {
                throw new Exception("Unable to create proxy user. Feature disabled in proxy configuration settings.");
            }
            var entAdapter = _adapterRegistry.GetAdapter<Dtos.Base.PersonProxyUser, Domain.Base.Entities.PersonProxyUser>();
            var dtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonProxyUser, Dtos.Base.PersonProxyUser>();

            var userEntity = entAdapter.MapToType(user);
            var resultEntity = await _personProxyUserRepository.CreatePersonProxyUserAsync(userEntity);
            var result = dtoAdapter.MapToType(resultEntity);
            return result;
        }

    }
}
