// Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for faculty office hours
    /// </summary>
    public interface IOfficeHoursService
    {
        /// <summary>
        /// Add office hours information for a faculty
        /// </summary>
        /// <param name="AddOfficeHours">The AddOfficeHours to create</param>
        /// <returns>Added OfficeHours</returns>
        Task<Dtos.Student.AddOfficeHours> AddOfficeHoursAsync(Dtos.Student.AddOfficeHours addOfficeHours);

        /// <summary>
        /// Update office hours information for a faculty
        /// </summary>
        /// <param name="UpdateOfficeHours">The office hours information to update</param>
        /// <returns>Updated OfficeHours</returns>
        Task<Dtos.Student.UpdateOfficeHours> UpdateOfficeHoursAsync(Dtos.Student.UpdateOfficeHours updateOfficeHours);

        /// <summary>
        /// delete office hours information for a faculty
        /// </summary>
        /// <param name="deleteOfficeHours">The office hours information to delete</param>
        /// <returns>Deleted OfficeHours</returns>
        Task<Dtos.Student.DeleteOfficeHours> DeleteOfficeHoursAsync(Dtos.Student.DeleteOfficeHours deleteOfficeHours);
    }
}
