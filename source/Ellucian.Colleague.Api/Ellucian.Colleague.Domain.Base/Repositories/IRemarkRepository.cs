/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IRemarkRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<Remark>, int>> GetRemarksAsync(int offset, int limit, string subjectMatter, string commentSubjectArea);
        Task<Remark> GetRemarkByGuidAsync(string guid);
        Task<Remark> UpdateRemarkAsync(Remark remark);
        Task DeleteRemarkAsync(string guid);

        Task<Dictionary<string, Dictionary<string, string>>> GetPersonDictionaryCollectionAsync(IEnumerable<string> personIds);
    }
}