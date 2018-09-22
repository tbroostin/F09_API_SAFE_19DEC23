using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Web.Http.EthosExtend
{
    /// <summary>
    /// Route resource information for Ethos endpoints
    /// </summary>
    [Serializable]
    public class EthosResourceRouteInfo
    {
        /// <summary>
        /// Name of the resource (api)
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Version Number for the resource
        /// </summary>
        public string ResourceVersionNumber { get; set; }

        /// <summary>
        /// Full representation of the custom media type
        /// </summary>
        public string EthosResourceIdentifier { get; set; }

        /// <summary>
        /// Extended Schema Identifier
        /// </summary>
        public string ExtendedSchemaResourceId { get; set; }
    }
}
