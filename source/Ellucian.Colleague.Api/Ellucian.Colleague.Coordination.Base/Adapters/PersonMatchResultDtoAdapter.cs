// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    /// <summary>
    /// Creates a PersonMatchResult entity from the corresponding dto
    /// </summary>
    public class PersonMatchResultDtoAdapter : AutoMapperAdapter<Dtos.Base.PersonMatchResult, Domain.Base.Entities.PersonMatchResult>
    {
        /// <summary>
        /// Initializes a new instance of the PersonMatchResultDtoAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public PersonMatchResultDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Maps a PersonMatchResult dto to the corresponding entity
        /// </summary>
        /// <param name="source">The <see cref="PersonMatchResult"/> Dto</param>
        /// <returns>The corresponding <see cref="PersonMatchResult"/> entity</returns>
        public override Domain.Base.Entities.PersonMatchResult MapToType(Dtos.Base.PersonMatchResult source)
        {
            if (source == null ){
                throw new ArgumentNullException("source");
            }
            var category = (source.MatchCategory == Dtos.Base.PersonMatchCategoryType.Definite)? "D" : "P";
            var result = new Domain.Base.Entities.PersonMatchResult(
                source.PersonId,
                source.MatchScore,
                category);
            return result;
        }
    }
}
