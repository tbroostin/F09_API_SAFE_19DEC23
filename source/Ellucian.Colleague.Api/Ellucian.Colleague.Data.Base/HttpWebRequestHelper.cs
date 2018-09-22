using Ellucian.Web.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base
{
    /// <summary>
    /// Use the HttpWebRequestHelper to encapsulate an HttpWebRequest and stub the behavior 
    /// of non-overridable methods and properties.
    /// HttwpWebRequestHelper can be injected into class constructors
    /// </summary>
    [RegisterType]
    public class HttpWebRequestHelper
    {
        /// <summary>
        /// The encapsulated HttpWebRequest
        /// </summary>
        private HttpWebRequest httpWebRequest;

        public HttpWebRequestHelper()
        {

        }

        /// <summary>
        /// Get the HttpWebRequest. Should always be called after SetHttpWebRequest
        /// </summary>
        /// <returns></returns>
        public virtual HttpWebRequest GetHttpWebRequest()
        {
            if (httpWebRequest == null)
            {
                throw new InvalidOperationException("HttpWebRequest has not been set");
            }
            return httpWebRequest;
        }

        /// <summary>
        /// Set the HttpWebRequest. Should be called for each new WebRequest
        /// </summary>
        /// <param name="webRequest"></param>
        public virtual void SetHttpWebRequest(HttpWebRequest webRequest)
        {
            this.httpWebRequest = webRequest;
        }

        /// <summary>
        /// Overrides the Host property of HttpWebRequest, but does not change the behavior.
        /// </summary>
        public virtual string Host
        {
            get
            {
                return httpWebRequest.Host;
            }
            set
            {
                httpWebRequest.Host = value;
            }
        }
    }
}
