// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System.Reflection;

namespace Ellucian.Web.Http.Configuration
{
    public static class ApiProductInfo
    {
        static string productId = "Colleague Web API";
        public static string ProductId { get { return productId; } }

        static string productVersion = getProductVersion();
        public static string ProductVersion { get { return productVersion; } }

        static string productNamespace = string.Join("/", "Ellucian", productId, productVersion);
        public static string ProductNamespace { get { return productNamespace; } }

        // TODO
        // settings.config should be modified to specify cluster name(s) for clusterd configuration. When that's done
        // the product namespace above should be appended with those values.

        static string getProductVersion()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            return assemblyVersion.ToString();
        }
    }

}
