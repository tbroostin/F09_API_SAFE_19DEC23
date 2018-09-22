using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Collections.Concurrent;
using System.Web.Http.Dispatcher;
using System.Web.Http;
using System.ComponentModel;

namespace Ellucian.Web.Http.Controllers
{
    public class AreaHttpControllerSelector : DefaultHttpControllerSelector
    {
        private const string AreaRouteVariableName = "area";

        private readonly HttpConfiguration _configuration;
        private readonly Lazy<ConcurrentDictionary<string, Type>> _apiControllerTypes;
        private readonly ConcurrentDictionary<string, HttpControllerDescriptor> _apiControllerDescriptors;

        public AreaHttpControllerSelector(HttpConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;
            _apiControllerTypes = new Lazy<ConcurrentDictionary<string, Type>>(GetControllerTypes);
            _apiControllerDescriptors = GetControllerDescriptors();
        }

        public override IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
        {
            return _apiControllerDescriptors;
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            return this.GetApiController(request);
        }

        private ConcurrentDictionary<string, HttpControllerDescriptor> GetControllerDescriptors()
        {
            var controllerDescriptorDictionary = new ConcurrentDictionary<string, HttpControllerDescriptor>();

            foreach (var type in _apiControllerTypes.Value)
            {
                var position = type.Key.LastIndexOf('.') + 1;
                var controllerName = type.Key.Substring(position).Replace(DefaultHttpControllerSelector.ControllerSuffix, string.Empty);
                var controllerType = type.Value;

                bool added = controllerDescriptorDictionary.TryAdd(controllerName, new HttpControllerDescriptor(_configuration, controllerName, controllerType));
            }

            return controllerDescriptorDictionary;
        }

        private static string GetAreaName(HttpRequestMessage request)
        {
            var data = request.GetRouteData();
            if (data.Route.DataTokens == null)
            {
                return null;
            }
            else
            {
                object areaName;
                return data.Route.DataTokens.TryGetValue(AreaRouteVariableName, out areaName) ? areaName.ToString() : null;
            }
        }

        private static ConcurrentDictionary<string, Type> GetControllerTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var types = assemblies
                .SelectMany(a => a
                    .GetTypes().Where(t =>
                        !t.IsAbstract &&
                        t.Name.EndsWith(ControllerSuffix, StringComparison.OrdinalIgnoreCase) &&
                        typeof(IHttpController).IsAssignableFrom(t)))
                .ToDictionary(t => t.FullName, t => t);

            return new ConcurrentDictionary<string, Type>(types);
        }

        private HttpControllerDescriptor GetApiController(HttpRequestMessage request)
        {
            var areaName = GetAreaName(request);
            var controllerName = GetControllerName(request);
            var type = GetControllerType(areaName, controllerName);

            return new HttpControllerDescriptor(_configuration, controllerName, type);
        }

        private Type GetControllerType(string areaName, string controllerName)
        {
            var query = _apiControllerTypes.Value.AsEnumerable();

            if (string.IsNullOrEmpty(areaName))
            {
                query = query.WithoutAreaName();
            }
            else
            {
                query = query.ByAreaName(areaName);
            }

            return query
                .ByControllerName(controllerName)
                .Select(x => x.Value)
                .Single();
        }
    }
}
