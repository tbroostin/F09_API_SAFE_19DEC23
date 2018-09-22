// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for proxy service
    /// </summary>
    public interface IProxyService
    {
        /// <summary>
        /// Creates or updates the proxy permissions for a proxy subject
        /// </summary>
        /// <param name="assignment">A proxy permission assignment object</param>
        /// <returns>A collection of proxy access permissions</returns>
        Task<IEnumerable<ProxyAccessPermission>> PostUserProxyPermissionsAsync(ProxyPermissionAssignment assignment);

        /// <summary>
        /// Gets the proxy configuration
        /// </summary>
        /// <returns>The proxy configuration</returns>
        Task<Dtos.Base.ProxyConfiguration> GetProxyConfigurationAsync();

        /// <summary>
        /// Gets a collection of proxy access permissions, by user, for the supplied person
        /// </summary>
        /// <param name="id">The identifier of the entity of interest</param>
        /// <returns>A collection of proxy access permissions, by user, for the supplied person</returns>
        Task<IEnumerable<Dtos.Base.ProxyUser>> GetUserProxyPermissionsAsync(string id);


        /// <summary>
        /// Gets a collection of proxy subjects who have granted access to the current user.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Dtos.Base.ProxySubject>> GetUserProxySubjectsAsync(string proxyPersonId);

        /// <summary>
        /// Creates a proxy candidate
        /// </summary>
        /// <param name="candidate">A <see cref="ProxyCandidate" /> containing the information to post to the database</param>
        /// <returns>The <see cref="ProxyCandidate"/> record that was created.</returns>
        Task<ProxyCandidate> PostProxyCandidateAsync(ProxyCandidate candidate);

        /// <summary>
        /// Gets a collection of proxy candidates that the proxy user has submitted for evaluation.
        /// </summary>
        /// <param name="grantorId">ID of the user granting access</param>
        /// <returns>A collection of proxy candidates</returns>
        Task<IEnumerable<Dtos.Base.ProxyCandidate>> GetUserProxyCandidatesAsync(string grantorId);

        /// <summary>
        /// Creates a person for the purposes of becoming a proxy user
        /// </summary>
        /// <param name="user">The <see cref="PersonProxyUser"/> to create</param>
        /// <returns>The created <see cref="personProxyUser"/></returns>
        Task<PersonProxyUser> PostPersonProxyUserAsync(PersonProxyUser user);
    }
}
