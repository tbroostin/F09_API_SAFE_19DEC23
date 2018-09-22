// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Student.Adapters
{
    /// <summary>
    /// Adapter for mapping from the ComponentTest Domain Entity to the ComponentTest DTO 
    /// </summary>
    public class ComponentTestEntityToComponentTestDtoAdapter : BaseAdapter<Ellucian.Colleague.Domain.Student.Entities.ComponentTest, Ellucian.Colleague.Dtos.Student.ComponentTest>
    {
        public ComponentTestEntityToComponentTestDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Map a ComponentTest Entity to a ComponentTest DTO
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Dtos.Student.ComponentTest MapToType(Domain.Student.Entities.ComponentTest source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "Component test source cannot be null.");
            }
            
            Dtos.Student.ComponentTest ComponentTestDto = new Dtos.Student.ComponentTest();

            ComponentTestDto.Test = source.Test;
            ComponentTestDto.Percentile = source.Percentile;
            ComponentTestDto.Score = source.Score.HasValue ? (int)Math.Round(source.Score.Value, 0, MidpointRounding.AwayFromZero) : default(int?);
            

            return ComponentTestDto;
            
        }
    }
}
