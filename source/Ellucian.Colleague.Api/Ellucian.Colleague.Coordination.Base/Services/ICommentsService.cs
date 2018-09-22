// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Comments services
    /// </summary>
    public interface ICommentsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.Comments>, int>> GetCommentsAsync(int offset, int limit, string subjectMatter, string commentSubjectArea, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.Comments> GetCommentByIdAsync(string id);
        Task<Ellucian.Colleague.Dtos.Comments> PutCommentAsync(string id,  Dtos.Comments comment);
        Task<Ellucian.Colleague.Dtos.Comments> PostCommentAsync(Dtos.Comments comment);
        Task DeleteCommentByIdAsync(string id);
    }
}
