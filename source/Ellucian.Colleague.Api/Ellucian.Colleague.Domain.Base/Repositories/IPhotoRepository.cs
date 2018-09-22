using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// PhotoRepository interface
    /// </summary>
    public interface IPhotoRepository
    {
        /// <summary>
        /// Gets a person's photo by person id.
        /// </summary>
        /// <param name="id">Person's id</param>
        /// <returns>Photograph for the person id.</returns>
        Photograph GetPersonPhoto(string id);
    }
}
