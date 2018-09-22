// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IRegistrationOptionsRepository
    {
        /// <summary>
        /// Returns the registration options for the given user.
        /// </summary>
        /// <param name="id">Id of a list of users</param>
        /// <returns>RegistrationOptions objects for each user</returns>
        Task<IEnumerable<RegistrationOptions>> GetAsync(IEnumerable<string> ids);
    }
}
