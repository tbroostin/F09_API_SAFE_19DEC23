// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for Comment Subject Area services
    /// </summary>
    public interface ICommentSubjectAreaService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.CommentSubjectArea>> GetCommentSubjectAreaAsync(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.CommentSubjectArea> GetCommentSubjectAreaByIdAsync(string id);
    }
}
