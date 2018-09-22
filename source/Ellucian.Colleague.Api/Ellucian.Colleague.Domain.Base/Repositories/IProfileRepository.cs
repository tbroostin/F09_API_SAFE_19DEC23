// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IProfileRepository
    {
        /// <summary>
        /// Gets Profile person: Base person with addresses and phones
        /// </summary>
        /// <param name="personId">Id of the person to retrieve</param>
        /// <param name="useCache">Indicates whether to use cache if available</param>
        /// <returns>Person <see cref="Profile">Profile</see> object</returns>
        Task<Domain.Base.Entities.Profile> GetProfileAsync(string personId, bool useCache = true);
        
        /// <summary>
        /// Gets person's current addresses
        /// </summary>
        /// <param name="personId">Id of the person</param>
        /// <param name="usePersonCache">Indicates whether to use person cache if available</param>
        /// <param name="useAddressCache">Indicates whether to use address cache if available</param>
        /// <returns>List of <see cref="Address">Address</see> objects</returns>
        Task<IEnumerable<Address>> GetPersonAddressesAsync(string personId, bool usePersonCache = true, bool useAddressCache = true);

        /// <summary>
        /// Gets person's current phones
        /// </summary>
        /// <param name="personId">Id of the person</param>
        /// <param name="usePersonCache">Indicates whether to use person cache if available</param>
        /// <param name="useAddressCache">Indicates whether to use address cache if available</param>
        /// <returns>List of <see cref="Phone">Phone</see> objects</returns>
        Task<PhoneNumber> GetPersonPhonesAsync(string personId, bool usePersonCache = true, bool useAddressCache = true);

        /// <summary>
        /// Updates the profile.
        /// </summary>
        /// <param name="profile">The profile entity to replace in the database</param>
        /// <returns>The newly updated <see cref="Profile">Profile</see> entity</returns>
        Task<Profile> UpdateProfileAsync(Profile profile);
    }
}
