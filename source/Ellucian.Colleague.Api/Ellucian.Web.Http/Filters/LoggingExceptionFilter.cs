using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using slf4net;
using Newtonsoft.Json;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Web.Http.Filters
{
    /// <summary>
    /// A web API filter for logging uncaught exceptions.
    /// </summary>
    public class LoggingExceptionFilter : IExceptionFilter
    {
        private readonly ILogger logger;
        public ILogger Logger { get { return logger; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingExceptionFilter"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public LoggingExceptionFilter(ILogger logger)
        {
            this.logger = logger;
        }

        public Task ExecuteExceptionFilterAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                var msg = string.Format("An unhandled exception was thrown by the controller {0}", 
                    actionExecutedContext.ActionContext.ControllerContext.Controller.ToString());
                var exception = actionExecutedContext.Exception;
                Logger.Error(exception, msg);

                // Override the default Web API behavior by returning just the exception message rather than the stacktrace.
                if (actionExecutedContext.Response == null)
                {
                    WebApiException exceptionObject = new WebApiException() { Message = exception.Message.Replace(Environment.NewLine, " ").Replace("\n", " ") };
                    var serialized = JsonConvert.SerializeObject(exceptionObject);
                    actionExecutedContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest) { Content = new StringContent(serialized) };
                }
            });
        }

        public bool AllowMultiple
        {
            get { return false; }
        }
    }
}
