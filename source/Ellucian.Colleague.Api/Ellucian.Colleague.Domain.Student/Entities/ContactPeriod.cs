using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Enumeration for model values of contact period (CONTACT.MEASURES)
    /// </summary>
    [Serializable]
    public enum ContactPeriod
    {
        notSet = 0,
        day, week, month, term
    }
}
