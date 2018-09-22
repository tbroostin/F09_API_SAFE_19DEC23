// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Collections.Generic;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Accesses Colleague for a person's proxy information.
    /// </summary>
    public interface IProxyRepository
    {
        /// <summary>
        /// Posts a user's grants and revocations for proxy access.
        /// </summary>
        /// <param name="assignment">Proxy permission assignment object</param>
        Task<IEnumerable<ProxyAccessPermission>> PostUserProxyPermissionsAsync(ProxyPermissionAssignment assignment);

        /// <summary>
        /// Get the proxy configuration.
        /// </summary>
        /// <returns>The <see cref="ProxyConfiguration">proxy configuration</see></returns>
        Task<ProxyConfiguration> GetProxyConfigurationAsync();

        /// <summary>
        /// Gets a collection of proxy access permissions, by user, for the supplied person
        /// </summary>
        /// <param name="id">The identifier of the entity of interest</param>
        /// <returns>A collection of proxy access permissions, by user, for the supplied person</returns>
        Task<IEnumerable<ProxyUser>> GetUserProxyPermissionsAsync(string id);


        /// <summary>
        /// Gets all the proxy subjects who have granted the proxy user permissions.
        /// </summary>
        /// <returns>A collection of proxy subjects</returns>
        Task<IEnumerable<ProxySubject>> GetUserProxySubjectsAsync(string proxyPersonId);

        /// <summary>
        /// Posts demographic information and people possibly meeting that information, for resolution; as well as the permissions to grant to the resolved person
        /// </summary>
        /// <param name="candidate">A <see cref="ProxyCandidate"/> containing the information to post</param>
        /// <returns>The created <see cref="ProxyCandidate"/></returns>
        Task<ProxyCandidate> PostProxyCandidateAsync(ProxyCandidate candidate);

        /// <summary>
        /// Gets all the proxy candidates for a proxy user.
        /// </summary>
        /// <param name="grantorId">ID of the user granting access</param>
        /// <returns>
        /// A collection of proxy candidates
        /// </returns>
        Task<IEnumerable<Domain.Base.Entities.ProxyCandidate>> GetUserProxyCandidatesAsync(string grantorId);
    }
}
