using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Planning.Adapters
{
    public class AdvisorEntityAdapter 
    {
        private IAdapterRegistry AdapterRegistry;
        private ILogger Logger;

        /// <summary>
        /// Constructor for the AdvisorEntityAdapter
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger</param>
        public AdvisorEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
        {
            AdapterRegistry = adapterRegistry;
            Logger = logger;
        }   
        
        public Ellucian.Colleague.Dtos.Planning.Advisor MapToType(Domain.Planning.Entities.Advisor source, string facultyEmailTypeCode)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Dtos.Planning.Advisor advisorDto = new Dtos.Planning.Advisor();
            advisorDto.Id = source.Id;
            advisorDto.FirstName = source.FirstName;
            advisorDto.LastName = source.LastName;
            advisorDto.MiddleName = source.MiddleName;
            advisorDto.EmailAddresses = source.GetEmailAddresses(facultyEmailTypeCode);
            return advisorDto;
        }
    }
}
