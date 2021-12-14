/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Repositories
{
    /// <summary>
    /// OutsideAwardsRepository interface
    /// </summary>
    public interface IOutsideAwardsRepository
    {
        /// <summary>
        /// Attempts to create and return the newly created outside award entity
        /// </summary>
        /// <param name="outsideAward">outside award entity</param>
        /// <returns>outside award entity</returns>
        Task<OutsideAward> CreateOutsideAwardAsync(OutsideAward outsideAward);

        /// <summary>
        /// Gets all outside awards for the specified student id and award year code
        /// </summary>
        /// <param name="studentId">student Id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <returns>List of OutsideAward entities</returns>
        Task<IEnumerable<OutsideAward>> GetOutsideAwardsAsync(string studentId, string awardYearCode);

        /// <summary>
        /// Deletes the outside award record with the specified id
        /// </summary>
        /// <param name="outsideAwardId">outside award record id</param>
        /// <returns></returns>
        Task DeleteOutsideAwardAsync(string outsideAwardId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="awardId"></param>
        /// <returns></returns>
        Task<OutsideAward> GetOutsideAwardsByAwardIdAsync(string awardId);

        /// <summary>
        /// Updates an outside award record with the specified id
        /// </summary>
        /// <param name="outsideAward">outside award entity</param>
        /// <returns></returns>
        Task <OutsideAward> UpdateOutsideAwardAsync(OutsideAward outsideAward);
    }
}
