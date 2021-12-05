// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Photo services
    /// </summary>
    public interface IPhotoService
    {
        /// <summary>
        /// Get a photo for a person id
        /// </summary>
        /// <param name="id">Person id.</param>
        /// <returns>A <see cref="Ellucian.Colleague.Dtos.Base.Photograph">Photo</see></returns>
        Task<Domain.Base.Entities.Photograph> GetPersonPhotoAsync(string id);
    }
}
