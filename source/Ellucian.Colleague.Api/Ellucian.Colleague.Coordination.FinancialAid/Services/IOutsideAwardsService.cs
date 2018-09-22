/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Dtos.FinancialAid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Services
{
    /// <summary>
    /// OutsideAwardsService interface
    /// </summary>
    public interface IOutsideAwardsService
    {
        /// <summary>
        /// Creates and returns an outside award dto
        /// </summary>
        /// <param name="outsideAwardDto">inout outside award dto</param>
        /// <returns>created outside award dto</returns>
        Task<OutsideAward> CreateOutsideAwardAsync(OutsideAward outsideAwardDto);

        /// <summary>
        /// Gets a list of outside awards for the specified student id and award year code
        /// </summary>
        /// <param name="studentId">student id</param>
        /// <param name="awardYearCode">award year code</param>
        /// <returns>List of OutsideAward DTOs</returns>
        Task<IEnumerable<OutsideAward>> GetOutsideAwardsAsync(string studentId, string awardYearCode);

        /// <summary>
        /// Deletes the outside award record with the specified id
        /// </summary>
        /// <param name="studentId">student id that award belogs to</param>
        /// <param name="outsideAwardId">outside award record id</param>
        /// <returns></returns>
        Task DeleteOutsideAwardAsync(string studentId, string outsideAwardId);

        /// <summary>
        /// Updates an outside award record with the specified id
        /// </summary>
        /// <param name="studentId">student id that award belogs to</param>
        /// <param name="outsideAwardId">outside award record id</param>
        /// <returns></returns>
        Task <OutsideAward> UpdateOutsideAwardAsync(OutsideAward outsideAwardDto);
    }
}
