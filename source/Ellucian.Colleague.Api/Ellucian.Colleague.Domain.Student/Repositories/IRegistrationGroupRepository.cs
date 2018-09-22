using Ellucian.Colleague.Domain.Student.Entities;
// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for the RegistrationUserRepository
    /// </summary>
    public interface IRegistrationGroupRepository
    {
        /// <summary>
        /// Returns the registration user IDs for a specific person ID.
        /// </summary>
        /// <param name="personId">The Colleague ID of the person</param>
        /// <returns>The Registration User Id to use for this person.</returns>
        Task<string> GetRegistrationGroupIdAsync(string personId);

        /// <summary>
        /// Returns the registration group for a specific group Id.
        /// </summary>
        /// <param name="registrationGroupId">The registration Group Id</param>
        /// <returns>The Registration User for this person.</returns>
        Task<RegistrationGroup> GetRegistrationGroupAsync(string registrationGroupId);
    }
}
