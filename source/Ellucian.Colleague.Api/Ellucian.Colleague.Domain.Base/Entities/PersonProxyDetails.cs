using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    [Serializable]
    public class PersonProxyDetails
    {

        public string PersonId { get; private set; }
        /// <summary>
        /// Preferred name for a given user
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// The email address that will be used for proxy communication
        /// </summary>
        public string ProxyEmailAddress { get; set; }

        public PersonProxyDetails(string personId, string preferredName, string proxyEmailAddress)
        {
            if(string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person Id cannot be null");
            }
            if(string.IsNullOrEmpty(preferredName))
            {
                throw new ArgumentNullException("preferredName", "Preferred name cannot be null");
            }
            PersonId = personId;
            PreferredName = preferredName;
            ProxyEmailAddress = proxyEmailAddress;
        }
    }
}
