// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// The interface for a PersonProxyUserRepository
    /// </summary>
    public interface IPersonProxyUserRepository
    {
        /// <summary>
        /// Creates a person for the purpose of becoming a proxy user
        /// </summary>
        /// <param name="person">The <see cref="PersonProxyUser"> person</see> to create</param>
        /// <returns>The created <see cref="PersonProxyUser"> person </see></returns>
        Task<PersonProxyUser> CreatePersonProxyUserAsync(PersonProxyUser person);
    }
}