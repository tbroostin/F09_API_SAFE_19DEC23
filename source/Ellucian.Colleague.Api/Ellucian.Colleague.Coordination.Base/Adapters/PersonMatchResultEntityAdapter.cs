// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    public class PersonMatchResultEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonMatchResult, Dtos.Base.PersonMatchResult >
    {
         /// <summary>
        /// Initializes a new instance of the PersonMatchResultEntityAdapter class
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry</param>
        /// <param name="logger">The logger</param>
        public PersonMatchResultEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        /// <summary>
        /// Maps a PersonMatchResult entity to the corresponding dto
        /// </summary>
        /// <param name="source">The <see cref="PersonMatchResult"/> entity</param>
        /// <returns>The corresponding <see cref="PersonMatchResult"/> Dto</returns>
        public override Dtos.Base.PersonMatchResult MapToType(Domain.Base.Entities.PersonMatchResult source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source", "A person match result entity must be supplied for DTO conversion.");
            }
            return new Dtos.Base.PersonMatchResult(){
                PersonId = source.PersonId,
                MatchScore = source.MatchScore,
                MatchCategory = (source.MatchCategory == Domain.Base.Entities.PersonMatchCategoryType.Definite) ? 
                    Dtos.Base.PersonMatchCategoryType.Definite :
                    Dtos.Base.PersonMatchCategoryType.Potential,
            };
        }
   }
}
