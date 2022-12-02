//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for Procurements utility services
    /// </summary>
    public interface IProcurementsUtilityService : IBaseService
    {
        /// <summary>
        /// Default Line Item information for given commodity code, vendor & AP type.
        /// </summary>
        /// <param name="commodityCode">commodity code</param>
        /// <param name="VendorId">vendor id</param>
        /// <param name="apType">AP type</param>
        /// <returns>Default Line Item information DTO</returns>
        Task<NewLineItemDefaultAdditionalInformation> GetNewLineItemDefaultAdditionalInformation(string commodityCode, string vendorId, string apType);

        /// <summary>
        /// Get attachments
        /// </summary>
        /// <param name="owner">Owner to get attachments for</param>
        /// <param name="documentTagOnePrefix">Tag one prefix</param>
        /// <param name="documentIds">List of document id's</param>
        /// <returns>List of <see cref="Attachment">Attachments</see></returns>
        Task<List<Attachment>> GetAttachmentsAsync(string owner, string documentTagOnePrefix, IEnumerable<string> documentIds);
    }
}
