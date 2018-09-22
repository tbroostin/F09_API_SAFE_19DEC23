using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos
{
    /// <summary>
    /// API version information.
    /// </summary>
    public class About
    {
        /// <summary>
        /// Api name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Api version
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Constructor for ApiVersion
        /// </summary>
        [JsonConstructor]
        public About()
        {
        }

        /// <summary>
        /// Constructor for ApiVersion
        /// </summary>
        /// <param name="name">Api name</param>
        /// <param name="version">Api version</param>
        public About(string name, string version)
        {
            Name = name;
            Version = version;
        }
    }
}
