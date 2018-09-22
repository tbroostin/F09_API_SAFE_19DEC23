using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ellucian.Web.Http.TestUtil
{
    /// <summary>
    /// A fake handler for injecting mocked responses into the HTTP stack.
    /// </summary>
    public class MockHandler : DelegatingHandler
    {
        public Queue<HttpResponseMessage> Responses = new Queue<HttpResponseMessage>();
        public HttpRequestMessage Request;

        protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Request = request;

            if (Responses.Count == 0)
            {
                return base.SendAsync(request, cancellationToken);
            }

            return Task.Factory.StartNew(() => Responses.Dequeue());
        }
    }
}
