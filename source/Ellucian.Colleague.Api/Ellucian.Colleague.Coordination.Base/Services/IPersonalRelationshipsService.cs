// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonalRelationshipsService : IBaseService
    {
        Task<Tuple<IEnumerable<Dtos.PersonalRelationship>, int>> GetAllPersonalRelationshipsAsync(int offset, int limit, bool bypassCache);
        Task<Dtos.PersonalRelationship> GetPersonalRelationshipByIdAsync(string id);
        Task<Tuple<IEnumerable<Dtos.PersonalRelationship>, int>> GetPersonalRelationshipsByFilterAsync(int offset, int limit, string subjectPerson = "", 
            string relatedPerson = "", string directRelationshipType = "", string directRelationshipDetailId = "");
        Task<Dtos.PersonalRelationship> UpdatePersonalRelationshipAsync(string id, Dtos.PersonalRelationship personalRelationship);
        Task<Dtos.PersonalRelationship> CreatePersonalRelationshipAsync(Dtos.PersonalRelationship personalRelationship);
    }
}
