using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Credit Types services
    /// </summary>
    public interface ICreditTypeService
    {
        IEnumerable<Ellucian.Colleague.Dtos.CreditCategory> GetCreditTypes();
    }
}
