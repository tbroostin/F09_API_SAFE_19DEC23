// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for mapping from the SubTestResult Domain Entity to the SubTestResult DTO 
    /// </summary>
    public class SubTestResultEntityToSubTestResultDtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.SubTestResult, Ellucian.Colleague.Dtos.Student.SubTestResult>
    {
        public SubTestResultEntityToSubTestResultDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Map a SubTestResult Entity to a SubTestResult DTO
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Dtos.Student.SubTestResult MapToType(Domain.Student.Entities.SubTestResult source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "Test Result source cannot be null.");
            }

            Dtos.Student.SubTestResult SubTestResultDto = new Dtos.Student.SubTestResult();
           
            SubTestResultDto.Code = source.Code;
            SubTestResultDto.DateTaken = source.DateTaken;
            SubTestResultDto.Description = source.Description;
            SubTestResultDto.Percentile = source.Percentile;
            SubTestResultDto.Score = source.Score.HasValue ? (int)Math.Round(source.Score.Value, 0, MidpointRounding.AwayFromZero) : default(int?);
            SubTestResultDto.StatusCode = source.StatusCode;
            SubTestResultDto.StatusDate = source.StatusDate;

            return SubTestResultDto;
            
        }
    }
}
