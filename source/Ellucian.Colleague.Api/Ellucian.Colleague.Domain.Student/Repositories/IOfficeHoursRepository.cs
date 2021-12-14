// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface to Add Office Hours
    /// </summary>
    public interface IOfficeHoursRepository
    {
        /// <summary>
        /// Add office hours information for faculty
        /// </summary>
        /// <param name="addOfficeHours">The addOfficeHours to create</param>
        /// <returns>Added OfficeHours</returns>
        Task<AddOfficeHours> AddOfficeHoursAsync(AddOfficeHours addOfficeHours);

        /// <summary>
        /// Update office hours information for a faculty
        /// </summary>
        /// <param name="UpdateOfficeHours">The office hours information to update</param>
        /// <returns>Updated OfficeHours</returns>
        Task<UpdateOfficeHours> UpdateOfficeHoursAsync(UpdateOfficeHours updateOfficeHours);

        /// <summary>
        /// delete office hours information for a faculty
        /// </summary>
        /// <param name="deleteOfficeHours">The office hours information to delete</param>
        /// <returns>Deleted OfficeHours</returns>
        Task<DeleteOfficeHours> DeleteOfficeHoursAsync(DeleteOfficeHours deleteOfficeHours);
    }
}
