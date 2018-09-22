using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Indicates whether the academic credit has been replaced by another academic credit, or may be replaced in the future.
    /// </summary>
    [Serializable]
    public enum ReplacedStatus
    {
        NotReplaced,
        Replaced,
        ReplaceInProgress,
    }
}
